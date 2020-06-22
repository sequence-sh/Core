using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Namotion.Reflection;
using Reductech.EDR.Utilities.Processes.mutable;
using Reductech.EDR.Utilities.Processes.mutable.chain;
using Reductech.EDR.Utilities.Processes.mutable.enumerations;
using Reductech.EDR.Utilities.Processes.mutable.injection;
using Reductech.Utilities.InstantConsole;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Utilities.Processes
{
    /// <summary>
    /// All nuix processes.
    /// </summary>
    public static class AllProcesses
    {

        /// <summary>
        /// All nuix processes.
        /// </summary>
        public static readonly IReadOnlyCollection<IDocumented> Processes = new List<Process>
        {
            new ChainLink(),
            new Chain(),
            new AssertError(),
            new AssertFalse(),
            new AssertTrue(),
            new CheckNumber(),
            new Conditional(),
            new CreateDirectory(),
            new DeleteItem(),
            new DoesFileContain(),
            new Loop(),
            new RunExternalProcess(),
            new Sequence(),
            new Unzip(),
            new WriteFile()
        }.Select(x=> new YamlObjectWrapper(x.GetType(), new DocumentationCategory("General Processes", typeof(Process))))
                .ToList();

        /// <summary>
        /// Objects that are useful to processes.
        /// </summary>
        public static readonly IReadOnlyCollection<IDocumented> EnumerationWrappers = new List<object>
        {
            new List(),
            new CSV(),
            new Directory()
        }.Select(x=> new YamlObjectWrapper(x.GetType(), new DocumentationCategory("Enumerations", typeof(Enumeration)))).ToList();

        /// <summary>
        /// Objects that are useful to processes.
        /// </summary>
        public static readonly IReadOnlyCollection<IDocumented> InjectionWrappers = new List<object>
        {
            new Injection()
        }.Select(x=> new YamlObjectWrapper(x.GetType(), new DocumentationCategory("Injections", typeof(Injection)))).ToList();
    }



}
