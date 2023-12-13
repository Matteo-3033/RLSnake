public class UISnakeMaxLength : UISnakeLength
{   
    private int _maxLength = -1;

    protected override string GetText(int length)
    {
        if (length <= _maxLength) return _maxLength.ToString();
        _maxLength = length;
        return _maxLength.ToString();
    }
}
