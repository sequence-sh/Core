namespace Reductech.Sequence.Core.Util;

/// <summary>
/// Contains helper methods for running steps
/// </summary>
public static partial class StepHelpers
{
    /// <summary>
    /// Evaluates steps and combines their results
    /// </summary>
    public static async Task<Result<T1, IError>> RunStepsAsync<T1>(
        this IStateMonad stateMonad,
        IRunnableStep<T1> s1,
        CancellationToken cancellationToken)
    {
        var r1 = await s1.Run(stateMonad, cancellationToken);
        return r1;
    }

    /// <summary>
    /// Evaluates steps and combines their results
    /// </summary>
    public static async Task<Result<(T1, T2), IError>> RunStepsAsync<T1, T2>(
        this IStateMonad stateMonad,
        IRunnableStep<T1> s1,
        IRunnableStep<T2> s2,
        CancellationToken cancellationToken)
    {
        var r1 = await s1.Run(stateMonad, cancellationToken);

        if (r1.IsFailure)
            return r1.ConvertFailure<(T1, T2)>();

        var r2 = await s2.Run(stateMonad, cancellationToken);

        if (r2.IsFailure)
            return r2.ConvertFailure<(T1, T2)>();

        return (r1.Value, r2.Value);
    }

    /// <summary>
    /// Wrap the output of a step - automatically converting it to a different type
    /// </summary>
    public static IRunnableStep<TOut> WrapStep<TIn, TOut>(
        this IRunnableStep<TIn> step,
        IStepValueMap<TIn, TOut> map)
    {
        return new StepValueMapper<TIn, TOut>(step, map);
    }

    /// <summary>
    /// Converts this to a RunnableStep that returns an array of string
    /// WARNING: Only use this if you want the array to be fully evaluated immediately
    /// </summary>
    public static IRunnableStep<IReadOnlyList<string>> WrapStringStreamArray(
        this IRunnableStep<Array<StringStream>> step) => WrapStep(
        step,
        StepMaps.Array(StepMaps.String())
    );

    /// <summary>
    /// Converts this to a RunnableStep that returns a list.
    /// WARNING: Only use this if you want the array to be fully evaluated immediately
    /// </summary>
    public static IRunnableStep<IReadOnlyList<T>> WrapArray<T>(this IRunnableStep<Array<T>> step)
        where T : ISCLObject => WrapStep(step, StepMaps.Array<T>());

    /// <summary>
    /// Converts this to a RunnableStep that returns a string
    /// </summary>
    public static IRunnableStep<string> WrapStringStream(this IStep<StringStream> step) =>
        WrapStep(step, StepMaps.String());

    /// <summary>
    /// Maps a OneOf with two type options
    /// </summary>
    public static IRunnableStep<OneOf<TOut0, TOut1>>
        WrapOneOf<TIn0, TIn1, TOut0, TOut1>(
            this IRunnableStep<SCLOneOf<TIn0, TIn1>> step,
            IStepValueMap<TIn0, TOut0> map0,
            IStepValueMap<TIn1, TOut1> map1)
        where TIn0 : ISCLObject
        where TIn1 : ISCLObject => WrapStep(step, StepMaps.OneOf(map0, map1));

    /// <summary>
    /// Maps a OneOf with three type options
    /// </summary>
    public static IRunnableStep<OneOf<TOut0, TOut1, TOut2>>
        WrapOneOf<TIn0, TIn1, TIn2, TOut0, TOut1, TOut2>(
            this IRunnableStep<SCLOneOf<TIn0, TIn1, TIn2>> step,
            IStepValueMap<TIn0, TOut0> map0,
            IStepValueMap<TIn1, TOut1> map1,
            IStepValueMap<TIn2, TOut2> map2)
        where TIn0 : ISCLObject
        where TIn1 : ISCLObject
        where TIn2 : ISCLObject => WrapStep(step, StepMaps.OneOf(map0, map1, map2));

    /// <summary>
    /// Converts this to a RunnableStep with a nullable result type
    /// </summary>
    public static IRunnableStep<Maybe<T>> WrapNullable<T>(this IRunnableStep<T>? step) =>
        WrapNullable(step, StepMaps.DoNothing<T>());

    /// <summary>
    /// Converts this to a RunnableStep with a nullable result type
    /// </summary>
    public static IRunnableStep<Maybe<TOut>> WrapNullable<TIn, TOut>(
        this IRunnableStep<TIn>? step,
        IStepValueMap<TIn, TOut> map)
    {
        if (step is null)
            return new NullableStepWrapper<TIn, TOut>(null, map);

        return new NullableStepWrapper<TIn, TOut>(step, map);
    }

    /// <summary>
    /// Wraps a step with entity conversion
    /// </summary>
    /// <typeparam name="T">The type to convert the entity into</typeparam>
    /// <param name="step">The step which returns the entity</param>
    /// <param name="parentStep">The parent step (for error location)</param>
    /// <returns></returns>
    public static IRunnableStep<T> WrapEntityConversion<T>(
        this IRunnableStep<Entity> step,
        IStep parentStep) => WrapStep(step, StepMaps.ConvertEntity<T>(parentStep));

    private sealed record StepValueMapper<TIn, TOut>(
            IRunnableStep<TIn> Step,
            IStepValueMap<TIn, TOut> Map)
        : IRunnableStep<TOut>
    {
        /// <inheritdoc />
        public async Task<Result<TOut, IError>> Run(
            IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var r = await Step.Run(stateMonad, cancellationToken)
                    .Bind(x => Map.Map(x, cancellationToken))
                ;

            return r;
        }
    }

    /// <summary>
    /// Wraps a nullable step
    /// </summary>
    private record NullableStepWrapper<TIn, TOut>
        (IRunnableStep<TIn>? Step, IStepValueMap<TIn, TOut> Map) : IRunnableStep<Maybe<TOut>>
    {
        /// <inheritdoc />
        public async Task<Result<Maybe<TOut>, IError>> Run(
            IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            if (Step is null)
                return Result.Success<Maybe<TOut>, IError>(Maybe<TOut>.None);

            var r = await Step.Run(stateMonad, cancellationToken);

            if (r.IsFailure)
                return r.ConvertFailure<Maybe<TOut>>();

            var r2 = await Map.Map(r.Value, cancellationToken)
                .Map(Maybe.From);

            return r2;
        }
    }
}

/// <summary>
/// Maps the results of steps
/// </summary>
public interface IStepValueMap<in TIn, TOut>
{
    /// <summary>
    /// Map the result of the step
    /// </summary>
    Task<Result<TOut, IError>> Map(TIn t, CancellationToken cancellationToken);
}

/// <summary>
/// Contains methods to help map the results of steps
/// </summary>
public static class StepMaps
{
    /// <summary>
    /// Maps StringStreams to strings
    /// </summary>
    public static IStepValueMap<Entity, JsonSchema> ConvertToSchema(IStep parentStep) =>
        new SchemaMap(parentStep);

    private record SchemaMap(IStep ParentStep) : IStepValueMap<Entity, JsonSchema>
    {
        /// <inheritdoc />
        public async Task<Result<JsonSchema, IError>> Map(
            Entity t,
            CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            JsonSchema schema;

            try
            {
                schema = JsonSchema.FromText(t.ToJsonElement().GetRawText());
            }
            catch (Exception e)
            {
                return Result.Failure<JsonSchema, IError>(
                    ErrorCode.Unknown.ToErrorBuilder(e.Message).WithLocation(ParentStep)
                );
            }

            return schema;
        }
    }

    /// <summary>
    /// Maps StringStreams to strings
    /// </summary>
    public static IStepValueMap<Entity, T> ConvertEntity<T>(IStep parentStep) =>
        new EntityConversionMap<T>(parentStep);

    /// <summary>
    /// Converts an entity to a particular type
    /// </summary>
    /// <typeparam name="T">The type to convert to</typeparam>
    private record EntityConversionMap<T>(IStep ParentStep) : IStepValueMap<Entity, T>
    {
        /// <inheritdoc />
        public async Task<Result<T, IError>> Map(Entity t, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            var conversionResult = EntityConversionHelpers.TryCreateFromEntity<T>(t)
                .MapError(x => x.WithLocation(ParentStep));

            if (conversionResult.IsFailure)
                return conversionResult.ConvertFailure<T>();

            return conversionResult.Value;
        }
    }

    /// <summary>
    /// Maps a OneOf with two type options
    /// </summary>
    public static IStepValueMap<SCLOneOf<TIn0, TIn1>, OneOf<TOut0, TOut1>>
        OneOf<TIn0, TIn1, TOut0, TOut1>(
            IStepValueMap<TIn0, TOut0> map0,
            IStepValueMap<TIn1, TOut1> map1)
        where TIn0 : ISCLObject where TIn1 : ISCLObject =>
        new OneOfMap<TIn0, TIn1, TOut0, TOut1>(map0, map1);

    /// <summary>
    /// Maps a OneOf with three type options
    /// </summary>
    public static IStepValueMap<SCLOneOf<TIn0, TIn1, TIn2>, OneOf<TOut0, TOut1, TOut2>>
        OneOf<TIn0, TIn1, TIn2, TOut0, TOut1, TOut2>(
            IStepValueMap<TIn0, TOut0> map0,
            IStepValueMap<TIn1, TOut1> map1,
            IStepValueMap<TIn2, TOut2> map2)
        where TIn0 : ISCLObject where TIn1 : ISCLObject where TIn2 : ISCLObject =>
        new OneOfMap<TIn0, TIn1, TIn2, TOut0, TOut1, TOut2>(map0, map1, map2);

    /// <summary>
    /// Maps a OneOf with two type options
    /// </summary>
    private record OneOfMap<TIn0, TIn1, TOut0, TOut1>
        (
            IStepValueMap<TIn0, TOut0> Map0,
            IStepValueMap<TIn1, TOut1> Map1)
        : IStepValueMap<SCLOneOf<TIn0, TIn1>, OneOf<TOut0, TOut1>>
        where TIn0 : ISCLObject
        where TIn1 : ISCLObject
    {
        /// <inheritdoc />
        public async Task<Result<OneOf<TOut0, TOut1>, IError>> Map(
            SCLOneOf<TIn0, TIn1> t,
            CancellationToken cancellationToken)
        {
            if (t.OneOf.TryPickT0(out var t0A, out var t1A))
            {
                var r2 = await Map0.Map(t0A, cancellationToken)
                    .Map(OneOf<TOut0, TOut1>.FromT0);

                return r2;
            }
            else
            {
                var r2 = await Map1.Map(t1A, cancellationToken)
                    .Map(OneOf<TOut0, TOut1>.FromT1);

                return r2;
            }
        }
    }

    /// <summary>
    /// Maps a OneOf with three type options
    /// </summary>
    private record OneOfMap<TIn0, TIn1, TIn2, TOut0, TOut1, TOut2>
        (
            IStepValueMap<TIn0, TOut0> Map0,
            IStepValueMap<TIn1, TOut1> Map1,
            IStepValueMap<TIn2, TOut2> Map2)
        : IStepValueMap<SCLOneOf<TIn0, TIn1, TIn2>, OneOf<TOut0, TOut1, TOut2>>
        where TIn0 : ISCLObject
        where TIn1 : ISCLObject
        where TIn2 : ISCLObject
    {
        /// <inheritdoc />
        public async Task<Result<OneOf<TOut0, TOut1, TOut2>, IError>> Map(
            SCLOneOf<TIn0, TIn1, TIn2> t,
            CancellationToken cancellationToken)
        {
            if (t.OneOf.TryPickT0(out var t0A, out var oneOf2))
            {
                var r2 = await Map0.Map(t0A, cancellationToken)
                    .Map(OneOf<TOut0, TOut1, TOut2>.FromT0);

                return r2;
            }
            else if (oneOf2.TryPickT0(out var t1A, out var t2A))
            {
                var r2 = await Map1.Map(t1A, cancellationToken)
                    .Map(OneOf<TOut0, TOut1, TOut2>.FromT1);

                return r2;
            }
            else
            {
                var r2 = await Map2.Map(t2A, cancellationToken)
                    .Map(OneOf<TOut0, TOut1, TOut2>.FromT2);

                return r2;
            }
        }
    }

    /// <summary>
    /// Maps StringStreams to strings
    /// </summary>
    public static IStepValueMap<StringStream, string> String() => StringMap.Instance;

    /// <summary>
    /// Maps StringStreams to strings
    /// </summary>
    private record StringMap : IStepValueMap<StringStream, string>
    {
        private StringMap() { }

        /// <summary>
        /// The instance
        /// </summary>
        public static StringMap Instance { get; } = new();

        /// <inheritdoc />
        public async Task<Result<string, IError>> Map(
            StringStream t,
            CancellationToken cancellationToken)
        {
            return await t.GetStringAsync();
        }
    }

    /// <summary>
    /// Maps an element to itself
    /// </summary>
    public static IStepValueMap<T, T> DoNothing<T>() => DoNothingMap<T>.Instance;

    /// <summary>
    /// Maps elements to themselves
    /// </summary>
    /// <typeparam name="T"></typeparam>
    private record DoNothingMap<T> : IStepValueMap<T, T>
    {
        private DoNothingMap() { }

        /// <summary>
        /// The instance
        /// </summary>
        public static DoNothingMap<T> Instance { get; } = new();

        /// <inheritdoc />
        public async Task<Result<T, IError>> Map(T t, CancellationToken _)
        {
            await Task.CompletedTask;
            return t;
        }
    }

    /// <summary>
    /// Maps the elements of an array
    /// </summary>
    public static IStepValueMap<Array<TIn>, IReadOnlyList<TIn>> Array<TIn>()
        where TIn : ISCLObject => new ArrayMap<TIn, TIn>(DoNothing<TIn>());

    /// <summary>
    /// Maps the elements of an array
    /// </summary>
    public static IStepValueMap<Array<TIn>, IReadOnlyList<TOut>> Array<TIn, TOut>(
        IStepValueMap<TIn, TOut> nestedMap)
        where TIn : ISCLObject => new ArrayMap<TIn, TOut>(nestedMap);

    /// <summary>
    /// Maps the elements of an array
    /// </summary>
    private record ArrayMap<TIn, TOut>
        (IStepValueMap<TIn, TOut> NestedMap)
        : IStepValueMap<Array<TIn>, IReadOnlyList<TOut>>
        where TIn : ISCLObject
    {
        /// <inheritdoc />
        public async Task<Result<IReadOnlyList<TOut>, IError>> Map(
            Array<TIn> t,
            CancellationToken cancellationToken)
        {
            var r = await t.GetElementsAsync(cancellationToken);

            if (r.IsFailure)
                return r.ConvertFailure<IReadOnlyList<TOut>>();

            if (NestedMap is DoNothingMap<TIn>
             && r.Value is IReadOnlyList<TOut> outList) //for performance
                return Result.Success<IReadOnlyList<TOut>, IError>(outList);

            var list = new List<TOut>(r.Value.Count);

            foreach (var m in r.Value)
            {
                var o = await NestedMap.Map(m, cancellationToken);

                if (o.IsFailure)
                    return o.ConvertFailure<IReadOnlyList<TOut>>();

                list.Add(o.Value);
            }

            return list;
        }
    }
}
