using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIToggleChangeActive : UIToggleChangeBase
{
    public GameObject[] gameObjects;

    public bool isReverse;

    public override void OnChangeEvent(bool isOn)
    {
        bool state = isReverse ? !isOn : isOn;

        foreach (GameObject go in gameObjects)
        {
            if (go != null)
                go.SetActive(state);
        }
    }
}
