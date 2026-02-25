package memtestsjava;

import java.util.HashSet;
import java.util.Set;

import static org.junit.jupiter.api.Assertions.assertTrue;
import org.junit.jupiter.api.Test;

class ReorderingObservationTest {
    // Shared variables
    int a, c;
    volatile int b; // Volatile to prevent certain reorderings

    @Test
    void reorderingObservation() throws InterruptedException {
        Set<String> observedOrders = new HashSet<>();
        int iterations = 100_000;

        for (int i = 0; i < iterations; i++) {
            a = 0;
            b = 1;
            c = 2;

            Thread t = new Thread(() -> {
                a = 10;
                b = 20;
                c = 30;
            });

            t.start();

            // Reader thread (main thread)
            int localA = a;
            int localB = b;
            int localC = c;

            String order = localA + "," + localB + "," + localC;
            observedOrders.add(order);

            t.join();
        }

        // Print observed orders
        observedOrders.forEach(System.out::println);

        // Assert that both initial and updated values are observed
        assertTrue(observedOrders.contains("0,1,2"), "Should observe initial values");
        assertTrue(observedOrders.contains("10,20,30"), "Should observe updated values");
    }
}
