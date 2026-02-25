namespace MemoryModelTests.Readonly;

public class HelperObject
{
    private readonly int NormalData;
    private readonly int ReadonlyData;

    public HelperObject(int num)
    {
        NormalData = num;
    }

    public HelperObject(int num, int readOnly)
    {
        NormalData = num;
        ReadonlyData = readOnly;
    }

    public int getReadonly()
    {
        return ReadonlyData;
    }

    public int getInteger()
    {
        return NormalData;
    }
}