using UnityEngine;
using UnityEngine.UI;

public class UICloseSettings : MonoBehaviour
{
    [SerializeField] private UIMenuManager menuManager;

    private void Start()
    {
        var button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        menuManager.ShowMainMenu();
    }
}