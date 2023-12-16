using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeHead : SnakeComponent
{
    public Direction CurrentDirection { get; private set; } = Direction.Right;
    private Direction? _lastDirection;
    
    [Range(0.0001F, 1)] public float timeBetweenMoves = 0.5F;

    [Serializable]
    public enum Direction
    {
        Left,
        Right,
        Up,
        Down
    }
    
    private readonly Dictionary<Direction, Vector2> _directionToVector = new()
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
    
    public bool IsValidDirection(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up when _lastDirection == Direction.Down:
            case Direction.Down when _lastDirection == Direction.Up:
            case Direction.Left when _lastDirection == Direction.Right:
            case Direction.Right when _lastDirection == Direction.Left:
                return false;
        }
        return true;
    }
    
    private IEnumerator UpdatePosition()
    {
        Debug.Log("Starting UpdatePosition coroutine");
        while (true)
        {
            yield return new WaitForSeconds(timeBetweenMoves);
            _lastDirection = CurrentDirection;
            MoveTo(GridPosition + _directionToVector[CurrentDirection]);
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
        _lastDirection = null;
        
        InitPosition(grid.GetRandomFreePosition());
        StartCoroutine(nameof(UpdatePosition));
    }

    public void Stop()
    {
        StopCoroutine(nameof(UpdatePosition));
    }
}
