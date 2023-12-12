using UnityEngine;

public class UIMenuManager : MonoBehaviour
{
     [SerializeField] private GameObject mainMenu;
     [SerializeField] private GameObject settingsMenu;

     private void Start()
     {
          mainMenu.SetActive(true);
          settingsMenu.SetActive(false);
     }
     
     public void ShowSettingsMenu()
     {
          mainMenu.SetActive(false);
          settingsMenu.SetActive(true);
     }
     
     public void ShowMainMenu()
     {
          mainMenu.SetActive(true);
          settingsMenu.SetActive(false);
     }
}