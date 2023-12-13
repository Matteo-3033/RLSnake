using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TdLambdaRlAgent : RlAgent
{
    [SerializeField, Range(0F, 1F)] private float lambda = 0.5F;
    
    protected override string ModelFileName => "tdLambda.json";
    
    private readonly Dictionary<StateAction, float> _e = new (new StateActionComparer());
    
    private readonly List<SnakeHead.Direction> _actions = Enum.GetValues(typeof(SnakeHead.Direction)).Cast<SnakeHead.Direction>().ToList();

    protected override void RlAlgorithm(InstanceManager.State state, SnakeHead.Direction action, int reward, InstanceManager.State nextState)
    {
        var policyAction = PI(nextState);
        var nextAction = GetMaxForState(nextState);
        
        UpdatePolicy(nextState, nextAction);
        
        var delta = reward + gamma * Q(nextState, nextAction) - Q(state, action);
        
        var zeroing = policyAction != nextAction;
        
        UpdateE(state, action, E(state, action) + 1);
        foreach (var a in _actions)
        {
            foreach (var s in environment.States)
            {
                UpdateQ(s, a, Q(s, a) + Alpha * delta * E(s, a));
                UpdateE(s, a, zeroing ? 0F : gamma * lambda * E(s, a));
            }
        }
    }

    protected override void Init()
    {
        base.Init();
        InitE();
    }
    
    protected override void InitParameters()
    {
        base.InitParameters();
        lambda = Settings.Lambda;
    }

    private void InitE()
    {
        foreach (var state in environment.States)
            foreach (var action in _actions)
                _e[new StateAction(state, action)] = 0F;
    }
    
    private void UpdateE(InstanceManager.State state, SnakeHead.Direction action, float value)
    {
        _e[new StateAction(state, action)] = value;
    }
    
    private float E(InstanceManager.State state, SnakeHead.Direction action)
    {
        return _e[new StateAction(state, action)];
    }

    protected override string GetJson()
    {
        return JsonUtility.ToJson(new TdLambdaJsonModel(this), true);
    }

    protected override JsonModel ParseJson(string json)
    {
        var jsonData = JsonUtility.FromJson<TdLambdaJsonModel>(json);
        lambda = jsonData.lambda;
        return jsonData;
    }

    [Serializable]
    private class TdLambdaJsonModel : JsonModel
    {
        public float lambda;
        
        public TdLambdaJsonModel(TdLambdaRlAgent agent) : base(agent)
        {
            lambda = agent.lambda;
        }
    } 
}
