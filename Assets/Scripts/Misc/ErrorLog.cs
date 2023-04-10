using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ErrorLog : MonoBehaviour
{
    void Start()
    {
        Application.logMessageReceived += LogCallback;
    }

    void LogCallback(string condition, string stackTrace, LogType type)
    {
        if(type == LogType.Error)
        {
            GetComponent<Text>().text = string.Format("{0}\n{1}", condition, stackTrace);
        }
    }
}
