using TMPro;
using UnityEngine;

public class UISnakeLength : MonoBehaviour
{
    [SerializeField] private InstanceManager instanceManager;
    
    private TextMeshProUGUI _text;
    private string _baseText;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
        _baseText = _text.text;
        SetText(GetText(0));
    }

    private void Start()
    {
        instanceManager.OnSnakeGrowth += OnSnakeGrowth;
    }

    private void OnSnakeGrowth(object sender, InstanceManager.OnSnakeGrowthArgs args)
    {
        SetText(GetText(args.Length));
    }

    private void SetText(string end)
    {
        _text.text = $"{_baseText}{end}";
    }

    protected virtual string GetText(int length)
    {
        return length.ToString();
    }
}
