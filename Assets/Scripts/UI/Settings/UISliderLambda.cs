using System;
using UnityEngine;

public class UISliderLambda : UISlider
{
    protected override void OnValueChanged(float arg0)
    {
        Debug.Log(arg0);
        Settings.Lambda = Math.Clamp(arg0, 0, 1);
    }

    protected override float GetDefaultValue()
    {
        return Settings.Lambda;
    }
}
