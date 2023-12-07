public class Td0RlAgent : RlAgent
{
    protected override string Name => "td0";

    protected override void RlAlgorithm(InstanceManager.State state, SnakeHead.Direction action, int reward, InstanceManager.State nextState)
    {
        var nextAction = GetMaxForState(nextState);
        UpdatePolicy(nextState, nextAction);
                
        UpdateQ(
            state,
            action,
            Q(state, action) * (1 - Alpha) + Alpha * (reward + gamma * Q(nextState, nextAction))
        );
    }
}