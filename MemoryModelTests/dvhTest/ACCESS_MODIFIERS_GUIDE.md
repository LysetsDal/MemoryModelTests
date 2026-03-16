# C# Access Modifiers - Quick Reference Guide

## Why More Than Just Public and Private?

**Public and Private are great for simple classes**, but as your codebase grows with inheritance, libraries, and multiple classes working together, you need more granular control.

## Visual Overview

```
┌─────────────────────────────────────────────────────────────┐
│ YOUR ASSEMBLY (YourLibrary.dll)                             │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌───────────────────────────────────┐                     │
│  │ public class Vehicle              │  ← Public: Anyone   │
│  ├───────────────────────────────────┤                     │
│  │ private int _engineSize;          │  ← Private: Only Vehicle │
│  │ protected int _speed;             │  ← Protected: Vehicle + derived │
│  │ internal void InternalHelper();   │  ← Internal: This assembly only │
│  │ protected internal void Log();    │  ← Protected OR Internal │
│  │ private protected void Init();    │  ← Protected AND Internal │
│  │ public void Accelerate();         │  ← Public: Anyone │
│  └───────────────────────────────────┘                     │
│           ▲                                                 │
│           │ Can access protected members                   │
│  ┌────────┴──────────────────────────┐                     │
│  │ class Car : Vehicle               │                     │
│  └───────────────────────────────────┘                     │
│                                                             │
│  ┌───────────────────────────────────┐                     │
│  │ internal class DatabaseHelper     │  ← Internal class   │
│  └───────────────────────────────────┘                     │
│                                                             │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│ OTHER ASSEMBLY (ConsumerApp.dll)                            │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  Can access:          Cannot access:                       │
│  ✅ public members     ❌ private                           │
│  ✅ protected (if      ❌ internal                          │
│     inheriting)       ❌ private protected                 │
│                       ✅ protected internal (if inheriting) │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

## Access Level Summary Table

| Modifier | Same Class | Derived (Same Assembly) | Other (Same Assembly) | Derived (Other Assembly) | Other (Other Assembly) |
|----------|------------|------------------------|---------------------|-------------------------|----------------------|
| `public` | ✅ | ✅ | ✅ | ✅ | ✅ |
| `protected` | ✅ | ✅ | ❌ | ✅ | ❌ |
| `internal` | ✅ | ✅ | ✅ | ❌ | ❌ |
| `protected internal` | ✅ | ✅ | ✅ | ✅ | ❌ |
| `private protected` | ✅ | ✅ | ❌ | ❌ | ❌ |
| `private` | ✅ | ❌ | ❌ | ❌ | ❌ |

## When to Use Each (Practical Guide)

### 🟢 PUBLIC
**"Everyone can see this"**

```csharp
public class BankAccount
{
    public void Deposit(decimal amount) { } // ← API method
    public decimal Balance { get; }          // ← API property
}
```

**Use when:**
- This is part of your public API
- You want anyone to be able to call it
- You're okay with this being part of your contract (hard to change later)

**Example:** Methods users of your library/class should call directly

---

### 🔵 PRIVATE
**"Only I can see this"**

```csharp
public class BankAccount
{
    private decimal _balance;              // ← Implementation detail
    private void ValidateAmount() { }      // ← Helper method
}
```

**Use when:**
- Implementation details that might change
- Helper methods only this class needs
- Fields you want to encapsulate behind properties
- Default choice for most fields

**Example:** Internal calculations, validation helpers, cached data

---

### 🟡 PROTECTED
**"My children can see this"**

```csharp
public class Vehicle
{
    protected int _speed;                      // ← Derived classes can access
    protected virtual void OnStarting() { }    // ← Override point
}

public class Car : Vehicle
{
    public void Accelerate()
    {
        _speed += 10;  // ✅ Can access protected field
    }
}
```

**Use when:**
- Designing for inheritance
- Extension points (virtual methods)
- Fields/methods derived classes need to access
- You want to allow customization

**Example:** Template Method pattern, framework base classes, event hooks

---

### 🟠 INTERNAL
**"My library can see this"**

```csharp
// In YourLibrary.dll
internal class CacheHelper                     // ← Only this assembly
{
    internal static void Clear() { }
}

public class UserRepository
{
    public void SaveUser(User user)
    {
        CacheHelper.Clear();  // ✅ Same assembly, can use internal class
    }
}

// In ConsumerApp.dll (references YourLibrary.dll)
var helper = new CacheHelper();  // ❌ ERROR - internal class not visible
```

**Use when:**
- Building a library with multiple classes that need to collaborate
- Implementation details shared across your classes, but not external users
- Helper classes that are part of implementation, not API
- Test utilities that should be assembly-private

**Example:** Database connection pools, caching systems, internal utilities

---

### 🟣 PROTECTED INTERNAL
**"My children OR my library can see this"** (OR logic)

```csharp
public class BaseLogger
{
    // Accessible by:
    // 1. Derived classes (even in other assemblies)
    // 2. Any class in this assembly
    protected internal virtual void WriteLog(string msg) { }
}

// In same assembly
public class LogAnalyzer
{
    public void Test(BaseLogger logger)
    {
        logger.WriteLog("test");  // ✅ Same assembly
    }
}

// In different assembly
public class FileLogger : BaseLogger
{
    protected override void WriteLog(string msg) { }  // ✅ Derived class
}
```

**Use when:**
- You want both inheritance extensibility AND internal collaboration
- Framework components that work together internally but are also extensible
- Less common than protected or internal alone

**Example:** Logging frameworks, plugin systems, extensible frameworks

---

### 🔴 PRIVATE PROTECTED
**"Only my children in my library can see this"** (AND logic)

```csharp
public class BaseComponent
{
    // Accessible by:
    // Derived classes ONLY if in same assembly
    private protected virtual void InitializeCore() { }
}

// In SAME assembly
public class MyComponent : BaseComponent
{
    private protected override void InitializeCore() { }  // ✅ Works
}

// In DIFFERENT assembly
public class ExternalComponent : BaseComponent
{
    private protected override void InitializeCore() { }  // ❌ ERROR
}
```

**Use when:**
- Framework design where you want to allow derivation but control HOW
- You need derived classes for implementation but don't want external assemblies to override internals
- Very specific scenarios (rare in everyday code)

**Example:** Internal plugin architecture, sealed abstractions

---

## Decision Flowchart

```
Should this be accessible from outside this class?
│
├─ NO → private
│
└─ YES → Is this for inheritance?
    │
    ├─ YES → Is it also used by non-derived classes in your assembly?
    │   │
    │   ├─ YES → protected internal
    │   │
    │   └─ NO → Should external assemblies be able to override?
    │       │
    │       ├─ YES → protected
    │       │
    │       └─ NO → private protected
    │
    └─ NO → Should it be publicly available?
        │
        ├─ YES → public
        │
        └─ NO → internal
```

## Real-World Analogy

Think of a company:

- **`public`** - Company website (anyone can see)
- **`internal`** - Internal company wiki (only employees)
- **`protected`** - Manager training (managers in any department)
- **`private`** - Your personal notes (only you)
- **`protected internal`** - Leadership resources (managers OR any employee)
- **`private protected`** - Department manager training (managers in your department only)

## Common Patterns

### 1. Repository Pattern
```csharp
// Public interface - your API
public interface IUserRepository
{
    User GetById(int id);
}

// Internal implementation - hidden
internal class SqlUserRepository : IUserRepository
{
    private readonly string _connectionString;  // Private field
    
    internal SqlUserRepository(string connStr)  // Internal constructor
    {
        _connectionString = connStr;
    }
    
    public User GetById(int id)  // Public (interface requirement)
    {
        return QueryDatabase(id);
    }
    
    private User QueryDatabase(int id) { ... }  // Private helper
}
```

### 2. Template Method Pattern
```csharp
public abstract class DataProcessor
{
    // Public entry point
    public void ProcessData()
    {
        LoadData();      // Protected - override
        ValidateData();  // Private - fixed logic
        SaveData();      // Protected - override
    }
    
    protected abstract void LoadData();
    protected abstract void SaveData();
    private void ValidateData() { ... }
}
```

### 3. Builder Pattern with Validation
```csharp
public class PersonBuilder
{
    private string _name;          // Private - encapsulated
    private int _age;              // Private - encapsulated
    
    public PersonBuilder WithName(string name)    // Public - fluent API
    {
        _name = name;
        return this;
    }
    
    public Person Build()                         // Public - fluent API
    {
        Validate();  // Private validation
        return new Person(_name, _age);
    }
    
    private void Validate() { ... }               // Private - implementation
}
```

## Tips

1. **Start with the most restrictive** that works, then widen if needed
   - Default to `private`, expose as needed

2. **`public` is a contract** - hard to change later
   - Think carefully before making something public
   - Consider interfaces to hide implementation

3. **`internal` for libraries** is your friend
   - Share implementation across your classes
   - Keep implementation details hidden from consumers

4. **`protected` for inheritance points** only
   - Don't make everything protected "just in case"
   - Each protected member is an extension point you need to maintain

5. **Favor composition over inheritance** when possible
   - Reduces need for `protected`
   - More flexible, fewer access modifier headaches

## See Also

- Run the `AccessModifierExamples.cs` tests to see working code examples
- Check `ASSEMBLY_AND_THREAD_SAFETY_GUIDE.md` for assembly concepts
