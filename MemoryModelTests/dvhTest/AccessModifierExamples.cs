using System;
using Xunit;
using Xunit.Abstractions;

namespace MemoryModelTests.dvhTest;

/// <summary>
/// Demonstrates practical use cases for all C# access modifiers
/// </summary>
public class AccessModifierExamples
{
    private readonly ITestOutputHelper _output;
    
    public AccessModifierExamples(ITestOutputHelper output)
    {
        _output = output;
    }

    // ==================================================================================
    // 1. PUBLIC - Accessible from anywhere
    // USE CASE: Public API that anyone can use
    // ==================================================================================
    
    public class BankAccount
    {
        private decimal _balance; // Implementation detail - hide it!
        
        // Public API - this is what users of your class should use
        public decimal Balance => _balance;
        
        public void Deposit(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be positive");
            _balance += amount;
        }
        
        public void Withdraw(decimal amount)
        {
            if (amount > _balance)
                throw new InvalidOperationException("Insufficient funds");
            _balance -= amount;
        }
    }

    [Fact]
    public void PublicMembers_AccessibleFromAnywhere()
    {
        var account = new BankAccount();
        account.Deposit(100); // Public method - anyone can call
        _output.WriteLine($"Balance: {account.Balance}"); // Public property - anyone can read
        
        // account._balance = 1000000; // ❌ COMPILE ERROR - private field not accessible
        // This is GOOD - we control access through public methods with validation
    }

    // ==================================================================================
    // 2. PRIVATE - Only accessible within the same class
    // USE CASE: Implementation details that should be hidden
    // ==================================================================================
    
    public class PasswordValidator
    {
        // Public API - what users call
        public bool IsValid(string password)
        {
            return HasMinimumLength(password) && 
                   HasUpperCase(password) && 
                   HasLowerCase(password) &&
                   HasDigit(password);
        }
        
        // Private helpers - implementation details users don't need to know about
        private bool HasMinimumLength(string password) => password.Length >= 8;
        private bool HasUpperCase(string password) => password.Any(char.IsUpper);
        private bool HasLowerCase(string password) => password.Any(char.IsLower);
        private bool HasDigit(string password) => password.Any(char.IsDigit);
        
        // If these were public, users might call them directly and misuse them
        // By keeping them private, we can change implementation without breaking anyone's code
    }

    [Fact]
    public void PrivateMembers_OnlyAccessibleWithinClass()
    {
        var validator = new PasswordValidator();
        bool result = validator.IsValid("Password123"); // ✅ Can call public method
        _output.WriteLine($"Valid: {result}");
        
        // validator.HasMinimumLength("test"); // ❌ COMPILE ERROR - private method
    }

    // ==================================================================================
    // 3. PROTECTED - Accessible in same class AND derived classes
    // USE CASE: Extension points for inheritance
    // ==================================================================================
    
    public class Vehicle
    {
        private string _vin; // Private - cannot be accessed by derived classes
        
        protected int _currentSpeed; // Protected - derived classes can access
        protected int _maxSpeed;     // Protected - derived classes can access
        
        public Vehicle(string vin, int maxSpeed)
        {
            _vin = vin;
            _maxSpeed = maxSpeed;
            _currentSpeed = 0;
        }
        
        // Protected method - derived classes can override or call
        protected virtual void OnSpeedChanged()
        {
            // Base implementation - can be overridden
            Console.WriteLine($"Speed changed to {_currentSpeed}");
        }
        
        public void Accelerate(int amount)
        {
            _currentSpeed = Math.Min(_currentSpeed + amount, _maxSpeed);
            OnSpeedChanged(); // Calls protected method
        }
    }
    
    public class ElectricCar : Vehicle
    {
        private int _batteryLevel = 100;
        
        public ElectricCar(string vin) : base(vin, maxSpeed: 120) { }
        
        // Can override protected method from base class
        protected override void OnSpeedChanged()
        {
            base.OnSpeedChanged();
            
            // Derived class can access protected fields
            if (_currentSpeed > 0)
                _batteryLevel -= _currentSpeed / 10;
            
            Console.WriteLine($"Battery: {_batteryLevel}%");
        }
        
        public void ShowSpeed()
        {
            // Can access protected fields from base class
            Console.WriteLine($"Current speed: {_currentSpeed}/{_maxSpeed}");
            
            // Cannot access private fields from base class
            // Console.WriteLine(_vin); // ❌ COMPILE ERROR
        }
    }

    [Fact]
    public void ProtectedMembers_AccessibleInDerivedClasses()
    {
        var car = new ElectricCar("VIN123");
        car.Accelerate(50); // Public method
        car.ShowSpeed();
        
        // car._currentSpeed = 200; // ❌ COMPILE ERROR - protected, not accessible outside class hierarchy
        // car.OnSpeedChanged(); // ❌ COMPILE ERROR - protected method
    }

    // ==================================================================================
    // 4. INTERNAL - Accessible within the same assembly only
    // USE CASE: Implementation shared across classes in your library, but not exposed to users
    // ==================================================================================
    
    // Imagine this is a library you're building. Internal classes/members are for YOUR use
    // within the library, but won't be visible to people who reference your DLL
    
    internal class DatabaseConnection
    {
        internal string ConnectionString { get; set; }
        
        internal void Connect()
        {
            Console.WriteLine("Connecting to database...");
        }
    }
    
    public class UserRepository
    {
        // This class is public (part of your API)
        // But it uses internal helper classes that users of your library shouldn't see
        
        private DatabaseConnection _connection = new(); // Can use internal class within same assembly
        
        public void SaveUser(string username)
        {
            _connection.Connect(); // Can call internal method
            Console.WriteLine($"Saving user: {username}");
        }
    }

    [Fact]
    public void InternalMembers_OnlyWithinSameAssembly()
    {
        // Within this assembly, we can use internal classes
        var connection = new DatabaseConnection();
        connection.ConnectionString = "server=localhost";
        
        // But if someone else references your DLL, they can't see internal classes
        // This lets you share implementation details across YOUR classes without exposing them
        
        var repo = new UserRepository();
        repo.SaveUser("Alice"); // ✅ Public method works
    }

    // ==================================================================================
    // 5. PROTECTED INTERNAL - Accessible in same assembly OR derived classes in other assemblies
    // USE CASE: Extension points that are also usable within your assembly
    // ==================================================================================
    
    public class BaseLogger
    {
        // protected internal = "protected OR internal"
        // Accessible by:
        // 1. Derived classes (even in other assemblies)
        // 2. Any class in the same assembly
        
        protected internal virtual void WriteLog(string message, string level)
        {
            Console.WriteLine($"[{level}] {DateTime.Now}: {message}");
        }
        
        public void LogInfo(string message) => WriteLog(message, "INFO");
        public void LogError(string message) => WriteLog(message, "ERROR");
    }
    
    // Another class in SAME assembly can access protected internal
    public class LogAnalyzer
    {
        public void TestLog(BaseLogger logger)
        {
            // Can access protected internal method because we're in same assembly
            logger.WriteLog("Test", "DEBUG");
        }
    }
    
    // Derived class can also access it (even from different assembly)
    public class FileLogger : BaseLogger
    {
        /*protected override void WriteLog(string message, string level)
        {
            // Override to write to file
            System.IO.File.AppendAllText("log.txt", $"[{level}] {message}\n");
            base.WriteLog(message, level);
        }*/
    }

    [Fact]
    public void ProtectedInternalMembers_AccessibleInSameAssemblyOrDerivedClass()
    {
        var logger = new BaseLogger();
        logger.LogInfo("Hello"); // ✅ Public method
        
        // In same assembly, can access protected internal
        logger.WriteLog("Direct call", "DEBUG"); // ✅ Works (we're in same assembly)
        
        var analyzer = new LogAnalyzer();
        analyzer.TestLog(logger); // ✅ Works
        
        // If this code were in DIFFERENT assembly and not derived class, would fail
    }

    // ==================================================================================
    // 6. PRIVATE PROTECTED - Accessible in same assembly AND only in derived classes
    // USE CASE: Very specific - extension points that should only be used by derived classes
    //           in the same assembly (rare, but useful for framework design)
    // ==================================================================================
    
    public class BaseComponent
    {
        // private protected = "private AND protected"
        // Accessible by:
        // Derived classes ONLY if they're in the same assembly
        
        private protected virtual void InitializeInternal()
        {
            Console.WriteLine("Initializing component internals");
        }
        
        public void Initialize()
        {
            InitializeInternal();
            Console.WriteLine("Component ready");
        }
    }
    
    // Derived class in SAME assembly can access it
    public class CustomComponent : BaseComponent
    {
        private protected override void InitializeInternal()
        {
            base.InitializeInternal();
            Console.WriteLine("Custom initialization");
        }
    }
    
    // But a derived class in DIFFERENT assembly CANNOT access private protected members
    // This is useful when you want to allow derivation but control HOW classes can extend

    [Fact]
    public void PrivateProtectedMembers_OnlyDerivedClassesInSameAssembly()
    {
        var component = new CustomComponent();
        component.Initialize(); // ✅ Public method
        
        // component.InitializeInternal(); // ❌ COMPILE ERROR - not accessible outside class
        
        _output.WriteLine("private protected prevents external assemblies from overriding internals");
    }

    // ==================================================================================
    // PRACTICAL DECISION TREE
    // ==================================================================================
    
    [Fact]
    public void AccessModifierDecisionTree()
    {
        _output.WriteLine("=== When to use each access modifier ===\n");
        
        _output.WriteLine("PUBLIC:");
        _output.WriteLine("  - Public API for your class/library");
        _output.WriteLine("  - Methods/properties users should directly call");
        _output.WriteLine("  - Example: BankAccount.Deposit()\n");
        
        _output.WriteLine("PRIVATE:");
        _output.WriteLine("  - Implementation details");
        _output.WriteLine("  - Helper methods only used within the class");
        _output.WriteLine("  - Fields that should be encapsulated");
        _output.WriteLine("  - Example: PasswordValidator.HasMinimumLength()\n");
        
        _output.WriteLine("PROTECTED:");
        _output.WriteLine("  - Extension points for inheritance");
        _output.WriteLine("  - Methods/fields derived classes need to access");
        _output.WriteLine("  - Virtual methods that can be overridden");
        _output.WriteLine("  - Example: Vehicle.OnSpeedChanged()\n");
        
        _output.WriteLine("INTERNAL:");
        _output.WriteLine("  - Shared implementation within your library");
        _output.WriteLine("  - Helper classes users shouldn't see");
        _output.WriteLine("  - Implementation details shared across multiple classes");
        _output.WriteLine("  - Example: DatabaseConnection class\n");
        
        _output.WriteLine("PROTECTED INTERNAL:");
        _output.WriteLine("  - Extension points that internal classes also need");
        _output.WriteLine("  - Allows both inheritance AND internal collaboration");
        _output.WriteLine("  - Example: BaseLogger.WriteLog()\n");
        
        _output.WriteLine("PRIVATE PROTECTED:");
        _output.WriteLine("  - Very specific: derived classes in same assembly only");
        _output.WriteLine("  - Framework design where you want tight control");
        _output.WriteLine("  - Prevents external assemblies from overriding internals");
        _output.WriteLine("  - Example: BaseComponent.InitializeInternal()\n");
    }

    // ==================================================================================
    // REAL WORLD EXAMPLE - Putting it all together
    // ==================================================================================
    
    public class PaymentProcessor
    {
        // PRIVATE - implementation detail
        private readonly ITestOutputHelper _logger;
        private decimal _totalProcessed;
        
        // PROTECTED - derived classes can customize
        protected virtual decimal CalculateFee(decimal amount)
        {
            return amount * 0.029m + 0.30m; // Default: 2.9% + $0.30
        }
        
        // INTERNAL - used by other classes in payment library, but not exposed externally
        internal void RecordTransaction(decimal amount)
        {
            _totalProcessed += amount;
        }
        
        // PUBLIC - main API
        public PaymentProcessor(ITestOutputHelper logger)
        {
            _logger = logger;
        }
        
        public decimal ProcessPayment(decimal amount)
        {
            // Private validation method
            ValidateAmount(amount);
            
            // Protected method - can be overridden
            decimal fee = CalculateFee(amount);
            decimal total = amount + fee;
            
            // Internal method - recording
            RecordTransaction(total);
            
            _logger.WriteLine($"Processed: ${amount}, Fee: ${fee}, Total: ${total}");
            return total;
        }
        
        // PRIVATE - helper
        private void ValidateAmount(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be positive");
        }
    }
    
    public class DiscountPaymentProcessor : PaymentProcessor
    {
        public DiscountPaymentProcessor(ITestOutputHelper logger) : base(logger) { }
        
        // Override PROTECTED method to customize behavior
        protected override decimal CalculateFee(decimal amount)
        {
            // Discounted rate for large amounts
            return amount > 1000 
                ? amount * 0.015m + 0.30m  // 1.5% + $0.30
                : base.CalculateFee(amount);
        }
    }

    [Fact]
    public void RealWorldExample_PaymentProcessing()
    {
        _output.WriteLine("=== Standard Processor ===");
        var standard = new PaymentProcessor(_output);
        standard.ProcessPayment(100);
        
        _output.WriteLine("\n=== Discount Processor ===");
        var discount = new DiscountPaymentProcessor(_output);
        discount.ProcessPayment(100);
        discount.ProcessPayment(2000); // Gets discounted fee
        
        _output.WriteLine("\n=== Access Summary ===");
        _output.WriteLine("✅ public ProcessPayment() - Anyone can call");
        _output.WriteLine("✅ protected CalculateFee() - Derived classes can override");
        _output.WriteLine("✅ internal RecordTransaction() - Other payment classes can use");
        _output.WriteLine("✅ private ValidateAmount() - Only within PaymentProcessor");
    }
}
