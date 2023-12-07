using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class UILoadingButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI buttonText;
    
    private string _baseText;
    private Button _button;
    private bool _loading;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnClick);
    }

    private void Start()
    {
        _baseText = buttonText.text;
    }

    private void OnClick()
    {
        ShowLoading();
        GetTask().ContinueWith(_ => HideLoading(), TaskScheduler.Default);
    }

    protected abstract Task GetTask();

    private void ShowLoading()
    {
        StartCoroutine(nameof(AnimateText));
    }

    private void HideLoading()
    {
        _loading = false;
    }
    
    private IEnumerator AnimateText()
    {
        _button.interactable = false;
        _loading = true;
        var cnt = 0;
        
        while (_loading)
        {
            buttonText.text = "Loading..."[..(7 + cnt)];
            yield return new WaitForSeconds(0.5F);
            cnt = (cnt + 1) % 4;
        }
        
        buttonText.text = _baseText;
        _button.interactable = true;
    }
}
