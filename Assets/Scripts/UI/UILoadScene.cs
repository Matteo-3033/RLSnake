using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UILoadScene : UILoadingButton
{
    [SerializeField] private Scene scene;

    protected override Task GetTask()
    {
        var loading = SceneManager.LoadSceneAsync(scene.ToString());
        return Task.Run(() =>
        {
            while (!loading.isDone) {}
        });
    }
}
