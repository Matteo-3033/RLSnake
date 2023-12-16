using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Environment : MonoBehaviour
{
    [SerializeField] private InstanceManager instanceManager;
    
    [SerializeField] private int correctDirectionReward = 1;
    [SerializeField] private int wrongDirectionReward = -1;
    [SerializeField] private int appleEatenReward = 2;
    [SerializeField] private int gameOverReward = -10;
    
    private int _nextReward;
    public readonly List<State> States = new();
    
    private SnakeHead Snake => instanceManager.SnakeHead;
    private SnakeGrid Grid => instanceManager.Grid;
    private AppleGenerator AppleGenerator => instanceManager.AppleGenerator;

    private void Awake()
    {
        InitStates();
    }

    private void Start()
    {
        instanceManager.OnGameOver += OnGameOver;
        instanceManager.OnSnakeGrowth += OnAppleEaten;
    }

    public float TimeBetweenActions => instanceManager.SnakeTimeBetweenMoves;
    
    private void InitStates()
    {
        var elements = Enum.GetValues(typeof(SnakeGrid.Element)).Cast<SnakeGrid.Element>().ToList();
        
        for (var d = AppleDirection.Left; d <= AppleDirection.BottomRight; d++)
            foreach (var top in elements)
                foreach (var bottom in elements)
                    foreach (var left in elements)
                        foreach (var right in elements)
                            States.Add(new State {
                                appleDirection = d,
                                top = top,
                                bottom = bottom,
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

    private readonly SnakeHead.Direction[] _directions = new[]{ SnakeHead.Direction.Up, SnakeHead.Direction.Right, SnakeHead.Direction.Down, SnakeHead.Direction.Left };

    private readonly Dictionary<SnakeHead.Direction, int> _directionToIndex = new()
    {
        { SnakeHead.Direction.Up, 0 },
        { SnakeHead.Direction.Right, 1 },
        { SnakeHead.Direction.Down, 2 },
        { SnakeHead.Direction.Left, 3 }
    };

    public int GetReward()
    {
        return _nextReward;
    }
    
    private void OnAppleEaten(object sender, InstanceManager.OnSnakeGrowthArgs args)
    {
        if (args.Length > 0)
            _nextReward = appleEatenReward;
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
        
        var state = new State
        {
            appleDirection = GetAppleDirection(),
            front = Grid.BreathFirstSearch(Snake.GridPosition + SnakeHead.DirectionToVector[Snake.CurrentDirection]),
            left = Grid.BreathFirstSearch(Snake.GridPosition + SnakeHead.DirectionToVector[leftDirection]),
            right = Grid.BreathFirstSearch(Snake.GridPosition + SnakeHead.DirectionToVector[rightDirection]),
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
        public int front;
        public int left;
        public int right;
    }

    [Serializable]
    public enum AppleDirection
    {
        Left, Right, Top, Bottom,
        TopLeft, TopRight, BottomLeft, BottomRight
    }
}
