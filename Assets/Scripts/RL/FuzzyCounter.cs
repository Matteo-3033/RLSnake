using System.Collections.Generic;

public class FuzzyCounter
{
    private enum InputClass
    {
        None,
        Low,
        Medium,
        High
    }
    
    private enum OutputClass
    {
        None,
        Low,
        Medium,
        High
    }
    
    private Dictionary<InputClass, OutputClass> _fam = new()
    {
        {InputClass.None, OutputClass.None},
        {InputClass.Low, OutputClass.Low},
        {InputClass.Medium, OutputClass.Medium},
        {InputClass.High, OutputClass.High}
    };
    
    public void CntToClass(int cnt)
    {
        var inputMembership = GetInputMembership(cnt);
    }

    private int[] GetInputMembership(int cnt)
    {
        var membership = new int[4];
        if (cnt == 0)
        {
            membership[0] = 1;
            return membership;
        }
    }
}