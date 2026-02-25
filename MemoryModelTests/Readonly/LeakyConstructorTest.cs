using Xunit;

namespace MemoryModelTests.Readonly;

public class LeakyConstructorTest
{
    public static LeakyObject GlobalInstance;

    private int observedData = -1;

    private Barrier barrier;

    [Fact]
    public void Test_Readonly_Can_Be_Zero_During_Construction()
    {
        for (var i = 0; i < 100_000; i++)
        {
            GlobalInstance = null;
            observedData = -1;

            barrier = new Barrier(2);
            
            // Thread 1: Constructs the object
            var t1 = new Thread(() =>
            {
                barrier.SignalAndWait();
                GlobalInstance = new LeakyObject(42);
            });

            // Thread 2: Tries to "sneak in" as soon as the reference is visible
            var t2 = new Thread(() =>
            {
                barrier.SignalAndWait();
                // Poll until the reference is leaked by the constructor
                while (GlobalInstance == null)
                {
                }

                // Read the readonly field before the constructor might be finished
                observedData = GlobalInstance.Data;
            });

            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();

            // If we catch it, observedData will be 0
            if (observedData == 0)
                // FAILURE FOUND: We saw the default value of a readonly field!
                Assert.Fail($"FAILURE: Readonly field was 0 at iteration {i}");
        }
    }

    public class LeakyObject
    {
        public readonly int Data;

        public LeakyObject(int value)
        {
            // VIOLATION: Publish 'this' before the field is set
            GlobalInstance = this;

            Data = value;
        }
    }
}