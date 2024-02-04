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
    public int SnakeLength => Score + 1;
    
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
        OnSnakeGrowth?.Invoke(this, new OnSnakeGrowthArgs(SnakeLength));
        appleGenerator.Reset();
    }

    private void OnDeath(object sender, EventArgs e)
    {
        Debug.Log("Game over");
        StopGame();
        OnGameOver?.Invoke(this, EventArgs.Empty);
    }
    
    public void StartGame()
    {
        Debug.Log("Starting game");
        Score = 0;
        
        snake.Stop();
        grid.Reset();
        appleGenerator.Reset();
        snake.Reset();
        
        Running = true;
        
        OnSnakeGrowth?.Invoke(this, new OnSnakeGrowthArgs(SnakeLength));
    }

    private void StopGame()
    {
        Debug.Log("Stopping game");
        Running = false;
        snake.Stop();
    }

    public bool ChangeDirection(SnakeHead.Direction direction)
    {
        if (playerControlled) return false;
        return snake.ChangeDirection(direction);
    }
}
