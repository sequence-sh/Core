using System.Collections.Generic;
using Reductech.EDR.Processes.Output;

namespace Reductech.EDR.Processes.Immutable
{
    /// <summary>
    /// An immutable process which returns a particular value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ReturnValue<T> : ImmutableProcess<T>
    {
        /// <inheritdoc />
        public ReturnValue(T value)
        {
            Value = value;
        }

        /// <summary>
        /// The value to return.
        /// </summary>
        public T Value { get; }

        /// <inheritdoc />
#pragma warning disable 1998
        public override async IAsyncEnumerable<IProcessOutput<T>> Execute()
#pragma warning restore 1998
        {
            yield return ProcessOutput<T>.Success(Value);
        }

        /// <inheritdoc />
        public override string Name => ProcessNameHelper.ReturnProcessSettingName(Value?.ToString()??"No Value");

        /// <inheritdoc />
        public override IProcessConverter? ProcessConverter => null;
    }
}