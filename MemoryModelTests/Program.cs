using MemoryModelTests.Readonly;

namespace MemoryModelTests;

internal class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Starting tests");
        var readonlyImmutable = new ReadonlyImmutable();
        readonlyImmutable.Test_Readonly_With_Barrier();
        Console.WriteLine("Finished tests");
    }
}