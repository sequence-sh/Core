using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes
{
    /// <summary>
    /// A process that is not a constant or a variable reference.
    /// </summary>
    public sealed class CompoundFreezableProcess : IFreezableProcess
    {
        /// <summary>
        /// Creates a new CompoundFreezableProcess.
        /// </summary>
        public CompoundFreezableProcess(RunnableProcessFactory processFactory, FreezableProcessData freezableProcessData)
        {
            ProcessFactory = processFactory;
            FreezableProcessData = freezableProcessData;
        }


        /// <summary>
        /// The factory for this process.
        /// </summary>
        public RunnableProcessFactory ProcessFactory { get; }

        /// <summary>
        /// The data for this process.
        /// </summary>
        public FreezableProcessData FreezableProcessData { get; }


        /// <inheritdoc />
        public Result<IRunnableProcess> TryFreeze(ProcessContext processContext) => ProcessFactory.TryFreeze(processContext, FreezableProcessData);

        /// <inheritdoc />
        public Result<IReadOnlyCollection<(VariableName VariableName, ITypeReference type)>> TryGetVariablesSet
        {
            get
            {
                var result = FreezableProcessData
                    .Dictionary.Values
                    .Select(TryGetProcessMemberVariablesSet)
                    .Combine()
                    .Map(x=>x.SelectMany(y=>y).ToList() as IReadOnlyCollection<(VariableName name, ITypeReference type)>);

                return result;


                 Result<IReadOnlyCollection<(VariableName, ITypeReference)>> TryGetProcessMemberVariablesSet(ProcessMember processMember) =>
                     processMember.Join(vn =>
                             ProcessFactory.GetTypeReferencesSet(vn, FreezableProcessData)
                                 .Map(y=> y.Map(x => new[] { (vn, x) } as IReadOnlyCollection<(VariableName, ITypeReference)>)
                                 .Unwrap(ImmutableArray<(VariableName, ITypeReference)>.Empty)),
                         y => y.TryGetVariablesSet,
                         y => y.Select(z => z.TryGetVariablesSet).Combine().Map(x =>
                             x.SelectMany(q => q).ToList() as IReadOnlyCollection<(VariableName, ITypeReference)>));
            }
        }



        /// <inheritdoc />
        public string ProcessName => ProcessFactory.ProcessNameBuilder.GetFromArguments(FreezableProcessData);

        /// <inheritdoc />
        public Result<ITypeReference> TryGetOutputTypeReference() => ProcessFactory.TryGetOutputTypeReference(FreezableProcessData);

        /// <inheritdoc />
        public override string ToString() => ProcessName;
    }
}