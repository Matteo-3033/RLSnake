public class UISliderAlpha : UISlider
{
    protected override void OnValueChanged(float arg0)
    {
        Settings.Alpha = arg0;
    }

    protected override float GetDefaultValue()
    {
        return Settings.Alpha;
    }
}
