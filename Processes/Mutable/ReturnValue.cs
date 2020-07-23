using System;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Immutable;
using Reductech.EDR.Processes.Mutable.Chain;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Processes.Mutable
{
    /// <summary>
    /// Returns a particular value of a particular type.
    /// </summary>
    public sealed class ReturnValue : Process
    {
        /// <summary>
        /// The value to return.
        /// </summary>
        [Required]
        [YamlMember(Order = 2)]
#pragma warning disable 8618
        public object Value

        {
            get;
            set;
        }

        /// <summary>
        /// The type of the value to return.
        /// </summary>
        [Required]
        [YamlMember(Order = 2)]
        public Type Type
        {
            get;
            set;
        }
#pragma warning restore 8618

        /// <inheritdoc />
        public override string GetReturnTypeInfo()
        {
            return "The value will be cast to the required type.";
        }

        /// <inheritdoc />
        public override string GetName() => ProcessNameHelper.GetReturnValueProcessName(Value!.ToString()!);

        /// <inheritdoc />
        public override Result<IImmutableProcess<TOutput>> TryFreeze<TOutput>(IProcessSettings processSettings)
        {
            var rv = GetGenericReturnValue(Value, Type);

            if (rv.IsFailure) return rv.ConvertFailure<IImmutableProcess<TOutput>>();


            return rv.Value.TryFreeze<TOutput>(processSettings);
        }

        /// <inheritdoc />
        public override Result<ChainLinkBuilder<TInput, TFinal>> TryCreateChainLinkBuilder<TInput, TFinal>()
        {
            var rv = GetGenericReturnValue(Value, Type);

            if (rv.IsFailure) return rv.ConvertFailure<ChainLinkBuilder<TInput, TFinal>>();

            return rv.Value.TryCreateChainLinkBuilder<TInput, TFinal>();
        }


        private static Result<Process> GetGenericReturnValue(object value, Type valueType)
        {
            Process process;
            try
            {
                var dataType = new[] { valueType};
                var genericBase = typeof(GenericReturnValue<>);
                var combinedType = genericBase.MakeGenericType(dataType);
                dynamic instance = Activator.CreateInstance(combinedType)!;
                dynamic convertedValue = Convert.ChangeType(value, valueType);
                instance.Value = convertedValue;
                process = (Process) instance;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            {
                return Result.Failure<Process>(e.Message);
            }
#pragma warning restore CA1031 // Do not catch general exception types

            return process;
        }

        private class GenericReturnValue<T> : Process
        {
            /// <summary>
            /// The value to return
            /// </summary>
#pragma warning disable 8618
            // ReSharper disable once MemberCanBePrivate.Local
            public T Value
#pragma warning restore 8618
            {
                get;
                // ReSharper disable once UnusedAutoPropertyAccessor.Local
                set;
            }


            /// <inheritdoc />
            public override string GetReturnTypeInfo()
            {
                return "The value will be cast to the required type.";
            }

            /// <inheritdoc />
            public override string GetName() => ProcessNameHelper.GetReturnValueProcessName(Value!.ToString()!);

            /// <inheritdoc />
            public override Result<IImmutableProcess<TOutput>> TryFreeze<TOutput>(IProcessSettings processSettings)
            {
                return TryConvertFreezeResult<TOutput, T>(new ReturnValue<T>(Value));
            }

            /// <inheritdoc />
            public override Result<ChainLinkBuilder<TInput, TFinal>> TryCreateChainLinkBuilder<TInput, TFinal>()
            {
                return new ChainLinkBuilder<TInput, T, TFinal, ReturnValue<T>, GenericReturnValue<T>>(this);
            }
        }
    }





}