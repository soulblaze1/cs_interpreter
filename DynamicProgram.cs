using System;

namespace aaa
{
	class Class2
	{
		public int addf(){return 1;}  //function overloads are selected based on parameter count
		public int addf(int x, int y){return x+y;}

		public static int x;
	}

	class Freaky
	{
		public string str;
		public int i;
		public bool b;
		InterpretedProgram p;

		public string fff()
		{
			p = new InterpretedProgram();
			return str + i + !p.b;
		}

		public Freaky f;
		//static self-referential fields are not implemented.

	}


	class InterpretedProgram
	{

		public bool b=false; //no initializers supported on class variables.

		static bool return_true(int a, int b){ if(new Class2().addf()>0) return true; return false; }
		public static int addxy(int x, int y){ int v = x; v += y; return v; }



		public static int Fibonacci(int number)
		{
			if (number <= 1)
				return 1;
			else
				return Fibonacci(number - 2) + Fibonacci(number - 1);
		}


		//Try pasting the function here and using it:  https://stackoverflow.com/a/8281221




		public static void DynamicMain (string[] args)
		{

			//This is where you can dynamically have the console window behind the IDE and (un)comment lines and hit Ctrl+S to have it change in realtime.
			//Some text editors should have auto-save. With something like Nuklear, you could make your own code editing window perhaps.



			Console.WriteLine ("HEYYEYAAEYAAAEYAEYAA.mp4" + " -> " + return_true(0,0));


			Class2 g = new Class2();

			bool l = return_true(5,1);
			Console.WriteLine (g.addf());
			Console.WriteLine (l);
			Console.WriteLine ("HEYYEYAAEYAAAEYAEYAA.mp4" + " -> " + l);

			Class2 x = new Class2();
			int intv = x.addf(6,0) * 2 / 2;
			bool h = true;
			if((h)) { Console.WriteLine("yooo"); }
			else Console.WriteLine("a");

			Console.WriteLine("int value: "+intv);
			Console.WriteLine("int value 2: "+x.addf());



			Console.WriteLine(addxy(12, 14));

			int ff = 0;
			ff = 9;
			Class2.x = 71;
			Console.WriteLine(ff);
			Console.WriteLine(Class2.x);


			Freaky f = new Freaky();
			f.str = "hhhh";
			f.i = 3;
			f.b = true;
			Console.WriteLine(f.fff());

			//This syntax doesn't work
			/*f.f = new Freaky();
			f.f.str = "hhhh";
			f.f.i = 3;
			f.f.b = true;
			Console.WriteLine(f.f.fff());*/

			Freaky y = new Freaky();
			f.f = y;
			y.f = y;
			y.f.f = new Freaky();
			y.str = "hhhh";
			y.i = 3;
			y.b = true;
			Console.WriteLine(y.fff());



			string[] j = new string[]{"ZERO", "ONE"};
			string k = j[1];
			Console.WriteLine(k);
			Console.WriteLine(new string[]{"ZERO", "ONE"}[0]);


			Console.WriteLine("addxy(9,9) = " + addxy(9,9));


			int fbn_n = 14; //change values to see how it gets slower.
			Console.WriteLine("Fibonacci("+fbn_n+") : " + Fibonacci(fbn_n));

		}











	}




}
