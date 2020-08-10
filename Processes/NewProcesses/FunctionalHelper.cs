using System;
using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.NewProcesses
{
    internal static class FunctionalHelper
    {
        public static bool TryMatch(this Regex regex, string s, out Match m)
        {
            m = regex.Match(s);
            return m.Success;

        }


        /// <summary>
        /// Casts the result to type T2.
        /// Returns failure if this cast is not possible.
        /// </summary>
        public static Result<T2> BindCast<T1, T2>(this Result<T1> result)
        {
            if (result.IsFailure) return result.ConvertFailure<T2>();

            if (result.Value is T2 t2) return t2;

            return Result.Failure<T2>($"{result.Value} is not of type '{typeof(T2).Name}'");
        }

        /// <summary>
        /// Create a tuple with 2 results.
        /// Func2 will not be evaluated unless result1 is success.
        /// </summary>
        public static Result<(T1, T2)> Compose<T1, T2>(this Result<T1> result1, Func<Result<T2>> func2)
        {
            if (result1.IsFailure) return result1.ConvertFailure<(T1, T2)>();

            var result2 = func2();

            if (result2.IsFailure) return result2.ConvertFailure<(T1, T2)>();

            return (result1.Value, result2.Value);

        }

        /// <summary>
        /// Create a tuple with 3 results.
        /// Func2 will not be evaluated unless result1 is success.
        /// Func3 will not be evaluated unless the result of Func2 is success.
        /// </summary>
        public static Result<(T1, T2, T3)> Compose<T1, T2, T3>(this Result<T1> result1, Func<Result<T2>> func2, Func<Result<T3>> func3)
        {
            if (result1.IsFailure) return result1.ConvertFailure<(T1, T2, T3)>();

            var result2 = func2();

            if (result2.IsFailure) return result2.ConvertFailure<(T1, T2, T3)>();

            var result3 = func3();

            if (result3.IsFailure) return result3.ConvertFailure<(T1, T2, T3)>();

            return (result1.Value, result2.Value, result3.Value);

        }



    }
}