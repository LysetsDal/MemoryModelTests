using MemoryModelTests.dvhTest;

var lol = new ctorTest("test", new EmbeddedClassTypeB(101));
Console.WriteLine(lol.ClassB.BI); // 42