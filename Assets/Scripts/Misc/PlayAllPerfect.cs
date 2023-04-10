using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAllPerfect : MonoBehaviour
{
    public GameObject Allperfect;
    public AudioTimeProvider timeProvider;
    void OnEnable()
    {
        Allperfect.SetActive(false);
    }

    void Update()
    {
        if(timeProvider.isStart&&transform.childCount==0&&Allperfect)
        {
            Allperfect.SetActive(true);
            this.enabled = false;
        }
    }
}
