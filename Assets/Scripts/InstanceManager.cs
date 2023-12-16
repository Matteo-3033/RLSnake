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
        public readonly int Length;

        public OnSnakeGrowthArgs(int length) => Length = length;
    }
    
    public bool Running { get; private set; }
    public int Score { get; private set; }
    
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

    public bool ChangeDirection(SnakeHead.Direction direction)
    {
        if (playerControlled) return false;
        return snake.ChangeDirection(direction);
    }

    public State GetGameState()
    {
        var state = new State
        {
            appleDirection = GetAppleDirection(),
            top = grid.GetElementAt(snake.GridPosition + Vector2.up),
            left = grid.GetElementAt(snake.GridPosition + Vector2.left),
            right = grid.GetElementAt(snake.GridPosition + Vector2.right),
            bottom = grid.GetElementAt(snake.GridPosition + Vector2.down)
        };

        switch (SnakeHead.OppositeDirection(SnakeHead.CurrentDirection))
        {
            case SnakeHead.Direction.Up:
                state.top = SnakeGrid.Element.Last;
                break;
            case SnakeHead.Direction.Down:
                state.bottom = SnakeGrid.Element.Last;
                break;
            case SnakeHead.Direction.Left:
                state.left = SnakeGrid.Element.Last;
                break;
            case SnakeHead.Direction.Right:
                state.right = SnakeGrid.Element.Last;
                break;
        }

        return state;
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

    [Serializable]
    public record State
    {
        public AppleDirection appleDirection;
        public SnakeGrid.Element top;
        public SnakeGrid.Element left;
        public SnakeGrid.Element right;
        public SnakeGrid.Element bottom;
    }

    [Serializable]
    public enum AppleDirection
    {
        Left, Right, Top, Bottom,
        TopLeft, TopRight, BottomLeft, BottomRight
    }
}
