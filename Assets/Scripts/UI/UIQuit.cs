using UnityEngine;
using UnityEngine.UI;

public class UIQuit : MonoBehaviour
{
    private void Awake()
    {
        var button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        Application.Quit();
    }
}
