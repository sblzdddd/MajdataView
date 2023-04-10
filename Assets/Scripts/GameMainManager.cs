using System;
using System.Runtime;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameMainManager : MonoBehaviour
{
    [Header("Manager")]
    public SimaiDataLoader loader;
    public AudioTimeProvider timeProvider;
    public BGManager bgManager;
    public SpriteRenderer bgCover;
    public MultTouchHandler multTouchHandler;
    public ObjectCounter objectCounter;
    public Transform Notes;
    public GameObject SongDetail;
    public SoundEffect SE;
    public MenuManager menuManager;
    public SettingsManager settings;
    
    [Space(10)]
    [Header("AudioRef")]
    public AudioSource bgm;

    [Space(10)]
    [Header("Settings")]
    public int difficulty = 0;
    public float startTime = 0f;
    public long startAt;
    public float audioSpeed = 1f;
    public float offset;
    
    [Space(10)]
    [Header("Debug")]
    public string editorInitPath;

    private bool inited = false;
    private int status = 0;

    // init loading & start playing method
    public void ChickStart()
    {
        startAt = System.DateTime.Now.Ticks;
        loader.noteSpeed = (float)(107.25 / (71.4184491 * Mathf.Pow(settings.noteSpeed + 0.9975f, -0.985558604f)));
        loader.touchSpeed = settings.touchSpeed;
        timeProvider.audioOffset = settings.offset;
        timeProvider.SetStartTime(startAt, startTime - offset, audioSpeed, menuManager.isRecord);
        objectCounter.ComboSetActive(settings.combo);
        loader.PlayLevel(menuManager.level, startTime);
        multTouchHandler.clearSlots();
        bgCover.color = new Color(0f, 0f, 0f, settings.bgCover);

        if (menuManager.isRecord)
        {
            Notes.GetComponent<PlayAllPerfect>().enabled = true;
            // disable playpause btn to prevent errors
            bgManager.PlaySongDetail();
        }
        else
        {
            Notes.GetComponent<PlayAllPerfect>().enabled = false;
        }
        inited = true;
        // set btn states
        menuManager.SetPlayMode();
        menuManager.HideMenu();
    }

    // callback of play/pause button
    public void TogglePlay()
    {
        if (!inited) {ChickStart();return;}
        if (timeProvider.isStart) {
            timeProvider.Pause();
            bgManager.PauseVideo();
            menuManager.SetPauseMode();
        } else {
            timeProvider.Resume();
            bgManager.ContinueVideo(audioSpeed);
            menuManager.SetPlayMode();
        }
    }

    // callback of stop button
    public void ChickStop()
    {
        // hide bgcover
        bgCover.color = new Color(0f, 0f, 0f, 0f);
        // reset audiotime
        startTime = 0f;
        timeProvider.ResetStartTime();
        // destroy all notes
        foreach (Transform child in Notes.transform) {
            GameObject.Destroy(child.gameObject);
        }
        // disable songdetail
        SongDetail.SetActive(false);
        // re-init on next start
        inited = false;
        // reset counter
        objectCounter.Reset();
        // set btn states
        menuManager.SetReadyMode();
    }

    // callback of upload button
    public void ChickLoad()
    {
        #if UNITY_EDITOR
            EditorLoad();
        #elif UNITY_WEBGL
            WebUpload();
        #endif
    }

    void EditorLoad()
    {
        if (status == 0)
        {
            loader.initFromFile(editorInitPath);
            Action audioCallback = () =>
            {
                menuManager.SetReadyMode();
                UpdateLevel();
                status = 3;
            };
            StartCoroutine(SE.LoadWebAudio(editorInitPath+"\\track.mp3", audioCallback));
        } else if (status == 3)
        // reset
        {SceneManager.LoadScene(0, LoadSceneMode.Single);}
    }

    public void WebUpload()
    {
        if (status == 0)
        {
            // open maidata.txt
            Action successCallback = () =>
            {
                menuManager.uploadText = "Upload Audio";
                status = 1;
            };
            Action<string> uploadedCallback = (path) =>
            {
                StartCoroutine(loader.initFromWeb(path, successCallback));
            };
            WebFileUploaderHelper.RequestFile(uploadedCallback, ".txt");
        }
        else if (status == 1)
        {
            // open track.mp3
            Action audioCallback = () =>
            {
                menuManager.uploadText = "Upload BG";
                status = 2;
            };
            Action<string> callback = (path) => 
            {
                StartCoroutine(SE.LoadWebAudio(path, audioCallback));
            };
            WebFileUploaderHelper.Dispose();
            WebFileUploaderHelper.RequestFile(callback, ".mp3");
        }
        else if (status == 2)
        {
            // open bg.jpg
            Action bgCallback = () =>
            {
                menuManager.SetReadyMode();
                UpdateLevel();
                status = 3;
            };
            Action<string> callback = (path) =>
            { 
                StartCoroutine(bgManager.LoadBGFromWeb(path, bgCallback));
            };
            WebFileUploaderHelper.Dispose();
            WebFileUploaderHelper.RequestFile(callback, ".jpg");
        }
        else if (status == 3)
        // reset
        {SceneManager.LoadScene(0, LoadSceneMode.Single);}
    }

    // method that checks if level is empty
    public void UpdateLevel()
    {
        int levelIndex = menuManager.level;
        Debug.Log("finding level: " + levelIndex);
        string fumens = SimaiProcess.fumens[levelIndex];
        if (fumens == null)
        {
            Debug.Log("Null level!");
            menuManager.DisablePlay();
        }
        else
        {
            if (SimaiProcess.Serialize(fumens) == -1) 
            {
            menuManager.DisablePlay();
                return;
            }
            Debug.Log("Total notes: "+SimaiProcess.notelist.Count);
            if (SimaiProcess.notelist.Count <= 0)
            {
                Debug.Log("Empty level!");
                menuManager.DisablePlay();
            }
            else {menuManager.SetReadyMode();}
        }
    }
}
