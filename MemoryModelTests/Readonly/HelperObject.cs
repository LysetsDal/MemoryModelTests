namespace MemoryModelTests.Readonly;

public class HelperObject
{
    private readonly int _data;
    // private int _data;

    public HelperObject(int data)
    {
        _data = data;
    }

    public int getData()
    {
        return _data;
    }
}