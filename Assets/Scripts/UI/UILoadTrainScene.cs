using System.Threading.Tasks;
using UnityEngine;

public class UILoadTrainScene : UILoadScene
{
    [SerializeField] private bool useModels;
    
    protected override Task GetTask()
    {
        Settings.UseModels = useModels;
        return base.GetTask();
    }
}
