using System;
using UnityEngine;

public class EpochsPlayer : MonoBehaviour
{
    public event EventHandler<OnEpochFinishedArgs> OnEpochFinished;
    public class OnEpochFinishedArgs: EventArgs
    {
        public readonly int EpochsCnt;
        public OnEpochFinishedArgs(int epochsCnt) => EpochsCnt = epochsCnt;
    }

    protected void InvokeOnEpochFinished(OnEpochFinishedArgs e)
    {
        OnEpochFinished?.Invoke(this, e);
    }
}