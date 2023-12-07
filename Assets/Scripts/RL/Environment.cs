using System;
using System.Collections.Generic;
using UnityEngine;

public class Environment : MonoBehaviour
{
    [SerializeField] private InstanceManager instanceManager;
    [SerializeField, Range(3, int.MaxValue)] private int squareSize = 3;
    
    [SerializeField] private int correctDirectionReward = 1;
    [SerializeField] private int wrongDirectionReward = -1;
    [SerializeField] private int appleEatenReward = 2;
    [SerializeField] private int gameOverReward = -10;
    
    private int _nextReward;
    public readonly List<InstanceManager.State> States = new();

    private void Awake()
    {
        if (squareSize % 2 == 0)
            throw new Exception("squareSize must be odd");
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
        var grids = GetGrids();
        for (var d = InstanceManager.AppleDirection.Left; d <= InstanceManager.AppleDirection.BottomRight; d++)
            foreach (var grid in grids)
                States.Add(new InstanceManager.State {
                    appleDirection = d,
                    Grid = grid
                });
    }

    private List<SnakeGrid.Element[][]> GetGrids()
    {
        var grids = new List<SnakeGrid.Element[][]>();
        
        var grid = new SnakeGrid.Element[squareSize][];
        for (var y = 0; y < grid.Length; y++)
        {
            grid[y] = new SnakeGrid.Element[squareSize];
            for (var x = 0; x < grid[y].Length; x++)
                grid[y][x] = SnakeGrid.Element.Snake;
        }
        
        InitGridsRecursive(grids, grid, 0, 0);
        return grids;
    }

    private void InitGridsRecursive(ICollection<SnakeGrid.Element[][]> grids, IReadOnlyList<SnakeGrid.Element[]> grid, int row, int col)
    {
        if (row == squareSize)
        {
            var gridCopy = new SnakeGrid.Element[squareSize][];
            for (var y = 0; y < gridCopy.Length; y++)
            {
                gridCopy[y] = new SnakeGrid.Element[squareSize];
                for (var x = 0; x < gridCopy[y].Length; x++)
                    gridCopy[y][x] = grid[y][x];
            }
            grids.Add(gridCopy);
        }
        else
        {
            for (var e = SnakeGrid.Element.Snake; e <= SnakeGrid.Element.None; e++)
            {
                grid[row][col] = e;
                var nextRow = col == squareSize - 1 ? row + 1 : row;
                var nextCol = col == squareSize - 1 ? 0 : col + 1;
                if (nextCol == squareSize / 2 && nextRow == squareSize / 2)
                    nextCol++;
                InitGridsRecursive(grids, grid, nextRow, nextCol);
            }
        }
    }

    public void MakeAction(SnakeHead.Direction action)
    {
        if (IsEpisodeFinished())
            throw new Exception("Episode already finished; invoke Reset to start a new one");
        _nextReward = GetDirectionReward(action);
        instanceManager.ChangeDirection(action);
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
        return instanceManager.GetGameState(squareSize);
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
