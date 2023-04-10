using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGroup : MonoBehaviour
{
    public List<GameObject> childComponents = new List<GameObject>{};
    public bool showChild;

    void Start() {Apply();}

    public void ToggleGroup()
    {
        showChild = !showChild;
        Apply();
    }

    public void Apply()
    {
        foreach(GameObject component in childComponents)
        {
            if (showChild) {component.SetActive(true);}
            else {component.SetActive(false);}
        }
    }
}
