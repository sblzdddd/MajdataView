using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadJustSprite : MonoBehaviour
{
    public int _0curv1str2wifi;
    public int indexOffset;
    
    void Start()
    {
        //gameObject.GetComponent<SpriteRenderer>().sprite = GameObject.Find("Outline").GetComponent<CustomSkin>().Just[_0curv1str2wifi + 3];
        //setR();
    }

    public int setR()
    {
        indexOffset = 0;
        refreshSprite();
        return _0curv1str2wifi;
    }

    public int setL()
    {
        indexOffset = 3;
        refreshSprite();
        return _0curv1str2wifi;
    }

    void refreshSprite()
    {
        gameObject.GetComponent<SpriteRenderer>().sprite = GameObject.Find("Skin").GetComponent<CustomSkin>().Just[_0curv1str2wifi + indexOffset];
    }
}
