using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace MemoryModelTests.dvhTest;

public class ReadOnlyPropertyExample
{
    ITestOutputHelper _testOutputHelper;
    public ReadOnlyPropertyExample(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }
    public class Settings
    {
        public Settings(string theme, int fontSize, bool bold)
        {
            Theme = theme;
            FontSize = fontSize;
            Bold = bold;
        }

        public string Theme { get; }
        public bool Bold { get; init; }
        public readonly int FontSize;
    }

    // Even a fully private/internal class doesn't stop reflection!
    internal class SecureSettings
    {
        internal SecureSettings(string theme, int fontSize, bool bold)
        {
            _theme = theme;
            _fontSize = fontSize;
            _bold = bold;
        }

        // All private backing fields
        private string _theme;
        private int _fontSize;
        private bool _bold;

        // Only expose via read-only properties
        public string Theme => _theme;
        public int FontSize => _fontSize;
        public bool Bold => _bold;
    }

    // Struct with readonly field (value type)
    public struct ReadOnlyStruct
    {
        public readonly int Value;

        public ReadOnlyStruct(int value)
        {
            Value = value;
        }
    }

    public class Application
    {
        public string Name { get; set; } = "";
        // This property is read-only - it can only be set during construction
        public Settings AppSettings = new("Light", 12, false);
    }

    /*[Fact]
    public static void Example()
    {
        // You can still initialize the nested object's properties
        // even though AppSettings property has no setter
        var app = new Application
        {
            Name = "MyApp",
            //AppSettings = new Settings() { Theme = "Dark", FontSize = 14, Bold = true }
        };
        

        Console.WriteLine($"App: {app.Name}, Theme: {app.AppSettings.Theme}, Font Size: {app.AppSettings.FontSize}");
    }*/

    [Fact]
    public void ReflectionOnGetOnlyProperty()
    {
        // Create a Settings instance - can't set Theme or FontSize in initializer
        var settings = new Settings("Light", 14, true) { Bold = false };

        _testOutputHelper.WriteLine($"Before reflection - Theme: {settings.Theme ?? "null"}, FontSize: {settings.FontSize}");

        // Try to set the get-only property using reflection
        var themeProp = typeof(Settings).GetProperty("Theme");
        var themeField = typeof(Settings).GetField("Theme");
        var backingField = typeof(Settings).GetField("<Theme>k__BackingField",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        _testOutputHelper.WriteLine($"Theme field found: {themeField != null}");
        _testOutputHelper.WriteLine($"Theme property found: {themeProp != null}");
        _testOutputHelper.WriteLine($"Theme backing field found: {backingField != null}");

        if (backingField != null)
        {
            // Set the backing field of the get-only property
            backingField.SetValue(settings, "Dark");
            _testOutputHelper.WriteLine($"Backing Field reflection - Theme: {settings.Theme}");
        }

        if (themeField != null)
        {
            // Set the readonly field
            themeField.SetValue(settings, "Grey");
            _testOutputHelper.WriteLine($"Field reflection - Theme: {settings.Theme}");
        }

        _testOutputHelper.WriteLine($"After reflection - Theme: {settings.Theme}, FontSize: {settings.FontSize}");
    }
    /*
Standard Output: 
Before reflection - Theme: Light, FontSize: 14
Theme field found: False
Theme property found: True
Theme backing field found: True
Backing Field reflection - Theme: Dark
After reflection - Theme: Dark, FontSize: 14

     */

    [Fact]
    public void ReflectionOnInitOnlyProperty()
    {
        var settings = new Settings("Light", 14, true);

        _testOutputHelper.WriteLine($"Before reflection - Bold: {settings.Bold}");

        // Try to set the init-only property using reflection
        var boldProperty = typeof(Settings).GetProperty("Bold");
        var boldField = typeof(Settings).GetField("Bold");
        var backingField = typeof(Settings).GetField("<Bold>k__BackingField",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        _testOutputHelper.WriteLine($"Bold field found: {boldField != null}");
        _testOutputHelper.WriteLine($"Bold property found: {boldProperty != null}");
        _testOutputHelper.WriteLine($"Bold backing field found: {backingField != null}");

        if (backingField != null)
        {
            // Set the backing field of the init-only property
            backingField.SetValue(settings, false);
            _testOutputHelper.WriteLine($"Backing Field reflection - Bold: {settings.Bold}");
        }

        if (boldField != null)
        {
            // Set the field directly
            boldField.SetValue(settings, true);
            _testOutputHelper.WriteLine($"Field reflection - Bold: {settings.Bold}");
        }

        _testOutputHelper.WriteLine($"After reflection - Bold: {settings.Bold}");
    }
    /*
Standard Output: 
Before reflection - Bold: True
Bold field found: False
Bold property found: True
Bold backing field found: True
Backing Field reflection - Bold: False
After reflection - Bold: False
    */

    [Fact]
    public void ReflectionOnReadonlyField()
    {
        var settings = new Settings("Light", 14, true);

        _testOutputHelper.WriteLine($"Before reflection - FontSize: {settings.FontSize}");

        // Try to set the readonly field using reflection
        var fontSizeField = typeof(Settings).GetField("FontSize");
        var fontSizeProperty = typeof(Settings).GetProperty("FontSize");
        var backingField = typeof(Settings).GetField("<FontSize>k__BackingField",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        _testOutputHelper.WriteLine($"FontSize field found: {fontSizeField != null}");
        _testOutputHelper.WriteLine($"FontSize property found: {fontSizeProperty != null}");
        _testOutputHelper.WriteLine($"FontSize backing field found: {backingField != null}");

        _testOutputHelper.WriteLine($"FontSize is readonly: {fontSizeField?.IsInitOnly}");

        if (backingField != null)
        {
            // Set the backing field of the get-only property
            backingField.SetValue(settings, 20);
            _testOutputHelper.WriteLine($"Backing Field reflection - FontSize: {settings.FontSize}");
        }

        if (fontSizeField != null)
        {
            // Set the readonly field
            fontSizeField.SetValue(settings, 16);
            _testOutputHelper.WriteLine($"Field reflection - FontSize: {settings.FontSize}");
        }
    }
    /*
Standard Output: 
Before reflection - FontSize: 14
FontSize field found: True
FontSize property found: False
FontSize backing field found: False
FontSize is readonly: True
Field reflection - FontSize: 16
     */

    [Fact]
    public void ReflectionAllThreeAtOnce()
    {
        var settings = new Settings("Light", 14, true);

        _testOutputHelper.WriteLine($"BEFORE - Theme: {settings.Theme ?? "null"}, FontSize: {settings.FontSize}, Bold: {settings.Bold}");

        // Set get-only property via backing field
        var themeBackingField = typeof(Settings).GetField("<Theme>k__BackingField",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        themeBackingField?.SetValue(settings, "Dark");

        // Set readonly field directly
        var fontSizeField = typeof(Settings).GetField("FontSize");
        fontSizeField?.SetValue(settings, 18);

        // Set init-only property via backing field
        var boldBackingField = typeof(Settings).GetField("<Bold>k__BackingField",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        boldBackingField?.SetValue(settings, false);

        _testOutputHelper.WriteLine($"AFTER  - Theme: {settings.Theme}, FontSize: {settings.FontSize}, Bold: {settings.Bold}");
    }

    [Fact]
    public void ReflectionBypassesPrivateFields()
    {
        // Even with ALL private fields, reflection can still access them!
        var secureSettings = new SecureSettings("Light", 14, true);

        _testOutputHelper.WriteLine($"BEFORE - Theme: {secureSettings.Theme}, FontSize: {secureSettings.FontSize}, Bold: {secureSettings.Bold}");

        // Access PRIVATE backing fields using NonPublic binding flag
        var themeField = typeof(SecureSettings).GetField("_theme",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var fontSizeField = typeof(SecureSettings).GetField("_fontSize",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var boldField = typeof(SecureSettings).GetField("_bold",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        _testOutputHelper.WriteLine($"Private _theme field found: {themeField != null}");
        _testOutputHelper.WriteLine($"Private _fontSize field found: {fontSizeField != null}");
        _testOutputHelper.WriteLine($"Private _bold field found: {boldField != null}");

        // Modify them even though they're private!
        themeField?.SetValue(secureSettings, "Dark");
        fontSizeField?.SetValue(secureSettings, 20);
        boldField?.SetValue(secureSettings, false);

        _testOutputHelper.WriteLine($"AFTER  - Theme: {secureSettings.Theme}, FontSize: {secureSettings.FontSize}, Bold: {secureSettings.Bold}");
        _testOutputHelper.WriteLine("");
        _testOutputHelper.WriteLine("CONCLUSION: private fields provide NO protection against reflection!");
    }

    [Fact]
    public void ReflectionOnStructCreatesACopy()
    {
        // Structs (value types) provide SOME protection - reflection works on copies!
        var readOnlyStruct = new ReadOnlyStruct(42);

        _testOutputHelper.WriteLine($"BEFORE - Value: {readOnlyStruct.Value}");

        var valueField = typeof(ReadOnlyStruct).GetField("Value");
        _testOutputHelper.WriteLine($"Value field found: {valueField != null}");

        if (valueField != null)
        {
            // This modifies a COPY, not the original!
            object boxedStruct = readOnlyStruct; // Boxing creates a copy
            valueField.SetValue(boxedStruct, 99);
            
            _testOutputHelper.WriteLine($"After reflection on copy - Original Value: {readOnlyStruct.Value}");
            _testOutputHelper.WriteLine($"After reflection on copy - Boxed copy Value: {((ReadOnlyStruct)boxedStruct).Value}");
            _testOutputHelper.WriteLine("");
            _testOutputHelper.WriteLine("CONCLUSION: Structs protect via COPIES, not access control!");
        }
    }

    [Fact]
    public void InternalClassStillAccessibleViaReflection()
    {
        // internal modifier prevents other ASSEMBLIES from accessing at compile-time
        // But reflection bypasses this at runtime!
        
        var secureSettings = new SecureSettings("Secret", 16, true);
        
        _testOutputHelper.WriteLine($"BEFORE - Theme: {secureSettings.Theme}");
        
        // Even though it's internal, reflection works
        var themeField = typeof(SecureSettings).GetField("_theme",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        themeField?.SetValue(secureSettings, "Hacked!");
        
        _testOutputHelper.WriteLine($"AFTER  - Theme: {secureSettings.Theme}");
        _testOutputHelper.WriteLine("");
        _testOutputHelper.WriteLine("CONCLUSION: internal provides compile-time protection only!");
    }
}

/*Reality: If code runs in the same process with sufficient trust, reflection can usually bypass most protections. True security requires process isolation or sandboxing.

SUMMARY OF PROTECTION MECHANISMS:
1. Access Modifiers (private, internal, etc.) - Compile-time only, reflection bypasses them
2. Readonly Structs - Reflection works on copies, original protected
3. Separate Assembly - Prevents compile-time access, but not reflection
4. Runtime Security Permissions - Largely obsolete in modern .NET
5. True Protection - Process isolation, sandboxing, or not storing sensitive data

FOR THREAD SAFETY:
- Use lock, Interlocked, or volatile for synchronization
- Access modifiers don't help with threading issues
- Reflection protection is a separate concern from thread safety
*/