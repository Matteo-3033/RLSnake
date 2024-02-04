using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeHead : SnakeComponent
{
    public Direction CurrentDirection { get; private set; } = Direction.Right;
    
    [Range(0.0001F, 1)] public float timeBetweenMoves = 0.5F;

    [Serializable]
    public enum Direction
    {
        Left,
        Right,
        Up,
        Down
    }
    
    public static readonly Dictionary<Direction, Vector2> DirectionToVector = new()
    {
        { Direction.Right, Vector2.right },
        { Direction.Left, Vector2.left },
        { Direction.Up, Vector2.up },
        { Direction.Down , Vector2.down }
    };

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
            ChangeDirection(Direction.Up);
        else if (Input.GetKeyDown(KeyCode.S))
            ChangeDirection(Direction.Down);
        else if (Input.GetKeyDown(KeyCode.A))
            ChangeDirection(Direction.Left);
        else if (Input.GetKeyDown(KeyCode.D))
            ChangeDirection(Direction.Right);
    }

    public bool ChangeDirection(Direction direction)
    {
        if (!IsValidDirection(direction)) return false;
        CurrentDirection = direction;
        return true;
    }

    private bool IsValidDirection(Direction direction)
    {
        return direction != OppositeDirection(CurrentDirection);
    }

    public static Direction OppositeDirection(Direction direction)
    {
        return direction switch
        {
            Direction.Up => Direction.Down,
            Direction.Down => Direction.Up,
            Direction.Left => Direction.Right,
            Direction.Right => Direction.Left,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
    }
    
    private IEnumerator UpdatePosition()
    {
        Debug.Log("Starting UpdatePosition coroutine");
        while (true)
        {
            yield return new WaitForSeconds(timeBetweenMoves);
            MoveTo(GridPosition + DirectionToVector[CurrentDirection]);
        }
        // ReSharper disable once IteratorNeverReturns
    }

    public override void Reset()
    {
        if (PreviousSnake != null)
        {
            PreviousSnake.Reset();
            PreviousSnake = null;
        }
        
        CurrentDirection = Direction.Right;
        
        InitPosition(grid.GetRandomFreePosition());
        StartCoroutine(nameof(UpdatePosition));
    }

    public void Stop()
    {
        StopCoroutine(nameof(UpdatePosition));
    }
}
