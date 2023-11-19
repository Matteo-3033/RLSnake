using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private SnakeHead snake;
    [SerializeField] private AppleGenerator appleGenerator;
    
    public int Score { get; private set; }
    
    private void Start()
    {
        Score = 0;
        SnakeGrid.Instance.OnCollision += SnakeGrid_OnCollision;
        SnakeGrid.Instance.OnMoveOutside += SnakeGrid_OnMoveOutside;
        SnakeGrid.Instance.OnAppleEaten += SnakeGrid_OnAppleEaten;
    }

    private void SnakeGrid_OnAppleEaten(object sender, EventArgs e)
    {
        Score += 1;
    }

    private void SnakeGrid_OnMoveOutside(object sender, EventArgs e)
    {
        Debug.Log("Movement outside of border: game reset");
        ResetGame();
    }

    private void SnakeGrid_OnCollision(object sender, EventArgs e)
    {
        Debug.Log("Collision: game reset");
        ResetGame();
    }

    private void ResetGame()
    {
        SnakeGrid.Instance.Reset();
        appleGenerator.Reset();
        snake.Reset();
        Score = 0;
    }
}
