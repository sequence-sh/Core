using System.Collections.Generic;
using Reductech.EDR.Utilities.Processes.immutable;
using Reductech.EDR.Utilities.Processes.mutable.injection;
using Reductech.EDR.Utilities.Processes.output;

namespace Reductech.EDR.Utilities.Processes.mutable.enumerations
{
    internal class LazyCSVEnumerationElements : ImmutableProcess<IEagerEnumerationElements>, ILazyEnumerationElements
    {
        public LazyCSVEnumerationElements(
            IImmutableProcess<string> subProcess,
            string delimiter,
            string? commentToken,
            bool hasFieldsEnclosedInQuotes,
            IReadOnlyCollection<ColumnInjection> columnInjections,
            bool distinct)
        {
            _subProcess = subProcess;
            Delimiter = delimiter;
            CommentToken = commentToken;
            HasFieldsEnclosedInQuotes = hasFieldsEnclosedInQuotes;
            ColumnInjections = columnInjections;
            Distinct = distinct;
        }


        /// <inheritdoc />
        public override string Name => _subProcess.Name;

        /// <inheritdoc />
        public override IProcessConverter? ProcessConverter => null;

        /// <inheritdoc />
        public override async IAsyncEnumerable<IProcessOutput<IEagerEnumerationElements>> Execute()
        {
            string? text = null;
            var anyErrors = false;

            await foreach (var r in _subProcess.Execute())
            {
                if (r.OutputType == OutputType.Success)
                {
                    text = r.Value;
                }
                else
                {
                    if (r.OutputType == OutputType.Error)
                        anyErrors = true;

                    yield return r.ConvertTo<IEagerEnumerationElements>(); //These methods failing is expected so it should not produce an error
                }
            }

            if (!anyErrors)
            {
                if (text != null)
                {
                    var csvResult = CsvReader.TryReadCSVFromString(text, Delimiter, CommentToken, HasFieldsEnclosedInQuotes);

                    if (csvResult.IsFailure)
                        yield return ProcessOutput<IEagerEnumerationElements>.Error(csvResult.Error);
                    else
                    {
                        using var dataTable = csvResult.Value;

                        var elementsResult = CSV.ConvertDataTable(dataTable, ColumnInjections, Distinct);
                        if (elementsResult.IsSuccess)
                            yield return ProcessOutput<IEagerEnumerationElements>.Success(elementsResult.Value);
                        else
                            yield return ProcessOutput<IEagerEnumerationElements>.Error(elementsResult.Error);
                    }
                }
                else
                    yield return ProcessOutput<IEagerEnumerationElements>.Error("A CSV string was not returned");
            }
        }

        public bool HasFieldsEnclosedInQuotes { get; }
        public IReadOnlyCollection<ColumnInjection> ColumnInjections { get; }

        public string? CommentToken { get; }

        public string Delimiter { get; }

        private readonly IImmutableProcess<string> _subProcess;

        public bool Distinct { get; }
    }
}