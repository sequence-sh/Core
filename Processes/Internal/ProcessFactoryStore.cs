using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Reductech.EDR.Processes.Internal
{
    /// <summary>
    /// Allows you to get a process factory from a process name.
    /// </summary>
    public class ProcessFactoryStore
    {
        /// <summary>
        /// Creates a new ProcessFactoryStore.
        /// </summary>
        public ProcessFactoryStore(IReadOnlyDictionary<string, RunnableProcessFactory> dictionary, IReadOnlyDictionary<string, Type> enumTypesDictionary)
        {
            Dictionary = dictionary;
            EnumTypesDictionary = enumTypesDictionary;
        }


        /// <summary>
        /// Create a process factory store using all ProcessFactories in the assembly.
        /// </summary>
        /// <returns></returns>
        public static ProcessFactoryStore CreateUsingReflection(Type anyAssemblyMember)
        {
            var factories = Assembly.GetAssembly(anyAssemblyMember)!
                .GetTypes()
                .Where(x => !x.IsAbstract)
                .Where(x => typeof(RunnableProcessFactory).IsAssignableFrom(x))
                .Select(x => x.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)!.GetValue(null))
                .Cast<RunnableProcessFactory>().ToList();

            var dictionary = factories.ToDictionary(x => x.TypeName);
            var enumTypesDictionary = factories.SelectMany(x => x.EnumTypes).Distinct()
                .ToDictionary(x => x.Name ?? "", StringComparer.OrdinalIgnoreCase);

            return new ProcessFactoryStore(dictionary, enumTypesDictionary!);


        }

        /// <summary>
        /// Types of enumerations that can be used by these processes.
        /// </summary>
        public IReadOnlyDictionary<string, Type> EnumTypesDictionary { get; }

        /// <summary>
        /// Dictionary mapping process names to process factories.
        /// </summary>
        public IReadOnlyDictionary<string, RunnableProcessFactory> Dictionary { get; }
    }
}