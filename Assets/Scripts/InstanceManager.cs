using System;
using UnityEngine;

public class InstanceManager : MonoBehaviour
{
    [SerializeField] private SnakeHead snake;
    [SerializeField] private AppleGenerator appleGenerator;
    [SerializeField] private SnakeGrid grid;
    [SerializeField] private bool playerControlled = true;
    
    public event EventHandler<EventArgs> OnGameOver;
    public event EventHandler<OnSnakeGrowthArgs> OnSnakeGrowth;
    
    public class OnSnakeGrowthArgs: EventArgs
    {
        public int Length;

        public OnSnakeGrowthArgs(int length)
        {
            Length = length;
        }
    }
    
    public bool Running { get; private set; }
    private int Score { get; set; }
    
    public SnakeHead SnakeHead => snake;
    public SnakeGrid Grid => grid;
    public AppleGenerator AppleGenerator => appleGenerator;
    
    public float SnakeTimeBetweenMoves => snake.timeBetweenMoves;
    
    private void Start()
    {
        Score = 0;
        grid.OnCollision += OnDeath;
        grid.OnMoveOutside += OnDeath;
        snake.OnGrowth += SnakeComponentOnGrowth;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            StartGame();
    }

    private void SnakeComponentOnGrowth(object sender, EventArgs e)
    {
        Score += 1;
        OnSnakeGrowth?.Invoke(this, new OnSnakeGrowthArgs(Score));
        appleGenerator.Reset();
    }

    private void OnDeath(object sender, EventArgs e)
    {
        Debug.Log("Game over");
        StopGame();
        OnGameOver?.Invoke(this, EventArgs.Empty);
    }
    
    public State StartGame()
    {
        Debug.Log("Starting game");
        Score = 0;
        
        grid.Reset();
        appleGenerator.Reset();
        snake.Reset();
        
        Running = true;
        
        OnSnakeGrowth?.Invoke(this, new OnSnakeGrowthArgs(Score));

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
            Grid = grid.GetSquareCenteredIn(snake.GridPosition, squareSize)
        };
    }

    public AppleDirection GetAppleDirection()
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
