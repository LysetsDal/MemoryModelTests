using Microsoft.VisualStudio.TestPlatform.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace MemoryModelTests.dvhTest;

public class CtorExec
{
    private readonly ITestOutputHelper _testOutputHelper;
    public CtorExec(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    class A
    {
        protected ITestOutputHelper _output;

        public A(ITestOutputHelper output)
        {
            _output = output;
            PrintFields();
        }

        public virtual void PrintFields() { }
    }
    
    class B : A
    {
        int x = 1;
        int y;

        public B(ITestOutputHelper output) : base(output)
        {
            y = -1;
        }

        public override void PrintFields() =>
            _output.WriteLine($"x = {x}, y = {y}");
    }

    [Fact]
    public void Test()
    {
        new B(_testOutputHelper);
    }
}
