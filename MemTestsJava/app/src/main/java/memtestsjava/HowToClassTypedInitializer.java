package memtestsjava;

public class HowToClassTypedInitializer {
    
    public static class EmbeddedClassTypeA {
        private int i;
        private boolean b;
        private String s;
        private EmbeddedClassTypeB classB;

        public int getI() { return i; }
        public void setI(int i) { this.i = i; }
        
        public boolean isB() { return b; }
        public void setB(boolean b) { this.b = b; }
        
        public String getS() { return s; }
        public void setS(String s) { this.s = s; }
        
        public EmbeddedClassTypeB getClassB() { return classB; }
        public void setClassB(EmbeddedClassTypeB classB) { this.classB = classB; }

        @Override
        public String toString() {
            return i + "|" + b + "|" + s + "|||" + classB;
        }

        public EmbeddedClassTypeA() {
            System.out.println("Entering EmbeddedClassTypeA constructor. Values are: " + this);
            i = 3;
            b = true;
            s = "abc";
            classB = new EmbeddedClassTypeB();
            classB.setBI(43);
            classB.setBB(true);
            System.out.println("Exiting EmbeddedClassTypeA constructor. Values are: " + this + ")");
        }
    }

    public static class EmbeddedClassTypeB {
        private int bI;
        private boolean bB;
        private String bS;

        public int getBI() { return bI; }
        public void setBI(int bI) { this.bI = bI; }
        
        public boolean isBB() { return bB; }
        public void setBB(boolean bB) { this.bB = bB; }
        
        public String getBS() { return bS; }
        public void setBS(String bS) { this.bS = bS; }

        @Override
        public String toString() {
            return bI + "|" + bB + "|" + bS;
        }

        public EmbeddedClassTypeB() {
            System.out.println("Entering EmbeddedClassTypeB constructor. Values are: " + this);
            bI = 23;
            bB = false;
            bS = "BBBabc";
            System.out.println("Exiting EmbeddedClassTypeB constructor. Values are: " + this + ")");
        }
    }

    public static void main(String[] args) {
        // First instance - modifies the EXISTING ClassB instance created in constructor
        EmbeddedClassTypeA a = new EmbeddedClassTypeA();
        a.setI(103);
        a.setB(false);
        // Note: This modifies the existing ClassB instance created in the constructor
        a.getClassB().setBI(100003);
        System.out.println("After initializing EmbeddedClassTypeA: " + a);

        // Second instance - creates a NEW ClassB instance
        EmbeddedClassTypeA a2 = new EmbeddedClassTypeA();
        a2.setI(103);
        a2.setB(false);
        // Note: This creates a brand new ClassB instance (different from C# first example)
        EmbeddedClassTypeB newClassB = new EmbeddedClassTypeB();
        newClassB.setBI(100003);
        a2.setClassB(newClassB);
        System.out.println("After initializing EmbeddedClassTypeA a2: " + a2);
    }

    /* Expected Output:
    Entering EmbeddedClassTypeA constructor. Values are: 0|false|null|||null
    Entering EmbeddedClassTypeB constructor. Values are: 0|false|null
    Exiting EmbeddedClassTypeB constructor. Values are: 23|false|BBBabc)
    Exiting EmbeddedClassTypeA constructor. Values are: 3|true|abc|||43|true|BBBabc)
    After initializing EmbeddedClassTypeA: 103|false|abc|||100003|true|BBBabc
    Entering EmbeddedClassTypeA constructor. Values are: 0|false|null|||null
    Entering EmbeddedClassTypeB constructor. Values are: 0|false|null
    Exiting EmbeddedClassTypeB constructor. Values are: 23|false|BBBabc)
    Exiting EmbeddedClassTypeA constructor. Values are: 3|true|abc|||43|true|BBBabc)
    Entering EmbeddedClassTypeB constructor. Values are: 0|false|null
    Exiting EmbeddedClassTypeB constructor. Values are: 23|false|BBBabc)
    After initializing EmbeddedClassTypeA a2: 103|false|abc|||100003|false|BBBabc
    */
}
