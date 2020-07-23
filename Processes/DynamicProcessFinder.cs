using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Reductech.EDR.Processes.Mutable;
using Reductech.EDR.Processes.Mutable.Enumerations;
using Reductech.EDR.Processes.Mutable.Injections;
using Reductech.Utilities.InstantConsole;
using Process = Reductech.EDR.Processes.Mutable.Process;

namespace Reductech.EDR.Processes
{
    /// <summary>
    /// Dynamically Gets all processes and related entities from an assembly.
    /// For use with InstantConsole.
    /// </summary>
    public static class DynamicProcessFinder
    {
        /// <summary>
        /// Dynamically Gets all processes and related entities from an assembly.
        /// For use with InstantConsole.
        /// </summary>
        public static IEnumerable<IDocumented> GetAllDocumented<T>(T settings, DocumentationCategory documentationCategory, Type anyAssemblyMember) where T : IProcessSettings
        {
            var processes = GetProcesses(settings, documentationCategory, anyAssemblyMember);
            var enumerations = GetWrappedEntities(typeof(Enumeration), EnumerationDocumentationCategory, anyAssemblyMember);
            var injections = GetWrappedEntities(typeof(Injection), InjectionDocumentationCategory, anyAssemblyMember);

            return processes.Concat(enumerations).Concat(injections);
        }


        private static IEnumerable<IDocumented> GetProcesses<T>(T settings, DocumentationCategory documentationCategory, Type anyAssemblyMember) where T : IProcessSettings
        {
            var assembly = Assembly.GetAssembly(anyAssemblyMember);

            Debug.Assert(assembly != null, nameof(assembly) + " != null");
            var types =  assembly!.GetTypes().Where(x => typeof(Process).IsAssignableFrom(x))
                .Where(x => !x.IsAbstract && !x.IsInterface).ToList();

            var processes = types.Select(x => new ProcessWrapper<T>(x, settings, documentationCategory)).ToList();

            return processes;
        }

        private static IEnumerable<IDocumented> GetWrappedEntities(Type entityType,  DocumentationCategory documentationCategory, Type anyAssemblyMember)
        {
            var assembly = Assembly.GetAssembly(anyAssemblyMember);

            Debug.Assert(assembly != null, nameof(assembly) + " != null");
            var types =  assembly!.GetTypes().Where(entityType.IsAssignableFrom)
                .Where(x => !x.IsAbstract && !x.IsInterface).ToList();

            var processes = types.Select(x => new YamlObjectWrapper(x, documentationCategory)).ToList();

            return processes;
        }

        /// <summary>
        /// The documentation category to use for enumerations
        /// </summary>
        public static readonly DocumentationCategory EnumerationDocumentationCategory = new DocumentationCategory("Enumerations", typeof(Enumeration));

        /// <summary>
        /// The documentation category to use for injections
        /// </summary>
        public static readonly DocumentationCategory InjectionDocumentationCategory = new DocumentationCategory("injections", typeof(Injection));
    }
}
