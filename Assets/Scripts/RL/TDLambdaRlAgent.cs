using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class TdLambdaRlAgent : RlAgent
{
    [SerializeField, Range(0F, 1F)] private float lambda = 0.5F;
    
    private readonly List<StateAction> _trace = new();
    
    protected override string Name => "tdLambda";

    protected override void RlAlgorithm(InstanceManager.State state, SnakeHead.Direction action, int reward, InstanceManager.State nextState)
    {
        var policyAction = PI(nextState);
        var nextAction = GetMaxForState(nextState);
        
        UpdatePolicy(nextState, nextAction);
        
        var delta = reward + gamma * Q(nextState, nextAction) - Q(state, action);
        
        _trace.Add(new StateAction { state = state, action = action });
        
        var zeroing = policyAction != nextAction;
        var tmpLambda = 1F;
        Debug.Log("Trace length: " + _trace.Count);
                
        for (var i = _trace.Count - 1; i >= 0; i--)
        {
            var stateAction = _trace[i];
            UpdateQ(
                stateAction.state,
                stateAction.action,
                Q(stateAction.state, stateAction.action) + Alpha * delta * tmpLambda
            );
            tmpLambda *= lambda;
        }

        if (zeroing) _trace.Clear();
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
