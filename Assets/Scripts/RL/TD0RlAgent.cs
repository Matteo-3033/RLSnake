public class Td0RlAgent : RlAgent
{ 
    protected override void RlAlgorithm(InstanceManager.State state, SnakeHead.Direction action, int reward, InstanceManager.State nextState)
    {
        var nextAction = GetMaxForState(nextState);
        UpdatePolicy(nextState, nextAction);
                
        UpdateQ(
            state,
            action,
            Q(state, action) * (1 - alpha) + alpha * (reward + gamma * Q(nextState, nextAction))
        );
    }
}
