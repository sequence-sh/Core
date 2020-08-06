using System;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.NewProcesses
{
    internal static class FunctionalHelper
    {
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




        ///// <summary>
        ///// Combine two results.
        ///// </summary>
        //public static Result<(T1, T2)> Combine<T1, T2>(this (Result<T1> result1, Result<T2> result2) tuple, string? errorMessageSeparator = null)
        //{
        //    var (result1, result2) = tuple;
        //    var array = new Result[] {result1, result2};

        //    if (array.All(x => x.IsSuccess))
        //        return (result1.Value, result2.Value);

        //    return array.Combine(errorMessageSeparator).ConvertFailure<(T1, T2)>();
        //}


        ///// <summary>
        ///// Combine three results.
        ///// </summary>
        //public static Result<(T1, T2, T3)> Combine<T1, T2, T3>(this (Result<T1> result1, Result<T2> result2, Result<T3> result3) tuple, string? errorMessageSeparator = null)
        //{
        //    var (result1, result2, result3) = tuple;
        //    var array = new Result[] { result1, result2, result3 };

        //    if (array.All(x => x.IsSuccess))
        //        return (result1.Value, result2.Value, result3.Value);

        //    return array.Combine(errorMessageSeparator).ConvertFailure<(T1, T2, T3)>();

        //}


    }
}