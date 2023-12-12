using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UILoadPlayScene : UILoadScene
{
    protected override Task GetTask()
    {
        PlayingAgent.ModelFileName = "tdLambda.json";
        return base.GetTask();
    }
}
