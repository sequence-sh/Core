using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        /// Create a new EntityStream from an enumerable
        /// </summary>
        public static EntityStream Create(IEnumerable<Entity> entities)
        {
            var block = new TransformBlock<Entity, Entity>(x=>x);

            foreach (var entity in entities)
            {
                block.Post(entity);
            }
            block.Complete();

            return new EntityStream(block);
        }

        /// <summary>
        /// Create a new EntityStream from an enumerable
        /// </summary>
        public static EntityStream Create(params Entity[] entities) => Create(entities.AsEnumerable());

        /// <summary>
        /// The source block
        /// </summary>
        public ISourceBlock<Entity> Source { get; }

        /// <summary>
        /// Gets a list of results
        /// </summary>
        public async Task<Result<IReadOnlyCollection<Entity>>> TryGetResultsAsync(CancellationToken cancellationToken)
        {
            var list = new List<Entity>();
            try
            {
                while (await Source.OutputAvailableAsync(cancellationToken))
                {
                    var r = await Source.ReceiveAsync(cancellationToken);
                    list.Add(r);
                }

                await Source.Completion;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            {
                return Result.Failure<IReadOnlyCollection<Entity>>(e.Message);
            }
#pragma warning restore CA1031 // Do not catch general exception types

            return list;
        }

        /// <summary>
        /// Transforms the records in this stream
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