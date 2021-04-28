using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Internal
{

/// <summary>
/// Allows you to get a step factory from a step name.
/// </summary>
public class StepFactoryStore
{
    /// <summary>
    /// Information about the available connectors
    /// </summary>
    public IReadOnlyList<ConnectorData> ConnectorData { get; }

    /// <summary>
    /// Types of enumerations that can be used by these steps.
    /// </summary>
    public IReadOnlyDictionary<string, Type> EnumTypesDictionary { get; }

    /// <summary>
    /// Dictionary mapping step names to step factories.
    /// </summary>
    public IReadOnlyDictionary<string, IStepFactory> Dictionary { get; }

    private StepFactoryStore(
        IReadOnlyList<ConnectorData> connectorData,
        IReadOnlyDictionary<string, IStepFactory> dictionary,
        IReadOnlyDictionary<string, Type> enumTypesDictionary)
    {
        ConnectorData = connectorData;

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
    /// Creates a StepFactoryStore with steps from the given assemblies
    /// </summary>
    public static StepFactoryStore Create(SCLSettings settings, params Assembly[] assemblies)
    {
        var settingsDict = ConnectorSettings.CreateFromSCLSettings(settings)
            .Select(x => x.Settings)
            .ToDictionary(x => x.Id);

        var data = new List<ConnectorData>();

        foreach (var assembly in assemblies)
        {
            var connectorSettings = ConnectorSettings.DefaultForAssembly(assembly);

            if (settingsDict.TryGetValue(assembly.GetName().Name!, out var cs))
                connectorSettings = cs;

            data.Add(new ConnectorData(connectorSettings, assembly));
        }

        return Create(data.ToArray());
    }

    /// <summary>
    /// Create a step factory store
    /// </summary>
    public static StepFactoryStore Create(
        IReadOnlyList<ConnectorData> connectorData,
        params IStepFactory[] factories) => Create(connectorData, factories.ToList());

    /// <summary>
    /// Create a step factory store
    /// </summary>
    public static StepFactoryStore Create(
        IReadOnlyList<ConnectorData> connectorData,
        IReadOnlyCollection<IStepFactory> factories)
    {
        var dictionary = factories
            .SelectMany(factory => GetStepNames(factory).Select(name => (factory, name)))
            .ToDictionary(x => x.name, x => x.factory);

        var enumTypesDictionary = dictionary.Values.SelectMany(x => x.EnumTypes)
            .Distinct()
            .ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

        return new StepFactoryStore(connectorData, dictionary, enumTypesDictionary);
    }

    /// <summary>
    /// Create a step factory store
    /// </summary>
    public static StepFactoryStore Create(params ConnectorData[] connectorData)
    {
        var steps = connectorData.Select(x => x.Assembly)
            .Prepend(typeof(IStep).Assembly)
            .WhereNotNull()
            .SelectMany(a => a!.GetTypes())
            .Distinct()
            .Where(x => !x.IsAbstract)
            .Where(x => typeof(ICompoundStep).IsAssignableFrom(x))
            .ToList();

        var factories =
            steps
                .Select(CreateStepFactory)
                .ToList();

        return Create(connectorData, factories);

        static IStepFactory CreateStepFactory(Type stepType)
        {
            Type closedType;

            if (stepType.IsGenericType)
            {
                var arguments = ((TypeInfo)stepType).GenericTypeParameters
                    .Select(_ => typeof(int))
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
    }

    /// <summary>
    /// Tries to get contexts injected by connectors
    /// </summary>
    public Result<(string Name, object Context)[], IErrorBuilder> TryGetInjectedContexts(
        SCLSettings settings)
    {
        var contexts = ConnectorData.Select(x => x.TryGetInjectedContexts(settings))
            .Combine(x => x.SelectMany(y => y).ToArray(), ErrorBuilderList.Combine);

        return contexts;
    }
}

}
