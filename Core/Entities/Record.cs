using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Entities
{
    /// <summary>
    /// A stream of records that will be lazily evaluated
    /// </summary>
    public sealed class RecordStream
    {
        /// <summary>
        /// Create a new RecordStream
        /// </summary>
        /// <param name="source"></param>
        public RecordStream(ISourceBlock<Record> source) => Source = source;

        /// <summary>
        /// The source block
        /// </summary>
        public ISourceBlock<Record> Source { get; }

        /// <summary>
        /// Transforms the records in the this stream
        /// </summary>
        public RecordStream Apply(Func<Record, Record> function)
        {
            var b = new TransformBlock<Record, Record>(function);

            Source.LinkTo(b, new DataflowLinkOptions()
            {
                PropagateCompletion = true
            });

            return new RecordStream(b);
        }


        /// <summary>
        /// Perform an action on every record.
        /// </summary>
        public async Task<Result<Unit, IError>> Act(Func<Record, Task> action, IErrorLocation errorLocation)
        {
            var finalBlock = new ActionBlock<Record>(action);

            Source.LinkTo(finalBlock, new DataflowLinkOptions
            {
                PropagateCompletion = true
            });

            await finalBlock.Completion;

            if (finalBlock.Completion.Exception == null)
                return Unit.Default;

            var e = ExtractError(finalBlock.Completion.Exception, errorLocation);
            if(e.HasValue)
                return Result.Failure<Unit, IError>(e.Value);
            else
                throw finalBlock.Completion.Exception;
        }


        private static Maybe<IError> ExtractError(AggregateException aggregateException, IErrorLocation errorLocation)
        {
            var l = new List<IError>();

            foreach (var innerException in aggregateException.InnerExceptions)
            {
                if (innerException is ErrorException ee)
                    l.Add(ee.Error);
                else if (innerException is ErrorBuilderException eb)
                    l.Add(eb.ErrorBuilder.WithLocation(errorLocation));
                else
                    return Maybe<IError>.None;
            }

            return ErrorList.Combine(l);
        }
    }




    /// <summary>
    /// A piece of data.
    /// </summary>
    public sealed class Record : IEnumerable<IGrouping<string, string>>
    {
        private readonly ILookup<string, string> _fields;

        /// <summary>
        /// Create a new record.
        /// </summary>
        /// <param name="fields"></param>
        public Record(IEnumerable<KeyValuePair<string, string>> fields) =>
            _fields = fields.ToLookup(x => x.Key, x => x.Value);

        /// <summary>
        /// Gets the names of different fields on this object.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetFieldNames() => _fields.Select(x => x.Key);

        /// <summary>
        /// Creates a copy of this with the new fields added or updated.
        /// </summary>
        /// <param name="newFields"></param>
        /// <returns></returns>
        public Record WithFields(IReadOnlyCollection<KeyValuePair<string, string>> newFields)
        {
            var usedKeys = new HashSet<string>(newFields.Select(x => x.Key));

            var allNewKeyValuePairs = newFields
                .Concat(_fields.Where(x => !usedKeys.Contains(x.Key))
                    .SelectMany(group => group.Select(x => new KeyValuePair<string, string>(group.Key, x))));

            return new Record(allNewKeyValuePairs);
        }

        /// <summary>
        /// Gets the values of a particular field.
        /// </summary>
        public IEnumerable<string> this[string key] => _fields[key];

        /// <summary>
        /// Gets the values of a particular field.
        /// </summary>
        public IEnumerable<string> GetField(string key) => this[key];

        IEnumerator<IGrouping<string, string>> IEnumerable<IGrouping<string, string>>.GetEnumerator() => _fields.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _fields.GetEnumerator();

        /// <inheritdoc />
        public override string ToString() => AsString();


        /// <summary>
        /// Converts this record into a string.
        /// </summary>
        /// <returns></returns>
        public string AsString()
        {
            var result = string.Join(", ",
                _fields.Select(field => $"{field.Key}: {string.Join(";", field)}"));

            return result;
        }

    }
}
