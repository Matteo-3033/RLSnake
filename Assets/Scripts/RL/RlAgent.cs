using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class RlAgent : MonoBehaviour
{
    [SerializeField] private Environment environment;
    
    [SerializeField] private float alpha = 0.9F;
    [SerializeField] private float alphaReductionFactor = 0.9999F;
    
    [SerializeField] private float epsilon = 0.9F;
    [SerializeField] private float epsilonDecay = 0.001F;
    [SerializeField] private float minEpsilon = 0.05F;
    
    [SerializeField] private float gamma = 0.9F;
    
    [SerializeField] private int epochs = 1000;
    
    public event EventHandler<OnEpochFinishedArgs> OnEpochFinished;
    public class OnEpochFinishedArgs: EventArgs
    {
        public readonly int EpochsCnt;
        public OnEpochFinishedArgs(int epochsCnt) => EpochsCnt = epochsCnt;
    }
    
    private int _currentEpoch;
    
    private InstanceManager.State _state;
    private SnakeHead.Direction _action;
    
    private readonly Dictionary<QKey, float> _qFunction = new (new QComparer());
    private readonly Dictionary<InstanceManager.State, SnakeHead.Direction> _policy = new(new StateComparer());

    private readonly List<SnakeHead.Direction> _actions =
        Enum.GetValues(typeof(SnakeHead.Direction)).Cast<SnakeHead.Direction>().ToList();

    private void Start()
    {
        InitPI();
        InitQ();
        
        StartCoroutine(nameof(MainLoop));
    }

    private void InitPI()
    {
        foreach (var state in environment.States)
            _policy[state] = GetRandomAction();
    }
    
    private void InitQ()
    {
        foreach (var state in environment.States)
            foreach (var action in _actions)
                _qFunction[new QKey { State = state, Action = action }] = 0F;
    }

    private IEnumerator MainLoop()
    {
        Debug.Log("Starting training");
        while (true)
        {
            if (_currentEpoch >= epochs) continue;
            
            _state = environment.ResetEnvironment();
            while (!environment.IsEpisodeFinished())
            {
                _action = PI(_state);
                environment.MakeAction(_action);

                yield return new WaitForSeconds(environment.TimeBetweenActions);

                Debug.Log("State: " + _state);
                Debug.Log("Action done: " + _action);
                
                var nextState = environment.GetState();
                var reward = environment.GetReward();
                Debug.Log("Reward: " + reward);

                var nextAction = GetMaxForState(nextState);
                _policy[nextState] = nextAction;
                
                UpdateQ(nextState, nextAction, reward);

                _state = nextState;
            }
            
            epsilon = Math.Clamp(epsilon * epsilonDecay, minEpsilon, 1F);
            alpha *= alphaReductionFactor;
            _currentEpoch++;
            OnEpochFinished?.Invoke(this, new OnEpochFinishedArgs(_currentEpoch));
        }
        // ReSharper disable once IteratorNeverReturns
    }

    private void UpdateQ(InstanceManager.State nextState, SnakeHead.Direction nextAction, int reward)
    {
        _qFunction[new QKey { State = _state, Action = _action }] =
            Q(_state, _action) * (1 - alpha) +
            alpha * (reward + gamma * Q(nextState, nextAction));
        
        //Debug.Log("New Q(" + _state + ", " + _action + ") = " + Q(_state, _action));
    }

    private SnakeHead.Direction GetRandomAction()
    {
        var actionIndex = Random.Range(0, _actions.Count);
        return _actions[actionIndex];
    }
    
    private SnakeHead.Direction GetMaxForState(InstanceManager.State state)
    {
        /*
        foreach (var a in Enum.GetValues(typeof(SnakeHead.Direction)))
            Debug.Log("Q(" + state + ", " + (SnakeHead.Direction) a + ") = " +  Q(state, (SnakeHead.Direction) a));
        */
        
        return _actions
            .OrderByDescending(
                action => Q(state, action)
            )
            .FirstOrDefault();
    }

    private float Q(InstanceManager.State s, SnakeHead.Direction a)
    {
        // print state s.grid
        foreach (var row in s.Grid)
        {
            foreach (var e in row)
                Debug.Log(e);
            Debug.Log("");
        }
        return _qFunction[new QKey { State = s, Action = a }];
    }

    private SnakeHead.Direction PI(InstanceManager.State s)
    {
        var action = _policy[s];
        
        if (Random.Range(0F, 1F) < epsilon)
            action = GetRandomAction();
        
        return action;
    }
}

internal record QKey
{
    public InstanceManager.State State;
    public SnakeHead.Direction Action;
}

internal class QComparer : IEqualityComparer<QKey>
{
    private readonly StateComparer _stateComparer = new StateComparer();
    
    public bool Equals(QKey x, QKey y)
    {
        if (y == null && x == null)
            return true;
        
        if (x == null || y == null)
            return false;
        
        if (x.Action != y.Action)
            return false;
     
        return _stateComparer.Equals(x.State, y.State);
    }

    public int GetHashCode(QKey obj)
    {
        return _stateComparer.GetHashCode(obj.State) * 23 + (int)obj.Action;
    }
}

internal class StateComparer : IEqualityComparer<InstanceManager.State>
{
    public bool Equals(InstanceManager.State x, InstanceManager.State y)
    {
        if (y == null && x == null)
            return true;
        
        if (x == null || y == null)
            return false;
        
        if (x.Grid.Length != y.Grid.Length || x.Grid[0].Length != y.Grid[0].Length)
            return false;
        
        if (x.AppleDirection != y.AppleDirection)
            return false;
        
        var xGrid = x!.Grid;
        var yGrid = y!.Grid;
        
        for (var i = 0; i < xGrid.Length; i++)
        for (var j = 0; j < xGrid[i].Length; j++)
            if (xGrid[i][j] != yGrid[i][j])
                return false;
        
        return true;
    }

    public int GetHashCode(InstanceManager.State obj)
    {
        var grid = obj.Grid;
        return grid.Aggregate(17, (current1, t) => t.Aggregate(current1, (current, t1) => current * 23 + (int)t1)) * 23 + (int)obj.AppleDirection;
    }
}