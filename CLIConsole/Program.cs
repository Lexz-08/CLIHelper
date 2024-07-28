using System;
using System.Collections.ObjectModel;
using System.Linq;

using CLIHelper;

namespace CLIConsole
{
	internal class Program
	{
		[Switch("arg1", typeof(SwitchType), SwitchType.Hyphen, 1, "Description of first argument")]
		[Switch("arg2", typeof(SwitchType), SwitchType.Hyphen, 2, "Description of second argument")]
		[Switch("arg3", typeof(ConsoleColor), SwitchType.Hyphen, 3, "Description of third argument")]
		[Switch("arg4", typeof(ConsoleColor), SwitchType.Hyphen, 4, "Description of fourth argument")]
		[Argument("arg5", CLIType.String, 5, "Description of fifth argument")]
		[OptionalSwitch("arg6", typeof(SwitchType), SwitchType.Hyphen, "Description of sixth argument")]
		[OptionalSwitch("arg7", typeof(SwitchType), SwitchType.Hyphen, "Description of seventh argument")]
		[OptionalSwitch("arg8", typeof(ConsoleColor), SwitchType.Hyphen, "Description of eighth argument")]
		[OptionalSwitch("arg9", typeof(ConsoleColor), SwitchType.Hyphen, "Description of nineth argument")]
		[OptionalArgument("arg10", CLIType.Number, "Description of tenth argument")]
		static void Main()
		{
			Console.Title = "CLIHelper Debug Console";

			try
			{
				ReadOnlyDictionary<string, CLI.Value> args = CLI.GetArguments();

				if (args != null) Console.WriteLine(string.Join(", ", args.Select(kv => $"{kv.Key}: {kv.Value}")));
			}
			catch (Exception ex) { Console.WriteLine("{0}: {1}", ex.GetType(), ex.Message); }

			Console.ReadKey();
		}
	}
}
