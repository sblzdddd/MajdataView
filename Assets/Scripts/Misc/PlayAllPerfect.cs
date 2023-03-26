using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAllPerfect : MonoBehaviour
{
    public GameObject Allperfect;
    public AudioTimeProvider timeProvider;
    private bool APlusPlayed = false;
    void OnEnable()
    {
        APlusPlayed = false;
        Allperfect.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(timeProvider.isStart&&transform.childCount==0&&Allperfect)
        {
            Allperfect.SetActive(true);
            if (!APlusPlayed) {
                GameObject.Find("Audio").GetComponent<AudioManager>().Play(11, true);
                APlusPlayed = true;
            }
            this.enabled = false;
        }
    }
}
