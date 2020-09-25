using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Extracts elements from a CSV file
    /// </summary>
    public sealed class ReadCsvProcessFactory : SimpleRunnableProcessFactory<ReadCsv, List<List<string>>>
    {
        private ReadCsvProcessFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleRunnableProcessFactory<ReadCsv, List<List<string>>> Instance { get; } = new ReadCsvProcessFactory();
    }


    /// <summary>
    /// Extracts elements from a CSV file
    /// </summary>
    public sealed class ReadCsv : CompoundRunnableProcess<List<List<string>>>
    {
        /// <inheritdoc />
        public override Result<List<List<string>>, IRunErrors> Run(ProcessState processState)
        {
            var textResult = Text.Run(processState);
            if (textResult.IsFailure) return textResult.ConvertFailure<List<List<string>>>();

            var delimiterResult = Delimiter.Run(processState);
            if (delimiterResult.IsFailure) return delimiterResult.ConvertFailure<List<List<string>>>();

            string? commentToken;

            if (CommentToken == null)
                commentToken = null;
            else
            {
                var commentTokenResult = CommentToken.Run(processState);
                if (commentTokenResult.IsFailure) return commentTokenResult.ConvertFailure<List<List<string>>>();
                commentToken = commentTokenResult.Value;
            }

            var fieldsEnclosedInQuotesResult = HasFieldsEnclosedInQuotes.Run(processState);
            if (fieldsEnclosedInQuotesResult.IsFailure) return fieldsEnclosedInQuotesResult.ConvertFailure<List<List<string>>>();

            var columnsToMapResult = ColumnsToMap.Run(processState);
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
        [RunnableProcessPropertyAttribute(Order = 1)]
        [Required]
        public IRunnableProcess<string> Text { get; set; } = null!;

        /// <summary>
        /// The delimiter to use to separate rows.
        /// </summary>
        [RunnableProcessPropertyAttribute(Order = 2)]
        [Required]
        [DefaultValueExplanation(",")]
        public IRunnableProcess<string> Delimiter { get; set; } = new Constant<string>(",");

        /// <summary>
        /// The token to use to indicate comments.
        /// </summary>
        [RunnableProcessPropertyAttribute(Order = 3)]
        public IRunnableProcess<string>? CommentToken { get; set; }

        /// <summary>
        /// Whether CSV fields are enclosed in quotes.
        /// </summary>
        [RunnableProcessPropertyAttribute(Order = 4)]
        [DefaultValueExplanation("false")]
        [Required]
        public IRunnableProcess<bool> HasFieldsEnclosedInQuotes { get; set; } = new Constant<bool>(false);

        /// <summary>
        /// The csv columns to map to result columns, in order.
        /// </summary>
        [RunnableProcessPropertyAttribute(Order = 5)]
        [Required]
        public IRunnableProcess<List<string>> ColumnsToMap { get; set; } = null!;

        /// <inheritdoc />
        public override IRunnableProcessFactory RunnableProcessFactory => ReadCsvProcessFactory.Instance;
    }
}