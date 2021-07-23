using System.CodeDom.Compiler;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Util
{

public static partial class StepHelpers
{
    /// <summary>
    /// Evaluates steps and combines their results
    /// </summary>
    [GeneratedCode("CreateStepHelpers", "1")]
    public static async Task<Result<(T1, T2, T3), IError>> RunStepsAsync<T1, T2, T3>(
        this IStateMonad stateMonad,
        IStep<T1> s1,
        IStep<T2> s2,
        IStep<T3> s3,
        CancellationToken cancellationToken)
    {
        var p = await RunStepsAsync(stateMonad, s1, s2, cancellationToken);

        if (p.IsFailure)
            return p.ConvertFailure<(T1, T2, T3)>();

        var r3 = await s3.Run(stateMonad, cancellationToken);

        if (r3.IsFailure)
            return r3.ConvertFailure<(T1, T2, T3)>();

        var result = (p.Value.Item1, p.Value.Item2, r3.Value);
        return result;
    }

    /// <summary>
    /// Evaluates steps and combines their results
    /// </summary>
    [GeneratedCode("CreateStepHelpers", "1")]
    public static async Task<Result<(T1, T2, T3, T4), IError>> RunStepsAsync<T1, T2, T3, T4>(
        this IStateMonad stateMonad,
        IStep<T1> s1,
        IStep<T2> s2,
        IStep<T3> s3,
        IStep<T4> s4,
        CancellationToken cancellationToken)
    {
        var p = await RunStepsAsync(stateMonad, s1, s2, s3, cancellationToken);

        if (p.IsFailure)
            return p.ConvertFailure<(T1, T2, T3, T4)>();

        var r4 = await s4.Run(stateMonad, cancellationToken);

        if (r4.IsFailure)
            return r4.ConvertFailure<(T1, T2, T3, T4)>();

        var result = (p.Value.Item1, p.Value.Item2, p.Value.Item3, r4.Value);
        return result;
    }

    /// <summary>
    /// Evaluates steps and combines their results
    /// </summary>
    [GeneratedCode("CreateStepHelpers", "1")]
    public static async Task<Result<(T1, T2, T3, T4, T5), IError>>
        RunStepsAsync<T1, T2, T3, T4, T5>(
            this IStateMonad stateMonad,
            IStep<T1> s1,
            IStep<T2> s2,
            IStep<T3> s3,
            IStep<T4> s4,
            IStep<T5> s5,
            CancellationToken cancellationToken)
    {
        var p = await RunStepsAsync(stateMonad, s1, s2, s3, s4, cancellationToken);

        if (p.IsFailure)
            return p.ConvertFailure<(T1, T2, T3, T4, T5)>();

        var r5 = await s5.Run(stateMonad, cancellationToken);

        if (r5.IsFailure)
            return r5.ConvertFailure<(T1, T2, T3, T4, T5)>();

        var result = (p.Value.Item1, p.Value.Item2, p.Value.Item3, p.Value.Item4, r5.Value);
        return result;
    }

    /// <summary>
    /// Evaluates steps and combines their results
    /// </summary>
    [GeneratedCode("CreateStepHelpers", "1")]
    public static async Task<Result<(T1, T2, T3, T4, T5, T6), IError>>
        RunStepsAsync<T1, T2, T3, T4, T5, T6>(
            this IStateMonad stateMonad,
            IStep<T1> s1,
            IStep<T2> s2,
            IStep<T3> s3,
            IStep<T4> s4,
            IStep<T5> s5,
            IStep<T6> s6,
            CancellationToken cancellationToken)
    {
        var p = await RunStepsAsync(stateMonad, s1, s2, s3, s4, s5, cancellationToken);

        if (p.IsFailure)
            return p.ConvertFailure<(T1, T2, T3, T4, T5, T6)>();

        var r6 = await s6.Run(stateMonad, cancellationToken);

        if (r6.IsFailure)
            return r6.ConvertFailure<(T1, T2, T3, T4, T5, T6)>();

        var result = (p.Value.Item1, p.Value.Item2, p.Value.Item3, p.Value.Item4, p.Value.Item5,
                      r6.Value);

        return result;
    }

    /// <summary>
    /// Evaluates steps and combines their results
    /// </summary>
    [GeneratedCode("CreateStepHelpers", "1")]
    public static async Task<Result<(T1, T2, T3, T4, T5, T6, T7), IError>>
        RunStepsAsync<T1, T2, T3, T4, T5, T6, T7>(
            this IStateMonad stateMonad,
            IStep<T1> s1,
            IStep<T2> s2,
            IStep<T3> s3,
            IStep<T4> s4,
            IStep<T5> s5,
            IStep<T6> s6,
            IStep<T7> s7,
            CancellationToken cancellationToken)
    {
        var p = await RunStepsAsync(stateMonad, s1, s2, s3, s4, s5, s6, cancellationToken);

        if (p.IsFailure)
            return p.ConvertFailure<(T1, T2, T3, T4, T5, T6, T7)>();

        var r7 = await s7.Run(stateMonad, cancellationToken);

        if (r7.IsFailure)
            return r7.ConvertFailure<(T1, T2, T3, T4, T5, T6, T7)>();

        var result = (p.Value.Item1, p.Value.Item2, p.Value.Item3, p.Value.Item4, p.Value.Item5,
                      p.Value.Item6, r7.Value);

        return result;
    }

    /// <summary>
    /// Evaluates steps and combines their results
    /// </summary>
    [GeneratedCode("CreateStepHelpers", "1")]
    public static async Task<Result<(T1, T2, T3, T4, T5, T6, T7, T8), IError>>
        RunStepsAsync<T1, T2, T3, T4, T5, T6, T7, T8>(
            this IStateMonad stateMonad,
            IStep<T1> s1,
            IStep<T2> s2,
            IStep<T3> s3,
            IStep<T4> s4,
            IStep<T5> s5,
            IStep<T6> s6,
            IStep<T7> s7,
            IStep<T8> s8,
            CancellationToken cancellationToken)
    {
        var p = await RunStepsAsync(stateMonad, s1, s2, s3, s4, s5, s6, s7, cancellationToken);

        if (p.IsFailure)
            return p.ConvertFailure<(T1, T2, T3, T4, T5, T6, T7, T8)>();

        var r8 = await s8.Run(stateMonad, cancellationToken);

        if (r8.IsFailure)
            return r8.ConvertFailure<(T1, T2, T3, T4, T5, T6, T7, T8)>();

        var result = (p.Value.Item1, p.Value.Item2, p.Value.Item3, p.Value.Item4, p.Value.Item5,
                      p.Value.Item6, p.Value.Item7, r8.Value);

        return result;
    }
}

}
