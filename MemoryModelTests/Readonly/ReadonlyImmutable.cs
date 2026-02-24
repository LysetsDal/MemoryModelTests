using Xunit;
using Xunit.Abstractions;
// ReSharper disable ConvertConstructorToMemberInitializers
// ReSharper disable ConvertToPrimaryConstructor


namespace MemoryModelTests.Readonly;

public class ReadonlyImmutable(ITestOutputHelper testOutputHelper)
{
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;

    private static Barrier barrier;

    [Fact]
    public void Test_Readonly_Publication_Safety()
    {
        for (int i = 0; i < 100_000; i++)
        {
            HelperObject helperObject = null;
            int observedData = 0;
            
            var startSignal = new ManualResetEventSlim(false);

            // Thread 1: Constructs the class with the readonly field
            Thread t1 = new Thread(() =>
            {
                startSignal.Wait();
                helperObject = new HelperObject(1);
            });

            // Thread 2: Reads the field after object construction finishes
            Thread t2 = new Thread(() =>
            {
                startSignal.Wait();
                
                while(helperObject is not HelperObject)
                {
                    Thread.SpinWait(1);
                }
                
                observedData = helperObject.getData();
            });

            t1.Start(); t2.Start();
            startSignal.Set(); // Release both threads at once
            t1.Join(); t2.Join();

            // If readonly semantics failed, observedData should be 0
            Assert.Equal(1, observedData);
        }
    }


    private class HelperObject
    {
        private readonly int _data;
            
        public HelperObject(int data)
        {
            _data = data;
        }

        public int getData()
        {
            return this._data;
        }
        
    }
    
}