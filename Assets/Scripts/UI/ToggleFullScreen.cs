using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class ToggleFullScreen : MonoBehaviour
{
    public Sprite ic_maximize;
    public Sprite ic_minimize;
    public void ToggleFullscreen()
    {
        Debug.Log("ToggleFullScreen");
        var resolutions = Screen.resolutions;
        if (Screen.fullScreen)
        {
            Screen.SetResolution(960, 600, false);
            GetComponent<Image>().sprite = ic_minimize;
        }
        else
        {
            Screen.SetResolution(resolutions[resolutions.Length - 1].width, resolutions[resolutions.Length - 1].height, true);
            GetComponent<Image>().sprite = ic_maximize;
        }
        Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
    }
    public void UpdateIcon()
    {
        if (Screen.fullScreen)
        {
            GetComponent<Image>().sprite = ic_minimize;
        }
        else
        {
            GetComponent<Image>().sprite = ic_maximize;
        }
    }
}
