using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class RlAgent : MonoBehaviour
{
    [SerializeField] private Environment environment;
    
    [SerializeField] private float alpha = 0.9F;
    protected float Alpha;
    [SerializeField] private float alphaReductionFactor = 0.9999F;
    
    [SerializeField] private float epsilon = 0.2F;
    private float _epsilon;
    [SerializeField] private float epsilonDecay = 0.99F;
    [SerializeField] private float minEpsilon = 0.05F;
    
    [SerializeField] protected float gamma = 0.9F;
    
    protected abstract string ModelFileName { get; }
    
    public event EventHandler<OnEpochFinishedArgs> OnEpochFinished;
    public class OnEpochFinishedArgs: EventArgs
    {
        public readonly int EpochsCnt;
        public OnEpochFinishedArgs(int epochsCnt) => EpochsCnt = epochsCnt;
    }
    
    private readonly Dictionary<StateAction, float> _qFunction = new (new StateActionComparer());
    private readonly Dictionary<InstanceManager.State, SnakeHead.Direction> _policy = new(new StateComparer());

    private readonly List<SnakeHead.Direction> _actions =
        Enum.GetValues(typeof(SnakeHead.Direction)).Cast<SnakeHead.Direction>().ToList();

    private void Start()
    {
        if (Settings.UseModels)
            LoadModel();
        else Init();
            
        StartCoroutine(nameof(MainLoop));
    }

    private void Init()
    {
        InitPI();
        InitQ();
        InitParameters();
    }

    protected virtual void InitParameters()
    {
        alpha = Settings.Alpha;
        gamma = Settings.Gamma;
        epsilon = Settings.Epsilon;
        
        Alpha = alpha;
        _epsilon = epsilon;
    }

    private void InitPI()
    {
        foreach (var state in environment.States)
            UpdatePolicy(state, GetRandomAction());
    }
    
    private void InitQ()
    {
        foreach (var state in environment.States)
            foreach (var action in _actions)
                UpdateQ(state, action, 0F);
    }

    private IEnumerator MainLoop()
    {
        Debug.Log("Starting training");
        
        var currentEpoch = 0;
        while (true)
        {
            var state = environment.ResetEnvironment();
            while (!environment.IsEpisodeFinished())
            {
                var action = PI(state);
                environment.MakeAction(action);

                yield return new WaitForSeconds(environment.TimeBetweenActions);

                Debug.Log("State: " + state);
                Debug.Log("Action done: " + action);
                
                var nextState = environment.GetState();
                var reward = environment.GetReward();
                Debug.Log("Reward: " + reward);
                
                RlAlgorithm(state, action, reward, nextState);

                state = nextState;
            }
            
            _epsilon = Math.Clamp(_epsilon * epsilonDecay, minEpsilon, 1F);
            Alpha *= alphaReductionFactor;
            currentEpoch++;
            OnEpochFinished?.Invoke(this, new OnEpochFinishedArgs(currentEpoch));
            if (currentEpoch % 250 == 0)
                SaveModel();
        }
        // ReSharper disable once IteratorNeverReturns
    }
    
    protected abstract void RlAlgorithm(InstanceManager.State state, SnakeHead.Direction action, int reward, InstanceManager.State nextState);

    protected SnakeHead.Direction PI(InstanceManager.State s)
    {
        var action = _policy[s];
        
        if (Random.Range(0F, 1F) < _epsilon)
            return GetRandomAction();
        
        return action;
    }
    
    protected float Q(InstanceManager.State s, SnakeHead.Direction a)
    {
        return _qFunction[new StateAction { state = s, action = a }];
    }

    protected void UpdatePolicy(InstanceManager.State state, SnakeHead.Direction action)
    {
        _policy[state] = action;
    }

    protected void UpdateQ(InstanceManager.State state, SnakeHead.Direction action, float expectedReward)
    {
        _qFunction[new StateAction { state = state, action = action }] = expectedReward;
    }

    private SnakeHead.Direction GetRandomAction()
    {
        var actionIndex = Random.Range(0, _actions.Count);
        return _actions[actionIndex];
    }
    
    protected SnakeHead.Direction GetMaxForState(InstanceManager.State state)
    {
        return _actions
            .OrderByDescending(
                action => Q(state, action)
            )
            .FirstOrDefault();
    }

    public Task SaveModel()
    {
        //var filename = Application.persistentDataPath + $"/{Name}_{timestamp}.json";
        
        return Task.Run(() =>
        {
            try
            {
                Debug.Log($"Saving model to {ModelFileName}");

                File.WriteAllText(ModelFileName, GetJson());
                Debug.Log("Saved");
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        });
    }
    
    private void LoadModel()
    {
        var json = File.ReadAllText(ModelFileName);
        var jsonData = ParseJson(json);
        
        alpha = jsonData.alpha;
        Alpha = jsonData.currentAlpha;
        alphaReductionFactor = jsonData.alphaReductionFactor;
        epsilon = jsonData.epsilon;
        _epsilon = jsonData.currentEpsilon;
        epsilonDecay = jsonData.epsilonDecay;
        minEpsilon = jsonData.minEpsilon;
        gamma = jsonData.gamma;

        var cnt = 0;
        foreach (var state in environment.States)
        {
            foreach (var action in _actions)
                _qFunction[new StateAction { state = state, action = action }] = jsonData.q[cnt++];
            _policy[state] = GetMaxForState(state);
        }
    }

    protected virtual JsonModel ParseJson(string json)
    {
        return JsonUtility.FromJson<JsonModel>(json);
    }

    protected virtual string GetJson()
    {
        return JsonUtility.ToJson(new JsonModel(this), true);
    }

    [Serializable]
    public class JsonModel
    {
        public float alpha;
        public float currentAlpha;
        public float alphaReductionFactor;

        public float epsilon;
        public float currentEpsilon;
        public float epsilonDecay;
        public float minEpsilon;

        public float gamma;

        public int epochs;

        [Serialize] public List<float> q;
        
        public JsonModel(RlAgent agent)
        {
            alpha = agent.alpha;
            currentAlpha = agent.Alpha;
            alphaReductionFactor = agent.alphaReductionFactor;
            
            epsilon = agent.epsilon;
            currentEpsilon = agent._epsilon;
            epsilonDecay = agent.epsilonDecay;
            minEpsilon = agent.minEpsilon;
            
            gamma = agent.gamma;

            q = new List<float>();
            
            foreach (var state in agent.environment.States)
                foreach (var action in agent._actions)
                    q.Add(agent.Q(state, action));
        }
    }
}

[Serializable]
internal record StateAction
{
    public InstanceManager.State state;
    public SnakeHead.Direction action;
}

internal class StateActionComparer : IEqualityComparer<StateAction>
{
    private readonly StateComparer _stateComparer = new StateComparer();
    
    public bool Equals(StateAction x, StateAction y)
    {
        if (y == null && x == null)
            return true;
        
        if (x == null || y == null)
            return false;
        
        if (x.action != y.action)
            return false;
     
        return _stateComparer.Equals(x.state, y.state);
    }

    public int GetHashCode(StateAction obj)
    {
        return _stateComparer.GetHashCode(obj.state) * 23 + (int)obj.action;
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
        
        return x.appleDirection == y.appleDirection && x.top == y.top && x.bottom == y.bottom && x.left == y.left && x.right == y.right;
    }

    public int GetHashCode(InstanceManager.State obj)
    {
        return (int)obj.appleDirection * 23 + (int)obj.top * 17 + (int)obj.bottom * 13 + (int)obj.left * 11 + (int)obj.right * 7;
    }
}