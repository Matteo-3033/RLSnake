using UnityEngine;

public class AppleGenerator : MonoBehaviour
{
    [SerializeField] private GameObject applePrefab;
    [SerializeField] private SnakeGrid grid;
    
    private GameObject _apple;
    
    public Vector2 GridPosition { get; private set; }
    
    public void Reset()
    {
        Destroy(_apple);
        GenerateApple();
    }

    private void GenerateApple()
    {
        GridPosition = grid.GetRandomFreePosition(GridPosition);
        var applePos = grid.Insert(SnakeGrid.Element.Apple, GridPosition);

        _apple = Instantiate(applePrefab, Vector3.zero, Quaternion.identity, transform);
        _apple.name = "Apple";
        _apple.transform.position = new Vector3(applePos.x, applePos.y, applePrefab.transform.position.z);
    }
}
