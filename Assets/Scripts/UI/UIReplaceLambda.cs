using System.Globalization;
using TMPro;
using UnityEngine;

public class UIReplaceLambda : MonoBehaviour
{
    private void Awake()
    {
        var text = GetComponent<TextMeshProUGUI>();
        var lambda = Mathf.Round(Settings.Lambda * 100) / 100;
        text.text = text.text.Replace("$lambda", lambda.ToString(CultureInfo.CurrentCulture));
    }
}
