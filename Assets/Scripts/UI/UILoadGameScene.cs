using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UILoadGameScene : UILoadScene
{
    [SerializeField] private bool withModel;
    
    protected override Task GetTask()
    {
        RlAgent.WithModel = withModel;
        return base.GetTask();
    }
}
