using System;
using System.Linq;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Utilities.Processes.immutable
{
    internal static class ImmutableProcessBuilder
    {
        public static Result<ImmutableProcess, ErrorList> CreateImmutableProcess(ImmutableProcess @if,
            ImmutableProcess then, ImmutableProcess @else)
        {
            var errors = new ErrorList();
            if (then.ResultType != @else.ResultType)
                errors.Add($"Then and Else should have the same type, but their types are '{then.ResultType}' and '{@else.ResultType}'");

            if (@if is ImmutableProcess<bool> ifProcess)
            {
                if(errors.Any())
                    return Result.Failure<ImmutableProcess, ErrorList>(errors);

                ImmutableProcess ip;

                try
                {
                    ip = CreateConditional(ifProcess, then as dynamic, @else as dynamic);
                }
                catch (Exception e)
                {
                    errors.Add(e.Message);
                    return Result.Failure<ImmutableProcess, ErrorList>(errors);
                }

                return Result.Success<ImmutableProcess, ErrorList>(ip);
            }
            else
            {
                errors.Add($"If process should have type bool");
                return Result.Failure<ImmutableProcess, ErrorList>(errors);
            }
        }

        private static ImmutableProcess CreateConditional<T>(ImmutableProcess<bool> ifP, ImmutableProcess<T> thenP, ImmutableProcess<T> elseP)
        {
            return new Conditional<T>(ifP, thenP, elseP);
        }
    }
}