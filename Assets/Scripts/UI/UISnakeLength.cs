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
    }

    private void Start()
    {
        instanceManager.OnSnakeGrowth += OnSnakeGrowth;
    }

    private void OnSnakeGrowth(object sender, InstanceManager.OnSnakeGrowthArgs args)
    {
        _text.text = $"{_baseText}{args.Length}";
    }
}
