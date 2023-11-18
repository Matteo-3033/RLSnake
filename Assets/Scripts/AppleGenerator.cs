using System;
using UnityEngine;

public class AppleGenerator : MonoBehaviour
{
    [SerializeField] private GameObject applePrefab;

    private GameObject _apple;
    
    private void Start()
    {
        GenerateApple();
        SnakeGrid.Instance.OnAppleEaten += SnakeGrid_OnAppleEaten;
    }

    private void SnakeGrid_OnAppleEaten(object sender, EventArgs e)
    {
        Debug.Log("Apple eaten");
        Reset();
    }
    
    public void Reset()
    {
        Destroy(_apple);
        GenerateApple();
    }

    private void GenerateApple()
    {
        var applePos = SnakeGrid.Instance.Insert(
            SnakeGrid.Element.Apple,
            SnakeGrid.Instance.GetRandomFreePosition()
        );
        
        _apple = Instantiate(applePrefab, Vector3.zero, Quaternion.identity, transform);
        _apple.name = "Apple";
        _apple.transform.position = new Vector3(applePos.x, applePos.y, applePrefab.transform.position.z);
    }
}
