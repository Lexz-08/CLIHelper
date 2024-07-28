# CLIHelper
## Description
Provides simpler management and checking of command-line arguments in an application.
## How To Use
```csharp
using CLIHelper;

internal class Program
{
    private enum MySwitch
    {
        // random switches for example
        b,
        v,
        x,
        
        // as long as the argument matches the name, it will parse
        debug
    }

    // attributes can be on any one method that is marked as just 'static'
    [Argument("arg1", CLIType.String, 1)]
    [Switch("arg12", typeof(MySwitch), SwitchIdentifier.Slash, 2)]
    [OptionalArgument("arg3", CLIType.String)]
    [OptionalSwitch("arg4", typeof(MySwitch), SwitchIdentifier.Hyphen)]
    static void Main()
    {
        var args = CLI.GetArguments(); // will return null if any parsing errors occurred, which in turn will be printed to the console
        
        if (args != null)
        {
            // and now do what you need with your arguments
            Console.WriteLine(args["arg1"].ToString());
            Console.WriteLine(args["arg2"].GetEnumValue<MySwitch>());
        }
    }
}
```
## To-Do
- [ ] Check for entry method named `Main` to ensure clarity with attribute usage
- [X] Print out available arguments if none are provided
- [X] Make use of `Description` metadata in `CLIAttribute`
- [ ] Replace `_` in enum value names with `-` for complex switch arguments
## Example of Argument Layout (given insufficient arguments)
![](image.png)
## Download
[CLIHelper.dll](https://github.com/Lexz-08/CLIHelper/releases/latest/download/CLIHelper.dll)
