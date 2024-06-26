using System;
using UnityEngine;

public class SnakeComponent : MonoBehaviour
{
    [SerializeField] private GameObject componentPrefab;
    [SerializeField] protected SnakeGrid grid;
    
    public event EventHandler<EventArgs> OnGrowth;

    private Vector2 _lastPosition;
    private bool _appleEaten;

    protected SnakeComponent PreviousSnake { get; set; }
    public Vector2 GridPosition { get; private set; }

    protected virtual void Start()
    {
        grid.OnAppleEaten += SnakeGrid_OnAppleEaten;
    }

    protected void MoveTo(Vector2 newGridPosition)
    {
        var newPosition = grid.MoveFromTo(GridPosition, newGridPosition);
        if (newPosition == null) return;
        
        transform.position = newPosition.Value;
        
        _lastPosition = GridPosition;
        GridPosition = newGridPosition;
        
        if (PreviousSnake != null)
            PreviousSnake.MoveTo(_lastPosition);

        if (!_appleEaten) return;
        
        _appleEaten = false;
        GenerateNext();
    }

    protected void InitPosition(Vector2 gridPosition)
    {
        GridPosition = gridPosition;
        transform.position = grid.Insert(SnakeGrid.Element.Snake, GridPosition);
    }

    public virtual void Reset()
    {
        if (PreviousSnake != null)
            PreviousSnake.Reset();
        Destroy(gameObject);
    }
    
    private void SnakeGrid_OnAppleEaten(object sender, EventArgs e)
    {
        if (PreviousSnake != null) return;
        _appleEaten = true;
    }
    
    private void GenerateNext()
    {
        var part = Instantiate(componentPrefab, Vector3.zero, Quaternion.identity, transform.parent);
        var snakePart = part.GetComponent<SnakeComponent>();
        snakePart.grid = grid;
        snakePart.OnGrowth = OnGrowth;
        PreviousSnake = snakePart;
        snakePart.InitPosition(_lastPosition);
        OnGrowth?.Invoke(this, EventArgs.Empty);
    }

    public Vector2 TailGridPosition()
    {
        return PreviousSnake == null ? GridPosition : PreviousSnake.TailGridPosition();
    }
}
