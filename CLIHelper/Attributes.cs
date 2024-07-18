using System;
using System.Runtime.InteropServices;

namespace CLIHelper
{
	/// <summary>
	/// Specifies the type of value for the cli-argument.
	/// </summary>
	public enum CLIType
	{
		/// <summary>
		/// The argument needs a numerical value: <see langword="int"/>, <see langword="float"/>, or <see langword="double"/>.
		/// </summary>
		Number,

		/// <summary>
		/// The argument needs a string value.
		/// </summary>
		String
	}

	/// <summary>
	/// Specifies the type of character used to identify a switch argument.
	/// </summary>
	public enum SwitchType
	{
		/// <summary>
		/// There must be a hyphen (-) before the switch value.
		/// </summary>
		Hyphen,

		/// <summary>
		/// The must be a forward slash (/) before the switch value.
		/// </summary>
		Slash
	}

	/// <summary>
	/// Base attribute that represents the metadata for a command-line argument.
	/// </summary>
	public abstract class CLIAttribute : Attribute
	{
		/// <summary>
		/// The name of the argument, used to identify the argument's value in code.
		/// </summary>
		public readonly string Name;

		/// <summary>
		/// What the argument is for, or maybe how it's used.
		/// </summary>
		public readonly string Description;

		/// <summary>
		/// Creates a new <see cref="CLIAttribute"/>.
		/// </summary>
		/// <param name="Name">The name of the argument, used to identify the argument's value in code.</param>
		/// <param name="Order">Whether this is the first, second, third, etc, argument.</param>
		/// <param name="Description">What the argument is for, or maybe how it's used.
		/// <br/><br/>OPTIONAL: <i>Does not need to be provided.</i></param>
		public CLIAttribute(string Name, [Optional, DefaultParameterValue("")] string Description)
		{
			this.Name = Name;
			this.Description = Description;
		}
	}

	/// <summary>
	/// Base attribute that represents the metadata for a required command-line argument.
	/// </summary>
	public abstract class RequiredCLIAttribute : CLIAttribute
	{
		/// <summary>
		/// Whether this is the first, second, third, etc, argument.
		/// </summary>
		public readonly int Order;

		/// <summary>
		/// Creates a new <see cref="RequiredCLIAttribute"/>.
		/// </summary>
		/// <param name="Name">The name of the argument, used to identify the argument's value in code.</param>
		/// <param name="Order">Whether this is the first, second, third, etc, argument.</param>
		/// <param name="Description">What the argument is for, or maybe how it's used.
		/// <br/><br/>OPTIONAL: <i>Does not need to be provided.</i></param>
		public RequiredCLIAttribute(string Name, int Order, [Optional, DefaultParameterValue("")] string Description)
			: base(Name, Description) => this.Order = Order;
	}

	/// <summary>
	/// Base attribute that represents the metadata for an optional command-line argument.
	/// </summary>
	public abstract class OptionalCLIAttribute : CLIAttribute
	{
		/// <summary>
		/// Creates a new <see cref="OptionalCLIAttribute"/>.
		/// </summary>
		/// <param name="Name">The name of the argument, used to identify the argument's value in code.</param>
		/// <param name="Description">What the argument is for, or maybe how it's used.
		/// <br/><br/>OPTIONAL: <i>Does not need to be provided.</i></param>
		public OptionalCLIAttribute(string Name, [Optional, DefaultParameterValue("")] string Description)
			: base(Name, Description) { }
	}

	/// <summary>
	/// Represents a command-line argument with a raw value, type specified by <see cref="CLIType"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public sealed class ArgumentAttribute : RequiredCLIAttribute
	{
		/// <summary>
		/// The type of value this argument should contain.
		/// </summary>
		public readonly CLIType Type;

		/// <summary>
		/// Creates a new <see cref="ArgumentAttribute"/>.
		/// </summary>
		/// <param name="Name">The general name of the argument, used to identify the argument's value in code.</param>
		/// <param name="Type">The type of value this argument should contain.</param>
		/// <param name="Order">Whether this is the first, second, third, etc, argument.</param>
		/// <param name="Description">What the argument is for, or maybe how it's used.
		/// <br/><br/>OPTIONAL: <i>Does not need to be provided.</i></param>
		public ArgumentAttribute(string Name, CLIType Type, int Order, [Optional, DefaultParameterValue("")] string Description)
			: base(Name, Order, Description) => this.Type = Type;
	}

	/// <summary>
	/// Represents a command-line argument with specifically accepted values.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public sealed class SwitchAttribute : RequiredCLIAttribute
	{
		/// <summary>
		/// The enum used to identify accepted values.
		/// </summary>
		public readonly Type Enum;

		/// <summary>
		/// The character used to identify the switch argument.
		/// </summary>
		public readonly SwitchType Identifier;

		/// <summary>
		/// Creates a new <see cref="SwitchAttribute"/>.
		/// </summary>
		/// <param name="Name">The general name of the argument, used to identify the argument's value in code.</param>
		/// <param name="Enum">The enum type used to identify the accepted values.</param>
		/// <param name="Identifier">The character used to identify the switch argument.</param>
		/// <param name="Order">Whether this is the first, second, third, etc, argument.</param>
		/// <param name="Description">What the argument is for, or maybe how it's used.
		/// <br/><br/>OPTIONAL: <i>Does not need to be provided.</i></param>
		public SwitchAttribute(string Name, Type Enum, SwitchType Identifier, int Order, [Optional, DefaultParameterValue("")] string Description)
			: base(Name, Order, Description) { this.Enum = Enum; this.Identifier = Identifier; }
	}

	/// <summary>
	/// Represents an <b>optional</b> command-line argument with a raw value, type specified by <see cref="CLIType"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public sealed class OptionalArgumentAttribute : OptionalCLIAttribute
	{
		/// <summary>
		/// The type of value this argument should contain.
		/// </summary>
		public readonly CLIType Type;

		/// <summary>
		/// Creates a new <see cref="OptionalArgumentAttribute"/>.
		/// </summary>
		/// <param name="Name">The name of the argument, used to identify the argument's value in code.</param>
		/// <param name="Type">The type of value this argument should contain.</param>
		/// <param name="Description">What the argument is for, or maybe how it's used.
		/// <br/><br/>OPTIONAL: <i>Does not need to be provided.</i></param>
		public OptionalArgumentAttribute(string Name, CLIType Type, [Optional, DefaultParameterValue("")] string Description)
			: base(Name, Description) => this.Type = Type;
	}

	/// <summary>
	/// Represents an <b>optional</b> command-line argument with specifically accepted values.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public sealed class OptionalSwitchAttribute : OptionalCLIAttribute
	{
		/// <summary>
		/// The enum used to identify accepted values.
		/// </summary>
		public readonly Type Enum;

		/// <summary>
		/// The character used to identify the switch argument.
		/// </summary>
		public readonly SwitchType Identifier;

		/// <summary>
		/// Creates a new <see cref="OptionalSwitchAttribute"/>.
		/// </summary>
		/// <param name="Name">The name of the argument, used to identify the argument's value in code.</param>
		/// <param name="Enum">The enum type used to identify the accepted values.</param>
		/// <param name="Identifier">The character used to identify the switch argument.</param>
		/// <param name="Description">What the argument is for, or maybe how it's used.
		/// <br/><br/>OPTIONAL: <i>Does not need to be provided.</i></param>
		public OptionalSwitchAttribute(string Name, Type Enum, SwitchType Identifier, [Optional, DefaultParameterValue("")] string Description)
			: base(Name, Description) { this.Enum = Enum; this.Identifier = Identifier; }
	}
}
