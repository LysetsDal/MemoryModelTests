using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace MemoryModelTests.dvhTest;

public class finalstuff
{
    ITestOutputHelper _testOutputHelper;
    public finalstuff(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    private List<string> list = new List<string>();

    [MethodImpl(MethodImplOptions.Synchronized)]
    public int findOrAdd(String s)
    {
        int ret = list.IndexOf(s);
        if (ret == -1)
        {
            list.Add(s);
        }
        return ret;
    }
    public int find(String s)
    {
        return list.IndexOf(s);
    }
}

class Test3
{
    int s_value = 42;
    object s_obj = new object();
    public void PrintValue()
    {
        Console.WriteLine(s_value);
        Console.WriteLine(s_obj == null);
    }
}