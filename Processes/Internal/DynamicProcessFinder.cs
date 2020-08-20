using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Reductech.Utilities.InstantConsole;

namespace Reductech.EDR.Processes.Internal
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
        public static IEnumerable<IDocumented> GetAllDocumented<T>(T settings, DocumentationCategory documentationCategory, Type anyAssemblyMember, ILogger logger) where T : IProcessSettings
        {
            var pfs = ProcessFactoryStore.CreateUsingReflection(anyAssemblyMember);

            var wrappers = pfs.Dictionary.Values
                .Select(x => new ProcessWrapper<T>(x, settings, logger, documentationCategory)).ToList();

            return wrappers;
        }

    }
}
