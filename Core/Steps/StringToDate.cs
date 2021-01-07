using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Internal.Parser;
using Reductech.EDR.Core.Parser;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Converts a date to the specified format, yyyyMMddHHmm by default.
    /// If no date is specified, returns the current time.
    /// </summary>
    [Alias("ConvertStringToDate")]
    public sealed class StringToDate : CompoundStep<DateTime>
    {
        /// <summary>
        /// The string to convert to DateTime
        /// </summary>
        [StepProperty(1)]
        [Required]
        [Example("2020/11/22 20:55:11")]
        [Alias("String")]
        public IStep<StringStream> Date { get; set; } = null!;

        /// <summary>
        /// The input format to use for conversion.
        /// If not set, will use DateTime.Parse by default.
        /// </summary>
        [StepProperty(2)]
        [DefaultValueExplanation("Will use DateTime to try and convert.")]
        [Example("yyyy/MM/dd HH:mm:ss")]
        public IStep<StringStream>? InputFormat { get; set; } = null;

        /// <summary>
        /// The culture to use for date conversion. Default is current culture.
        /// </summary>
        [StepProperty(3)]
        [DefaultValueExplanation("Current culture")]
        [Example("en-GB")]
        public IStep<StringStream> Culture { get; set; } = new StringConstant(
            new StringStream(CultureInfo.CurrentCulture.Name));

        /// <inheritdoc />
        protected override async Task<Result<DateTime, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {

            var dateResult = await Date.Run(stateMonad, cancellationToken).Map(async x => await x.GetStringAsync());
            if (dateResult.IsFailure)
                return dateResult.ConvertFailure<DateTime>();

            string? inputFormat = null;
            
            if (InputFormat != null)
            {
                var inputFormatResult = await InputFormat.Run(stateMonad, cancellationToken)
                    .Map(async x => await x.GetStringAsync());
                if (inputFormatResult.IsFailure)
                    return inputFormatResult.ConvertFailure<DateTime>();
                inputFormat = inputFormatResult.Value;
            }

            var cultureResult = await Culture.Run(stateMonad, cancellationToken).Map(async x => await x.GetStringAsync());
            if (cultureResult.IsFailure)
                return cultureResult.ConvertFailure<DateTime>();

            CultureInfo ci;

            try
            {
                ci = new CultureInfo(cultureResult.Value);
            }
            catch (CultureNotFoundException)
            {
                return new SingleError(
                    $"Culture is not supported. {cultureResult.Value} is an invalid culture identifier.",
                    ErrorCode.CouldNotParse,
                    new StepErrorLocation(this)
                );
            }

            DateTime date;
            
            if (inputFormat == null)
            {
                try
                {
                    date = DateTime.Parse(dateResult.Value, ci);
                }
                catch (FormatException fe)
                {
                    return new SingleError(fe, ErrorCode.CouldNotParse, new StepErrorLocation(this));
                }
            }
            else
            {
                try
                {
                    date = DateTime.ParseExact(dateResult.Value, inputFormat, ci);
                }
                catch (FormatException fe)
                {
                    return new SingleError(fe, ErrorCode.CouldNotParse, new StepErrorLocation(this));
                }
            }

            return date;
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => StringToDateStepFactory.Instance;
    }

    /// <summary>
    /// Parses a string and returns a DateTime object.
    /// </summary>
    public sealed class StringToDateStepFactory : SimpleStepFactory<StringToDate, DateTime>
    {
        private StringToDateStepFactory() { }
        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<StringToDate, DateTime> Instance { get; } = new StringToDateStepFactory();
    }
}