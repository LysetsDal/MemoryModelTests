using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace MemoryModelTests.dvhTest
{
    public class refVsNonRef
    {
        ITestOutputHelper _testOutputHelper;
        public refVsNonRef(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void test()
        {

            // Initialize a and b
            int a = 10, b = 12;

            // Display initial values
            _testOutputHelper.WriteLine("Initial value of a is {0}", a);
            _testOutputHelper.WriteLine("Initial value of b is {0}", b);
            _testOutputHelper.WriteLine("");

            // Call addValue method by value
            addValue(a);

            // Display modified value of a
            _testOutputHelper.WriteLine("Value of a after addition" +
                                  " operation is {0}", a);

            // Call subtractValue method by ref
            subtractValue(ref b);

            // Display modified value of b
            _testOutputHelper.WriteLine("Value of b after " +
                "subtraction operation is {0}", b);
        }

        // Define addValue
        // Parameters passed by value
        public static void addValue(int a)
        {
            a += 10;
        }

        // Define subtractValue
        // Parameters passed by ref
        public static void subtractValue(ref int b)
        {
            b -= 5;
        }

        // ========================================================================================
        // MULTITHREADING EXAMPLES - Showing ref does NOT provide thread safety
        // ========================================================================================

        [Fact]
        public void RefDoesNotProvideAtomicity()
        {
            int counter = 0;
            const int iterations = 10000;

            // Using ref does NOT make this thread-safe!
            var tasks = new Task[2];
            tasks[0] = Task.Run(() => IncrementWithRef(ref counter, iterations));
            tasks[1] = Task.Run(() => IncrementWithRef(ref counter, iterations));

            Task.WaitAll(tasks);

            _testOutputHelper.WriteLine($"Expected: {iterations * 2}");
            _testOutputHelper.WriteLine($"Actual: {counter}");
            _testOutputHelper.WriteLine($"Lost updates: {(iterations * 2) - counter}");
            
            // counter will likely be LESS than 20000 due to race conditions!
            // ref doesn't help with thread safety at all
        }

        static void IncrementWithRef(ref int value, int iterations)
        {
            for (int i = 0; i < iterations; i++)
            {
                value++; // NOT ATOMIC! Read-Modify-Write race condition
            }
        }

        [Fact]
        public void VolatileDoesNotProvideAtomicity()
        {
            var state = new VolatileState();
            const int iterations = 10000;

            // volatile ensures visibility but NOT atomicity!
            var tasks = new Task[2];
            tasks[0] = Task.Run(() => state.IncrementVolatile(iterations));
            tasks[1] = Task.Run(() => state.IncrementVolatile(iterations));

            Task.WaitAll(tasks);

            _testOutputHelper.WriteLine($"Expected: {iterations * 2}");
            _testOutputHelper.WriteLine($"Actual (volatile): {state.VolatileCounter}");
            _testOutputHelper.WriteLine($"Lost updates: {(iterations * 2) - state.VolatileCounter}");
            
            // Still loses updates! volatile != atomic
        }

        [Fact]
        public void InterlockedProvidesAtomicity()
        {
            int counter = 0;
            const int iterations = 10000;

            // Interlocked provides TRUE atomicity
            var tasks = new Task[2];
            tasks[0] = Task.Run(() =>
            {
                for (int i = 0; i < iterations; i++)
                    Interlocked.Increment(ref counter);
            });
            tasks[1] = Task.Run(() =>
            {
                for (int i = 0; i < iterations; i++)
                    Interlocked.Increment(ref counter);
            });

            Task.WaitAll(tasks);

            _testOutputHelper.WriteLine($"Expected: {iterations * 2}");
            _testOutputHelper.WriteLine($"Actual (Interlocked): {counter}");
            _testOutputHelper.WriteLine($"Lost updates: {(iterations * 2) - counter}");
            
            // Should be exactly 20000 - NO lost updates!
        }

        [Fact]
        public void LockProvidesAtomicity()
        {
            int counter = 0;
            object lockObj = new object();
            const int iterations = 10000;

            // lock provides atomicity and visibility
            var tasks = new Task[2];
            tasks[0] = Task.Run(() =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    lock (lockObj)
                    {
                        counter++;
                    }
                }
            });
            tasks[1] = Task.Run(() =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    lock (lockObj)
                    {
                        counter++;
                    }
                }
            });

            Task.WaitAll(tasks);

            _testOutputHelper.WriteLine($"Expected: {iterations * 2}");
            _testOutputHelper.WriteLine($"Actual (lock): {counter}");
            _testOutputHelper.WriteLine($"Lost updates: {(iterations * 2) - counter}");
            
            // Should be exactly 20000 - NO lost updates!
        }

        [Fact]
        public void CompareVolatileVsNonVolatileVisibility()
        {
            var state = new VolatileState();
            bool continueRunning = true;

            // Thread 1: Waits for flag to change
            var reader = Task.Run(() =>
            {
                _testOutputHelper.WriteLine("Reader: Waiting for non-volatile flag...");
                while (!state.NonVolatileFlag) 
                { 
                    // May loop forever due to CPU caching!
                    // Compiler/CPU might optimize this to: if (!flag) while(true) {}
                }
                _testOutputHelper.WriteLine("Reader: Non-volatile flag changed!");

                _testOutputHelper.WriteLine("Reader: Waiting for volatile flag...");
                while (!state.VolatileFlag) 
                { 
                    // Will see the change reliably
                }
                _testOutputHelper.WriteLine("Reader: Volatile flag changed!");
            });

            // Thread 2: Sets flags after delay
            var writer = Task.Run(() =>
            {
                Thread.Sleep(100);
                _testOutputHelper.WriteLine("Writer: Setting non-volatile flag");
                state.NonVolatileFlag = true;

                Thread.Sleep(100);
                _testOutputHelper.WriteLine("Writer: Setting volatile flag");
                state.VolatileFlag = true;
            });

            Task.WaitAll(reader, writer);
        }

        class VolatileState
        {
            public int VolatileCounter;
            public bool NonVolatileFlag;
            public volatile bool VolatileFlag;

            public void IncrementVolatile(int iterations)
            {
                for (int i = 0; i < iterations; i++)
                {
                    VolatileCounter++; // Still not atomic!
                }
            }
        }
    }
}
