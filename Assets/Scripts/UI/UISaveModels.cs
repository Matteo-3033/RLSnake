using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class UISaveModels : UILoadingButton
{
    [SerializeField] private RlAgent[] agents;

    protected override Task GetTask()
    {
        var tasks = agents.Select(agent => agent.SaveModel()).ToArray();
        return Task.WhenAll(tasks);
    }
}
