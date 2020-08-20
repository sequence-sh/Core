using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes
{
    /// <summary>
    /// The data used by a Freezable Process.
    /// </summary>
    public sealed class FreezableProcessData
    {
        /// <summary>
        /// Create a new FreezableProcessData
        /// </summary>
        public FreezableProcessData(IReadOnlyDictionary<string, ProcessMember> dictionary, ProcessConfiguration? processConfiguration)
        {
            Dictionary = dictionary;
            ProcessConfiguration = processConfiguration;
        }

        /// <summary>
        /// Configuration for this process.
        /// </summary>
        public ProcessConfiguration? ProcessConfiguration { get; }

        /// <summary>
        /// Dictionary mapping property names to process members.
        /// </summary>
        public IReadOnlyDictionary<string, ProcessMember> Dictionary { get; }

        /// <summary>
        /// Gets a variable name.
        /// </summary>
        public Result<VariableName> GetVariableName(string name) =>
            Dictionary.TryFindOrFail(name, null).Bind(x => x.AsVariableName(name));

        /// <summary>
        /// Gets an argument.
        /// </summary>
        public Result<IFreezableProcess> GetArgument(string name) =>
            Dictionary.TryFindOrFail(name, null).Bind(x => x.AsArgument(name));

        /// <summary>
        /// Gets a list argument.
        /// </summary>
        public Result<IReadOnlyList<IFreezableProcess>> GetListArgument(string name) =>
            Dictionary.TryFindOrFail(name, null).Bind(x => x.AsListArgument(name));

        /// <inheritdoc />
        public override string ToString()
        {
            return new
            {
                Variables = Dictionary.Count(x => x.Value.MemberType == MemberType.VariableName),
                Processes = Dictionary.Count(x => x.Value.MemberType == MemberType.Process),
                ProcessLists = Dictionary.Count(x => x.Value.MemberType == MemberType.ProcessList)
            }.ToString()!;
        }
    }
}