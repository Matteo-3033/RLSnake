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
    public readonly List<InstanceManager.State> States = new();

    public enum Action
    {
        Forward = 0,
        TurnLeft = -1,
        TurnRight = 1
    }

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
        
        for (var d = InstanceManager.AppleDirection.Left; d <= InstanceManager.AppleDirection.BottomRight; d++)
            foreach (var top in elements)
                foreach (var bottom in elements)
                    foreach (var left in elements)
                        foreach (var right in elements)
                            States.Add(new InstanceManager.State {
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
        var directions = new[]{ SnakeHead.Direction.Up, SnakeHead.Direction.Right, SnakeHead.Direction.Down, SnakeHead.Direction.Left }; 
        var index = instanceManager.SnakeHead.CurrentDirection switch
        {
            SnakeHead.Direction.Up => 0,
            SnakeHead.Direction.Right => 1,
            SnakeHead.Direction.Down => 2,
            SnakeHead.Direction.Left => 3
        };
        index += (int) action;
        return directions[(index + 4) % 4];
    }

    private int GetDirectionReward(SnakeHead.Direction action)
    {
        var appleDirection = instanceManager.GetAppleDirection();
        
        switch (action)
        {
            case SnakeHead.Direction.Up when appleDirection is InstanceManager.AppleDirection.Top or InstanceManager.AppleDirection.TopLeft or InstanceManager.AppleDirection.TopRight:
            case SnakeHead.Direction.Down when appleDirection is InstanceManager.AppleDirection.Bottom or InstanceManager.AppleDirection.BottomLeft or InstanceManager.AppleDirection.BottomRight:
            case SnakeHead.Direction.Left when appleDirection is InstanceManager.AppleDirection.Left or InstanceManager.AppleDirection.TopLeft or InstanceManager.AppleDirection.BottomLeft:
            case SnakeHead.Direction.Right when appleDirection is InstanceManager.AppleDirection.Right or InstanceManager.AppleDirection.TopRight or InstanceManager.AppleDirection.BottomRight:
                return correctDirectionReward;
            default:
                return wrongDirectionReward;
        }
    }

    public InstanceManager.State GetState()
    {
        return instanceManager.GetGameState();
    }

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

    public InstanceManager.State ResetEnvironment()
    {
        Debug.Log("Environment reset");
        _nextReward = 0;
        return instanceManager.StartGame();
    }
}
