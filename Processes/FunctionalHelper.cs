using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes
{
    /// <summary>
    /// Functional methods
    /// </summary>
    public static class FunctionalHelper
    {
        /// <summary>
        /// Tries to match this regex.
        /// </summary>
        public static bool TryMatch(this Regex regex, string s, out Match m)
        {
            m = regex.Match(s);
            return m.Success;

        }

        /// <summary>
        /// Casts this object to type T. Returns failure if the cast fails.
        /// </summary>
        public static Result<T> TryCast<T>(this object obj)
        {
            if (obj is T type)
                return type;

            return Result.Failure<T>($"Could not cast '{obj}' to {typeof(T).Name}");
        }

        /// <summary>
        /// Converts this object to type T. Returns failure if the cast fails.
        /// </summary>
        public static Result<T> TryConvert<T>(this object obj)
        {
            if (obj is T objAsT)
                return objAsT;

            var converted = Convert.ChangeType(obj, typeof(T));

            if (converted is T objConverted)
                return objConverted;

            return Result.Failure<T>($"Could not cast '{obj}' to {typeof(T).Name}");
        }

        /// <summary>
        /// Casts the elements of the sequence to type T. Returns failure if the cast fails.
        /// </summary>
        public static Result<IReadOnlyCollection<T2>> TryCastElements<T1, T2>(this IReadOnlyCollection<T1> collection) where T2 : T1 =>
            collection.Select(x =>
                x!.TryCast<T2>()).Combine().Map(x => x.ToList() as IReadOnlyCollection<T2>);


        /// <summary>
        /// Converts the elements of the sequence to type T2. Returns failure if the cast fails.
        /// </summary>
        public static Result<IReadOnlyCollection<T2>> TryConvertElements<T1, T2>(this IReadOnlyCollection<T1> collection) where T2 : T1 =>
            collection.Select(x => x!.TryConvert<T2>()).Combine().Map(x => x.ToList() as IReadOnlyCollection<T2>);


        /// <summary>
        /// Casts the elements of the sequence to type T2. Returns failure if the cast fails.
        /// </summary>
        public static Result<IReadOnlyCollection<T2>> TryConvertElements<T1, T2>(this IReadOnlyCollection<T1> collection, Func<T1, Result<T2>> tryConvert) where T2 : T1 =>
            collection.Select(tryConvert).Combine().Map(x => x.ToList() as IReadOnlyCollection<T2>);


        /// <summary>
        /// If the result is a failure, convert the error to a string.
        /// </summary>
        public static Result<T> MapFailure<T, TE>(this Result<T, TE> result, Func<TE, string> convertError)
        {
            if (result.IsSuccess) return result.Value!;

            var errorString = convertError(result.Error);

            return Result.Failure<T>(errorString);
        }


        /// <summary>
        /// If the result is a failure, convert the error to another type.
        /// </summary>
        public static Result<T, TE> MapFailure<T, TE>(this Result<T> result, Func<string, TE> convertError)
        {
            if (result.IsSuccess) return result.Value!;

            var error2 = convertError(result.Error);

            return error2!;
        }

        /// <summary>
        /// If the result is a failure, convert the error to another type.
        /// </summary>
        public static Result<T,TE2> MapFailure<T, TE1, TE2>(this Result<T, TE1> result, Func<TE1, TE2> convertError)
        {
            if (result.IsSuccess) return result.Value!;

            var error2 = convertError(result.Error);

            return error2!;
        }

        /// <summary>
        /// Casts the result to type T2.
        /// Returns failure if this cast is not possible.
        /// </summary>
        public static Result<T2> BindCast<T1, T2>(this Result<T1> result) where T2 : T1
        {
            if (result.IsFailure) return result.ConvertFailure<T2>();

            if (result.Value is T2 t2) return t2;

            return Result.Failure<T2>($"{result.Value} is not of type '{typeof(T2).Name}'");
        }


        /// <summary>
        /// Casts the result to type T2.
        /// Returns failure if this cast is not possible.
        /// </summary>
        public static Result<T2, TE> BindCast<T1, T2, TE>(this Result<T1, TE> result, TE error)// where T2 : T1
        {
            if (result.IsFailure) return result.ConvertFailure<T2>();

            if (result.Value is T2 t2) return t2;

            return Result.Failure<T2, TE>(error);
        }


        /// <summary>
        /// Casts the maybe to type T2.
        /// Returns failure if this cast is not possible.
        /// </summary>
        public static Maybe<T2> BindCast<T1, T2>(this Maybe<T1> result)  where T2 : T1
        {
            if (result.HasNoValue) return Maybe<T2>.None;

            if (result.Value is T2 t2) return t2;

            return Maybe<T2>.None;
        }


        /// <summary>
        /// Returns a single value from the sequence or a failure.
        /// </summary>
        public static Result<T> BindSingle<T>(this Result<IReadOnlyList<T>> result)
        {
            if (result.IsFailure) return result.ConvertFailure<T>();

            if (result.Value.Count == 0) return Result.Failure<T>("Sequence has no elements");
            else if(result.Value.Count > 1) return Result.Failure<T>("Sequence has more than one element");

            return result.Value.Single()!;
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


        /// <summary>
        /// Create a tuple with 2 results.
        /// Func2 will not be evaluated unless result1 is success.
        /// </summary>
        public static Result<(T1, T2), TE> Compose<T1, T2, TE>(this Result<T1, TE> result1, Func<Result<T2, TE>> func2)
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
        public static Result<(T1, T2, T3), TE> Compose<T1, T2, T3, TE>(this Result<T1, TE> result1, Func<Result<T2, TE>> func2, Func<Result<T3, TE>> func3)
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