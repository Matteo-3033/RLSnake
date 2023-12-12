using System;

public class UISliderGamma : UISlider
{
    protected override void OnValueChanged(float arg0)
    {
        Settings.Gamma = Math.Clamp(arg0, 0, 1);
    }

    protected override float GetDefaultValue()
    {
        return Settings.Gamma;
    }
}
