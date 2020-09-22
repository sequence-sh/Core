using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.Internal
{
    /// <summary>
    /// A process that is not a constant or a variable reference.
    /// </summary>
    public sealed class CompoundFreezableProcess : IFreezableProcess, IEquatable<CompoundFreezableProcess>
    {
        /// <summary>
        /// Creates a new CompoundFreezableProcess.
        /// </summary>
        public CompoundFreezableProcess(IRunnableProcessFactory processFactory, FreezableProcessData freezableProcessData, ProcessConfiguration? processConfiguration)
        {
            ProcessFactory = processFactory;
            FreezableProcessData = freezableProcessData;
            ProcessConfiguration = processConfiguration;
        }


        /// <summary>
        /// The factory for this process.
        /// </summary>
        public IRunnableProcessFactory ProcessFactory { get; }

        /// <summary>
        /// The data for this process.
        /// </summary>
        public FreezableProcessData FreezableProcessData { get; }

        /// <summary>
        /// Configuration for this process.
        /// </summary>
        public ProcessConfiguration? ProcessConfiguration { get; }


        /// <inheritdoc />
        public Result<IRunnableProcess> TryFreeze(ProcessContext processContext) => ProcessFactory.TryFreeze(processContext, FreezableProcessData, ProcessConfiguration);

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

        /// <inheritdoc />
        public bool Equals(CompoundFreezableProcess? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return ProcessFactory.Equals(other.ProcessFactory) &&
                   FreezableProcessData.Equals(other.FreezableProcessData) &&
                   Equals(ProcessConfiguration, other.ProcessConfiguration);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is CompoundFreezableProcess other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(ProcessFactory, FreezableProcessData, ProcessConfiguration);

        /// <summary>
        /// Equals Operator.
        /// </summary>
        public static bool operator ==(CompoundFreezableProcess? left, CompoundFreezableProcess? right) => Equals(left, right);

        /// <summary>
        /// Not Equals Operator.
        /// </summary>
        public static bool operator !=(CompoundFreezableProcess? left, CompoundFreezableProcess? right) => !Equals(left, right);
    }
}