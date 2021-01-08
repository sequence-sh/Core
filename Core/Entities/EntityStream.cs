//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using CSharpFunctionalExtensions;
//using Reductech.EDR.Core.Internal;
//using Reductech.EDR.Core.Internal.Errors;
//using Reductech.EDR.Core.Util;

//namespace Reductech.EDR.Core.Entities
//{
//    /// <summary>
//    /// A stream of entities that will be lazily evaluated
//    /// </summary>
//    public sealed class EntityStream
//    {
//        /// <summary>
//        /// Create a new EntityStream
//        /// </summary>
//        /// <param name="source"></param>
//        public EntityStream(IAsyncEnumerable<Entity> source) => SourceEnumerable = source;

//        /// <summary>
//        /// Create a new EntityStream from an enumerable
//        /// </summary>
//        public static EntityStream Create(IEnumerable<Entity> entities) => new EntityStream(entities.ToAsyncEnumerable());

//        /// <summary>
//        /// Create a new EntityStream from an enumerable
//        /// </summary>
//        public static EntityStream Create(params Entity[] entities) => Create(entities.AsEnumerable());

//        /// <summary>
//        /// Combines streams. Preserves order.
//        /// </summary>
//        public static EntityStream Concatenate(IReadOnlyCollection<EntityStream> streams)
//        {
//            if (streams.Count == 0) return Create();//Empty
//            if (streams.Count == 1) return streams.Single();

//            var newEnumerable = streams.Select(x => x.SourceEnumerable)
//                .Aggregate((a,b)=> a.Concat(b));

//            return new EntityStream(newEnumerable);
//        }

//        /// <summary>
//        /// The source block
//        /// </summary>
//        public IAsyncEnumerable<Entity> SourceEnumerable { get; }

//        /// <summary>
//        /// Gets a list of results
//        /// </summary>
//        public async Task<Result<IReadOnlyCollection<Entity>, IError>> TryGetResultsAsync(
//            CancellationToken cancellationToken)
//        {
//            try
//            {
//                var list = await SourceEnumerable.ToListAsync(cancellationToken);

//                return list;
//            }
//            catch (ErrorException errorException)
//            {
//                return Result.Failure<IReadOnlyCollection<Entity>, IError>(errorException.Error);
//            }

//#pragma warning disable CA1031 // Do not catch general exception types
//            catch (Exception e)
//            {
//                return Result.Failure<IReadOnlyCollection<Entity>, IError>(new SingleError_Core(e, ErrorCode.Unknown, EntireSequenceLocation.Instance));
//            }
//#pragma warning restore CA1031 // Do not catch general exception types
//        }

//        /// <summary>
//        /// Transforms the records in this stream
//        /// </summary>
//        public EntityStream Apply(Func<Entity, Entity> function)
//        {
//            var newEnumerable = SourceEnumerable.Select(function);

//            return new EntityStream(newEnumerable);
//        }

//        /// <summary>
//        /// Transforms the records in this stream
//        /// </summary>
//        public EntityStream Apply(Func<Entity, ValueTask<Entity>> function)
//        {
//            var newEnumerable = SourceEnumerable.SelectAwait(function);

//            return new EntityStream(newEnumerable);
//        }

//        /// <summary>
//        /// Transforms the records in this stream
//        /// </summary>
//        public EntityStream ApplyMaybe(Func<Entity, Maybe<Entity>> function)
//        {
//            var newEnumerable = SourceEnumerable
//                .SelectMany(x=> function(x).ToAsyncEnumerable());

//            return new EntityStream(newEnumerable);
//        }

//        /// <summary>
//        /// Transforms the records in this stream
//        /// </summary>
//        public EntityStream ApplyMaybe(Func<Entity, Task<Maybe<Entity>>> function)
//        {
//            var newEnumerable = SourceEnumerable
//                .SelectMany(x => function(x).ToAsyncEnumerable());

//            return new EntityStream(newEnumerable);
//        }

//        /// <summary>
//        /// Perform an action on every record.
//        /// </summary>
//        public async Task<Result<Unit, IError>> Act(Func<Entity, Task> action)
//        {
//            try
//            {
//                await foreach (var a in SourceEnumerable)
//                    await action(a);
//            }
//            catch (ErrorException exception)
//            {
//                return Result.Failure<Unit, IError>(exception.Error);
//            }

//            catch (AggregateException exception)
//            {
//                var error = ExtractError(exception);
//                if (error.HasValue)
//                    return Result.Failure<Unit, IError>(error.Value);
//                throw;
//            }

//            return Unit.Default;
//        }

//        private static Maybe<IError> ExtractError(AggregateException aggregateException)
//        {
//            var l = new List<IError>();

//            foreach (var innerException in aggregateException.InnerExceptions)
//            {
//                switch (innerException)
//                {
//                    case ErrorException ee:
//                        l.Add(ee.Error);
//                        break;
//                    case AggregateException ae:
//                    {
//                        var e = ExtractError(ae);
//                        if(e.HasValue)
//                            l.Add(e.Value);
//                        else
//                            return Maybe<IError>.None;
//                        break;
//                    }
//                    default:
//                        return Maybe<IError>.None;
//                }
//            }

//            return Maybe<IError>.From(ErrorList.Combine(l));
//        }
//    }
//}


