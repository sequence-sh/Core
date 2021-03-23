using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using Reductech.EDR.Core.Attributes;

namespace Reductech.EDR.Core.Internal
{

/// <summary>
/// Allows you to get a step factory from a step name.
/// </summary>
public class StepFactoryStore
{
    private StepFactoryStore(
        IReadOnlyList<ConnectorInformation> connectorInformations,
        IReadOnlyDictionary<string, IStepFactory> dictionary,
        IReadOnlyDictionary<string, Type> enumTypesDictionary)
    {
        ConnectorInformations = connectorInformations;

        Dictionary = dictionary.ToDictionary(
            x => x.Key!,
            x => x.Value!,
            StringComparer.OrdinalIgnoreCase
        )!;

        EnumTypesDictionary = enumTypesDictionary.ToDictionary(
            x => x.Key!,
            x => x.Value!,
            StringComparer.OrdinalIgnoreCase
        )!;
    }

    /// <summary>
    /// Creates a new StepFactoryStore.
    /// </summary>
    public static StepFactoryStore Create(
        IReadOnlyList<ConnectorInformation> connectorInformations,
        params IStepFactory[] factories) => Create(connectorInformations, factories.AsEnumerable());

    /// <summary>
    /// Creates a new StepFactoryStore.
    /// </summary>
    public static StepFactoryStore Create(
        IReadOnlyList<ConnectorInformation> connectorInformations,
        IEnumerable<IStepFactory> factories)
    {
        var dictionary = factories
            .SelectMany(factory => GetStepNames(factory).Select(name => (factory, name)))
            .ToDictionary(x => x.name, x => x.factory);

        var enumTypesDictionary = dictionary.Values.SelectMany(x => x.EnumTypes)
            .Distinct()
            .ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

        return new StepFactoryStore(connectorInformations, dictionary, enumTypesDictionary!);
    }

    private static IEnumerable<string> GetStepNames(IStepFactory stepFactory)
    {
        yield return stepFactory.TypeName;

        foreach (var alias in stepFactory.StepType.GetCustomAttributes<AliasAttribute>())
            yield return alias.Name;
    }

    /// <summary>
    /// Is this property a scoped function
    /// </summary>
    [Pure]
    public bool IsScopedFunction(string stepName, StepParameterReference stepParameterReference)
    {
        if (!Dictionary.TryGetValue(stepName, out var factory))
            return false;

        return factory.IsScopedFunction(stepParameterReference);
    }

    /// <summary>
    /// Create a step factory store using all StepFactories in the assembly.
    /// </summary>
    /// <returns></returns>
    public static StepFactoryStore CreateUsingReflection(params Type[] assemblyMemberTypes)
    {
        var assemblies = assemblyMemberTypes
            .Prepend(typeof(ICompoundStep))
            .Select(Assembly.GetAssembly)
            .Distinct()
            .ToList();

        var factories =
            assemblies
                .SelectMany(a => a!.GetTypes())
                .Distinct()
                .Where(x => !x.IsAbstract)
                .Where(x => typeof(ICompoundStep).IsAssignableFrom(x))
                .Select(CreateStepFactory)
                .ToList();

        var connectorInfo = assemblies.Select(ConnectorInformation.Create!).ToList();

        return Create(connectorInfo, factories);
    }

    /// <summary>
    /// Create a StepFactory from a step type
    /// </summary>
    public static IStepFactory CreateStepFactory(Type stepType)
    {
        Type closedType;

        if (stepType.IsGenericType)
        {
            var arguments = ((TypeInfo)stepType).GenericTypeParameters
                .Select(_ => typeof(int)) //arbitrarily choose int as the type
                .ToArray();

            try
            {
                closedType = stepType.MakeGenericType(arguments);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        else
        {
            closedType = stepType;
        }

        var instance = Activator.CreateInstance(closedType);
        var step     = instance as ICompoundStep;

        var stepFactory = step!.StepFactory;

        if (stepFactory is null)
            throw new Exception($"Step Factory for {stepType.Name} is null");

        return step!.StepFactory;
    }

    /// <summary>
    /// Information about the available connectors
    /// </summary>
    public IReadOnlyList<ConnectorInformation> ConnectorInformations { get; }

    /// <summary>
    /// Types of enumerations that can be used by these steps.
    /// </summary>
    public IReadOnlyDictionary<string, Type> EnumTypesDictionary { get; }

    /// <summary>
    /// Dictionary mapping step names to step factories.
    /// </summary>
    public IReadOnlyDictionary<string, IStepFactory> Dictionary { get; }
}

/// <summary>
/// Information about a connector
/// </summary>
public record ConnectorInformation(string Name, string Version)
{
    /// <summary>
    /// Create from an assembly
    /// </summary>
    public static ConnectorInformation Create(Assembly assembly)
    {
        FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
        string          version         = fileVersionInfo.ProductVersion!;

        return new ConnectorInformation(assembly.GetName().Name!, version);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{Name} {Version}";
    }
}

}
