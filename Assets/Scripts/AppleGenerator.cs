using System;
using UnityEngine;

public class AppleGenerator : MonoBehaviour
{
    [SerializeField] private GameObject applePrefab;
    
    private GameObject _apple;
    
    public Vector2 GridPosition { get; private set; }
    
    private void Start()
    {
        GenerateApple();
    }
    
    public void Reset()
    {
        Destroy(_apple);
        GenerateApple();
    }

    private void GenerateApple()
    {
        GridPosition = SnakeGrid.Instance.GetRandomFreePosition(GridPosition);
        var applePos = SnakeGrid.Instance.Insert(SnakeGrid.Element.Apple, GridPosition);

        _apple = Instantiate(applePrefab, Vector3.zero, Quaternion.identity, transform);
        _apple.name = "Apple";
        _apple.transform.position = new Vector3(applePos.x, applePos.y, applePrefab.transform.position.z);
    }
}
