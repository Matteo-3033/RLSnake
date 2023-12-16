using System;
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
    private readonly Dictionary<Environment.State, Environment.Action> _policy = new(new StateComparer());

    private readonly List<Environment.Action> _actions =
        Enum.GetValues(typeof(Environment.Action)).Cast<Environment.Action>().ToList();

    private void Start()
    {
        LoadModel();
        _state = environment.ResetEnvironment();
    }

    private void Update()
    {
        _state = environment.GetState();
        if (environment.IsEpisodeFinished())
        {
            InvokeOnEpochFinished(new OnEpochFinishedArgs(_matchesCnt++));
            _state = environment.ResetEnvironment();
        }
        
        var action = _policy[_state];
        environment.MakeAction(action);
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
        var qFunction = new Dictionary<StateAction, float>(new StateActionComparer());

        foreach (var state in environment.States)
        {
            foreach (var action in _actions)
                qFunction[new StateAction(state, action)] = jsonData.q[cnt++];
            _policy[state] = GetMaxForState(qFunction, state);
        }
    }
}
