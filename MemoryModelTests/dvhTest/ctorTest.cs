using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryModelTests.dvhTest
{
    public class ctorTest
    {
        public string S { get; set; }
        public EmbeddedClassTypeB ClassB { get; set; }
        public ctorTest(string s, EmbeddedClassTypeB classB)
        {
            S = s;
            ClassB = classB;
        }
    }
    public class EmbeddedClassTypeB
    {
        public readonly int BI;

        public EmbeddedClassTypeB()
        {
                
        }
        public EmbeddedClassTypeB(int bi)
        {
            BI = bi;
        }
    }
}
