using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class UISnakeAverageLength : MonoBehaviour
{
    [SerializeField] private InstanceManager instanceManager;
    [SerializeField] private EpochsPlayer agent;
    [SerializeField] private int cnt = 10;
    
    private TextMeshProUGUI _text;
    private string _baseText;
    
    private readonly Queue<int> _queue = new();

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
        _baseText = _text.text;
        SetText();
    }

    private void Start()
    {
        agent.OnEpochFinished += OnEpochFinished;
    }

    private void OnEpochFinished(object sender, EpochsPlayer.OnEpochFinishedArgs args)
    {
        _queue.Enqueue(instanceManager.Score);
        if (_queue.Count > cnt)
            _queue.Dequeue();
        SetText();
    }

    private void SetText()
    {
        _text.text = $"{_baseText}{Average()} \u00b1 {Std()}";
    }
    
    private int Average()
    {
        if (!_queue.Any()) return 0;
        return _queue.Sum() / _queue.Count;
    }

    private float Std()
    {
        float std;
        
        if (_queue.Any())
        {
            var avg = Average();
            var sum = _queue.Sum(x => Mathf.Pow(x - avg, 2));
            std = Mathf.Sqrt(sum / _queue.Count);{}
        } else std = 0;
        
        return Mathf.Round(std * 100) / 100;
    }
}

