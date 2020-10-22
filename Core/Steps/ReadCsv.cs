using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Extracts elements from a CSV file
    /// </summary>
    public sealed class ReadCsv : CompoundStep<List<List<string>>>
    {
        /// <inheritdoc />
        public override async Task< Result<List<List<string>>, IError>> Run(StateMonad stateMonad, CancellationToken cancellationToken)
        {
            var textResult = await Text.Run(stateMonad, cancellationToken);
            if (textResult.IsFailure)
                return textResult.ConvertFailure<List<List<string>>>();

            var delimiterResult = await Delimiter.Run(stateMonad, cancellationToken);
            if (delimiterResult.IsFailure)
                return delimiterResult.ConvertFailure<List<List<string>>>();

            string? commentToken;

            if (CommentToken == null)
                commentToken = null;
            else
            {
                var commentTokenResult = await CommentToken.Run(stateMonad, cancellationToken);
                if (commentTokenResult.IsFailure)
                    return commentTokenResult.ConvertFailure<List<List<string>>>();
                commentToken = commentTokenResult.Value;
            }

            var fieldsEnclosedInQuotesResult = await HasFieldsEnclosedInQuotes.Run(stateMonad, cancellationToken);
            if (fieldsEnclosedInQuotesResult.IsFailure)
                return fieldsEnclosedInQuotesResult.ConvertFailure<List<List<string>>>();

            var columnsToMapResult = await ColumnsToMap.Run(stateMonad, cancellationToken);
            if (columnsToMapResult.IsFailure)
                return columnsToMapResult.ConvertFailure<List<List<string>>>();


            var dataTableResult = CsvReader.TryReadCSVFromString(textResult.Value, delimiterResult.Value, commentToken,
                fieldsEnclosedInQuotesResult.Value);

            if (dataTableResult.IsFailure) return dataTableResult
                .MapError(x=> x.WithLocation(this))
                .ConvertFailure<List<List<string>>>();

            var missingColumnsErrors = columnsToMapResult.Value
                .Where(x => !dataTableResult.Value.Columns.Contains(x))
                .Select(x=> new SingleError($"Missing Column: '{x}'", ErrorCode.CSVError, new StepErrorLocation(this)))
                .ToList();

            if (missingColumnsErrors.Any())
                return new ErrorList(missingColumnsErrors);

            var results = new List<List<string>>();

            foreach (DataRow row in dataTableResult.Value.Rows.Cast<DataRow>())
            {
                var result = new List<string>();

                foreach (var columnName in columnsToMapResult.Value)
                {
                    var current = row[columnName, DataRowVersion.Default];

                    result.Add(current.ToString()!);
                }

                results.Add(result);
            }

            return results;
        }


        /// <summary>
        /// The text of the CSV file.
        /// </summary>
        [StepProperty(Order = 1)]
        [Required]
        public IStep<string> Text { get; set; } = null!;

        /// <summary>
        /// The delimiter to use to separate rows.
        /// </summary>
        [StepProperty(Order = 2)]
        [DefaultValueExplanation(",")]
        public IStep<string> Delimiter { get; set; } = new Constant<string>(",");

        /// <summary>
        /// The token to use to indicate comments.
        /// </summary>
        [StepProperty(Order = 3)]
        public IStep<string>? CommentToken { get; set; }

        /// <summary>
        /// Whether CSV fields are enclosed in quotes.
        /// </summary>
        [StepProperty(Order = 4)]
        [DefaultValueExplanation("false")]
        public IStep<bool> HasFieldsEnclosedInQuotes { get; set; } = new Constant<bool>(false);

        /// <summary>
        /// The csv columns to map to result columns, in order.
        /// </summary>
        [StepProperty(Order = 5)]
        [Required]
        public IStep<List<string>> ColumnsToMap { get; set; } = null!;

        /// <inheritdoc />
        public override IStepFactory StepFactory => ReadCsvStepFactory.Instance;
    }


    /// <summary>
    /// Extracts elements from a CSV file
    /// </summary>
    public sealed class ReadCsvStepFactory : SimpleStepFactory<ReadCsv, List<List<string>>>
    {
        private ReadCsvStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<ReadCsv, List<List<string>>> Instance { get; } = new ReadCsvStepFactory();
    }
}