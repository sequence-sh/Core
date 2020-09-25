# Developer Guide
*How to create a connector*

 ## Class Library

 A coonector is a C# class library.
 It need to target the dot net core runtime.
 It needs a reference to Reductech.EDR.Processes

 ## Processes

Every function you connector can perform is represented by a *process*.

A *process* is a C# class containing a property for each of its parameters, and a run method which executes the process.

The class should implement `CompoundRunnableProcess<TOutput>` where `TOutput` is the output type. This could be a static type (like `string`) or a generic type.

### Arguments

The process class will usually contain properties reprenting the process arguments.

These properties must have the return type of `IRunnableProcess<TProperty>` where `TProperty` is their output type and have the `RunnableProcessProperty]` attribute.
If a property is optional it should be nullable, otherwise it should have the `Required` attribute.
If a property has a default value that value should be a Constant and the property should have the `DefaultValueExplanation` attribute.
The property may optionally have the `[Example]` attribute.
The property should have an xml-doc summary as this will be used in the automatically generated documentation.

Here is an example of a property from the ReadCSV process

```csharp
/// <summary>
/// The delimiter to use to separate rows.
/// </summary>
[RunnableProcessPropertyAttribute(Order = 2)]
[Required]
[DefaultValueExplanation(",")]
public IRunnableProcess<string> Delimiter { get; set; } = new Constant<string>(",");
```


### The Run Method

`CompoundRunnableProcess<Type>` requires that you implement the method `public override Result<T, IRunErrors> Run(ProcessState processState)`

The run method takes a `ProcessState` - an argument that contains the `IProcessSettings`, the `ILogger` and the `IExternalProcessRunner` and returns a Result - either on object of the output type, or an `IRunErrors` object.

The run method is where the business logic of your process should reside. 
You are expected to get the values of each of your properties either zero or one times depending on branching.

To get the value of your property, call the `Run(IProcessState processState)` method. This will return a `Result<TProperty>` object. 

You should check the IsFailure property and, if true return the  `ConvertFailure<TOutput>()` method on it;

Otherwise you should use the `Value` property of the result in your business logic.

Here is an example of the `LengthOfString` process:

```csharp
/// <inheritdoc />
public override Result<int, IRunErrors> Run(ProcessState processState)
{
    var str = String.Run(processState);
    if (str.IsFailure) return str.ConvertFailure<int>();

    return str.Value.Length;

}
```

### The Process Factory

The other abstract property you must implement is the factory.

`public abstract IRunnableProcessFactory RunnableProcessFactory { get; }` 

To implement this you need to create a singleton class representing the factory for your process and return the instance.

For almost all use cases, implement the `SimpleRunnableProcessFactory<TProcess, TOutput>` class.

For example:

```csharp
/// <summary>
/// Extracts elements from a CSV file
/// </summary>
public sealed class ReadCsvProcessFactory : SimpleRunnableProcessFactory<ReadCsv, List<List<string>>>
{
    private ReadCsvProcessFactory() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static SimpleRunnableProcessFactory<ReadCsv, List<List<string>>> Instance { get; } = new ReadCsvProcessFactory();
}
```

### The settings File
Your process may require external settings in order to work.

To do this, create an interface that inherits from `IProcessSettings` and add all the required properties.

Inside processes that use those properties, you can attempt to cast the IProcessSettings in the ProcessState to your interface. 

This is an example from the Nuix connector
```csharp
/// <summary>
/// Settings required to run a nuix process.
/// </summary>
public interface INuixProcessSettings : IProcessSettings
{
    /// <summary>
    /// Whether to use a dongle for nuix authentication.
    /// </summary>
    bool UseDongle { get; }

    /// <summary>
    /// The path to the nuix console executable.
    /// </summary>
    string NuixExeConsolePath { get; }

    /// <summary>
    /// The version of Nuix
    /// </summary>
    Version NuixVersion { get; }

    /// <summary>
    /// A list of available Nuix features.
    /// </summary>
    IReadOnlyCollection<NuixFeature> NuixFeatures { get; }

}
```
Your console runner (see below) will need to ensure that the settings object that is passed to the runner implements that interface.


### The Console Runner

To be able to use your processes you need to make them available inside your console runner.
Your Console runner should have a reference to the CommandDotNet nuget package https://github.com/bilal-fazlani/commanddotnet

See https://gitlab.com/reductech/edr/connectors/nuix/-/blob/master/NuixConsole/Program.cs for an example of how to do this.

 

