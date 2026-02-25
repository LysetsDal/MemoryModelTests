using MemoryModelTests.Readonly;

namespace MemoryModelTests;

internal class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Starting tests");
        var readonlyImmutable = new ReadonlyImmutable();
        readonlyImmutable.Prove_Readonly_Safety_By_Contrast();
        Console.WriteLine("Finished tests");
    }
}