using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISaveModels : MonoBehaviour
{
    [SerializeField] private RlAgent[] agents;
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
        if (agents.Length == 0) return;
        
        ShowLoading();
        var tasks = agents.Select(agent => agent.SaveModel()).ToArray();
        
        Task.WhenAll(tasks).ContinueWith(_ => HideLoading(), TaskScheduler.Default);
    }

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
