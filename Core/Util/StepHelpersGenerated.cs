using System.CodeDom.Compiler;

namespace Reductech.EDR.Core.Util;

public static partial class StepHelpers
{
    /// <summary>
    /// Evaluates steps and combines their results
    /// </summary>
    [GeneratedCode("CreateStepHelpers", "1")]
    public static async Task<Result<(T1, T2, T3), IError>> RunStepsAsync<T1, T2, T3>(
        this IStateMonad stateMonad,
        IRunnableStep<T1> s1,
        IRunnableStep<T2> s2,
        IRunnableStep<T3> s3,
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
        IRunnableStep<T1> s1,
        IRunnableStep<T2> s2,
        IRunnableStep<T3> s3,
        IRunnableStep<T4> s4,
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
            IRunnableStep<T1> s1,
            IRunnableStep<T2> s2,
            IRunnableStep<T3> s3,
            IRunnableStep<T4> s4,
            IRunnableStep<T5> s5,
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
            IRunnableStep<T1> s1,
            IRunnableStep<T2> s2,
            IRunnableStep<T3> s3,
            IRunnableStep<T4> s4,
            IRunnableStep<T5> s5,
            IRunnableStep<T6> s6,
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
            IRunnableStep<T1> s1,
            IRunnableStep<T2> s2,
            IRunnableStep<T3> s3,
            IRunnableStep<T4> s4,
            IRunnableStep<T5> s5,
            IRunnableStep<T6> s6,
            IRunnableStep<T7> s7,
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
            IRunnableStep<T1> s1,
            IRunnableStep<T2> s2,
            IRunnableStep<T3> s3,
            IRunnableStep<T4> s4,
            IRunnableStep<T5> s5,
            IRunnableStep<T6> s6,
            IRunnableStep<T7> s7,
            IRunnableStep<T8> s8,
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

    /// <summary>
    /// Evaluates steps and combines their results
    /// </summary>
    [GeneratedCode("CreateStepHelpers", "1")]
    public static async Task<Result<(T1, T2, T3, T4, T5, T6, T7, T8, T9), IError>>
        RunStepsAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            this IStateMonad stateMonad,
            IRunnableStep<T1> s1,
            IRunnableStep<T2> s2,
            IRunnableStep<T3> s3,
            IRunnableStep<T4> s4,
            IRunnableStep<T5> s5,
            IRunnableStep<T6> s6,
            IRunnableStep<T7> s7,
            IRunnableStep<T8> s8,
            IRunnableStep<T9> s9,
            CancellationToken cancellationToken)
    {
        var p = await RunStepsAsync(stateMonad, s1, s2, s3, s4, s5, s6, s7, s8, cancellationToken);

        if (p.IsFailure)
            return p.ConvertFailure<(T1, T2, T3, T4, T5, T6, T7, T8, T9)>();

        var r9 = await s9.Run(stateMonad, cancellationToken);

        if (r9.IsFailure)
            return r9.ConvertFailure<(T1, T2, T3, T4, T5, T6, T7, T8, T9)>();

        var result = (p.Value.Item1, p.Value.Item2, p.Value.Item3, p.Value.Item4, p.Value.Item5,
                      p.Value.Item6, p.Value.Item7, p.Value.Item8, r9.Value);

        return result;
    }

    /// <summary>
    /// Evaluates steps and combines their results
    /// </summary>
    [GeneratedCode("CreateStepHelpers", "1")]
    public static async Task<Result<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10), IError>>
        RunStepsAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            this IStateMonad stateMonad,
            IRunnableStep<T1> s1,
            IRunnableStep<T2> s2,
            IRunnableStep<T3> s3,
            IRunnableStep<T4> s4,
            IRunnableStep<T5> s5,
            IRunnableStep<T6> s6,
            IRunnableStep<T7> s7,
            IRunnableStep<T8> s8,
            IRunnableStep<T9> s9,
            IRunnableStep<T10> s10,
            CancellationToken cancellationToken)
    {
        var p = await RunStepsAsync(
            stateMonad,
            s1,
            s2,
            s3,
            s4,
            s5,
            s6,
            s7,
            s8,
            s9,
            cancellationToken
        );

        if (p.IsFailure)
            return p.ConvertFailure<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10)>();

        var r10 = await s10.Run(stateMonad, cancellationToken);

        if (r10.IsFailure)
            return r10.ConvertFailure<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10)>();

        var result = (p.Value.Item1, p.Value.Item2, p.Value.Item3, p.Value.Item4, p.Value.Item5,
                      p.Value.Item6, p.Value.Item7, p.Value.Item8, p.Value.Item9, r10.Value);

        return result;
    }

    /// <summary>
    /// Evaluates steps and combines their results
    /// </summary>
    [GeneratedCode("CreateStepHelpers", "1")]
    public static async Task<Result<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11), IError>>
        RunStepsAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
            this IStateMonad stateMonad,
            IRunnableStep<T1> s1,
            IRunnableStep<T2> s2,
            IRunnableStep<T3> s3,
            IRunnableStep<T4> s4,
            IRunnableStep<T5> s5,
            IRunnableStep<T6> s6,
            IRunnableStep<T7> s7,
            IRunnableStep<T8> s8,
            IRunnableStep<T9> s9,
            IRunnableStep<T10> s10,
            IRunnableStep<T11> s11,
            CancellationToken cancellationToken)
    {
        var p = await RunStepsAsync(
            stateMonad,
            s1,
            s2,
            s3,
            s4,
            s5,
            s6,
            s7,
            s8,
            s9,
            s10,
            cancellationToken
        );

        if (p.IsFailure)
            return p.ConvertFailure<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11)>();

        var r11 = await s11.Run(stateMonad, cancellationToken);

        if (r11.IsFailure)
            return r11.ConvertFailure<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11)>();

        var result = (p.Value.Item1, p.Value.Item2, p.Value.Item3, p.Value.Item4, p.Value.Item5,
                      p.Value.Item6, p.Value.Item7, p.Value.Item8, p.Value.Item9, p.Value.Item10,
                      r11.Value);

        return result;
    }

    /// <summary>
    /// Evaluates steps and combines their results
    /// </summary>
    [GeneratedCode("CreateStepHelpers", "1")]
    public static async Task<Result<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12), IError>>
        RunStepsAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
            this IStateMonad stateMonad,
            IRunnableStep<T1> s1,
            IRunnableStep<T2> s2,
            IRunnableStep<T3> s3,
            IRunnableStep<T4> s4,
            IRunnableStep<T5> s5,
            IRunnableStep<T6> s6,
            IRunnableStep<T7> s7,
            IRunnableStep<T8> s8,
            IRunnableStep<T9> s9,
            IRunnableStep<T10> s10,
            IRunnableStep<T11> s11,
            IRunnableStep<T12> s12,
            CancellationToken cancellationToken)
    {
        var p = await RunStepsAsync(
            stateMonad,
            s1,
            s2,
            s3,
            s4,
            s5,
            s6,
            s7,
            s8,
            s9,
            s10,
            s11,
            cancellationToken
        );

        if (p.IsFailure)
            return p.ConvertFailure<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12)>();

        var r12 = await s12.Run(stateMonad, cancellationToken);

        if (r12.IsFailure)
            return r12.ConvertFailure<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12)>();

        var result = (p.Value.Item1, p.Value.Item2, p.Value.Item3, p.Value.Item4, p.Value.Item5,
                      p.Value.Item6, p.Value.Item7, p.Value.Item8, p.Value.Item9, p.Value.Item10,
                      p.Value.Item11, r12.Value);

        return result;
    }

    /// <summary>
    /// Evaluates steps and combines their results
    /// </summary>
    [GeneratedCode("CreateStepHelpers", "1")]
    public static async
        Task<Result<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13), IError>>
        RunStepsAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
            this IStateMonad stateMonad,
            IRunnableStep<T1> s1,
            IRunnableStep<T2> s2,
            IRunnableStep<T3> s3,
            IRunnableStep<T4> s4,
            IRunnableStep<T5> s5,
            IRunnableStep<T6> s6,
            IRunnableStep<T7> s7,
            IRunnableStep<T8> s8,
            IRunnableStep<T9> s9,
            IRunnableStep<T10> s10,
            IRunnableStep<T11> s11,
            IRunnableStep<T12> s12,
            IRunnableStep<T13> s13,
            CancellationToken cancellationToken)
    {
        var p = await RunStepsAsync(
            stateMonad,
            s1,
            s2,
            s3,
            s4,
            s5,
            s6,
            s7,
            s8,
            s9,
            s10,
            s11,
            s12,
            cancellationToken
        );

        if (p.IsFailure)
            return p.ConvertFailure<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13)>();

        var r13 = await s13.Run(stateMonad, cancellationToken);

        if (r13.IsFailure)
            return r13.ConvertFailure<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13)>();

        var result = (p.Value.Item1, p.Value.Item2, p.Value.Item3, p.Value.Item4, p.Value.Item5,
                      p.Value.Item6, p.Value.Item7, p.Value.Item8, p.Value.Item9, p.Value.Item10,
                      p.Value.Item11, p.Value.Item12, r13.Value);

        return result;
    }

    /// <summary>
    /// Evaluates steps and combines their results
    /// </summary>
    [GeneratedCode("CreateStepHelpers", "1")]
    public static async
        Task<Result<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14), IError>>
        RunStepsAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
            this IStateMonad stateMonad,
            IRunnableStep<T1> s1,
            IRunnableStep<T2> s2,
            IRunnableStep<T3> s3,
            IRunnableStep<T4> s4,
            IRunnableStep<T5> s5,
            IRunnableStep<T6> s6,
            IRunnableStep<T7> s7,
            IRunnableStep<T8> s8,
            IRunnableStep<T9> s9,
            IRunnableStep<T10> s10,
            IRunnableStep<T11> s11,
            IRunnableStep<T12> s12,
            IRunnableStep<T13> s13,
            IRunnableStep<T14> s14,
            CancellationToken cancellationToken)
    {
        var p = await RunStepsAsync(
            stateMonad,
            s1,
            s2,
            s3,
            s4,
            s5,
            s6,
            s7,
            s8,
            s9,
            s10,
            s11,
            s12,
            s13,
            cancellationToken
        );

        if (p.IsFailure)
            return p
                .ConvertFailure<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14)>();

        var r14 = await s14.Run(stateMonad, cancellationToken);

        if (r14.IsFailure)
            return r14
                .ConvertFailure<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14)>();

        var result = (p.Value.Item1, p.Value.Item2, p.Value.Item3, p.Value.Item4, p.Value.Item5,
                      p.Value.Item6, p.Value.Item7, p.Value.Item8, p.Value.Item9, p.Value.Item10,
                      p.Value.Item11, p.Value.Item12, p.Value.Item13, r14.Value);

        return result;
    }

    /// <summary>
    /// Evaluates steps and combines their results
    /// </summary>
    [GeneratedCode("CreateStepHelpers", "1")]
    public static async
        Task<Result<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15), IError>>
        RunStepsAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
            this IStateMonad stateMonad,
            IRunnableStep<T1> s1,
            IRunnableStep<T2> s2,
            IRunnableStep<T3> s3,
            IRunnableStep<T4> s4,
            IRunnableStep<T5> s5,
            IRunnableStep<T6> s6,
            IRunnableStep<T7> s7,
            IRunnableStep<T8> s8,
            IRunnableStep<T9> s9,
            IRunnableStep<T10> s10,
            IRunnableStep<T11> s11,
            IRunnableStep<T12> s12,
            IRunnableStep<T13> s13,
            IRunnableStep<T14> s14,
            IRunnableStep<T15> s15,
            CancellationToken cancellationToken)
    {
        var p = await RunStepsAsync(
            stateMonad,
            s1,
            s2,
            s3,
            s4,
            s5,
            s6,
            s7,
            s8,
            s9,
            s10,
            s11,
            s12,
            s13,
            s14,
            cancellationToken
        );

        if (p.IsFailure)
            return p
                .ConvertFailure<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15
                    )>();

        var r15 = await s15.Run(stateMonad, cancellationToken);

        if (r15.IsFailure)
            return r15
                .ConvertFailure<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15
                    )>();

        var result = (p.Value.Item1, p.Value.Item2, p.Value.Item3, p.Value.Item4, p.Value.Item5,
                      p.Value.Item6, p.Value.Item7, p.Value.Item8, p.Value.Item9, p.Value.Item10,
                      p.Value.Item11, p.Value.Item12, p.Value.Item13, p.Value.Item14, r15.Value);

        return result;
    }

    /// <summary>
    /// Evaluates steps and combines their results
    /// </summary>
    [GeneratedCode("CreateStepHelpers", "1")]
    public static async
        Task<Result<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16),
            IError>>
        RunStepsAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(
            this IStateMonad stateMonad,
            IRunnableStep<T1> s1,
            IRunnableStep<T2> s2,
            IRunnableStep<T3> s3,
            IRunnableStep<T4> s4,
            IRunnableStep<T5> s5,
            IRunnableStep<T6> s6,
            IRunnableStep<T7> s7,
            IRunnableStep<T8> s8,
            IRunnableStep<T9> s9,
            IRunnableStep<T10> s10,
            IRunnableStep<T11> s11,
            IRunnableStep<T12> s12,
            IRunnableStep<T13> s13,
            IRunnableStep<T14> s14,
            IRunnableStep<T15> s15,
            IRunnableStep<T16> s16,
            CancellationToken cancellationToken)
    {
        var p = await RunStepsAsync(
            stateMonad,
            s1,
            s2,
            s3,
            s4,
            s5,
            s6,
            s7,
            s8,
            s9,
            s10,
            s11,
            s12,
            s13,
            s14,
            s15,
            cancellationToken
        );

        if (p.IsFailure)
            return p
                .ConvertFailure<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15,
                    T16)>();

        var r16 = await s16.Run(stateMonad, cancellationToken);

        if (r16.IsFailure)
            return r16
                .ConvertFailure<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15,
                    T16)>();

        var result = (p.Value.Item1, p.Value.Item2, p.Value.Item3, p.Value.Item4, p.Value.Item5,
                      p.Value.Item6, p.Value.Item7, p.Value.Item8, p.Value.Item9, p.Value.Item10,
                      p.Value.Item11, p.Value.Item12, p.Value.Item13, p.Value.Item14,
                      p.Value.Item15, r16.Value);

        return result;
    }
}
