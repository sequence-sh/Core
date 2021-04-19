using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Connectors;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

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
    /// Create a step factory store by loading connectors from paths specified in the SCLSettings
    /// </summary>
    public static Result<StepFactoryStore, IErrorBuilder> TryCreateFromSettings(
        SCLSettings sclSettings,
        ILogger logger)
    {
        List<string> connectorPaths;

        var connectors = sclSettings.Entity.TryGetValue("connectors");

        if (connectors.HasValue && connectors.Value is EntityValue.NestedEntity nestedEntity)
        {
            connectorPaths = new List<string>();

            foreach (var connectorProperty in nestedEntity.Value)
                if (connectorProperty.BestValue is EntityValue.NestedEntity connector)
                {
                    var path = connector.Value.TryGetNestedString("path");

                    if (path.HasValue)
                        connectorPaths.Add(path.Value);
                }
        }
        else
        {
            connectorPaths = new List<string>();
        }

        var assemblies = connectorPaths
            .Select(PluginLoadContext.GetAbsolutePath)
            .Select(x => PluginLoadContext.LoadPlugin(x, logger))
            .Combine(x => x.ToArray(), ErrorBuilderList.Combine);

        if (assemblies.IsFailure)
            return assemblies.ConvertFailure<StepFactoryStore>();

        var stepFactoryStore = CreateFromAssemblies(assemblies.Value);

        return stepFactoryStore;
    }

    /// <summary>
    /// Create a step factory store using all StepFactories in the assembly.
    /// </summary>
    /// <returns></returns>
    public static StepFactoryStore CreateFromAssemblies(params Assembly[] assemblies)
    {
        var steps = assemblies.Prepend(typeof(IStep).Assembly)
            .SelectMany(a => a!.GetTypes())
            .Distinct()
            .Where(x => !x.IsAbstract)
            .Where(x => typeof(ICompoundStep).IsAssignableFrom(x))
            .ToList();

        var factories =
            steps
                .Select(CreateStepFactory)
                .ToList();

        var connectorInfo =
            assemblies.Select(ConnectorInformation.TryCreate!).WhereNotNull().ToList();

        return Create(connectorInfo, factories);

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
    public static ConnectorInformation? TryCreate(Assembly? assembly)
    {
        if (assembly is null)
            return null;

        try
        {
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            string          version         = fileVersionInfo.ProductVersion!;

            return new ConnectorInformation(assembly.GetName().Name!, version);
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{Name} {Version}";
    }
}

}
