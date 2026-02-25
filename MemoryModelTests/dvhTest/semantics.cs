using System;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace MemoryModelTests.dvhTest;

public class semantics
{
    ITestOutputHelper _testOutputHelper;
    public semantics(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    int _a;
    volatile int _b;
    int _c;

    [Fact]
    public void Reordering_Observation_Test()
    {
        // Results storage
        var observedOrders = new HashSet<string>();
        var iterations = 100_000;

        for (int i = 0; i < iterations; i++)
        {
            _a = 0;
            _b = 1;
            _c = 2;

            var t = new Thread(() =>
            {
                // Randomize writes to increase chance of reorderings
                _a = 10;
                _b = 20;
                _c = 30;
            });

            t.Start();

            // Reader thread
            int a = _a;
            int b = _b;
            int c = _c;

            string order = $"{a},{b},{c}";
            observedOrders.Add(order);
            t.Join();

        }

        // Print observed orders for debugging
        foreach (var order in observedOrders)
            _testOutputHelper.WriteLine(order);

        // You may need to adjust these assertions based on what you observe
        Assert.Contains("10,20,30", observedOrders); // All updated
        Assert.Contains("0,1,2", observedOrders);    // All initial
        // You may see mixed orders due to reordering
    }
}