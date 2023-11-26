using System;
using UnityEngine;

public class SnakeComponent : MonoBehaviour
{
    [SerializeField] private GameObject componentPrefab;
    public static event EventHandler<EventArgs> OnGrow; 

    private Vector2 _lastPosition;
    private bool _appleEaten;

    protected SnakeComponent PreviousSnake { get; set; }
    public Vector2 GridPosition { get; private set; }

    protected virtual void Start()
    {
        SnakeGrid.Instance.OnAppleEaten += SnakeGrid_OnAppleEaten;
    }

    protected void MoveTo(Vector2 newGridPosition)
    {
        var newPosition = SnakeGrid.Instance.MoveFromTo(GridPosition, newGridPosition);
        if (newPosition == null) return;
        
        transform.position = newPosition.Value;
        
        _lastPosition = GridPosition;
        GridPosition = newGridPosition;
        
        if (PreviousSnake != null)
            PreviousSnake.MoveTo(_lastPosition);
        
        if (_appleEaten)
        {
            _appleEaten = false;
            GenerateNext();
        }
    }

    protected void InitPosition(Vector2 gridPosition)
    {
        GridPosition = gridPosition;
        transform.position = SnakeGrid.Instance.Insert(SnakeGrid.Element.Snake, GridPosition);
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
        PreviousSnake = snakePart;
        snakePart.InitPosition(_lastPosition);
        OnGrow?.Invoke(this, EventArgs.Empty);
    }
}
