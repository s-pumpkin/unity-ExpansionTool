using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class UIToggleChangeBase : MonoBehaviour
{
    private Toggle toggle;
    private void Awake()
    {
        toggle = GetComponent<Toggle>();
        if (toggle != null)
        {
            toggle.onValueChanged.AddListener(delegate (bool isOn) { OnChangeEvent(isOn); });
            OnChangeEvent(toggle.isOn);
        }
    }

    public abstract void OnChangeEvent(bool isOn);

    public void DoForceRefresh()
    {
        OnChangeEvent(toggle.isOn);
    }
}
