using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class PlayingAgent : EpochsPlayer
{
    [SerializeField] private string modelFileName;
    [SerializeField] private Environment environment;

    private int _matchesCnt;
    private Environment.State _state;
    private readonly Dictionary<Environment.State, Environment.Action> _policy = new();

    private readonly List<Environment.Action> _actions =
        Enum.GetValues(typeof(Environment.Action)).Cast<Environment.Action>().ToList();

    private void Start()
    {
        LoadModel();
        StartCoroutine(nameof(MainLoop));
    }

    private IEnumerator MainLoop()
    {
        Debug.Log("Starting playing");

        while (true)
        {
            _state = environment.ResetEnvironment();
            while (!environment.IsEpisodeFinished())
            {
                _state = environment.GetState();
                var action = _policy[_state];
                Debug.Log("State: " + _state);
                Debug.Log("Selected action: " + action);
                environment.MakeAction(action);
                yield return new WaitForSeconds(environment.TimeBetweenActions);
            }
            InvokeOnEpochFinished(new OnEpochFinishedArgs(_matchesCnt++));
        }
        // ReSharper disable once IteratorNeverReturns
    }
    
    private Environment.Action GetMaxForState(IReadOnlyDictionary<StateAction, float> q, Environment.State state)
    {
        return _actions
            .OrderByDescending(
                action => q[new StateAction(state, action)]
            )
            .FirstOrDefault();
    }

    private void LoadModel()
    {
        var json = File.ReadAllText(modelFileName);
        var jsonData = JsonUtility.FromJson<RlAgent.JsonModel>(json);

        var cnt = 0; 
        var qFunction = new Dictionary<StateAction, float>();

        foreach (var state in environment.States)
        {
            foreach (var action in _actions)
                qFunction[new StateAction(state, action)] = jsonData.q[cnt++];
            _policy[state] = GetMaxForState(qFunction, state);
        }
    }
}
