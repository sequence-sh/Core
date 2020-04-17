using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes.immutable;
using Reductech.EDR.Utilities.Processes.mutable.injection;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Utilities.Processes.mutable.enumerations
{
    /// <summary>
    /// Enumerates through a CSV file.
    /// </summary>
    public class CSV : Enumeration
    {

        private enum CSVSource
        {
            File, Text, Process   
        }

        internal static Result<EagerEnumerationElements, ErrorList> ConvertDataTable 
            (DataTable dataTable, IReadOnlyDictionary<string, Injection> injectColumns, bool distinct)
        {
            var errors = new ErrorList();
            var injectors = new List<IProcessInjector>();
            var usedInjectors = distinct ? new HashSet<ProcessInjector>() : null; 

            var columnInjections = new List<(Injection injection, DataColumn column)>();

            foreach (var (columnName, value) in injectColumns)
            {
                var column = dataTable.Columns[columnName];
                if (column == null) errors.Add($"Could not find column '{columnName}'");
                else
                    columnInjections.Add((value, column));
            }

            if (errors.Any())
                return Result.Failure<EagerEnumerationElements, ErrorList>(errors);

            foreach (var dataTableRow in dataTable.Rows.Cast<DataRow>())
            {
                if (dataTableRow == null) continue;
                var processInjector = new ProcessInjector();
                foreach (var (injection, column) in columnInjections)
                {
                    var val = dataTableRow[column];
                    var stringValue = val?.ToString() ?? string.Empty;

                    processInjector.Add(stringValue, injection);
                }

                if(usedInjectors == null || usedInjectors.Add(processInjector))
                    injectors.Add(processInjector);
            }
            return Result.Success<EagerEnumerationElements, ErrorList>(new EagerEnumerationElements(injectors));
        }


        /// <inheritdoc />
        internal override Result<IEnumerationElements, ErrorList> TryGetElements(IProcessSettings processSettings)
        {
            var sources = new List<CSVSource>();

                if(CSVFilePath != null) sources.Add(CSVSource.File);
                if(CSVText != null) sources.Add(CSVSource.Text);
                if(CSVProcess != null) sources.Add(CSVSource.Process);

                if (sources.Count == 0)
                    return Result.Failure<IEnumerationElements, ErrorList>(new ErrorList
                        {$"Either {nameof(CSVFilePath)}, {nameof(CSVText)}, or {nameof(CSVProcess)} should be set."});
                if (sources.Count > 1)
                    return Result.Failure<IEnumerationElements, ErrorList>(new ErrorList
                    {
                        $"Only one of {nameof(CSVFilePath)}, {nameof(CSVText)}, and {nameof(CSVProcess)} may be set."
                    });


                Result<DataTable, ErrorList> csvResult;

                switch (sources.Single())
                {
                    case CSVSource.File when CSVFilePath != null:
                    {
                        csvResult = CsvReader.TryReadCSVFromFile(CSVFilePath, Delimiter, CommentToken, HasFieldsEnclosedInQuotes); }
                        break;
                    case CSVSource.Text when CSVText != null:
                        csvResult = CsvReader.TryReadCSVFromString(CSVText, Delimiter, CommentToken, HasFieldsEnclosedInQuotes);
                        break;
                    case CSVSource.Process when CSVProcess != null:
                    {
                        var subProcessFreezeResult = CSVProcess.TryFreeze(processSettings);

                        if (subProcessFreezeResult.IsFailure) return subProcessFreezeResult.ConvertFailure<IEnumerationElements>();
                        switch (subProcessFreezeResult.Value)
                        {
                            case ImmutableProcess<string> stringProcess:
                            {
                                var lazyElements = new LazyCSVEnumerationElements(stringProcess, Delimiter, CommentToken, HasFieldsEnclosedInQuotes, 
                                    new ReadOnlyDictionary<string, Injection>(InjectColumns), Distinct);
                                return Result.Success<IEnumerationElements, ErrorList>(lazyElements);
                            }
                            default:
                                return Result.Failure<IEnumerationElements, ErrorList>(new ErrorList
                                {
                                    $"{nameof(CSVProcess)} should have type string"
                                });
                        }
                    }
                    default: return Result.Failure<IEnumerationElements, ErrorList>(new ErrorList
                    {
                        "Something went wrong getting CSV elements"
                    });
                }

                if (csvResult.IsFailure) return csvResult.ConvertFailure<IEnumerationElements>();

                using var dataTable = csvResult.Value;

                var r = ConvertDataTable(dataTable, InjectColumns, Distinct);
                return r.Map(x => x as IEnumerationElements);
        }

        internal override string Name
        {
            get
            {
                if (CSVFilePath != null)
                    return $"Csv from '{CSVFilePath}'";

                else if (CSVText != null)
                    return CSVText.Split("\n")[0];

                return "CSV";
            }
        }

        /// <summary>
        /// The path to the CSV file.
        /// Either this, CSVText, or CSVProcess must be set (but not more than one).
        /// </summary>
        
        [YamlMember]
        public string? CSVFilePath { get; set; }
        
        /// <summary>
        /// Raw CSV.
        /// Either this, CSVFilePath, or CSVProcess must be set (but not more than one).
        /// </summary>
        
        [YamlMember]
        public string? CSVText { get; set; }

        /// <summary>
        /// A process which produces a string in CSV format.
        /// Either this, CSVFilePath, or CSVText must be set (but not more than one).
        /// </summary>
        [YamlMember]
        public Process? CSVProcess { get; set; }


#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        /// <summary>
        /// List of mappings from CSV headers to property injection.
        /// </summary>
        [Required]
        [YamlMember]
        public Dictionary<string, Injection>  InjectColumns { get; set; }

        /// <summary>
        /// The delimiter used in the CSV file.
        /// </summary>
        [Required]
        
        [YamlMember]
        public string Delimiter { get; set; } = ",";
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        /// <summary>
        /// A string that, when placed at the beginning of a line, indicates that the line is a comment and should be ignored by the parser.
        /// </summary>
        
        [YamlMember]
        public string? CommentToken { get; set; }

        /// <summary>
        /// Determines whether fields are enclosed in quotation marks.
        /// </summary>
        
        [YamlMember]
        public bool HasFieldsEnclosedInQuotes { get; set; } = false;

        /// <summary>
        /// Whether to only enumerate unique values from the CSV.
        /// Uniqueness is determined only from the columns which are being injected.
        /// </summary>
        [YamlMember]
        public bool Distinct { get; set; }
    }
}