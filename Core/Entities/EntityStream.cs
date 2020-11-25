using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;
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
        /// Combines streams. Does not preserve order between streams.
        /// </summary>
        public static EntityStream Combine(IReadOnlyCollection<EntityStream> streams)
        {
            if (streams.Count == 0) return Create();//Empty
            if (streams.Count == 1) return streams.Single();

            var block = new TransformBlock<Entity, Entity>(x=>x);


            foreach (var entityStream in streams)
                entityStream.Source.LinkTo(block, new DataflowLinkOptions{PropagateCompletion = false});

            Task.WhenAll(streams.Select(x => x.Source.Completion))
                .ContinueWith(x =>
            {
                block.Complete();
            });

            return new EntityStream(block);
        }

        /// <summary>
        /// Concatenates streams. Preservers order but evaluates the streams.
        /// </summary>
        public static async Task<Result<EntityStream, IError>> Concatenate(IReadOnlyCollection<EntityStream> streams, CancellationToken cancellationToken)
        {
            if (streams.Count == 0) return Create();//Empty
            if (streams.Count == 1) return streams.Single();

            var entities = new List<Entity>();

            foreach (var entityStream in streams)
            {
                var partialResult = await entityStream.TryGetResultsAsync(cancellationToken);
                if (partialResult.IsFailure) return partialResult.ConvertFailure<EntityStream>();

                entities.AddRange(partialResult.Value);
            }

            return Create(entities);
        }

        /// <summary>
        /// The source block
        /// </summary>
        public ISourceBlock<Entity> Source { get; }

        /// <summary>
        /// Gets a list of results
        /// </summary>
        public async Task<Result<IReadOnlyCollection<Entity>, IError>> TryGetResultsAsync(
            CancellationToken cancellationToken)
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
            catch (ErrorException errorException)
            {
                return Result.Failure<IReadOnlyCollection<Entity>, IError>(errorException.Error);
            }

#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            {
                return Result.Failure<IReadOnlyCollection<Entity>, IError>(new SingleError(e, ErrorCode.Unknown, EntireSequenceLocation.Instance));
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

            Source.LinkTo(b, new DataflowLinkOptions
            {
                PropagateCompletion = true
            });

            return new EntityStream(b);
        }

        /// <summary>
        /// Transforms the records in this stream
        /// </summary>
        public EntityStream Apply(Func<Entity, Task<Entity>> function)
        {
            var b = new TransformBlock<Entity, Entity>(function);

            Source.LinkTo(b, new DataflowLinkOptions
            {
                PropagateCompletion = true
            });

            return new EntityStream(b);
        }


        /// <summary>
        /// Transforms the records in this stream
        /// </summary>
        public EntityStream ApplyMaybe(Func<Entity, Maybe<Entity>> function)
        {
            var b = new TransformManyBlock<Entity, Entity>(entity => function(entity).ToList());

            Source.LinkTo(b, new DataflowLinkOptions
            {
                PropagateCompletion = true
            });

            return new EntityStream(b);
        }

        /// <summary>
        /// Transforms the records in this stream
        /// </summary>
        public EntityStream ApplyMaybe(Func<Entity, Task<Maybe<Entity>>> function)
        {
            var b = new TransformManyBlock<Entity, Entity>(x=> MapAsync(function(x)));

            Source.LinkTo(b, new DataflowLinkOptions
            {
                PropagateCompletion = true
            });

            return new EntityStream(b);
        }

        private static async Task<IEnumerable<T>> MapAsync<T>(Task<Maybe<T>> maybe)
        {
            var m = await maybe;

            return m.ToList();
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

            try
            {
                await finalBlock.Completion;
            }
            catch (AggregateException exception)
            {
                var error = ExtractError(exception);
                if (error.HasValue)
                    return Result.Failure<Unit, IError>(error.Value);
                throw;
            }

            if (finalBlock.Completion.Exception == null)
                return Unit.Default;

            var e = ExtractError(finalBlock.Completion.Exception);
            if(e.HasValue)
                return Result.Failure<Unit, IError>(e.Value);
            else
                throw finalBlock.Completion.Exception;
        }


        private static Maybe<IError> ExtractError(AggregateException aggregateException)
        {
            var l = new List<IError>();

            foreach (var innerException in aggregateException.InnerExceptions)
            {
                if (innerException is ErrorException ee)
                    l.Add(ee.Error);
                //else if (innerException is ErrorBuilderException eb)
                //    l.Add(eb.ErrorBuilder.WithLocation(errorLocation));
                else
                    return Maybe<IError>.None;
            }

            return Maybe<IError>.From(ErrorList.Combine(l));
        }
    }
}