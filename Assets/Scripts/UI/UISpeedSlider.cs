using UnityEngine;

public class UISpeedSlider : MonoBehaviour
{
    [SerializeField] private SnakeHead[] snakes;
    
    private void Start()
    {
        var slider = GetComponent<UnityEngine.UI.Slider>();
        slider.onValueChanged.AddListener(OnSpeedChanged);
        OnSpeedChanged(slider.value);
    }

    private void OnSpeedChanged(float speed)
    {
        foreach (var snake in snakes)
            snake.timeBetweenMoves = speed;
    }
}
