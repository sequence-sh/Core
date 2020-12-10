using System;
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
    /// A step that returns a constant value.
    /// </summary>
    public abstract class ConstantBase<T> : IStep<T>, IConstantStep
    {
        /// <summary>
        /// Create a new Constant
        /// </summary>
        protected ConstantBase(T value) => Value = value;

        /// <inheritdoc />
        public async Task<Result<T, IError>> Run(IStateMonad stateMonad, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            return Value;
        }

        /// <summary>
        /// The value of this constant.
        /// </summary>
        public T Value { get; }

        /// <inheritdoc />
        public string Name => Value!.ToString()!;

        /// <inheritdoc />
        public object ValueObject => Value!;

        /// <inheritdoc />
        public abstract IFreezableStep Unfreeze();

        /// <inheritdoc />
        public async Task<Result<T1, IError>> Run<T1>(IStateMonad stateMonad, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var r = Value!.TryConvert<T1>()
                .MapError(x => new SingleError(x, ErrorCode.InvalidCast, new StepErrorLocation(this)) as IError);

            return r;
        }

        /// <inheritdoc />
        public Result<Unit, IError> Verify(ISettings settings) => Unit.Default;

        /// <inheritdoc />
        public Configuration? Configuration { get; set; } = null;

        /// <inheritdoc />
        public Type OutputType => typeof(T);

        /// <inheritdoc />
        public abstract Task<string> SerializeAsync(CancellationToken cancellationToken);
    }

    /// <summary>
    /// A Constant String
    /// </summary>
    public class StringConstant : ConstantBase<StringStream>
    {
        /// <inheritdoc />
        public StringConstant(StringStream value) : base(value) { }

        /// <inheritdoc />
        public override IFreezableStep Unfreeze() => new ConstantFreezableStep(Value);

        /// <inheritdoc />
        public override async Task<string> SerializeAsync(CancellationToken cancellationToken) => await Value.SerializeAsync(cancellationToken);
    }


    /// <summary>
    /// A Constant int
    /// </summary>
    public class IntConstant : ConstantBase<int>
    {
        /// <inheritdoc />
        public IntConstant(int value) : base(value) { }

        /// <inheritdoc />
        public override IFreezableStep Unfreeze() => new ConstantFreezableStep(Value);

        /// <inheritdoc />
        public override async Task<string> SerializeAsync(CancellationToken cancellationToken) => await ValueTask.FromResult(Value.ToString());
    }

    /// <summary>
    /// A constant double
    /// </summary>
    public class DoubleConstant : ConstantBase<double>
    {
        /// <inheritdoc />
        public DoubleConstant(double value) : base(value) {}

        /// <inheritdoc />
        public override IFreezableStep Unfreeze() => new ConstantFreezableStep(Value);

        /// <inheritdoc />
        public override async Task<string> SerializeAsync(CancellationToken cancellationToken) => await ValueTask.FromResult(Value.ToString("G17"));
    }

    /// <summary>
    /// A constant bool
    /// </summary>
    public class BoolConstant : ConstantBase<bool>
    {
        /// <inheritdoc />
        public BoolConstant(bool value) : base(value) {}

        /// <inheritdoc />
        public override IFreezableStep Unfreeze() => new ConstantFreezableStep(Value);

        /// <inheritdoc />
        public override async Task<string> SerializeAsync(CancellationToken cancellationToken) => await ValueTask.FromResult(Value.ToString());
    }

    /// <summary>
    /// A constant enum value
    /// </summary>
    public class EnumConstant<T> : ConstantBase<T> where T : Enum
    {
        /// <inheritdoc />
        public EnumConstant(T value) : base(value){}

        /// <inheritdoc />
        public override IFreezableStep Unfreeze()
        {
            return new ConstantFreezableStep(new Enumeration(typeof(T).Name, Value.ToString()));
        }

        /// <inheritdoc />
        public override async Task<string> SerializeAsync(CancellationToken cancellationToken)
        {
            var enumeration = new Enumeration(typeof(T).Name, Value.ToString());
            return await ValueTask.FromResult(enumeration.ToString());
        }
    }

    public static class EnumConstantHelper
    {

        /// <summary>
        /// Tries to create an enum constant from a value.
        /// Will fail if the value is not an enum.
        /// </summary>
        public static Result<IStep, IErrorBuilder> TryCreateEnumConstant(object value)
        {
            var type = value.GetType();

            if (!type.IsEnum)
                return new ErrorBuilder($"{type.Name} is not an enum type", ErrorCode.InvalidCast);

            Type stepType = typeof(EnumConstant<>).MakeGenericType(type);

            var stepAsObject = Activator.CreateInstance(stepType, value);

            try
            {
                var step = (IStep)stepAsObject!;
                return  Result.Success<IStep, IErrorBuilder>(step);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            {
                return new ErrorBuilder(e, ErrorCode.InvalidCast);
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }
    }


    /// <summary>
    /// A constant date time value
    /// </summary>
    public class DateTimeConstant : ConstantBase<DateTime>
    {
        /// <inheritdoc />
        public DateTimeConstant(DateTime value) : base(value) {}

        /// <inheritdoc />
        public override IFreezableStep Unfreeze() => new ConstantFreezableStep(Value);

        /// <inheritdoc />
        public override async Task<string> SerializeAsync(CancellationToken cancellationToken) => await ValueTask.FromResult(Value.ToString("O"));
    }

    /// <summary>
    /// A constant entity value
    /// </summary>
    public class EntityConstant : ConstantBase<Entity>
    {
        /// <inheritdoc />
        public EntityConstant(Entity value) : base(value) {}

        /// <inheritdoc />
        public override IFreezableStep Unfreeze() => new ConstantFreezableStep(Value);

        /// <inheritdoc />
        public override async Task<string> SerializeAsync(CancellationToken cancellationToken) => await ValueTask.FromResult(Value.Serialize());
    }

    /// <summary>
    /// A constant entityStream value
    /// </summary>
    public class EntityStreamConstant : ConstantBase<EntityStream>
    {
        /// <inheritdoc />
        public EntityStreamConstant(EntityStream value) : base(value)
        {
        }

        /// <inheritdoc />
        public override IFreezableStep Unfreeze() => new ConstantFreezableStep(Value);

        /// <inheritdoc />
        public override async Task<string> SerializeAsync(CancellationToken cancellationToken) => await SerializationMethods.SerializeEntityStreamAsync(Value, cancellationToken);
    }


//    /// <summary>
//    /// A step that returns a fixed value when run.
//    /// </summary>
//    public class Constant<T> : IStep<T>, IConstantStep
//    {
//        /// <summary>
//        /// Creates a new Constant.
//        /// </summary>
//        /// <param name="value"></param>
//        public Constant(T value) => Value = value;

//        /// <summary>
//        /// The value that this will return when run.
//        /// </summary>
//        public T Value { get; }



//        /// <inheritdoc />
//        public string Name => $"{Value}";

//        /// <inheritdoc />
//        public IFreezableStep Unfreeze()
//        {
//            return UnfreezeObject(Value!);

//            IFreezableStep UnfreezeObject(object o)
//            {
//                return o switch
//                {
//                    StringStream s => new ConstantFreezableStep(s),
//                    int i => new ConstantFreezableStep(i),
//                    double d => new ConstantFreezableStep(d),
//                    bool b => new ConstantFreezableStep(b),
//                    Enum e => new ConstantFreezableStep(new Enumeration(e.GetType().Name, e.ToString())),
//                    DateTime dt => new ConstantFreezableStep(dt),
//                    Entity ent => new ConstantFreezableStep(ent),
//                    EntityStream es => new ConstantFreezableStep(es),
//                    Schema schema => new ConstantFreezableStep(schema.ConvertToEntity()),
//                    IEnumerable enumerable => throw new Exception("Enumerable constant"),
//                    _ => throw new Exception($"Cannot unfreeze {typeof(T)}")
//                };
//            }

//            IFreezableStep UnfreezeList(IEnumerable enumerable)
//            {
//                var l = enumerable.Cast<object>().Select(UnfreezeObject).ToImmutableList();

//                var a = FreezableFactory.CreateFreezableList(l, null, new StepErrorLocation(this));

//                return a;
//            }
//        }

//        /// <inheritdoc />
//#pragma warning disable 1998
//        public async Task<Result<T, IError>> Run(IStateMonad stateMonad, CancellationToken cancellationToken) => Value!;

//        /// <inheritdoc />
//        public async Task<Result<T1, IError>> Run<T1>(IStateMonad stateMonad, CancellationToken cancellationToken)
//        {
//            var r = Value!.TryConvert<T1>()
//                .MapError(x => new SingleError(x, ErrorCode.InvalidCast, new StepErrorLocation(this)) as IError);

//            return r;
//        }
//        #pragma warning restore 1998

//        /// <inheritdoc />
//        public Result<Unit, IError> Verify(ISettings settings) => Unit.Default;

//        /// <inheritdoc />
//        public Configuration? Configuration { get; set; } = null;

//        /// <inheritdoc />
//        public Type OutputType => typeof(T);

//        /// <param name="cancellationToken"></param>
//        /// <inheritdoc />
//        public async Task<string> SerializeAsync(CancellationToken cancellationToken)
//        {
//            var result =  await SerializeObject(Value!, cancellationToken);

//            return result;

//            static async ValueTask<string> SerializeObject(object o, CancellationToken cancellationToken)
//            {
//                return o switch
//                {
//                    string s => SerializationMethods.DoubleQuote(s),
//                    int i => i.ToString(),
//                    double d => d.ToString("G17"),
//                    bool b => b.ToString(),
//                    Enum e => new Enumeration(e.GetType().Name, e.ToString()).ToString(),
//                    DateTime dt => dt.ToString("O"),
//                    Entity ent => ent.Serialize(),
//                    EntityStream es => await SerializeEntityStream(es, cancellationToken),
//                    StringStream ds => await ds.SerializeAsync(cancellationToken),
//                    Schema schema => schema.ConvertToEntity().Serialize(),
//                    IEnumerable enumerable => throw new Exception("Enumerable constant"),
//                _ => throw new Exception($"Cannot serialize {typeof(T)}")
//                };
//            }


//            static async ValueTask<string> SerializeEnumerable(IEnumerable enumerable, CancellationToken cancellationToken)
//            {
//                var strings = new List<string>();

//                foreach (var o in enumerable.Cast<object>())
//                {
//                    var v = await SerializeObject(o, cancellationToken);
//                    strings.Add(v);
//                }

//                var s = SerializationMethods.SerializeList(strings);
//                return s;
//            }


//            static async ValueTask<string> SerializeEntityStream(EntityStream entityStream, CancellationToken cancellationToken)
//            {
//                var entities = await entityStream.SourceEnumerable
//                    .Select(x=>x.Serialize())
//                    .ToListAsync(cancellationToken);

//                var s = SerializationMethods.SerializeList(entities);

//                return s;
//            }
//        }

//        /// <inheritdoc />
//        public object ValueObject => Value!;
//    }
}