using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIToggleChangeColor : UIToggleChangeBase
{
    public Graphic[] graphics;

    public Color ShowColor = Color.black;
    public Color HideColor = Color.white;

    public override void OnChangeEvent(bool isOn)
    {
        Color color = isOn ? ShowColor : HideColor;

        foreach (Graphic g in graphics)
        {
            if (g != null)
                g.color = color;
        }
    }
}
