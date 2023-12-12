using UnityEngine;
using UnityEngine.UI;

public abstract class UISlider : MonoBehaviour
{
    private void Start()
    {
        var slider = GetComponent<Slider>();
        slider.onValueChanged.AddListener(OnValueChanged);
        slider.value = GetDefaultValue();
    }

    protected abstract void OnValueChanged(float arg0);
    
    protected abstract float GetDefaultValue();
}