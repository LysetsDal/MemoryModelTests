using Xunit;

namespace MemoryModelTests.Readonly;

public class ReadonlyImmutable
{
    [Fact]
    public void Test_Readonly_Publication_Safety()
    {
        for (var i = 0; i < 100_000; i++)
        {
            HelperObject helperObject = null;
            var observedData = -1;

            var startSignal = new ManualResetEventSlim(false);

            // Thread 1: Constructs the class with the readonly field
            var t1 = new Thread(() =>
            {
                startSignal.Wait();
                var obj = new HelperObject(1);
                helperObject = obj;
            });

            // Thread 2: Reads the field after object construction finishes
            var t2 = new Thread(() =>
            {
                startSignal.Wait();

                while (helperObject is not HelperObject) Thread.SpinWait(1);

                observedData = helperObject.getData();
            });

            t1.Start();
            t2.Start();
            startSignal.Set(); // Release both threads at once
            t1.Join();
            t2.Join();

            // If readonly semantics failed, observedData should be 0
            Assert.Equal(1, observedData);
        }
    }

    [Fact]
    public void Test_Readonly_With_Barrier()
    {
        for (var i = 0; i < 100_000; i++)
        {
            HelperObject helperObject = null;
            var observedData = -1;
            var barrier = new Barrier(2);

            // Thread 1: Constructs the class with the readonly field
            var t1 = new Thread(() =>
            {
                barrier.SignalAndWait();
                var obj = new HelperObject(1);
                helperObject = obj;
            });

            // Thread 2: Reads the field after object construction finishes
            var t2 = new Thread(() =>
            {
                barrier.SignalAndWait();
                while (helperObject is not HelperObject) Thread.SpinWait(1);

                observedData = helperObject.getData();
            });

            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();

            // If readonly semantics failed, observedData should be 0
            Assert.Equal(1, observedData);
        }
    }

    [Fact]
    public void Test_Readonly_Store_Load_Semantics()
    {
        for (var i = 0; i < 100_000; i++)
        {
            HelperObject helperObject1 = null;
            HelperObject helperObject2 = null;
            var observedData1 = -1;
            var observedData2 = -1;

            var startSignal = new ManualResetEventSlim(false);

            // Thread 1: Constructs the class with the readonly field
            var t1 = new Thread(() =>
            {
                startSignal.Wait();
                helperObject1 = new HelperObject(1);

                while (helperObject2 == null) Thread.SpinWait(1);

                observedData1 = helperObject2.getData();
            });

            // Thread 2: Reads the field after object construction finishes
            var t2 = new Thread(() =>
            {
                startSignal.Wait();
                helperObject2 = new HelperObject(2);

                while (helperObject1 == null) Thread.SpinWait(1);

                observedData2 = helperObject1.getData();
            });

            t1.Start();
            t2.Start();
            startSignal.Set(); // Release both threads at once
            t1.Join();
            t2.Join();

            // If readonly semantics failed, observedData should be 0
            Assert.Equal(2, observedData1);
            Assert.Equal(1, observedData2);
        }
    }
}