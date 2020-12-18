using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Parser;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Converts a date to the specified format, yyyy/MM/dd HH:mm:ss by default.
    /// If no date is specified, uses the current date and time.
    /// </summary>
    [Alias("DateNow")]
    [Alias("ConvertDateToString")]
    public sealed class DateToString : CompoundStep<StringStream>
    {
        /// <summary>
        /// The date and time to convert to the specified format.
        /// </summary>
        [StepProperty(1)]
        [DefaultValueExplanation("DateTime.Now")]
        public IStep<DateTime> Date { get; set; } = new DateTimeConstant(DateTime.Now);
        
        /// <summary>
        /// The output format to use for the date.
        /// </summary>
        [StepProperty(2)]
        [DefaultValueExplanation("yyyy/MM/dd HH:mm:ss")]
        [Example("O")]
        public IStep<StringStream> Format { get; set; } = new StringConstant(new StringStream("yyyy/MM/dd HH:mm:ss"));
        
        /// <inheritdoc />
        public override async Task<Result<StringStream, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {

            var dateResult = await Date.Run(stateMonad, cancellationToken);
            if (dateResult.IsFailure)
                return dateResult.ConvertFailure<StringStream>();
            
            var formatResult = await Format.Run(stateMonad, cancellationToken).Map(async x=> await x.GetStringAsync());
            if (formatResult.IsFailure)
                return formatResult.ConvertFailure<StringStream>();

            var dateOut = dateResult.Value.ToString(formatResult.Value);
            
            return new StringStream(dateOut);
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => DateToStringStepFactory.Instance;
    }

    /// <summary>
    /// Returns the 
    /// </summary>
    public sealed class DateToStringStepFactory : SimpleStepFactory<DateToString, StringStream>
    {
        private DateToStringStepFactory() { }
        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<DateToString, StringStream> Instance { get; } = new DateToStringStepFactory();
    }
}