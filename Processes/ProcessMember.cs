using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes
{
    /// <summary>
    /// Any member of a process.
    /// </summary>
    public sealed class ProcessMember
    {
        public ProcessMember(VariableName variableName) => VariableName = variableName;
        public ProcessMember(IFreezableProcess argument) => Argument = argument;
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

        public VariableName? VariableName { get; set; }

        public IFreezableProcess? Argument { get; set; }

        public IReadOnlyList<IFreezableProcess>? ListArgument { get; set; }


        public T Join<T>(Func<VariableName, T> handleVariableName, Func<IFreezableProcess, T> handleArgument,
            Func<IReadOnlyList<IFreezableProcess>, T> handleListArgument)
        {
            if (VariableName != null) return handleVariableName(VariableName.Value);

            if(Argument!= null) return handleArgument(Argument);

            if(ListArgument != null) return handleListArgument(ListArgument);

            throw new Exception("Process Member has no property");
        }

        public Result<VariableName> AsVariableName(string propertyName)
        {
            if (VariableName != null) return VariableName.Value;
            return Result.Failure<VariableName>($"{propertyName} was not a VariableName");
        }

        public Result<IFreezableProcess> AsArgument(string propertyName)
        {
            if (Argument != null) return Result.Success(Argument);
            return Result.Failure<IFreezableProcess>($"{propertyName} was not an argument");
        }

        public Result<IReadOnlyList<IFreezableProcess>> AsListArgument(string propertyName)
        {
            if (ListArgument != null) return Result.Success(ListArgument);
            return Result.Failure<IReadOnlyList<IFreezableProcess>>($"{propertyName} was not an list argument");
        }

        /// <summary>
        /// A string representation of the member.
        /// </summary>
        public string MemberString => VariableName?.ToString()??Argument?.ToString()??ListArgument?.ToString()??"Unknown";

        /// <inheritdoc />
        public override string ToString() => new {MemberType, Value=MemberString}.ToString()!;
    }
}