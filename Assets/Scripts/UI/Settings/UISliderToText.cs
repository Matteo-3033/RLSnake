using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISliderToText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    public void Start()
    {
        var slider = GetComponent<Slider>();
        slider.onValueChanged.AddListener(OnValueChanged);
        OnValueChanged(slider.value);
    }

    private void OnValueChanged(float arg0)
    {
        arg0 = Mathf.Round(arg0 * 100) / 100;
        text.text = arg0.ToString(CultureInfo.InvariantCulture);
    }
}
