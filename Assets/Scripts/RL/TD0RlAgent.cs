public class Td0RlAgent : RlAgent
{
    protected override string ModelFileName => "td0.json";

    protected override void RlAlgorithm(Environment.State state, Environment.Action action, int reward, Environment.State nextState)
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
