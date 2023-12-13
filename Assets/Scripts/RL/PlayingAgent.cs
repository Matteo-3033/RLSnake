using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class PlayingAgent : MonoBehaviour
{
    [SerializeField] private string modelFileName;
    [SerializeField] private Environment environment;

    private InstanceManager.State _state;
    private readonly Dictionary<InstanceManager.State, SnakeHead.Direction> _policy = new(new StateComparer());

    private readonly List<SnakeHead.Direction> _actions =
        Enum.GetValues(typeof(SnakeHead.Direction)).Cast<SnakeHead.Direction>().ToList();

    private void Start()
    {
        LoadModel();
        _state = environment.ResetEnvironment();
    }

    private void Update()
    {
        _state = environment.GetState();
        if (environment.IsEpisodeFinished())
            _state = environment.ResetEnvironment();
        
        var action = _policy[_state];
        environment.MakeAction(action);
    }
    
    private SnakeHead.Direction GetMaxForState(Dictionary<StateAction, float> q, InstanceManager.State state)
    {
        return _actions
            .OrderByDescending(
                action => q[new StateAction { state = state, action = action }]
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
                qFunction[new StateAction { state = state, action = action }] = jsonData.q[cnt++];
            _policy[state] = GetMaxForState(qFunction, state);
        }
    }
}
