# Developer Guide
*How to create a connector*

 ## Class Library

 A connector is a C# class library.
 It need to target the dot net core runtime.
 It needs a reference to Reductech.EDR.Core

 ## Steps

Every function you connector can perform is represented by a *step*.

A *step* is a C# class containing a property for each of its arguments, and a run method which executes the step.

The class should implement `CompoundStep<TOutput>` where `TOutput` is the output type. This could be a static type (like `string`) or a generic type.

### Arguments

The step class will usually contain properties reprenting the arguments.

These properties must have the return type of `IStep<TProperty>` where `TProperty` is their output type and have the `[StepProperty]` attribute.
If a property is optional it should be nullable, otherwise it should have the `Required` attribute.
If a property has a default value the property should have the `DefaultValueExplanation` attribute and be initialized to an instant of `Constant<TProperty>`
The property may optionally have the `[Example]` attribute.
The property should have an xml-doc summary as this will be used in the automatically generated documentation.

Here is an example of an argument from the ReadCSV step

```csharp
/// <summary>
/// The delimiter to use to separate rows.
/// </summary>
[StepProperty(Order = 2)]
[Required]
[DefaultValueExplanation(",")]
public IStep<string> Delimiter { get; set; } = new Constant<string>(",");
```


### The Run Method

`CompoundStep<TOutput>` requires that you implement the method `public abstract Result<TOutput, IRunErrors> Run(StateMonad stateMonad);`

The run method takes a `StateMonad` - an argument that contains the `ISettings`, the `ILogger` and the `IExternalProcessRunner` and returns a Result - either a `TOutput`, or an `IRunErrors` object.

The run method is where the business logic of your step should reside. 
You are expected to get the values of each of your properties either zero or one times depending on branching.

To get the value of your property, call the `Run(StateMonad stateMonad)` method. This will return a `Result<TProperty>` object. 

You should check the IsFailure property and, if true return the  `ConvertFailure<TOutput>()` method on it;

Otherwise you should use the `Value` property of the result in your business logic.

Here is an example of the `LengthOfString` step:

```csharp
/// <inheritdoc />
public override Result<int, IRunErrors> Run(StateMonad stateMonad)
{
    var str = String.Run(stateMonad);
    if (str.IsFailure) return str.ConvertFailure<int>();

    return str.Value.Length;
}
```

### The Step Factory

The other abstract property you must implement is the factory.

`public abstract IStepFactory StepFactory { get; }` 

To implement this you need to create a singleton class representing the factory for your step and return the instance.

For almost all use cases, implement the `SimpleStepFactory<TStep, TOutput>` class.

For example:

```csharp
/// <summary>
/// Extracts elements from a CSV file
/// </summary>
public sealed class ReadCsvStepFactory : SimpleStepFactory<ReadCsv, List<List<string>>>
{
    private ReadCsvStepFactory() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static SimpleStepFactory<ReadCsv, List<List<string>>> Instance { get; } = new ReadCsvStepFactory();
}
```

### The settings File
Your step may require external settings in order to work.

To do this, create an interface that inherits from `ISettings` and add all the required properties.

Inside steps that use those properties, you can attempt to cast the ISettings in the StateMonad to your interface. 

This is an example from the Nuix connector
```csharp
/// <summary>
/// Settings required to run a nuix step.
/// </summary>
public interface INuixSettings : ISettings
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

To be able to use your steps you need to make them available inside your console runner.
Your Console runner should have a reference to the CommandDotNet nuget package https://github.com/bilal-fazlani/commanddotnet

See https://gitlab.com/reductech/edr/connectors/nuix/-/blob/master/NuixConsole/Program.cs for an example of how to do this.

 

