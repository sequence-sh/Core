using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Entities
{
    /// <summary>
    /// A stream of entities that will be lazily evaluated
    /// </summary>
    public sealed class EntityStream
    {
        /// <summary>
        /// Create a new EntityStream
        /// </summary>
        /// <param name="source"></param>
        public EntityStream(ISourceBlock<Entity> source) => Source = source;

        /// <summary>
        /// The source block
        /// </summary>
        public ISourceBlock<Entity> Source { get; }

        /// <summary>
        /// Transforms the records in the this stream
        /// </summary>
        public EntityStream Apply(Func<Entity, Entity> function)
        {
            var b = new TransformBlock<Entity, Entity>(function);

            Source.LinkTo(b, new DataflowLinkOptions()
            {
                PropagateCompletion = true
            });

            return new EntityStream(b);
        }


        /// <summary>
        /// Perform an action on every record.
        /// </summary>
        public async Task<Result<Unit, IError>> Act(Func<Entity, Task> action, IErrorLocation errorLocation)
        {
            var finalBlock = new ActionBlock<Entity>(action);

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
}