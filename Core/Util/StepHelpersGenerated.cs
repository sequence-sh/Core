using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Util
{
    public static partial class StepHelpers
    {
        [GeneratedCode("CreateStepHelpers", "1")]
        public static async Task<Result<(T1, T2, T3), IError>> RunSteps<T1, T2, T3>(IRunnableStep<T1> s1, IRunnableStep<T2> s2,
            IRunnableStep<T3> s3, IStateMonad stateMonad, CancellationToken cancellationToken)
        {
            var p = await RunSteps(s1, s2, stateMonad, cancellationToken);
            if (p.IsFailure) return p.ConvertFailure<(T1, T2, T3)>();
            var r3 = await s3.Run(stateMonad, cancellationToken);
            if (r3.IsFailure) return r3.ConvertFailure<(T1, T2, T3)>();
            var result = (p.Value.Item1, p.Value.Item2, r3.Value);
            return result;
        }


        [GeneratedCode("CreateStepHelpers", "1")]
        public static async Task<Result<(T1, T2, T3, T4), IError>> RunSteps<T1, T2, T3, T4>(IRunnableStep<T1> s1, IRunnableStep<T2> s2,
            IRunnableStep<T3> s3, IRunnableStep<T4> s4, IStateMonad stateMonad, CancellationToken cancellationToken)
        {
            var p = await RunSteps(s1, s2, s3, stateMonad, cancellationToken);
            if (p.IsFailure) return p.ConvertFailure<(T1, T2, T3, T4)>();
            var r4 = await s4.Run(stateMonad, cancellationToken);
            if (r4.IsFailure) return r4.ConvertFailure<(T1, T2, T3, T4)>();
            var result = (p.Value.Item1, p.Value.Item2, p.Value.Item3, r4.Value);
            return result;
        }


        [GeneratedCode("CreateStepHelpers", "1")]
        public static async Task<Result<(T1, T2, T3, T4, T5), IError>> RunSteps<T1, T2, T3, T4, T5>(IRunnableStep<T1> s1,
            IRunnableStep<T2> s2, IRunnableStep<T3> s3, IRunnableStep<T4> s4, IRunnableStep<T5> s5, IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var p = await RunSteps(s1, s2, s3, s4, stateMonad, cancellationToken);
            if (p.IsFailure) return p.ConvertFailure<(T1, T2, T3, T4, T5)>();
            var r5 = await s5.Run(stateMonad, cancellationToken);
            if (r5.IsFailure) return r5.ConvertFailure<(T1, T2, T3, T4, T5)>();
            var result = (p.Value.Item1, p.Value.Item2, p.Value.Item3, p.Value.Item4, r5.Value);
            return result;
        }


        [GeneratedCode("CreateStepHelpers", "1")]
        public static async Task<Result<(T1, T2, T3, T4, T5, T6), IError>> RunSteps<T1, T2, T3, T4, T5, T6>(
            IRunnableStep<T1> s1, IRunnableStep<T2> s2, IRunnableStep<T3> s3, IRunnableStep<T4> s4, IRunnableStep<T5> s5, IRunnableStep<T6> s6, IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var p = await RunSteps(s1, s2, s3, s4, s5, stateMonad, cancellationToken);
            if (p.IsFailure) return p.ConvertFailure<(T1, T2, T3, T4, T5, T6)>();
            var r6 = await s6.Run(stateMonad, cancellationToken);
            if (r6.IsFailure) return r6.ConvertFailure<(T1, T2, T3, T4, T5, T6)>();
            var result = (p.Value.Item1, p.Value.Item2, p.Value.Item3, p.Value.Item4, p.Value.Item5, r6.Value);
            return result;
        }


        [GeneratedCode("CreateStepHelpers", "1")]
        public static async Task<Result<(T1, T2, T3, T4, T5, T6, T7), IError>> RunSteps<T1, T2, T3, T4, T5, T6, T7>(
            IRunnableStep<T1> s1, IRunnableStep<T2> s2, IRunnableStep<T3> s3, IRunnableStep<T4> s4, IRunnableStep<T5> s5, IRunnableStep<T6> s6, IRunnableStep<T7> s7,
            IStateMonad stateMonad, CancellationToken cancellationToken)
        {
            var p = await RunSteps(s1, s2, s3, s4, s5, s6, stateMonad, cancellationToken);
            if (p.IsFailure) return p.ConvertFailure<(T1, T2, T3, T4, T5, T6, T7)>();
            var r7 = await s7.Run(stateMonad, cancellationToken);
            if (r7.IsFailure) return r7.ConvertFailure<(T1, T2, T3, T4, T5, T6, T7)>();
            var result = (p.Value.Item1, p.Value.Item2, p.Value.Item3, p.Value.Item4, p.Value.Item5, p.Value.Item6,
                r7.Value);
            return result;
        }


        [GeneratedCode("CreateStepHelpers", "1")]
        public static async Task<Result<(T1, T2, T3, T4, T5, T6, T7, T8), IError>>
            RunSteps<T1, T2, T3, T4, T5, T6, T7, T8>(IRunnableStep<T1> s1, IRunnableStep<T2> s2, IRunnableStep<T3> s3, IRunnableStep<T4> s4,
                IRunnableStep<T5> s5, IRunnableStep<T6> s6, IRunnableStep<T7> s7, IRunnableStep<T8> s8, IStateMonad stateMonad,
                CancellationToken cancellationToken)
        {
            var p = await RunSteps(s1, s2, s3, s4, s5, s6, s7, stateMonad, cancellationToken);
            if (p.IsFailure) return p.ConvertFailure<(T1, T2, T3, T4, T5, T6, T7, T8)>();
            var r8 = await s8.Run(stateMonad, cancellationToken);
            if (r8.IsFailure) return r8.ConvertFailure<(T1, T2, T3, T4, T5, T6, T7, T8)>();
            var result = (p.Value.Item1, p.Value.Item2, p.Value.Item3, p.Value.Item4, p.Value.Item5, p.Value.Item6,
                p.Value.Item7, r8.Value);
            return result;
        }
    }
}
