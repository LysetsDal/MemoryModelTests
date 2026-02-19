using System.ComponentModel.Design.Serialization;
using Xunit;
using Xunit.Abstractions;

namespace MemoryModelTests.Volatiles;

public class AcquireSemantics(ITestOutputHelper testOutputHelper)
{
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;
    
    private int _data_1 = 0;
    private int _data_2 = 0;
    private volatile bool _volatileFlag;


    [Fact]
    public void Test_VolatileRead_Prevents_Subsequent_Load_From_Hoisting()
    {
        for (int i = 0; i < 100_000; i++)
        {
            _data_1 = 0;
            _data_2 = 0;
            _volatileFlag = false;

            // Thread A: The Writer (Release)
            Thread t1 = new Thread(() =>
            {
                _data_1 = 42;
                _data_2 = 69;
                _volatileFlag = true; // Store (Release)
            });
            
            bool failedAcquire = false;
            int observedData = 0;
            // Thread B: The Reader (Acquire)
            Thread t2 = new Thread(() =>
            {
                bool acquireRead = _volatileFlag; // Load (Acquire) 
                observedData = _data_2; 

                if (acquireRead && observedData == 0)
                {
                    failedAcquire = true;
                }
            });

            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();
            
            Assert.False(failedAcquire,
                $"Acquire fence failed on iteration {i}. Read of _data_2 in t2 moved above volatile read of _volatileFlag.");
        }
    }

    [Fact]
    public void Test_VolatileRead_Prevents_Subsequent_Store_Hoisting()
    {
        for (int i = 0; i < 100_000; i++)
        {
            _volatileFlag = false;
            _data_2 = 0;
            bool t1SawStoreEarly = false;

            Thread t1 = new Thread(() =>
            {
                // If t2 hoists the store, t1 might see _data_2 == 420 
                // while _volatileFlag is still false.
                if (_data_2 == 420 && !_volatileFlag)
                {
                    t1SawStoreEarly = true;
                }
                _volatileFlag = true; 
            });

            Thread t2 = new Thread(() =>
            {
                bool acquireRead = _volatileFlag; // Load (Acquire)
                _data_2 = 420;                    // Subsequent Store
            });

            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();

            // If Acquire semantics hold, t2's store to _data_2 CANNOT 
            // happen until it has done the load of _volatileFlag.
            Assert.False(t1SawStoreEarly, "The store moved above the volatile read!");
        }
    }
}
