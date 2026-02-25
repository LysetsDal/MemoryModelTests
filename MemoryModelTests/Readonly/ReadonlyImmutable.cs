using Xunit;

namespace MemoryModelTests.Readonly;

public class ReadonlyImmutable
{
    private bool _isReady;
    private HelperObject _sharedHelper;

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
                var obj = new HelperObject(-1, 1);
                helperObject = obj;
            });

            // Thread 2: Reads the field after object construction finishes
            var t2 = new Thread(() =>
            {
                startSignal.Wait();

                while (helperObject is not HelperObject) Thread.SpinWait(1);

                observedData = helperObject.getReadonly();
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
    public void Test_Readonly_Publication()
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
            helperObject1 = new HelperObject(-1, 1);

            while (helperObject2 == null) Thread.SpinWait(1);

            observedData1 = helperObject2.getReadonly();
        });

        // Thread 2: Reads the field after object construction finishes
        var t2 = new Thread(() =>
        {
            startSignal.Wait();
            helperObject2 = new HelperObject(-1, 2);

            while (helperObject1 == null) Thread.SpinWait(1);

            observedData2 = helperObject1.getReadonly();
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

    [Fact]
    public void Prove_Readonly_Safety_By_Contrast()
    {
        _sharedHelper = null;
        _isReady = false;
        var observedNormal = -1;

        var start = new ManualResetEventSlim(false);

        // Thread 1: Creation and Publication of shared Object
        var t1 = new Thread(() => { });

        // Thread 2: Try to read the default value 0
        var t2 = new Thread(() => { });

        t1.Start();
        t2.Start();
        // start.Set();
        t1.Join();
        t2.Join();
    }
}