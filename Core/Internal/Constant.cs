using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Parser;
using Reductech.EDR.Core.Serialization;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// A step that returns a fixed value when run.
    /// </summary>
    public class Constant<T> : IStep<T>, IConstantStep
    {
        /// <summary>
        /// Creates a new Constant.
        /// </summary>
        /// <param name="value"></param>
        public Constant(T value) => Value = value;

        /// <summary>
        /// The value that this will return when run.
        /// </summary>
        public T Value { get; }



        /// <inheritdoc />
        public string Name => $"{Value}";

        /// <inheritdoc />
        public IFreezableStep Unfreeze()
        {
            return Value switch
            {
                string s => new ConstantFreezableStep(s),
                int i => new ConstantFreezableStep(i),
                double d => new ConstantFreezableStep(d),
                bool b => new ConstantFreezableStep(b),
                Enum e => new ConstantFreezableStep(new Enumeration(e.GetType().Name, e.ToString())),
                DateTime dt => new ConstantFreezableStep(dt),
                Entities.Entity ent => new ConstantFreezableStep(ent),
                EntityStream es => new ConstantFreezableStep(es),
                DataStream ds => new ConstantFreezableStep(ds),
                //TODO list
                _ => throw new Exception($"Cannot unfreeze {typeof(T)}")
            };
        }

        /// <inheritdoc />
#pragma warning disable 1998
        public async Task<Result<T, IError>> Run(IStateMonad stateMonad, CancellationToken cancellationToken) => Value!;

        /// <inheritdoc />
        public async Task<Result<T1, IError>> Run<T1>(IStateMonad stateMonad, CancellationToken cancellationToken)
        {
            var r = Value!.TryConvert<T1>()
                .MapError(x => new SingleError(x, ErrorCode.InvalidCast, new StepErrorLocation(this)) as IError);

            return r;
        }
        #pragma warning restore 1998

        /// <inheritdoc />
        public Result<Unit, IError> Verify(ISettings settings) => Unit.Default;

        /// <inheritdoc />
        public Configuration? Configuration { get; set; } = null;

        /// <inheritdoc />
        public Type OutputType => typeof(T);

        /// <param name="cancellationToken"></param>
        /// <inheritdoc />
        public async Task<string> SerializeAsync(CancellationToken cancellationToken)
        {
            return Value switch
            {
                string s => SerializationMethods.DoubleQuote(s),
                int i => i.ToString(),
                double d => d.ToString("G17"),
                bool b =>b.ToString(),
                Enum e => new Enumeration(e.GetType().Name, e.ToString()).ToString(),
                DateTime dt => dt.ToString("O"),
                Entities.Entity ent => ent.Serialize(),
                EntityStream es => await SerializeEntityStream(es),
                DataStream ds =>ds.Serialize(),
                //TODO list
                _ => throw new Exception($"Cannot unfreeze {typeof(T)}")
            };


            static async Task<string> SerializeEntityStream(EntityStream entityStream)
            {
                var entities = await entityStream.SourceEnumerable
                    .Select(x=>x.Serialize())
                    .ToListAsync();

                var s = SerializationMethods.SerializeList(entities);

                return s;
            }
        }

        /// <inheritdoc />
        public object ValueObject => Value;
    }
}