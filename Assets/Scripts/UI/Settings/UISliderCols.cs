public class UISliderCols : UISlider
{
    protected override void OnValueChanged(float arg0)
    {
        Settings.GridColumns = (int)arg0;
    }

    protected override float GetDefaultValue()
    {
        return Settings.GridColumns;
    }
}
