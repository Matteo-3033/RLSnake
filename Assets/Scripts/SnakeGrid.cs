using System;
using System.Linq;
using UnityEngine;

public class SnakeGrid: MonoBehaviour {
        
    public event EventHandler<EventArgs> OnCollision;
    public event EventHandler<EventArgs> OnAppleEaten;
    public event EventHandler<EventArgs> OnMoveOutside;
    
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private float cellSize;
    [SerializeField] private GameObject cellPrefab; 

    private float _startingY;
    private float _startingX;

    public int Width => width;
    public int Height => height;
    private Element[][] _grid;

    private static SnakeGrid _instance;
    public static SnakeGrid Instance => _instance;
    
    public enum Element
    {
        Snake,
        Apple,
        Void,
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
        _grid = new Element[height][];

        for (var y = 0; y < _grid.Length; y++)
        {
            _grid[y] = new Element[width];
            for (var x = 0; x < _grid[y].Length; x++)
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
        //Debug.Log($"from {from} ({fromElement}) to {to} ({toElement})");
        
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
        
        _grid[(int) coord.y][(int) coord.x] = el;
        
        return GetPositionAt(coord);
    }

    private Element GetElementAt(Vector2 coord)
    {
        return _grid[(int) coord.y][(int) coord.x];
    }

    private void SetElementAt(Vector2 coord, Element el)
    {
        _grid[(int)coord.y][(int)coord.x] = el;
    }
    
    private Vector3 GetPositionAt(Vector2 coord)
    {
        return new Vector3(_startingX + coord.x * cellSize, _startingY + coord.y * cellSize, 0);
    }

    private bool IsEmpty(Vector2 coord)
    {
        return GetElementAt(coord) == Element.None;
    }

    private bool IsValid(Vector2 coord)
    {
        return coord.x >= 0 && coord.x < width && coord.y >= 0 && coord.y < height;
    }

    public void Reset()
    {
        foreach (var t in _grid)
        {
            for (var x = 0; x < t.Length; x++)
            {
                t[x] = Element.None;
            }
        }
    }

    public Vector2 GetRandomFreePosition(Vector2? not = null)
    {
        return Enumerable.Range(0, _grid.Length)
            .SelectMany(y => Enumerable.Range(0, _grid[y].Length).Select(x => new Vector2(x, y)))
            .Where(coord => coord != not && IsEmpty(coord))
            .OrderBy(_ => Guid.NewGuid())
            .FirstOrDefault();
    }
    
    public Element[][] GetSquareCenteredIn(Vector2 center, int size = 3)
    {
        if (size % 2 == 0)
            throw new Exception("size must be odd");
        
        var res = new Element[size][];

        var offset = size / 2;
        for (var i = 0; i < size; i++)
        {
            res[i] = new Element[size];
            for (var j = 0; j < size; j++)
            {
                var y = (int)center.y + i - offset;
                var x = (int)center.x + j - offset;

                if (y < 0 || y >= _grid.Length || x < 0 || x >= _grid[y].Length)
                    res[i][j] = Element.Void;
                else res[i][j] = _grid[y][x];    // == Element.Snake ? Element.Snake : Element.None;
            }
        }

        return res;
    }
}
