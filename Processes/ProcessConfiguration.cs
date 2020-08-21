using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Processes
{
    /// <summary>
    /// Additional process configuration that may be needed in some use cases.
    /// </summary>
    public sealed class ProcessConfiguration
    {
        /// <summary>
        /// Additional requirements, beyond the default for this process.
        /// </summary>
        [YamlMember(Order = 1)]
        public List<Requirement>? AdditionalRequirements { get; set; }

        /// <summary>
        /// Tags that the target machine must have (defined in a the config file) for this to be run on that machine.
        /// </summary>
        [YamlMember(Order = 2)]
        public List<string>? TargetMachineTags { get; set; }

        /// <summary>
        /// Conditional true, this process will not be split into multiple subProcesses.
        /// </summary>
        [YamlMember(Order = 3)]
        public bool DoNotSplit { get; set; }

        /// <summary>
        /// The priority of this process. Processes with higher priorities will be run first.
        /// </summary>
        [YamlMember(Order = 5)]
        public byte? Priority { get; set; }

        /// <summary>
        /// Combines two process configurations, deferring to the child where there is a conflict.
        /// </summary>
        public static ProcessConfiguration? Combine(ProcessConfiguration? parent, ProcessConfiguration? child)
        {
            if (parent == null)
                return child;
            if (child == null)
                return parent;

            return new ProcessConfiguration
            {
                AdditionalRequirements = Combine(parent.AdditionalRequirements, child.AdditionalRequirements, true),
                TargetMachineTags = Combine(parent.TargetMachineTags, child.TargetMachineTags, true),
                DoNotSplit = parent.DoNotSplit || child.DoNotSplit,
                Priority = child.Priority?? parent.Priority
            };

        }

        /// <summary>
        /// Tries to convert an object to a process configuration.
        /// </summary>
        public static Result<ProcessConfiguration> TryConvert(object o)
        {
            if (o is ProcessConfiguration pc) return pc;

            if (o is IReadOnlyDictionary<object, object> dictionary)
            {
                var processConfiguration = new ProcessConfiguration();

                var results = new List<Result>();


                foreach (var (key, value) in dictionary.Where(x=>x.Value != null))
                {
                    var result = key switch
                    {
                        nameof(Priority)=> value.TryConvert<byte>().Tap(x=>processConfiguration.Priority = x),
                        nameof(DoNotSplit)=> value.TryConvert<bool>().Tap(x=>processConfiguration.DoNotSplit = x),
                        nameof(TargetMachineTags)=> value.TryCast<List<object>>()
                            .Bind(x=>x.TryConvertElements<object, string>())
                            .Tap(x=>processConfiguration.TargetMachineTags = x.ToList()),
                        nameof(AdditionalRequirements)=> value.TryCast<List<object>>()
                            .Bind(x=>x.TryConvertElements(Requirement.TryConvert))
                            .Tap(x=>processConfiguration.AdditionalRequirements = x.ToList()),
                        _ => Result.Failure($"Could not recognize property '{key}'.")
                    };

                    results.Add(result);
                }

                var r = results
                    .Combine().Bind<ProcessConfiguration>(() => processConfiguration);

                return r;

            }
            return Result.Failure<ProcessConfiguration>("Could not deserialize process configuration.");
        }


        private static List<T>? Combine<T>(List<T>? l1, List<T>? l2, bool distinct)
        {
            if (l1 == null) return l2;
            if (l2 == null) return l1;

            return distinct ? l1.Concat(l2).Distinct().ToList() : l1.Concat(l2).ToList();
        }
    }
}