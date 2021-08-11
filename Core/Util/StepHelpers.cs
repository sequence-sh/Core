using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using OneOf;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Util
{

/// <summary>
/// Contains methods to help steps evaluate their parameters
/// </summary>
public static partial class StepHelpers
{
    /// <summary>
    /// Converts this to a RunnableStep that returns an array of string
    /// WARNING: Only use this if you want the array to be fully evaluated immediately
    /// </summary>
    public static IRunnableStep<IReadOnlyList<string>> WrapStringStreamArray(
        this IRunnableStep<Array<StringStream>> step)
    {
        return new StringStreamArrayWrapper(step);
    }

    /// <summary>
    /// Converts this to a RunnableStep that returns a list.
    /// WARNING: Only use this if you want the array to be fully evaluated immediately
    /// </summary>
    public static IRunnableStep<IReadOnlyList<T>> WrapArray<T>(this IRunnableStep<Array<T>> step)
    {
        return new ArrayWrapper<T>(step);
    }

    /// <summary>
    /// Converts this to a RunnableStep that returns a string
    /// </summary>
    public static IRunnableStep<string> WrapStringStream(this IRunnableStep<StringStream> step)
    {
        return new StringStreamWrapper(step);
    }

    /// <summary>
    /// Converts this to a RunnableStep with a nullable result type
    /// </summary>
    public static IRunnableStep<Maybe<T>> WrapNullable<T>(this IRunnableStep<T>? step)
    {
        return new NullableStepWrapper<T>(step);
    }

    /// <summary>
    /// Converts this to a RunnableStep with a nullable result type
    /// </summary>
    public static IRunnableStep<Maybe<T2>> WrapNullable<T1, T2>(
        this IRunnableStep<T1>? step,
        Func<IRunnableStep<T1>, IRunnableStep<T2>> map)
    {
        if (step is null)
            return new NullableStepWrapper<T2>(null);

        var mappedStep = map(step);

        return new NullableStepWrapper<T2>(mappedStep);
    }

    /// <summary>
    /// Wraps a step with entity conversion
    /// </summary>
    /// <typeparam name="T">The type to convert the entity into</typeparam>
    /// <param name="step">The step which returns the entity</param>
    /// <param name="parentStep">The parent step (for error location)</param>
    /// <returns></returns>
    public static IRunnableStep<T> WrapEntityConversion<T>(
        this IRunnableStep<Entity> step,
        IStep parentStep)
    {
        return new EntityConversionWrapper<T>(step, parentStep);
    }

    /// <summary>
    /// Evaluates steps and combines their results
    /// </summary>
    public static async Task<Result<(T1, T2), IError>> RunStepsAsync<T1, T2>(
        this IStateMonad stateMonad,
        IRunnableStep<T1> s1,
        IRunnableStep<T2> s2,
        CancellationToken cancellationToken)
    {
        var r1 = await s1.Run(stateMonad, cancellationToken);

        if (r1.IsFailure)
            return r1.ConvertFailure<(T1, T2)>();

        var r2 = await s2.Run(stateMonad, cancellationToken);

        if (r2.IsFailure)
            return r2.ConvertFailure<(T1, T2)>();

        return (r1.Value, r2.Value);
    }

    /// <summary>
    /// Lets you apply mappings to the result of a one of step
    /// </summary>
    public static IRunnableStep<OneOf<T0B, T1B>> WrapOneOf<T0A, T1A, T0B, T1B>(
        this IRunnableStep<OneOf<T0A, T1A>> step,
        Func<T0A, CancellationToken, Task<Result<T0B, IError>>> map0,
        Func<T1A, CancellationToken, Task<Result<T1B, IError>>> map1)
    {
        return new OneOfWrapper<T0A, T1A, T0B, T1B>(step, map0, map1);
    }

    /// <summary>
    /// Lets you apply mappings to the result of a one of step
    /// </summary>
    public static IRunnableStep<OneOf<T0B, T1B, T2B>> WrapOneOf<T0A, T1A, T2A, T0B, T1B, T2B>(
        this IRunnableStep<OneOf<T0A, T1A, T2A>> step,
        Func<T0A, CancellationToken, Task<Result<T0B, IError>>> map0,
        Func<T1A, CancellationToken, Task<Result<T1B, IError>>> map1,
        Func<T2A, CancellationToken, Task<Result<T2B, IError>>> map2)
    {
        return new OneOfWrapper<T0A, T1A, T2A, T0B, T1B, T2B>(step, map0, map1, map2);
    }

    /// <summary>
    /// Does nothing
    /// </summary>
    public static Task<Result<T, IError>> NoMap<T>(T t, CancellationToken _)
    {
        return Task.FromResult(Result.Success<T, IError>(t));
    }

    /// <summary>
    /// Converts a StringStream to a String
    /// </summary>
    public static async Task<Result<string, IError>> MapStringStream(
        StringStream s,
        CancellationToken cancellationToken)
    {
        return await s.GetStringAsync();
    }

    /// <summary>
    /// Converts an array to a list
    /// </summary>
    public static Task<Result<IReadOnlyList<T>, IError>> MapArray<T>(
        Array<T> array,
        CancellationToken cancellationToken)
    {
        return array.GetElementsAsync(cancellationToken);
    }

    private class OneOfWrapper<T0A, T1A, T0B, T1B> : IRunnableStep<OneOf<T0B, T1B>>
    {
        private readonly IRunnableStep<OneOf<T0A, T1A>> _step;
        private readonly Func<T0A, CancellationToken, Task<Result<T0B, IError>>> _map0;
        private readonly Func<T1A, CancellationToken, Task<Result<T1B, IError>>> _map1;

        public OneOfWrapper(
            IRunnableStep<OneOf<T0A, T1A>> step,
            Func<T0A, CancellationToken, Task<Result<T0B, IError>>> map0,
            Func<T1A, CancellationToken, Task<Result<T1B, IError>>> map1)
        {
            _step = step;
            _map0 = map0;
            _map1 = map1;
        }

        /// <inheritdoc />
        public async Task<Result<OneOf<T0B, T1B>, IError>> Run(
            IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var r1 = await _step.Run(stateMonad, cancellationToken);

            if (r1.IsFailure)
                return r1.ConvertFailure<OneOf<T0B, T1B>>();

            if (r1.Value.TryPickT0(out var t0A, out var t1A))
            {
                var r2 = await _map0(t0A, cancellationToken)
                    .Map(OneOf<T0B, T1B>.FromT0);

                return r2;
            }
            else
            {
                var r2 = await _map1(t1A, cancellationToken)
                    .Map(OneOf<T0B, T1B>.FromT1);

                return r2;
            }
        }
    }

    private class OneOfWrapper<T0A, T1A, T2A, T0B, T1B, T2B> : IRunnableStep<OneOf<T0B, T1B, T2B>>
    {
        private readonly IRunnableStep<OneOf<T0A, T1A, T2A>> _step;
        private readonly Func<T0A, CancellationToken, Task<Result<T0B, IError>>> _map0;
        private readonly Func<T1A, CancellationToken, Task<Result<T1B, IError>>> _map1;
        private readonly Func<T2A, CancellationToken, Task<Result<T2B, IError>>> _map2;

        public OneOfWrapper(
            IRunnableStep<OneOf<T0A, T1A, T2A>> step,
            Func<T0A, CancellationToken, Task<Result<T0B, IError>>> map0,
            Func<T1A, CancellationToken, Task<Result<T1B, IError>>> map1,
            Func<T2A, CancellationToken, Task<Result<T2B, IError>>> map2)
        {
            _step = step;
            _map0 = map0;
            _map1 = map1;
            _map2 = map2;
        }

        /// <inheritdoc />
        public async Task<Result<OneOf<T0B, T1B, T2B>, IError>> Run(
            IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var r1 = await _step.Run(stateMonad, cancellationToken);

            if (r1.IsFailure)
                return r1.ConvertFailure<OneOf<T0B, T1B, T2B>>();

            if (r1.Value.TryPickT0(out var t0A, out var oo2))
            {
                var r2 = await _map0(t0A, cancellationToken)
                    .Map(OneOf<T0B, T1B, T2B>.FromT0);

                return r2;
            }
            else if (oo2.TryPickT0(out var t1A, out var t2A))
            {
                var r2 = await _map1(t1A, cancellationToken)
                    .Map(OneOf<T0B, T1B, T2B>.FromT1);

                return r2;
            }
            else
            {
                var r2 = await _map2(t2A, cancellationToken)
                    .Map(OneOf<T0B, T1B, T2B>.FromT2);

                return r2;
            }
        }
    }

    private class ArrayWrapper<T> : IRunnableStep<IReadOnlyList<T>>
    {
        private readonly IRunnableStep<Array<T>> _step;

        public ArrayWrapper(IRunnableStep<Array<T>> step)
        {
            _step = step;
        }

        /// <inheritdoc />
        public async Task<Result<IReadOnlyList<T>, IError>> Run(
            IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var r = await _step.Run(stateMonad, cancellationToken)
                .Bind(x => x.GetElementsAsync(cancellationToken));

            return r;
        }
    }

    private class StringStreamWrapper : IRunnableStep<string>
    {
        private readonly IRunnableStep<StringStream> _step;

        public StringStreamWrapper(IRunnableStep<StringStream> step)
        {
            _step = step;
        }

        /// <inheritdoc />
        public async Task<Result<string, IError>> Run(
            IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            return await _step.Run(stateMonad, cancellationToken).Map(x => x.GetStringAsync());
        }
    }

    private class NullableStepWrapper<T> : IRunnableStep<Maybe<T>>
    {
        private readonly IRunnableStep<T>? _step;

        public NullableStepWrapper(IRunnableStep<T>? step)
        {
            _step = step;
        }

        /// <inheritdoc />
        public async Task<Result<Maybe<T>, IError>> Run(
            IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            if (_step is null)
                return Result.Success<Maybe<T>, IError>(Maybe<T>.None);

            var r = await _step.Run(stateMonad, cancellationToken);

            if (r.IsFailure)
                return r.ConvertFailure<Maybe<T>>();

            return Maybe<T>.From(r.Value);
        }
    }

    private class StringStreamArrayWrapper : IRunnableStep<IReadOnlyList<string>>
    {
        private readonly IRunnableStep<Array<StringStream>> _step;

        public StringStreamArrayWrapper(IRunnableStep<Array<StringStream>> step)
        {
            _step = step;
        }

        /// <inheritdoc />
        public async Task<Result<IReadOnlyList<string>, IError>> Run(
            IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var r = await _step.Run(stateMonad, cancellationToken)
                .Bind(x => x.GetElementsAsync(cancellationToken));

            if (r.IsFailure)
                return r.ConvertFailure<IReadOnlyList<string>>();

            var l = new List<string>(r.Value.Count);

            foreach (var stringStream in r.Value)
            {
                var s = await stringStream.GetStringAsync();
                l.Add(s);
            }

            return l;
        }
    }

    private class EntityConversionWrapper<T> : IRunnableStep<T>
    {
        private readonly IRunnableStep<Entity> _step;
        private readonly IStep _parentStep;

        public EntityConversionWrapper(IRunnableStep<Entity> step, IStep parentStep)
        {
            _step       = step;
            _parentStep = parentStep;
        }

        /// <inheritdoc />
        public async Task<Result<T, IError>> Run(
            IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var eResult = await _step.Run(stateMonad, cancellationToken);

            if (eResult.IsFailure)
                return eResult.ConvertFailure<T>();

            var conversionResult = EntityConversionHelpers.TryCreateFromEntity<T>(eResult.Value)
                .MapError(x => x.WithLocation(_parentStep));

            if (conversionResult.IsFailure)
                return conversionResult.ConvertFailure<T>();

            return conversionResult.Value;
        }
    }
}

}
