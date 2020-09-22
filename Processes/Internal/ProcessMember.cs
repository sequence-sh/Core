using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.General;

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
        public ProcessMember(VariableName variableName) => VariableName = variableName;

        /// <summary>
        /// Create a new IFreezableProcess ProcessMember
        /// </summary>
        public ProcessMember(IFreezableProcess argument) => Argument = argument;

        /// <summary>
        /// Create a new ListArgument ProcessMember
        /// </summary>
        public ProcessMember(IReadOnlyList<IFreezableProcess> listArgument) => ListArgument = listArgument;


        /// <summary>
        /// The member type of this Process Member.
        /// </summary>
        public MemberType MemberType
        {
            get
            {
                if (VariableName.HasValue) return MemberType.VariableName;
                else if (Argument != null) return MemberType.Process;
                else if (ListArgument != null) return MemberType.ProcessList;

                return MemberType.NotAMember;
            }
        }

        /// <summary>
        /// The ProcessMember, if it is a VariableName
        /// </summary>
        public VariableName? VariableName { get;  }

        /// <summary>
        /// The ProcessMember, if it is an Argument
        /// </summary>
        public IFreezableProcess? Argument { get;  }

        /// <summary>
        /// The ProcessMember, if it is a ListArgument
        /// </summary>
        public IReadOnlyList<IFreezableProcess>? ListArgument { get;  }

        /// <summary>
        /// Use this ProcessMember.
        /// </summary>
        public T Join<T>(Func<VariableName, T> handleVariableName, Func<IFreezableProcess, T> handleArgument,
            Func<IReadOnlyList<IFreezableProcess>, T> handleListArgument)
        {
            if (VariableName != null) return handleVariableName(VariableName.Value);

            if(Argument!= null) return handleArgument(Argument);

            if(ListArgument != null) return handleListArgument(ListArgument);

            throw new Exception("Process Member has no property");
        }

        /// <summary>
        /// Gets the processMember if it is a VariableName.
        /// </summary>
        public Result<VariableName> AsVariableName(string propertyName)
        {
            if (VariableName != null)
                return VariableName.Value;
            return Result.Failure<VariableName>($"{propertyName} was a {MemberType}, not a VariableName");
        }

        /// <summary>
        /// Gets the processMember if it is an argument.
        /// </summary>
        public Result<IFreezableProcess> AsArgument(string propertyName)
        {
            if (Argument != null)
                return Result.Success(Argument);
            return Result.Failure<IFreezableProcess>($"{propertyName} was a {MemberType}, not an argument");
        }

        /// <summary>
        /// Gets the processMember if it is a listArgument.
        /// </summary>
        public Result<IReadOnlyList<IFreezableProcess>> AsListArgument(string propertyName)
        {
            if (ListArgument != null)
                return Result.Success(ListArgument);
            return Result.Failure<IReadOnlyList<IFreezableProcess>>($"{propertyName} was a {MemberType}, not an list argument");
        }

        /// <summary>
        /// Tries to convert this process member to a particular type.
        /// </summary>
        public Result<ProcessMember> TryConvert(MemberType convertType)
        {
            if (MemberType == convertType) return this;

            if (MemberType == MemberType.VariableName && convertType == MemberType.Process)
            {
                var getVariableProcess = GetVariableProcessFactory.CreateFreezable(VariableName!.Value);

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
            return Nullable.Equals(VariableName, other.VariableName) &&
                   Equals(Argument, other.Argument) &&
                   ListsAreEqual(ListArgument, other.ListArgument);

            static bool ListsAreEqual(IReadOnlyList<IFreezableProcess>? a1, IReadOnlyList<IFreezableProcess>? a2)
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
        public override int GetHashCode() => HashCode.Combine(VariableName, Argument, ListArgument);

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