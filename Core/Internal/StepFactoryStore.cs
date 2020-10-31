using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// Allows you to get a step factory from a step name.
    /// </summary>
    public class StepFactoryStore
    {
        /// <summary>
        /// Creates a new StepFactoryStore.
        /// </summary>
        public StepFactoryStore(IReadOnlyDictionary<string, IStepFactory> dictionary, IReadOnlyDictionary<string, Type> enumTypesDictionary)
        {
            Dictionary = dictionary.ToDictionary(x=>x.Key!, x=>x.Value!, StringComparer.OrdinalIgnoreCase)!;
            EnumTypesDictionary = enumTypesDictionary.ToDictionary(x=>x.Key!, x=>x.Value!, StringComparer.OrdinalIgnoreCase)!;
        }

        /// <summary>
        /// Creates a new StepFactoryStore.
        /// </summary>
        public static StepFactoryStore Create(params IStepFactory[] factories) => Create(factories.AsEnumerable());

        /// <summary>
        /// Creates a new StepFactoryStore.
        /// </summary>
        public static StepFactoryStore Create(IEnumerable<IStepFactory> factories)
        {
            var dictionary = factories.ToDictionary(x => x.TypeName);
            var enumTypesDictionary = factories.SelectMany(x => x.EnumTypes).Distinct()
                .ToDictionary(x => x.Name ?? "", StringComparer.OrdinalIgnoreCase);

            return new StepFactoryStore(dictionary, enumTypesDictionary!);
        }

        /// <summary>
        /// Create a step factory store using all StepFactories in the assembly.
        /// </summary>
        /// <returns></returns>
        public static StepFactoryStore CreateUsingReflection(params Type[] assemblyMemberTypes)
        {
            var factories =
                assemblyMemberTypes.Select(Assembly.GetAssembly)
                        .Distinct()
                        .SelectMany(a=>a!.GetTypes())
                        .Distinct()
                .Where(x => !x.IsAbstract)
                .Where(x => typeof(IStepFactory).IsAssignableFrom(x))
                .Select(x => x.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)!.GetValue(null))
                .Cast<IStepFactory>().ToList();

            return Create(factories);
        }

        /// <summary>
        /// Types of enumerations that can be used by these steps.
        /// </summary>
        public IReadOnlyDictionary<string, Type> EnumTypesDictionary { get; }

        /// <summary>
        /// Dictionary mapping step names to step factories.
        /// </summary>
        public IReadOnlyDictionary<string, IStepFactory> Dictionary { get; }
    }
}