using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Environment : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private int squareSize = 3;
    
    private int _nextReward;
    public List<GameManager.State> States = new();
    public List<SnakeGrid.Element[][]> Grids = new();

    private void Awake()
    {
        InitStates();
    }

    private void Start()
    {
        SnakeGrid.Instance.OnCollision += SnakeGrid_OnCollision;
        SnakeGrid.Instance.OnMoveOutside += SnakeGrid_OnMoveOutside;
        SnakeComponent.OnGrow += SnakeComponent_OnGrow;
    }

    public float TimeBetweenActions => gameManager.SnakeTimeBetweenMoves;
    
    private void InitStates()
    {
        InitGrids();
        for (var d = GameManager.AppleDirection.Left; d <= GameManager.AppleDirection.BottomRight; d++)
        {
            foreach (var grid in Grids)
            {
                States.Add(new GameManager.State
                {
                    AppleDirection = d,
                    Grid = grid
                });
            }
        }
    }

    private void InitGrids()
    {
        var grid = new SnakeGrid.Element[squareSize][];
        for (var y = 0; y < grid.Length; y++)
        {
            grid[y] = new SnakeGrid.Element[squareSize];
            for (var x = 0; x < grid[y].Length; x++)
                grid[y][x] = SnakeGrid.Element.Void;
        }
        
        InitGridsRecursive(grid, 0, 0);
    }

    private void InitGridsRecursive(SnakeGrid.Element[][] grid, int row, int col)
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
            Grids.Add(gridCopy);
        }
        else
        {
            for (var e = SnakeGrid.Element.Snake; e <= SnakeGrid.Element.None; e++)
            {
                grid[row][col] = e;
                var nextRow = col == squareSize - 1 ? row + 1 : row;
                var nextCol = col == squareSize - 1 ? 0 : col + 1;
                InitGridsRecursive(grid, nextRow, nextCol);
            }
        }
    }

    public void MakeAction(SnakeHead.Direction action)
    {
        if (IsEpisodeFinished())
            throw new Exception("Episode already finished; invoke Reset to start a new one");
        _nextReward = GetDirectionReward(action);
        gameManager.ChangeDirection(action);
    }

    private int GetDirectionReward(SnakeHead.Direction action)
    {
        var appleDirection = gameManager.GetAppleDirection();
        
        switch (action)
        {
            case SnakeHead.Direction.Up when appleDirection is GameManager.AppleDirection.Top or GameManager.AppleDirection.TopLeft or GameManager.AppleDirection.TopRight:
            case SnakeHead.Direction.Down when appleDirection is GameManager.AppleDirection.Bottom or GameManager.AppleDirection.BottomLeft or GameManager.AppleDirection.BottomRight:
            case SnakeHead.Direction.Left when appleDirection is GameManager.AppleDirection.Left or GameManager.AppleDirection.TopLeft or GameManager.AppleDirection.BottomLeft:
            case SnakeHead.Direction.Right when appleDirection is GameManager.AppleDirection.Right or GameManager.AppleDirection.TopRight or GameManager.AppleDirection.BottomRight:
                return 1;
            default:
                return -1;
        }
    }

    public GameManager.State GetState()
    {
        return gameManager.GetGameState(squareSize);
    }

    public int GetReward()
    {
        return _nextReward;
    }
    
    private void SnakeGrid_OnMoveOutside(object sender, EventArgs e)
    {
        OnDeath();
    }

    private void SnakeGrid_OnCollision(object sender, EventArgs e)
    {
        OnDeath();
    }
    
    private void SnakeComponent_OnGrow(object sender, EventArgs e)
    {
        _nextReward = 2;
    }
    
    private void OnDeath()
    {
        _nextReward = -2;
    }

    public bool IsEpisodeFinished()
    {
        return !gameManager.Running;
    }

    public GameManager.State ResetEnvironment()
    {
        Debug.Log("Environment reset");
        _nextReward = 0;
        return gameManager.StartGame();
    }
}
