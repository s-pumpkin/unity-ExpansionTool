using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIToggleChangeEnabled : UIToggleChangeBase
{
    public MonoBehaviour[] Ons;    

    public bool isReverse;

    public override void OnChangeEvent(bool isOn)
    {
        bool state = isReverse ? !isOn : isOn;

        foreach (MonoBehaviour item in Ons)
        {
            if (item != null)
                item.enabled = state;
        }
    }
}
