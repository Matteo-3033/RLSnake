using System.Collections.Generic;
using UnityEngine;

public class TdLambdaRlAgent : RlAgent
{
    [SerializeField, Range(0F, 1F)] private float lambda = 0.5F;
    
    private readonly List<StateAction> _trace = new();

    protected override void RlAlgorithm(InstanceManager.State state, SnakeHead.Direction action, int reward, InstanceManager.State nextState)
    {
        var policyAction = PI(nextState);
        var nextAction = GetMaxForState(nextState);
        
        UpdatePolicy(nextState, nextAction);
        
        var delta = reward + gamma * Q(nextState, nextAction) - Q(state, action);
        
        _trace.Add(new StateAction { State = state, Action = action });
        
        var zeroing = policyAction != nextAction;
        var tmpLambda = 1F;
        Debug.Log("Trace length: " + _trace.Count);
                
        for (var i = _trace.Count - 1; i >= 0; i--)
        {
            var stateAction = _trace[i];
            UpdateQ(
                stateAction.State,
                stateAction.Action,
                Q(stateAction.State, stateAction.Action) + alpha * delta * tmpLambda
            );
            tmpLambda *= lambda;
        }

        if (zeroing) _trace.Clear();
    }
}
