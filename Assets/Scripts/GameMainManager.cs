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
    public AudioManager AM;
    public SpriteRenderer bgCover;
    public MultTouchHandler multTouchHandler;
    public ObjectCounter objectCounter;
    public Transform Notes;
    public GameObject SongDetail;

    [Space(10)]
    [Header("UI")]
    public GameObject Menu;
    public Button Upload;
    public Button PlayPause;
    public Button Stop;
    public TMP_Dropdown levelSelector;
    public Toggle recordMode;
    public Slider Offset;
    public TMP_Text OffsetDisplay;
    
    [Space(10)]
    [Header("AudioRef")]
    public AudioSource bgm;

    [Space(10)]
    [Header("Settings")]
    public bool comboStatusType;
    public int difficulty = 0;
    public float startTime = 0f;
    public long startAt;
    public float audioSpeed = 1f;
    public float noteSpeed = 7f;
    public float touchSpeed = 7f;
    public float backgroundCover = 0.6f;
    public float offset;

    [Space(10)]
    [Header("Sprites")]
    public Sprite ic_play;
    public Sprite ic_pause;
    public Sprite ic_upload;
    public Sprite ic_reset;
    private bool inited = false;
    private int status = 0;

    public void ChickStart()
    {
        startAt = System.DateTime.Now.Ticks;
        timeProvider.SetStartTime(startAt, startTime - offset, audioSpeed, recordMode.isOn);
        loader.noteSpeed = (float)(107.25 / (71.4184491 * Mathf.Pow(noteSpeed + 0.9975f, -0.985558604f)));
        loader.touchSpeed = touchSpeed;
        objectCounter.ComboSetActive(comboStatusType);
        loader.PlayLevel(levelSelector.value, startTime);
        multTouchHandler.clearSlots();
        bgCover.color = new Color(0f, 0f, 0f, backgroundCover);
        if (recordMode.isOn)
        {
            bgm.PlayDelayed(5f);
            Notes.GetComponent<PlayAllPerfect>().enabled = true;
            bgManager.PlaySongDetail();
        }
        else
        {
            bgm.Play();
            Notes.GetComponent<PlayAllPerfect>().enabled = false;
        }
        inited = true;
        PlayPause.gameObject.GetComponentInChildren<TMP_Text>().text = "Pause";
        PlayPause.gameObject.GetComponentsInChildren<Image>()[1].sprite = ic_pause;
        Stop.interactable = true;
        levelSelector.interactable = false;
        recordMode.interactable = false;
        HideMenu();
    }

    public void TogglePlay()
    {
        if (!inited) {ChickStart();return;}
        if (timeProvider.isStart) {
            startTime = timeProvider.AudioTime;
            timeProvider.isStart = false;
            bgManager.PauseVideo();
            PlayPause.gameObject.GetComponentInChildren<TMP_Text>().text = "Play";
            PlayPause.gameObject.GetComponentsInChildren<Image>()[1].sprite = ic_play;
            bgm.Stop();
        } else {
            startAt = System.DateTime.Now.Ticks;
            timeProvider.SetStartTime(startAt, startTime, audioSpeed);
            bgManager.ContinueVideo(audioSpeed);
            PlayPause.gameObject.GetComponentInChildren<TMP_Text>().text = "Pause";
            PlayPause.gameObject.GetComponentsInChildren<Image>()[1].sprite = ic_pause;
            bgm.time = timeProvider.AudioTime;
            bgm.Play();
        }
    }

    public void ChickStop()
    {
        // reset audiotime
        startTime = 0f;
        timeProvider.ResetStartTime();
        // destroy all notes
        foreach (Transform child in Notes.transform) {
            GameObject.Destroy(child.gameObject);
        }
        // stop bgm completely
        bgm.Stop();
        bgm.time = 0f;
        // stop sounds
        AM.StopAll();
        // maybe songdetail is playing...
        SongDetail.SetActive(false);
        // re-init on next start
        inited = false;
        // reset counter
        objectCounter.tapSum = 0;
        objectCounter.holdSum = 0;
        objectCounter.breakSum = 0;
        objectCounter.touchSum = 0;
        objectCounter.slideSum = 0;
        objectCounter.tapCount = 0;
        objectCounter.holdCount = 0;
        objectCounter.breakCount = 0;
        objectCounter.slideCount = 0;
        objectCounter.touchCount = 0;
        objectCounter.UpdateSideOutput();
        // set UIs
        PlayPause.gameObject.GetComponentInChildren<TMP_Text>().text = "Play";
        PlayPause.gameObject.GetComponentsInChildren<Image>()[1].sprite = ic_play;
        Stop.interactable = false;
        levelSelector.interactable = true;
        recordMode.interactable = true;
    }

    public void ChickLoad()
    {
        if (status == 0)
        {
            // open maidata.txt
            Action<string> callback = (path) => {StartCoroutine(initDataFromWeb(path));};
            FileUploaderHelper.RequestFile(callback, ".txt");
        }
        else if (status == 1)
        {
            // open track.mp3
            Action<string> callback = (path) => { StartCoroutine(LoadWebAudio(path));};
            FileUploaderHelper.Dispose();
            FileUploaderHelper.RequestFile(callback, ".mp3");
        }
        else if (status == 2)
        {
            // open bg.jpg
            Action<string> callback = (path) => { LoadBG(path);};
            FileUploaderHelper.Dispose();
            FileUploaderHelper.RequestFile(callback, ".jpg");
        }
        else if (status == 3)
        // reset
        {SceneManager.LoadScene(0, LoadSceneMode.Single);}
    }

    public void UpdateOffset()
    {
        offset = Offset.value / 1000;
        OffsetDisplay.text = string.Format("OFFSET: {0} MS", Offset.value);
    }

    public void UpdateLevel()
    {
        if (SimaiProcess.Serialize(SimaiProcess.fumens[levelSelector.value]) == -1) {PlayPause.interactable = false; return;}
        Debug.Log(SimaiProcess.notelist.Count);
        if (SimaiProcess.notelist.Count == 0) 
        {
            PlayPause.interactable = false;
        }
        else
        {
            PlayPause.interactable = true;
        }
    }

    public void LoadBG(string path)
    {
        if (path == string.Empty) {Debug.LogError("Empty path!"); return;}
        Debug.Log(path);
        bgManager.LoadBGFromWeb(path);
        Upload.gameObject.GetComponentInChildren<TMP_Text>().text = "Reset";
        Upload.gameObject.GetComponentsInChildren<Image>()[1].sprite = ic_reset;
        PlayPause.interactable = true;
        levelSelector.interactable = true;
        recordMode.interactable = true;
        UpdateLevel();
        status = 3;
    }

    IEnumerator initDataFromWeb(string path)
    {
        if (path == string.Empty) {Debug.LogError("Empty path!"); yield break;}
        Debug.Log("从" + path + "加载谱面...");
        SimaiProcess.ClearData();
        UnityWebRequest www = UnityWebRequest.Get(path);
        yield return www.SendWebRequest();
 
        if (www.result != UnityWebRequest.Result.Success) {
            Debug.LogError(www.error);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);
            if (!SimaiProcess.ReadDataRaw(www.downloadHandler.text.Split((Environment.NewLine))))
            {
                Debug.LogError("Error Loading Chart.");
            }
            else {
                Upload.gameObject.GetComponentInChildren<TMP_Text>().text = "Upload Audio";
                status = 1;
            }
        }
    }

    IEnumerator LoadWebAudio(string path)
    {
        if (path == string.Empty) {Debug.LogError("Empty path!"); yield break;}
        Debug.Log("从" + path + "加载音频...");
        UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.MPEG);
        yield return www.SendWebRequest();
 
        if (www.result != UnityWebRequest.Result.Success) {
            Debug.LogError(www.error);
        }
        else
        {
            var clip = DownloadHandlerAudioClip.GetContent(www);
            if(clip != null) {
                bgm.clip = clip;
                Upload.gameObject.GetComponentInChildren<TMP_Text>().text = "Upload BG";
                status = 2;
            } else {Debug.LogError("AudioClip is None Error!");}
        }
    }

    public void ShowMenu() {Menu.SetActive(true);}

    public void HideMenu() {Menu.SetActive(false);}

    void Start()
    {
        // loader.initFromFile("D:\\DNV\\games\\maimai\\track\\viewtest");
        // StartCoroutine(LoadWebAudio("file:\\\\D:\\DNV\\games\\maimai\\track\\viewtest\\track.mp3"));
        // PlayPause.interactable = true;
        // Stop.interactable = false;
        // levelSelector.interactable = true;
        // recordMode.interactable = true;
        Upload.gameObject.GetComponentsInChildren<Image>()[1].sprite = ic_upload;
        PlayPause.gameObject.GetComponentsInChildren<Image>()[1].sprite = ic_play;
    }
}
