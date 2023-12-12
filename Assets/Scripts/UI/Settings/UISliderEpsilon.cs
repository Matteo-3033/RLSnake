using System;

public class UISliderEpsilon : UISlider
{
    protected override void OnValueChanged(float arg0)
    {
        Settings.Epsilon = Math.Clamp(arg0, 0, 1);
    }

    protected override float GetDefaultValue()
    {
        return Settings.Epsilon;
    }
}
