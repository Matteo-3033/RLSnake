using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private SnakeHead snake;
    [SerializeField] private AppleGenerator appleGenerator;
    
    private void Start()
    {
        SnakeGrid.Instance.OnCollision += SnakeGrid_OnCollision;
        SnakeGrid.Instance.OnMoveOutside += SnakeGrid_OnMoveOutside;
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
    }
}
