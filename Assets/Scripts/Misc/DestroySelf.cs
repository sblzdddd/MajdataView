using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroySelf : MonoBehaviour
{

    public bool ifDestroy;
    public bool ifStopRecording;

    void Update()
    {
        if (ifDestroy) Destroy(gameObject);
    }
}
