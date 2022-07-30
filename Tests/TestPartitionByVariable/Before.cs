namespace MyNamespace.Something
{
    public abstract class MyClass<TSomething> : BaseClass, ISomething2
        where TSomething : ISomething1
    {
        public static readonly int MyVariable1 = 10;
        bool MyVar3;

        public MyClass()
        {
            string myVar3 = "test";
        }
    }
}