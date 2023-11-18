using System;
using System.Linq;
using UnityEngine;

public class SnakeGrid: MonoBehaviour {
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private float cellSize;
    [SerializeField] private GameObject cellPrefab; 
    
    private float _startingY;
    private float _startingX;
    private Element[,] _gridArray;

    public int Width => width;
    public int Height => height;
    
    public event EventHandler<EventArgs> OnReset;
    public event EventHandler<EventArgs> OnCollision;
    public event EventHandler<EventArgs> OnAppleEaten;
    public event EventHandler<EventArgs> OnMoveOutside;
    
    private static SnakeGrid _instance;
    public static SnakeGrid Instance => _instance;

    public enum Element
    {
        Snake,
        Apple,
        None
    }

    private void InitSingleton()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
    }
    
    private void Awake()
    {
        InitSingleton();
        
        _startingX = transform.position.x - cellSize * width / 2 + cellSize / 2;
        _startingY = transform.position.y - cellSize * height / 2 + cellSize / 2;
        _gridArray = new Element[height, width];

        for (var y = 0; y < _gridArray.GetLength(0); y++)
        {
            for (var x = 0; x < _gridArray.GetLength(1); x++)
            {
                var position = new Vector3(_startingX + x * cellSize, _startingY + y * cellSize, transform.position.z);
                var cell = Instantiate(cellPrefab, position, Quaternion.identity, transform);
                cell.name = $"Cell ({x}, {y})";
            }
        }
        
        Reset();
    }

    public Vector3? MoveFromTo(Vector2 from, Vector2 to)
    {
        var fromElement = GetElementAt(from);

        if (from == to) return null;
        
        if (!IsValid(to))
        {
            OnMoveOutside?.Invoke(this, EventArgs.Empty);
            return null;
        }

        var toElement = GetElementAt(to);
        Debug.Log($"from {from} ({fromElement}) to {to} ({toElement})");
        
        SetElementAt(from, Element.None);
        SetElementAt(to, fromElement);
        
        switch (toElement)
        {
            case Element.None:
                return GetPositionAt(to);
            case Element.Apple:
                OnAppleEaten?.Invoke(this, EventArgs.Empty);
                return GetPositionAt(to);
            case Element.Snake:
            default:
                OnCollision?.Invoke(this, EventArgs.Empty);
                return null;
        }
    }

    public Vector3 Insert(Element el, Vector2 coord)
    {
        Debug.Log($"Insert {el} in {coord} ({GetElementAt(coord)})");
        if (!IsEmpty(coord))
            throw new Exception("Cannot insert element at non-empty cell");
        
        _gridArray[(int) coord.y, (int) coord.x] = el;
        
        return GetPositionAt(coord);
    }

    public Element GetElementAt(Vector2 coord)
    {
        return _gridArray[(int) coord.y, (int) coord.x];
    }
    
    public void SetElementAt(Vector2 coord, Element el)
    {
        _gridArray[(int)coord.y, (int)coord.x] = el;
    }
    
    private Vector3 GetPositionAt(Vector2 coord)
    {
        return new Vector3(_startingX + coord.x * cellSize, _startingY + coord.y * cellSize, 0);
    }
    
    public bool IsEmpty(Vector2 coord)
    {
        return GetElementAt(coord) == Element.None;
    }

    public bool IsValid(Vector2 coord)
    {
        return coord.x >= 0 && coord.x < width && coord.y >= 0 && coord.y < height;
    }

    public void Reset()
    {
        for (var y = 0; y < _gridArray.GetLength(0); y++)
        {
            for (var x = 0; x < _gridArray.GetLength(1); x++)
            {
                _gridArray[y, x] = Element.None;
            }
        }
        
        OnReset?.Invoke(this, EventArgs.Empty);
    }

    public Vector2 GetRandomFreePosition()
    {
        return Enumerable.Range(0, _gridArray.GetLength(0))
            .SelectMany(y => Enumerable.Range(0, _gridArray.GetLength(1)).Select(x => new Vector2(x, y)))
            .Where(coord => GetElementAt(coord) == Element.None)
            .OrderBy(_ => Guid.NewGuid())
            .FirstOrDefault();
    }
}
