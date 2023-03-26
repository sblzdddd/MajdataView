using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FPSMonitor : MonoBehaviour
{
    public int frameRate = 120;
    
    private int _frame;
    private float _lastTime;
    private float _frameDeltaTime;
    private float _Fps;
    public float _frameCalcInterval = 1f;
    public Text displayer;
    public Text about;

 
    void Start()
    {
        Application.targetFrameRate = frameRate;
        _lastTime = Time.realtimeSinceStartup;
    }
 
    void Update()
    {
        FrameCalculate();
        string msg = string.Format("FPS: {0:N1}({1})\nDeltaTime: {2:N4}", _Fps, frameRate, _frameDeltaTime);
        displayer.text = msg;
        about.text = string.Format("{0}\nGenerated by MajDataView", Application.version);
    }
 
    private void FrameCalculate()
    {
        _frame++;
        if (Time.realtimeSinceStartup - _lastTime < _frameCalcInterval) {return;}
 
        float time = Time.realtimeSinceStartup - _lastTime;
        _Fps = _frame / time;
        _frameDeltaTime = time / _frame;
 
        _lastTime = Time.realtimeSinceStartup;
        _frame = 0;
    }

    public void setTargetFPS(int fps)
    {
        frameRate = fps;
        Application.targetFrameRate = frameRate;
    }
}