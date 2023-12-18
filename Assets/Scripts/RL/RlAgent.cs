using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class RlAgent : EpochsPlayer
{
    [SerializeField] private Environment environment;
    
    [SerializeField] private float alpha = 0.9F;
    protected float Alpha;
    [SerializeField] private float alphaReductionFactor = 0.9999F;
    
    [SerializeField] private float epsilon = 0.2F;
    private float _epsilon;
    [SerializeField] private float epsilonDecay = 0.99F;
    [SerializeField] private float minEpsilon = 0.01F;
    
    [SerializeField] protected float gamma = 0.9F;

    protected Environment Environment => environment;
    protected abstract string ModelFileName { get; }
    
    private readonly Dictionary<StateAction, float> _qFunction = new();
    private readonly Dictionary<Environment.State, Environment.Action> _policy = new();

    private readonly List<Environment.Action> _actions =
        Enum.GetValues(typeof(Environment.Action)).Cast<Environment.Action>().ToList();

    private void Start()
    {
        Init();
        StartCoroutine(nameof(MainLoop));
    }

    protected virtual void Init()
    {
        if (Settings.UseModels)
            LoadModel();
        else
        {
            InitPI();
            InitQ();
            InitParameters();
        }
    }

    protected virtual void InitParameters()
    {
        alpha = Settings.Alpha;
        gamma = Settings.Gamma;
        epsilon = Settings.Epsilon;
        minEpsilon = Settings.MinEpsilon;
        
        Alpha = alpha;
        _epsilon = epsilon;
    }

    protected virtual void InitPI()
    {
        foreach (var state in environment.States)
            UpdatePolicy(state, GetRandomAction());
    }
    
    protected virtual void InitQ()
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
                Debug.Log("State: " + state);
                Debug.Log("Selected action: " + action);
                environment.MakeAction(action);

                yield return new WaitForSeconds(environment.TimeBetweenActions);
                
                var nextState = environment.GetState();
                var reward = environment.GetReward();
                Debug.Log("New state: " + nextState);
                Debug.Log("Reward: " + reward);
                
                RlAlgorithm(state, action, reward, nextState);

                state = nextState;
            }
            
            _epsilon = Math.Clamp(_epsilon * epsilonDecay, minEpsilon, 1F);
            Alpha *= alphaReductionFactor;
            currentEpoch++;
            InvokeOnEpochFinished(new OnEpochFinishedArgs(currentEpoch));
            if (currentEpoch % 250 == 0)
                SaveModel();
        }
        // ReSharper disable once IteratorNeverReturns
    }
    
    protected abstract void RlAlgorithm(Environment.State state, Environment.Action action, int reward, Environment.State nextState);

    protected Environment.Action PI(Environment.State s)
    {
        var action = _policy[s];
        
        if (Random.Range(0F, 1F) < _epsilon)
            return GetRandomAction();
        
        return action;
    }
    
    protected float Q(Environment.State s, Environment.Action a)
    {
        return _qFunction[new StateAction(s, a)];
    }

    protected void UpdatePolicy(Environment.State state, Environment.Action action)
    {
        _policy[state] = action;
    }

    protected void UpdateQ(Environment.State state, Environment.Action action, float expectedReward)
    {
        _qFunction[new StateAction(state, action)] = expectedReward;
    }

    private Environment.Action GetRandomAction()
    {
        var actionIndex = Random.Range(0, _actions.Count);
        return _actions[actionIndex];
    }
    
    protected Environment.Action GetMaxForState(Environment.State state)
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
                _qFunction[new StateAction(state, action)] = jsonData.q[cnt++];
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
    public readonly Environment.State State;
    public readonly Environment.Action Action;

    public StateAction(Environment.State state, Environment.Action action)
    {
        State = state;
        Action = action;
    }

    public virtual bool Equals(StateAction other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return State.Equals(other.State) && Action == other.Action;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(State, (int)Action);
    }
}
