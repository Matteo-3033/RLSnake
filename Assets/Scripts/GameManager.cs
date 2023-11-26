using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private SnakeHead snake;
    [SerializeField] private AppleGenerator appleGenerator;
    [SerializeField] private bool playerControlled = true;
    
    public bool Running { get; private set; }
    private int Score { get; set; }
    
    public float SnakeTimeBetweenMoves => snake.timeBetweenMoves;
    
    private void Start()
    {
        Score = 0;
        SnakeGrid.Instance.OnCollision += SnakeGrid_OnCollision;
        SnakeGrid.Instance.OnMoveOutside += SnakeGrid_OnMoveOutside;
        SnakeComponent.OnGrow += SnakeComponent_OnGrow;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            StartGame();
    }

    private void SnakeComponent_OnGrow(object sender, EventArgs e)
    {
        Score += 1;
        appleGenerator.Reset();
    }

    private void SnakeGrid_OnMoveOutside(object sender, EventArgs e)
    {
        Debug.Log("Movement outside of border");
        StopGame();
    }

    private void SnakeGrid_OnCollision(object sender, EventArgs e)
    {
        Debug.Log("Collision");
        StopGame();
    }
    
    public State StartGame()
    {
        Debug.Log("Starting game");
        Score = 0;
        
        SnakeGrid.Instance.Reset();
        appleGenerator.Reset();
        snake.Reset();
        
        Running = true;

        return GetGameState();
    }

    private void StopGame()
    {
        Debug.Log("Game over");
        Running = false;
        snake.Stop();
    }

    public void ChangeDirection(SnakeHead.Direction direction)
    {
        if (playerControlled) return;
        snake.ChangeDirection(direction);
    }

    public State GetGameState(int squareSize = 3)
    {
        return new State
        {
            AppleDirection = GetAppleDirection(),
            Grid = SnakeGrid.Instance.GetSquareCenteredIn(snake.GridPosition, squareSize)
        };
    }

    private AppleDirection GetAppleDirection()
    {
        var applePos = appleGenerator.GridPosition;
        var snakePos = snake.GridPosition;
        
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

    public record State
    {
        public AppleDirection AppleDirection;
        public SnakeGrid.Element[][] Grid;
    }

    public enum AppleDirection
    {
        Left, Right, Top, Bottom,
        TopLeft, TopRight, BottomLeft, BottomRight
    }
}
