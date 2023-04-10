using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Floating : MonoBehaviour
{
    public float rotate;
    public float wx;
    public float wy;
    Vector3 startPos;
    RectTransform rectTransform;
    
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        startPos = rectTransform.localPosition;
    }

    void Update()
    {
        rectTransform.Rotate(new Vector3(0f, 0f, rotate));
        rectTransform.localPosition = startPos + new Vector3(Mathf.Sin(Time.time * wx), Mathf.Sin(Time.time * wy));
    }
}
