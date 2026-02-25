using MemoryModelTests.dvhTest;

var t1 = new Thread(() =>
{
    var t3 = new Test3();
    t3.PrintValue();
});
var t2 = new Thread(() => {
    var t3 = new Test3();
    t3.PrintValue();
});