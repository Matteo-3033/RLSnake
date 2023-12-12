public class UISliderRows : UISlider
{
    protected override void OnValueChanged(float arg0)
    {
        Settings.GridRows = (int)arg0;
    }

    protected override float GetDefaultValue()
    {
        return Settings.GridRows;
    }
}
