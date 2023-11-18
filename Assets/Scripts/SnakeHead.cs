using System.Collections;
using UnityEngine;

public class SnakeHead : SnakeComponent
{
    private Vector2 _direction = Vector2.right;
    private Vector2? _lastDirection;

    protected override void Start()
    {
        base.Start();

        Reset();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W) && _lastDirection != Vector2.down)
        {
            _direction = Vector2.up;
        }
        else if (Input.GetKeyDown(KeyCode.S) && _lastDirection != Vector2.up)
        {
            _direction = Vector2.down;
        }
        else if (Input.GetKeyDown(KeyCode.A) && _lastDirection != Vector2.right)
        {
            _direction = Vector2.left;
        }
        else if (Input.GetKeyDown(KeyCode.D) && _lastDirection != Vector2.left)
        {
            _direction = Vector2.right;
        }
    }

    private IEnumerator UpdatePosition()
    {
        while (true)
        {
            _lastDirection = _direction;
            yield return new WaitForSeconds(0.5F);
            MoveTo(GridPosition + _direction);
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
        
        _direction = Vector2.right;
        _lastDirection = null;
        
        StopCoroutine(nameof(UpdatePosition));
        InitPosition(SnakeGrid.Instance.GetRandomFreePosition());
        StartCoroutine(nameof(UpdatePosition));
    }
}
