using TMPro;
using UnityEngine;

public class UIEpochCnt : MonoBehaviour
{
    [SerializeField] private RlAgent agent;
    
    private TextMeshProUGUI _text;
    private string _baseText;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
        _baseText = _text.text; 
        _text.text = $"{_baseText}0";
    }

    private void Start()
    {
        agent.OnEpochFinished += OnEpochFinished;
    }

    private void OnEpochFinished(object sender, RlAgent.OnEpochFinishedArgs args)
    {
        _text.text = $"{_baseText}{args.EpochsCnt}";
    }
}
