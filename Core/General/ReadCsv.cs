using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.General
{
    /// <summary>
    /// Extracts elements from a CSV file
    /// </summary>
    public sealed class ReadCsv : CompoundStep<List<List<string>>>
    {
        /// <inheritdoc />
        public override Result<List<List<string>>, IRunErrors> Run(StateMonad stateMonad)
        {
            var textResult = Text.Run(stateMonad);
            if (textResult.IsFailure) return textResult.ConvertFailure<List<List<string>>>();

            var delimiterResult = Delimiter.Run(stateMonad);
            if (delimiterResult.IsFailure) return delimiterResult.ConvertFailure<List<List<string>>>();

            string? commentToken;

            if (CommentToken == null)
                commentToken = null;
            else
            {
                var commentTokenResult = CommentToken.Run(stateMonad);
                if (commentTokenResult.IsFailure) return commentTokenResult.ConvertFailure<List<List<string>>>();
                commentToken = commentTokenResult.Value;
            }

            var fieldsEnclosedInQuotesResult = HasFieldsEnclosedInQuotes.Run(stateMonad);
            if (fieldsEnclosedInQuotesResult.IsFailure) return fieldsEnclosedInQuotesResult.ConvertFailure<List<List<string>>>();

            var columnsToMapResult = ColumnsToMap.Run(stateMonad);
            if (columnsToMapResult.IsFailure) return columnsToMapResult.ConvertFailure<List<List<string>>>();


            var dataTableResult = CsvReader.TryReadCSVFromString(textResult.Value, delimiterResult.Value, commentToken,
                fieldsEnclosedInQuotesResult.Value);

            if (dataTableResult.IsFailure) return dataTableResult
                .MapFailure(x=> new RunError(x, nameof(RunExternalProcess), null, ErrorCode.CSVError) as IRunErrors)
                .ConvertFailure<List<List<string>>>();

            var missingColumnsErrors = columnsToMapResult.Value
                .Where(x => !dataTableResult.Value.Columns.Contains(x))
                .Select(x=> new RunError($"Missing Column: '{x}'", nameof(ReadCsv), null, ErrorCode.CSVError))
                .ToList();

            if (missingColumnsErrors.Any())
                return new RunErrorList(missingColumnsErrors);

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
        [Required]
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
        [Required]
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
}