using System;
using MemoryModelTests.Readonly;
using Xunit.Abstractions;

namespace MemoryModelTests;

class Program
{
    public static void Main(string[] args)
    {
        var readonlyImmutable = new ReadonlyImmutable();
        // readonlyImmutable.Test_Readonly_Publication_Safety();
        readonlyImmutable.Test_Readonly_Store_Load_Semantics();
    }
}
