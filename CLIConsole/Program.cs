using System;
using System.Collections.Generic;

using CLIHelper;

namespace CLIConsole
{
	internal class Program
	{
		private enum MyType
		{ bt, xv }

		[Switch("arg1", typeof(MyType), SwitchType.Hyphen, 1)]
		static void Main()
		{
			Console.Title = "CLIHelper Debug Console";

			try
			{
				Dictionary<string, CLI.Value> args = CLI.GetArguments();

				if (args != null) Console.WriteLine(string.Join(", ", args));
			}
			catch (Exception ex) { Console.WriteLine("{0}: {1}", ex.GetType(), ex.Message); }

			Console.ReadKey();
		}
	}
}
