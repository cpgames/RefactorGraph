public class MyClass
{
	public MyClass()
	{
		Func1(10);
	}

	private void Func1(int a)
	{
		int length1 = Func2("a") + Func2("aa") + 5;
		int length2 = Func2("a");
	}

	private int Func2(string b)
    {
		return b.Length;
    }
}
