using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Environment : MonoBehaviour
{
    [SerializeField] private InstanceManager instanceManager;
    
    [SerializeField] private int correctDirectionReward = 1;
    [SerializeField] private int wrongDirectionReward = -1;
    [SerializeField] private int gameOverReward = -10;
    
    private int _nextReward;
    public readonly List<State> States = new();
    
    private SnakeHead Snake => instanceManager.SnakeHead;
    private SnakeGrid Grid => instanceManager.Grid;
    private AppleGenerator AppleGenerator => instanceManager.AppleGenerator;
    
    private readonly SnakeHead.Direction[] _directions = { SnakeHead.Direction.Up, SnakeHead.Direction.Right, SnakeHead.Direction.Down, SnakeHead.Direction.Left };

    private readonly Dictionary<SnakeHead.Direction, int> _directionToIndex = new()
    {
        { SnakeHead.Direction.Up, 0 },
        { SnakeHead.Direction.Right, 1 },
        { SnakeHead.Direction.Down, 2 },
        { SnakeHead.Direction.Left, 3 }
    };

    private void Awake()
    {
        InitStates();
    }

    private void Start()
    {
        instanceManager.OnGameOver += OnGameOver;
    }

    public float TimeBetweenActions => instanceManager.SnakeTimeBetweenMoves;
    
    private void InitStates()
    {
        var appleDirectionsList = Enum.GetValues(typeof(AppleDirection)).Cast<AppleDirection>().ToList();
        var snakeDirectionsList = Enum.GetValues(typeof(SnakeHead.Direction)).Cast<SnakeHead.Direction>().ToList();
        var freeCellsList = Enum.GetValues(typeof(FreeCells)).Cast<FreeCells>().ToList();

        foreach (var appleDir in appleDirectionsList)
            foreach (var snakeDir in snakeDirectionsList)
                foreach (var front in freeCellsList)
                    foreach (var left in freeCellsList)
                        foreach (var right in freeCellsList)
                            States.Add(new State {
                                appleDirection = appleDir,
                                snakeDirection = snakeDir,
                                front = front,
                                left = left,
                                right = right
                            });
    }

    public void MakeAction(Action action)
    {
        if (IsEpisodeFinished())
            throw new Exception("Episode already finished; invoke Reset to start a new one");
        var direction = ActionToDirection(action);
        _nextReward = GetDirectionReward(direction);
        instanceManager.ChangeDirection(direction);
    }
    private SnakeHead.Direction ActionToDirection(Action action)
    {
        var index = instanceManager.SnakeHead.CurrentDirection switch
        {
            SnakeHead.Direction.Up => 0,
            SnakeHead.Direction.Right => 1,
            SnakeHead.Direction.Down => 2,
            SnakeHead.Direction.Left => 3,
            _ => throw new ArgumentOutOfRangeException()
        };
        index += (int) action;
        return _directions[(index + 4) % 4];
    }

    private int GetDirectionReward(SnakeHead.Direction action)
    {
        var appleDirection = GetAppleDirection();
        
        switch (action)
        {
            case SnakeHead.Direction.Up when appleDirection is AppleDirection.Top or AppleDirection.TopLeft or AppleDirection.TopRight:
            case SnakeHead.Direction.Down when appleDirection is AppleDirection.Bottom or AppleDirection.BottomLeft or AppleDirection.BottomRight:
            case SnakeHead.Direction.Left when appleDirection is AppleDirection.Left or AppleDirection.TopLeft or AppleDirection.BottomLeft:
            case SnakeHead.Direction.Right when appleDirection is AppleDirection.Right or AppleDirection.TopRight or AppleDirection.BottomRight:
                return correctDirectionReward;
            default:
                return wrongDirectionReward;
        }
    }

    public int GetReward()
    {
        return _nextReward;
    }
    
    private void OnGameOver(object sender, EventArgs e)
    {
        _nextReward = gameOverReward;
    }

    public bool IsEpisodeFinished()
    {
        return !instanceManager.Running;
    }

    public State ResetEnvironment()
    {
        Debug.Log("Environment reset");
        _nextReward = 0;
        instanceManager.StartGame();
        return GetState();
    }
    
    public State GetState()
    {
        var index = _directionToIndex[Snake.CurrentDirection];
        var leftDirection = _directions[((index - 1) + 4) % 4];
        var rightDirection = _directions[((index + 1) + 4) % 4];

        var front = Grid.BreathFirstSearch(Snake.GridPosition + SnakeHead.DirectionToVector[Snake.CurrentDirection]);
        var left = Grid.BreathFirstSearch(Snake.GridPosition + SnakeHead.DirectionToVector[leftDirection]);
        var right = Grid.BreathFirstSearch(Snake.GridPosition + SnakeHead.DirectionToVector[rightDirection]);

        Dictionary<int, FreeCells> dirEnums = new();
        
        var l = new List<Tuple<int, int>>
        {
            Tuple.Create(0, front),
            Tuple.Create(1, left),
            Tuple.Create(2, right)
        };
        l.Sort((a, b) => b.Item2.CompareTo(a.Item2));

        if (l[0].Item2 == 0)
            dirEnums[l[0].Item1] = FreeCells.None;
        else dirEnums[l[0].Item1] = FreeCells.More;
        
        if (l[1].Item2 == 0)
            dirEnums[l[1].Item1] = FreeCells.None;
        else if (l[1].Item2 == l[0].Item2)
            dirEnums[l[1].Item1] = dirEnums[l[0].Item1];
        else dirEnums[l[1].Item1] = dirEnums[l[0].Item1] - 1;
        
        if (l[2].Item2 == 0)
            dirEnums[l[2].Item1] = FreeCells.None;
        else if (l[2].Item2 == l[1].Item2)
            dirEnums[l[2].Item1] = dirEnums[l[1].Item1];
        else dirEnums[l[2].Item1] = dirEnums[l[1].Item1] - 1;
        
        var state = new State
        {
            appleDirection = GetAppleDirection(),
            snakeDirection = Snake.CurrentDirection,
            front = dirEnums[0],
            left = dirEnums[1],
            right = dirEnums[2]
        };

        return state;
    }

    private AppleDirection GetAppleDirection()
    {
        var applePos = AppleGenerator.GridPosition;
        var snakePos = Snake.GridPosition;
        
        if (applePos.x < snakePos.x)
        {
            if (applePos.y < snakePos.y)
                return AppleDirection.BottomLeft;
            if (applePos.y > snakePos.y)
                return AppleDirection.TopLeft;
            return AppleDirection.Left;
        }
        
        if (applePos.x > snakePos.x)
        {
            if (applePos.y < snakePos.y)
                return AppleDirection.BottomRight;
            if (applePos.y > snakePos.y)
                return AppleDirection.TopRight;
            return AppleDirection.Right;
        }
        
        if (applePos.y < snakePos.y)
            return AppleDirection.Bottom;
        if (applePos.y > snakePos.y)
            return AppleDirection.Top;
        
        return AppleDirection.Left;
    }
    
    [Serializable]
    public enum Action
    {
        Forward = 0,
        TurnLeft = -1,
        TurnRight = 1
    }
    
    [Serializable]
    public record State
    {
        public AppleDirection appleDirection;
        public SnakeHead.Direction snakeDirection;
        public FreeCells front;
        public FreeCells left;
        public FreeCells right;
    }

    [Serializable]
    public enum AppleDirection
    {
        Left, Right, Top, Bottom,
        TopLeft, TopRight, BottomLeft, BottomRight
    }

    [Serializable]
    public enum FreeCells
    {
        None, Less, More
    }
}
