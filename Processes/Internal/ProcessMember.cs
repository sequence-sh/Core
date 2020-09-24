using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.General;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Processes.Internal
{
    /// <summary>
    /// Any member of a process.
    /// </summary>
    public sealed class ProcessMember : IEquatable<ProcessMember>
    {
        /// <summary>
        /// Create a new VariableName ProcessMember
        /// </summary>
        public ProcessMember(VariableName variableName) => Option = new Option<VariableName, IFreezableProcess, IReadOnlyList<IFreezableProcess>>(variableName);

        /// <summary>
        /// Create a new IFreezableProcess ProcessMember
        /// </summary>
        public ProcessMember(IFreezableProcess argument) => Option = new Option<VariableName, IFreezableProcess, IReadOnlyList<IFreezableProcess>>(argument);

        /// <summary>
        /// Create a new ListArgument ProcessMember
        /// </summary>
        public ProcessMember(IReadOnlyList<IFreezableProcess> listArgument) => Option = new Option<VariableName, IFreezableProcess, IReadOnlyList<IFreezableProcess>>(listArgument);


        /// <summary>
        /// The member type of this Process Member.
        /// </summary>
        public MemberType MemberType
        {
            get
            {
                if (Option.Choice1.HasValue) return MemberType.VariableName;
                if (Option.Choice2.HasValue) return MemberType.Process;
                if (Option.Choice3.HasValue) return MemberType.ProcessList;

                return MemberType.NotAMember;
            }
        }

        /// <summary>
        /// The chosen option.
        /// </summary>
        public Option<VariableName, IFreezableProcess, IReadOnlyList<IFreezableProcess>> Option { get; }

        /// <summary>
        /// Use this ProcessMember.
        /// </summary>
        public T Join<T>(Func<VariableName, T> handleVariableName, Func<IFreezableProcess, T> handleArgument,
            Func<IReadOnlyList<IFreezableProcess>, T> handleListArgument) =>
            Option.Join(handleVariableName, handleArgument, handleListArgument);

        /// <summary>
        /// Gets the processMember if it is a VariableName.
        /// </summary>
        public Result<VariableName> AsVariableName(string propertyName) => Option.Choice1.ToResult($"{propertyName} was a {MemberType}, not a VariableName");

        /// <summary>
        /// Gets the processMember if it is an argument.
        /// </summary>
        public Result<IFreezableProcess> AsArgument(string propertyName) => Option.Choice2.ToResult($"{propertyName} was a {MemberType}, not an argument");

        /// <summary>
        /// Gets the processMember if it is a listArgument.
        /// </summary>
        public Result<IReadOnlyList<IFreezableProcess>> AsListArgument(string propertyName) => Option.Choice3.ToResult($"{propertyName} was a {MemberType}, not an list argument");

        /// <summary>
        /// Tries to convert this process member to a particular type.
        /// </summary>
        public Result<ProcessMember> TryConvert(MemberType convertType)
        {
            if (MemberType == convertType) return this;

            if (MemberType == MemberType.ProcessList && convertType == MemberType.Process)
            {
                var sequenceProcess = SequenceProcessFactory.CreateFreezable(Option.Choice3.Value, null);
                return new ProcessMember(sequenceProcess);
            }

            if (MemberType == MemberType.VariableName && convertType == MemberType.Process)
            {
                var getVariableProcess = GetVariableProcessFactory.CreateFreezable(Option.Choice1.Value);
                return new ProcessMember(getVariableProcess);
            }

            return Result.Failure<ProcessMember>("Could not convert");
        }

        /// <summary>
        /// A string representation of the member.
        /// </summary>
        public string MemberString
        {
            get
            {
                return Join(x =>
                        x.ToString()!,
                    x => x.ToString()!,
                    x => x.ToString()!);

            }
        }

        /// <inheritdoc />
        public override string ToString() => new {MemberType, Value=MemberString}.ToString()!;

        /// <inheritdoc />
        public bool Equals(ProcessMember? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return Option.Equals(other.Option,
                (a, b) => a.Equals(b),
                (a, b) => a.Equals(b),
                ListsAreEqual);

            static bool ListsAreEqual(IReadOnlyList<IFreezableProcess> a1, IReadOnlyList<IFreezableProcess> a2)
            {
                if (a1 is null)
                    return a2 is null;

                if (a2 is null)
                    return false;

                return a1.SequenceEqual(a2);
            }
        }



        /// <inheritdoc />
        public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is ProcessMember other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(Option.Choice1, Option.Choice2, Option.Choice3.Select(x=>x.Count));

        /// <summary>
        /// Equals operator
        /// </summary>
        public static bool operator ==(ProcessMember? left, ProcessMember? right) => Equals(left, right);

        /// <summary>
        /// Not Equals operator
        /// </summary>
        public static bool operator !=(ProcessMember? left, ProcessMember? right) => !Equals(left, right);
    }
}