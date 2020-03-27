using System;

namespace Reductech.EDR.Utilities.Processes.output
{
    /// <summary>
    /// The partial output of a process.
    /// </summary>
    public sealed class ProcessOutput<T> : IProcessOutput<T>
    {

        /// <summary>
        /// Creates a new success output
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IProcessOutput<T> Success(T value)
        {
            var message =
                (value is Unit)?  "Process completed successfully with no return value" :
                    $"Process completed successfully with value '{value?.ToString()}'";

            return new ProcessOutput<T>(message, OutputType.Success, value );
        }


#pragma warning disable CS8604 // Possible null reference argument.

        /// <summary>
        /// Creates a new message output.
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        public static IProcessOutput<T> Message(string error) {

            return new ProcessOutput<T>(error, OutputType.Message, default );
        }


        /// <summary>
        /// Creates a new warning output.
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        public static IProcessOutput<T> Warning(string error) {

            return new ProcessOutput<T>(error, OutputType.Warning, default );
        }

        /// <summary>
        /// Creates a new error output.
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        public static IProcessOutput<T> Error(string error) {

            return new ProcessOutput<T>(error, OutputType.Error, default );
        }

        /// <inheritdoc />
        public IProcessOutput<TN> ConvertTo<TN>()
        {
            if(OutputType == OutputType.Success)
                throw new InvalidOperationException("Cannot convert Success output type");

            return new ProcessOutput<TN>(Text, OutputType, default);
        }
#pragma warning restore CS8604 // Possible null reference argument.

        private ProcessOutput(string text, OutputType outputType, T value)
        {
            Text = text;
            OutputType = outputType;
            _value = value;
        }

        /// <inheritdoc />
        public string Text { get; }

        /// <inheritdoc />
        public OutputType OutputType { get; }

        

        private readonly T _value;

        /// <inheritdoc />
        public T Value => OutputType == OutputType.Success ? _value : 
            throw new InvalidOperationException("Cannot access outputType for ProcessOutput which is not of Success type");

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is ProcessOutput<T> pot && 
                   Text == pot.Text && 
                   OutputType == pot.OutputType;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(OutputType, Text);
        }
    }
}