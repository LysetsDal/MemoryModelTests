using Xunit;
using Xunit.Abstractions;

namespace MemoryModelTests.Readonly;

public class ReadonlyImmutable()
{
    [Fact]
    public void Test_Readonly_Publication_Safety()
    {
        for (int i = 0; i < 100_000; i++)
        {
            HelperObject helperObject = null;
            int observedData = -1;
            
            var startSignal = new ManualResetEventSlim(false);

            // Thread 1: Constructs the class with the readonly field
            Thread t1 = new Thread(() =>
            {
                startSignal.Wait();
                var obj = new HelperObject(1);
                helperObject = obj;
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
    
    [Fact]
    public void Test_Readonly_Store_Load_Semantics()
    {
        for (int i = 0; i < 100_000; i++)
        {
            HelperObject helperObject1 = null;
            HelperObject helperObject2 = null;
            int observedData1 = -1;
            int observedData2 = -1;
            
            var startSignal = new ManualResetEventSlim(false);

            // Thread 1: Constructs the class with the readonly field
            Thread t1 = new Thread(() =>
            {
                startSignal.Wait();
                helperObject1 = new HelperObject(1);
                
                while(helperObject2 == null)
                {
                    Thread.SpinWait(1);
                }
                
                observedData1 = helperObject2.getData();
            });

            // Thread 2: Reads the field after object construction finishes
            Thread t2 = new Thread(() =>
            {
                startSignal.Wait();
                helperObject2 = new HelperObject(2);
                
                while(helperObject1 == null)
                {
                    Thread.SpinWait(1);
                }
                
                observedData2 = helperObject1.getData();

            });

            t1.Start(); t2.Start();
            startSignal.Set(); // Release both threads at once
            t1.Join(); t2.Join();

            // If readonly semantics failed, observedData should be 0
            Assert.Equal(2, observedData1);
            Assert.Equal(1, observedData2);
        }
    }
    

    private class HelperObject
    {
        private int _data;
        // private readonly int _data;
            
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