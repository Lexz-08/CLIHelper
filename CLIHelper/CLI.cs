﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CLIHelper
{
	public static class CLI
	{
		/// <summary>
		/// Represents a generic value that can represent a <see langword="string"/>, enum value, or number (<see langword="int"/>, <see langword="float"/>, or <see langword="double"/>).
		/// </summary>
		public readonly struct Value
		{
			private readonly object _v;

			/// <summary>
			/// Creates a new <see cref="Value"/>.
			/// </summary>
			/// <param name="Value">The generic value to be stored.</param>
			public Value(object Value) => _v = Value;

			/// <summary>
			/// Attempts to parse the generic value to the 3 different supported numerical types.
			/// </summary>
			public bool IsNumber()
			{
				return int.TryParse(_v.ToString(), out _) ||
					float.TryParse(_v.ToString(), out _) ||
					double.TryParse(_v.ToString(), out _);
			}

			/// <summary>
			/// Parses this generic value as an enum.
			/// </summary>
			/// <typeparam name="TEnum">The type of enum to be parsed to.</typeparam>
			public TEnum GetEnumValue<TEnum>()
			{
				return (TEnum)Enum.GetValues(typeof(TEnum)).GetValue(Array.IndexOf(Enum.GetNames(typeof(TEnum)), _v.ToString().Substring(1)));
			}

			public override string ToString() => _v.ToString();

			public static implicit operator string(Value Value) => Value._v.ToString();

			public static implicit operator int(Value Value) => int.Parse(Value);

			public static implicit operator float(Value Value) => float.Parse(Value);

			public static implicit operator double(Value Value) => double.Parse(Value);
		}

		/// <summary>
		/// Collects the command-line arguments into a collection of values identifiable by argument names.
		/// </summary>
		/// <param name="ParseArguments">Whether the command-line arguments will be parsed by this method.</param>
		/// <returns>A <see cref="Dictionary{TKey, TValue}"/> where <b>TKey</b> is the argument name and <b>TValue</b> is the command-line argument.</returns>
		public static Dictionary<string, Value> GetArguments()
		{
			Assembly asm = Assembly.GetCallingAssembly();
			
			if (asm.GetTypes().Count(type => type.GetMethods(BindingFlags.NonPublic | BindingFlags.Static).Count() > 0) == 0)
				throw new InvalidOperationException("No possible entry methods found");
			if (asm.GetTypes()
				.Count(type => type.GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
				.Count(mi => mi.CustomAttributes
				.Count(ca => ca.AttributeType.BaseType.BaseType == typeof(CLIAttribute)) > 0) > 0) == 0)
				throw new InvalidOperationException("No marked entry methods found");
			if (asm.GetTypes()
				.Count(type => type.GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
				.Count(mi => mi.CustomAttributes
				.Count(ca => ca.AttributeType.BaseType.BaseType == typeof(CLIAttribute)) > 0) > 1) > 0)
				throw new InvalidOperationException("Multiple marked entry methods found");

			Type[] types = asm.GetTypes()
				.Where(type => type.GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
				.Count(mi => mi.CustomAttributes
				.Count(ca => ca.AttributeType.BaseType.BaseType == typeof(CLIAttribute)) > 0) == 1).ToArray();
			Type entType = types[0];
			MethodInfo[] mis = entType.GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
				.Where(mi => mi.CustomAttributes.Count(ca => ca.AttributeType.BaseType.BaseType == typeof(CLIAttribute)) > 0).ToArray();
			MethodInfo entry = mis[0];
			CLIAttribute[] clis = entry.GetCustomAttributes<CLIAttribute>().ToArray();

			if (clis.Count(cli => cli.GetType().BaseType == typeof(RequiredCLIAttribute) && ((RequiredCLIAttribute)cli).Order < 1) > 0)
				throw new InvalidOperationException("Please begin order of required arguments at 1.");

			RequiredCLIAttribute[] required = clis.OfType<RequiredCLIAttribute>().OrderBy(cli => cli.Order).ToArray();
			OptionalCLIAttribute[] optional = clis.OfType<OptionalCLIAttribute>().ToArray();
			string[] args = Environment.GetCommandLineArgs().Skip(1).ToArray();

			foreach (var grp in required.GroupBy(cli => cli.Order))
				if (grp.Count() > 1) throw new InvalidOperationException("Cannot have required arguments with the same order value.");
			foreach (var grp in required.GroupBy(cli => cli.Order))
				if (grp.Count() > 1) throw new InvalidOperationException("Cannot have optional arguments with the same order value.");

			if (args.Length < required.Length)
			{
				Console.WriteLine("Insufficient arguments provided...");
				return null;
			}
			if ((args.Length > required.Length && optional.Length == 0) ||
				(args.Length > required.Length + optional.Length && optional.Length > 0))
			{
				Console.WriteLine("Too many arguments provided...");
				return null;
			}

			Dictionary<string, Value> arguments = new Dictionary<string, Value>();

			for (int i = 0; i < required.Length; i++)
			{
				Value arg = new Value(args[i]);
				RequiredCLIAttribute cli = required[i];

				if (cli is ArgumentAttribute nmArg)
				{
					if (nmArg.Type == CLIType.String && arg.IsNumber())
					{
						Console.WriteLine("Expected text value, got: '{0}'", arg);
						return null;
					}
					else if (nmArg.Type == CLIType.Number && !arg.IsNumber())
					{
						Console.WriteLine("Expected numerical value, got: '{0}'", arg);
						return null;
					}
				}

				if (cli is SwitchAttribute swArg)
				{
					if (!Enum.IsDefined(swArg.Enum, arg.ToString().Substring(1)))
					{
						Console.WriteLine("Expected switch value, got: '{0}'", arg);
						return null;
					}
					else if ((arg.ToString()[0] == '/' && swArg.Identifier == SwitchType.Hyphen) ||
						(arg.ToString()[0] == '-' && swArg.Identifier == SwitchType.Slash))
					{
						Console.WriteLine("Invalid switch identifier for '{0}', got: '{1}', expected '{2}'",
							cli.Name, arg.ToString()[0], swArg.Identifier == SwitchType.Hyphen ? '-' : '/');
						return null;
					}
				}

				arguments.Add(cli.Name, arg);
			}

			if (optional.Length == 0) return arguments;
			else if (args.Length > required.Length)
			{
				var optArgs = args.Skip(required.Length);
				if (optArgs.Any(arg => !arg.Contains('=')))
				{
					optArgs.Where(arg => !arg.Contains("="));
					Console.WriteLine("Found optional argument without splitting indicator: '{0}'", optArgs.ElementAt(0));
					return null;
				}

				foreach (var (Name, Value) in optArgs.Select(arg => (Name: arg.Split('=')[0], Value: new Value(arg.Split('=')[1]))))
					if (optional.Any(opt => opt.Name.Equals(Name)))
					{
						OptionalCLIAttribute cli = optional.Where(opt => opt.Name.Equals(Name)).First();

						if (cli is OptionalArgumentAttribute nmArg)
						{
							if (nmArg.Type == CLIType.String && Value.IsNumber())
							{
								Console.WriteLine("Expected text value, got: '{0}'", Value);
								return null;
							}
							else if (nmArg.Type == CLIType.Number && !Value.IsNumber())
							{
								Console.WriteLine("Expected numerical value, got: '{0}'", Value);
								return null;
							}
						}

						if (cli is OptionalSwitchAttribute swArg)
						{
							if (!Enum.IsDefined(swArg.Enum, Value.ToString().Substring(1)))
							{
								Console.WriteLine("Expected switch value, got: '{0}'", Value);
								return null;
							}
							else if ((Value.ToString()[0] == '/' && swArg.Identifier == SwitchType.Hyphen) ||
								(Value.ToString()[0] == '-' && swArg.Identifier == SwitchType.Slash))
							{
								Console.WriteLine("Invalid switch identifier for '{0}', got: '{1}', expected '{2}'",
									Name, Value.ToString()[0], swArg.Identifier == SwitchType.Hyphen ? '-' : '/');
								return null;
							}
						}

						arguments.Add(Name, Value);
					}
			}
			else return null;

			return arguments.Count > 0 ? arguments : null;
		}
	}
}
