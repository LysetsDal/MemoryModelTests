# Assembly Structure and Thread Safety Guide

## Understanding Assemblies

### What is an Assembly?
An **assembly** is a compiled .dll or .exe file. It's the **deployment unit** in .NET.

```
Your Current Solution:
MemoryModelTests.sln
└── MemoryModelTests.csproj  ← Compiles to ONE assembly: MemoryModelTests.dll
    ├── namespace MemoryModelTests.dvhTest
    │   ├── ReadOnlyPropertyExample.cs
    │   ├── refVsNonRef.cs
    │   └── other test files
    ├── namespace MemoryModelTests.Volatiles
    │   └── various classes
    └── namespace MemoryModelTests.Utils
        └── utility classes

Everything above is in THE SAME ASSEMBLY!
```

### Namespace vs Assembly
- **Namespace**: Just organizational naming (like folders in your mind)
- **Assembly**: Physical file boundary (.dll or .exe)
- **Multiple namespaces can exist in one assembly**

## How to Separate Into Different Assemblies

### Option 1: Create a Class Library Project

```
MemoryModelTests.sln
├── MemoryModelTests.Core.csproj  ← New class library (MemoryModelTests.Core.dll)
│   └── Settings.cs
│   └── SecureSettings.cs
│   └── Your production code
│
└── MemoryModelTests.csproj  ← Test project (MemoryModelTests.dll)
    └── Project Reference → MemoryModelTests.Core
    └── Your test files
```

### Steps to Create Separate Assembly:

1. **Add new project:**
   ```powershell
   dotnet new classlib -n MemoryModelTests.Core
   ```

2. **Move your production classes** (Settings, etc.) to the new project

3. **Add project reference** from test project to core project:
   ```powershell
   dotnet add MemoryModelTests\MemoryModelTests.csproj reference MemoryModelTests.Core\MemoryModelTests.Core.csproj
   ```

4. **Now you have TWO assemblies:**
   - `MemoryModelTests.Core.dll` - Your production code
   - `MemoryModelTests.dll` - Your tests

## What Access Modifiers Actually Protect

### Compile-Time Protection

| Modifier | Same Assembly | Different Assembly |
|----------|---------------|-------------------|
| `public` | ✅ Accessible | ✅ Accessible |
| `internal` | ✅ Accessible | ❌ Not accessible |
| `private` | ✅ Within class only | ❌ Not accessible |

### Runtime Protection (Reflection)

| Modifier | Reflection from Same Assembly | Reflection from Different Assembly |
|----------|------------------------------|-----------------------------------|
| `public` | ✅ Accessible | ✅ Accessible |
| `internal` | ✅ Accessible with BindingFlags.NonPublic | ✅ Accessible with BindingFlags.NonPublic |
| `private` | ✅ Accessible with BindingFlags.NonPublic | ✅ Accessible with BindingFlags.NonPublic |

**CONCLUSION: Reflection bypasses ALL access modifiers!**

## Thread Safety ≠ Reflection Protection

### These are TWO SEPARATE CONCERNS:

#### 1. **Thread Safety** - Preventing race conditions
```csharp
public class ThreadSafeCounter
{
    private int _count;
    private readonly object _lock = new object();

    public void Increment()
    {
        lock (_lock)
        {
            _count++;
        }
    }

    public int GetCount()
    {
        lock (_lock)
        {
            return _count;
        }
    }
}
```
- Uses `lock`, `Interlocked`, or `volatile` for synchronization
- Prevents multiple threads from corrupting data
- **Access modifiers are irrelevant to thread safety!**

#### 2. **Reflection Protection** - Preventing unauthorized access
```csharp
// This DOESN'T work - reflection bypasses it!
public class "Secure"Settings  // ❌ Not actually secure
{
    private string _apiKey = "secret";  // ❌ Reflection can read this
}
```

## Proper Approaches to Security

### 1. **Don't Store Sensitive Data in Memory Long-Term**
```csharp
// BAD: Storing password in memory
string password = "MyPassword123";

// BETTER: Use SecureString (though still not perfect)
SecureString securePassword = new SecureString();
// Or better: Don't store it at all, use authentication tokens
```

### 2. **Validate at Boundaries**
```csharp
public class Settings
{
    private int _fontSize;

    public int FontSize 
    { 
        get => _fontSize;
        set 
        {
            if (value < 8 || value > 72)
                throw new ArgumentOutOfRangeException();
            _fontSize = value;
        }
    }
}
```
- Even if reflection bypasses the property setter
- Your business logic validates at usage points

### 3. **Use Immutable Objects**
```csharp
public class ImmutableSettings
{
    public ImmutableSettings(string theme, int fontSize)
    {
        Theme = theme;
        FontSize = fontSize;
    }

    public string Theme { get; }
    public int FontSize { get; }

    // To "change", create a new instance
    public ImmutableSettings WithTheme(string newTheme)
        => new ImmutableSettings(newTheme, FontSize);
}
```
- Reflection can still modify backing fields
- But defensively copy when passing to untrusted code

### 4. **Struct-Based Protection** (Limited)
```csharp
public readonly struct SecureValue
{
    public readonly int Value;

    public SecureValue(int value) => Value = value;
}
```
- Reflection works on **copies** of structs
- Original remains unchanged
- Only works for value types

### 5. **Process Isolation** (True Security)
- Run untrusted code in separate process
- Use sandboxing (containers, virtual machines)
- Operating system enforces boundaries

## Practical Recommendation for Your Case

### For Thread-Safe Classes:

```csharp
// In MemoryModelTests.Core.dll (separate assembly)
public class ThreadSafeSettings
{
    private string _theme;
    private int _fontSize;
    private readonly object _lock = new object();

    public string Theme
    {
        get 
        { 
            lock (_lock) return _theme; 
        }
        set 
        { 
            lock (_lock) _theme = value; 
        }
    }

    public int FontSize
    {
        get 
        { 
            lock (_lock) return _fontSize; 
        }
        set 
        { 
            lock (_lock) _fontSize = value; 
        }
    }
}
```

**Key Points:**
- Use `lock` for thread safety (or `Interlocked` for simple operations)
- Don't worry about reflection from your own test code
- In production, assume malicious code can use reflection
- Design defensively: validate, use immutability, process isolation for truly sensitive data

## Testing Thread Safety

Your tests should focus on:
1. **Correctness** - No lost updates under concurrent access
2. **Atomicity** - Operations complete as a unit
3. **Visibility** - Changes visible across threads
4. **Deadlock prevention** - No circular waits

**Not:**
- Trying to prevent reflection (impossible in same process)
- Over-using access modifiers for "security"

## Summary

1. **Assembly** = .dll/.exe file (compilation unit)
2. **Namespace** = organizational only
3. **Same assembly** = everything in one .csproj
4. **Different assembly** = multiple .csproj files
5. **Access modifiers** protect at compile-time only
6. **Reflection** bypasses all access modifiers
7. **Thread safety** ≠ reflection protection (separate concerns!)
8. **True security** requires process isolation or not storing sensitive data

For your thread-safe class: Focus on proper synchronization (lock, Interlocked, volatile), not on preventing reflection from your own tests.
