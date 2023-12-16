using System;
using System.Collections.Generic;
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

    public int Width => width;
    public int Height => height;
    private Element[][] _grid;
    
    private float StartingX => transform.position.x - cellSize * width / 2 + cellSize / 2;
    private float StartingY => transform.position.y - cellSize * height / 2 + cellSize / 2;
    
    [Serializable]
    public enum Element
    {
        Snake,
        Apple,
        None,
        Last
    }
    
    private void Awake()
    {
        height = Settings.GridRows;
        width = Settings.GridColumns;
        
        var pos = transform.position;
        _grid = new Element[height][];

        for (var y = 0; y < _grid.Length; y++)
        {
            _grid[y] = new Element[width];
            for (var x = 0; x < _grid[y].Length; x++)
            {
                var position = new Vector3(StartingX + x * cellSize, StartingY + y * cellSize, pos.z);
                var cell = Instantiate(cellPrefab, position, Quaternion.identity, transform);
                cell.name = $"Cell ({x}, {y})";
            }
        }
        
        Reset();
    }

    public Vector3? MoveFromTo(Vector2 from, Vector2 to)
    {
        if (from == to) return null;
        
        if (IsOutside(to))
        {
            OnMoveOutside?.Invoke(this, EventArgs.Empty);
            return null;
        }

        var fromElement = GetElementAt(from);
        var toElement = GetElementAt(to);
        
        Debug.Log($"from {from} ({fromElement}) to {to} ({toElement})");

        if (toElement is Element.None or Element.Apple)
        {
            SetElementAt(from, Element.None);
            SetElementAt(to, fromElement);
            
            if (toElement == Element.Apple)
                OnAppleEaten?.Invoke(this, EventArgs.Empty);
            return GetPositionAt(to);
        }

        OnCollision?.Invoke(this, EventArgs.Empty);
        return null;
    }

    public Vector3 Insert(Element el, Vector2 coord)
    {
        Debug.Log($"Insert {el} at {coord} ({GetElementAt(coord)})");
        if (!IsEmpty(coord))
            throw new Exception("Cannot insert element at non-empty cell");
        
        _grid[(int) coord.y][(int) coord.x] = el;
        
        return GetPositionAt(coord);
    }

    public Element GetElementAt(Vector2 coord)
    {
        return IsOutside(coord) ? Element.Snake : _grid[(int) coord.y][(int) coord.x];
    }

    private void SetElementAt(Vector2 coord, Element el)
    {
        _grid[(int)coord.y][(int)coord.x] = el;
    }
    
    private Vector3 GetPositionAt(Vector2 coord)
    {
        return new Vector3(StartingX + coord.x * cellSize, StartingY + coord.y * cellSize, transform.position.z - 1);
    }

    private bool IsEmpty(Vector2 coord)
    {
        return GetElementAt(coord) == Element.None;
    }

    private bool IsOutside(Vector2 coord)
    {
        return coord.x < 0 || coord.x >= width || coord.y < 0 || coord.y >= height;
    }

    public void Reset()
    {
        foreach (var t in _grid)
            for (var x = 0; x < t.Length; x++)
                t[x] = Element.None;
    }

    public Vector2 GetRandomFreePosition(Vector2? not = null)
    {
        return Enumerable.Range(0, _grid.Length)
            .SelectMany(y => Enumerable.Range(0, _grid[y].Length).Select(x => new Vector2(x, y)))
            .Where(coord => coord != not && IsEmpty(coord))
            .OrderBy(_ => Guid.NewGuid())
            .FirstOrDefault();
    }

    public int BreathFirstSearch(Vector2 start)
    {
        if (GetElementAt(start) == Element.Snake)
            return 0;
        
        var visited = new bool[height][];
        for (var y = 0; y < visited.Length; y++)
            visited[y] = new bool[width];
        
        var queue = new Queue<Vector2>();
        queue.Enqueue(start);
        visited[(int) start.y][(int) start.x] = true;
        
        var count = 0;
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            count++;
            
            foreach (var neighbour in new[]{ current + Vector2.up, current + Vector2.right, current + Vector2.down, current + Vector2.left})
            {
                if (visited[(int) neighbour.y][(int) neighbour.x]) continue;
                if (GetElementAt(neighbour) == Element.Snake) continue;
                visited[(int) neighbour.y][(int) neighbour.x] = true;
                queue.Enqueue(neighbour);
            }
        }
        
        return count;
    }
}
