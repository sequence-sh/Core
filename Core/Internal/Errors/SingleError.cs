using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Core;

namespace Reductech.EDR.Core.Internal.Errors
{
    /// <summary>
    /// An error without a location.
    /// </summary>
    public interface IErrorBuilder
    {
        /// <summary>
        /// Converts this errorBuilder to an error
        /// </summary>
        public IError WithLocation(IErrorLocation location);

        /// <summary>
        /// The error builders.
        /// </summary>
        public IEnumerable<ErrorBuilder> GetErrorBuilders();

        public string AsString { get; }
    }

    public static class ErrorLocationHelper
    {
        public static IError WithLocation(this IErrorBuilder errorBuilder, IStep step)=> errorBuilder.WithLocation(new StepErrorLocation(step));
        public static IError WithLocation(this IErrorBuilder errorBuilder, IFreezableStep step)=> errorBuilder.WithLocation(new FreezableStepErrorLocation(step));
        public static IError WithLocation(this IErrorBuilder errorBuilder, IStepFactory factory, FreezableStepData data)=> errorBuilder.WithLocation(new FreezableStepErrorLocation(factory, data));

    }


    public class EntireSequenceLocation : IErrorLocation
    {
        private EntireSequenceLocation() {}

        public static IErrorLocation Instance { get; } = new EntireSequenceLocation();

        /// <inheritdoc />
        public string AsString => "Entire Sequence";
    }

    /// <summary>
    /// The location in the yaml where the error occured.
    /// </summary>
    public class YamlErrorLocation : IErrorLocation
    {
        public YamlErrorLocation(YamlException yamlException) : this(yamlException.Start, yamlException.End)
        {
        }

        public YamlErrorLocation(Mark start, Mark end)
        {
            Start = start;
            End = end;
        }

        public Mark Start { get; }

        public Mark End { get; }

        /// <inheritdoc />
        public string AsString => $"{Start} - {End}";
    }

    /// <summary>
    /// The step where the error originated
    /// </summary>
    public class StepErrorLocation : IErrorLocation
    {
        /// <summary>
        /// Creates a new StepErrorLocation
        /// </summary>
        /// <param name="step"></param>
        public StepErrorLocation(IStep step) => Step = step;

        /// <summary>
        /// The step
        /// </summary>
        public IStep Step { get; }

        /// <inheritdoc />
        public string AsString => Step.Name;
    }

    /// <summary>
    /// The freezable step where the error originated
    /// </summary>
    public class FreezableStepErrorLocation : IErrorLocation
    {
        /// <summary>
        /// Creates a new FreezableStepErrorLocation
        /// </summary>
        public FreezableStepErrorLocation(IFreezableStep freezableStep) => FreezableStep = freezableStep;

        /// <summary>
        /// Creates a new FreezableStepErrorLocation
        /// </summary>
        public FreezableStepErrorLocation(IStepFactory stepFactory, FreezableStepData data) => FreezableStep = new CompoundFreezableStep(stepFactory, data, null);

        /// <summary>
        /// The freezable step
        /// </summary>
        public IFreezableStep FreezableStep { get; }

        /// <inheritdoc />
        public string AsString => FreezableStep.StepName;
    }

    /// <summary>
    /// An error without a location.
    /// </summary>
    public class ErrorBuilder : IErrorBuilder
    {
        /// <summary>
        /// Create a new error.
        /// </summary>
        public ErrorBuilder(string message, ErrorCode errorCode, IError? innerError = null)
        {
            Message = message;
            InnerError = innerError;
            Exception = null;
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Create a new error with an exception.
        /// </summary>
        public ErrorBuilder(Exception exception,  ErrorCode errorCode)
        {
            Message = exception.Message;
            InnerError = null;
            Exception = exception;
            ErrorCode = errorCode;
        }


        /// <summary>
        /// Error Message Text.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// The error that caused this error.
        /// </summary>
        public IError? InnerError { get; }

        /// <summary>
        /// Associated Exception if there is one
        /// </summary>
        public Exception? Exception { get; }

        /// <summary>
        /// The error code.
        /// </summary>
        public ErrorCode ErrorCode { get; }


        /// <summary>
        /// Returns a SingleError with the given location.
        /// </summary>
        public SingleError WithLocationSingle(IErrorLocation location)
        {
            if (Exception != null)
                return new SingleError(Exception, ErrorCode, location);

            return new SingleError(Message, ErrorCode, location, InnerError);
        }

        /// <inheritdoc />
        public IError WithLocation(IErrorLocation location) => WithLocationSingle(location);

        /// <inheritdoc />
        public IEnumerable<ErrorBuilder> GetErrorBuilders()
        {
            yield return this;
        }

        /// <inheritdoc />
        public string AsString => Message;

        /// <inheritdoc />
        public override string ToString() => AsString;
    }

    /// <summary>
    /// More than one errorBuilders
    /// </summary>
    public class ErrorBuilderList : IErrorBuilder
    {
        /// <summary>
        /// Create a new ErrorBuilderList
        /// </summary>
        public ErrorBuilderList(IEnumerable<ErrorBuilder> errorBuilders) => ErrorBuilders = errorBuilders.ToList();

        /// <summary>
        /// The errorBuilders
        /// </summary>
        public IReadOnlyCollection<ErrorBuilder> ErrorBuilders { get; }

        /// <inheritdoc />
        public IError WithLocation(IErrorLocation location) => new ErrorList(ErrorBuilders.Select(x=>x.WithLocationSingle(location)).ToList());

        /// <inheritdoc />
        public IEnumerable<ErrorBuilder> GetErrorBuilders() => ErrorBuilders;

        /// <inheritdoc />
        public string AsString => string.Join("; ", ErrorBuilders.Select(x => x.Message));

        /// <inheritdoc />
        public override string ToString() => AsString;

        /// <summary>
        /// Combine multiple ErrorBuilders
        /// </summary>
        public static IErrorBuilder Combine(IEnumerable<IErrorBuilder> errorBuilders) => new ErrorBuilderList(errorBuilders.SelectMany(x=>x.GetErrorBuilders()));
    }


    /// <summary>
    /// The location from which the error originates.
    /// </summary>
    public interface IErrorLocation
    {
        /// <summary>
        /// The error location as a string.
        /// </summary>
        string AsString { get; }
    }

    /// <summary>
    /// An single error.
    /// </summary>
    public class SingleError : IError
    {
        /// <summary>
        /// Create a new error.
        /// </summary>
        public SingleError(string message, ErrorCode errorCode, IErrorLocation location, IError? innerError = null)
        {
            Message = message;
            Location = location;
            InnerError = innerError;
            Exception = null;
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Create a new error with an exception.
        /// </summary>
        public SingleError(Exception exception, ErrorCode errorCode, IErrorLocation location)
        {
            Message = exception.Message;
            Location = location;
            InnerError = null;
            Exception = exception;
            ErrorCode = errorCode;
            Timestamp = DateTime.Now;
        }


        /// <summary>
        /// Error Message Text.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// The time the error was created.
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// The location where this error arose. This could be a line number.
        /// </summary>
        public IErrorLocation Location { get; }

        /// <summary>
        /// The error that caused this error.
        /// </summary>
        public IError? InnerError { get; }

        /// <summary>
        /// Associated Exception if there is one
        /// </summary>
        public Exception? Exception { get; }

        /// <summary>
        /// The error code.
        /// </summary>
        public ErrorCode ErrorCode { get; }

        /// <inheritdoc />
        public IEnumerable<SingleError> GetAllErrors()
        {
            yield return this;
        }

        /// <inheritdoc />
        public string AsString => Message;

        /// <inheritdoc />
        public override string ToString() => AsString;
    }
}