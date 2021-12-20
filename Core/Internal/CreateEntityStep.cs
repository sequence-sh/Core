using System.Text;

namespace Reductech.EDR.Core.Internal;

/// <summary>
/// A step that creates and returns an entity.
/// </summary>
public record CreateEntityStep
    (IReadOnlyDictionary<EntityPropertyKey, IStep> Properties) : IStep<Entity>
{
    /// <inheritdoc />
    public async Task<Result<Entity, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var pairs = new List<(EntityPropertyKey, ISCLObject)>();

        foreach (var (key, step) in Properties)
        {
            var r = await step.Run<ISCLObject>(stateMonad, cancellationToken)
                .Bind(x => EntityHelper.TryUnpackObjectAsync(x, cancellationToken));

            if (r.IsFailure)
                return r.ConvertFailure<Entity>();

            pairs.Add((key, r.Value));
        }

        return Entity.Create(pairs);
    }

    /// <inheritdoc />
    public Task<Result<ISCLObject, IError>> RunUntyped(
        IStateMonad stateMonad,
        CancellationToken cancellationToken) =>
        Run(stateMonad, cancellationToken).Map(x => x as ISCLObject);

    /// <inheritdoc />
    public string Name => "Create Entity";

    /// <inheritdoc />
    public async Task<Result<T, IError>> Run<T>(
        IStateMonad stateMonad,
        CancellationToken cancellationToken) where T : ISCLObject
    {
        return await Run(stateMonad, cancellationToken)
            .BindCast<Entity, T, IError>(
                ErrorCode.InvalidCast.ToErrorBuilder(
                        Name,
                        typeof(T).Name
                    )
                    .WithLocation(this)
            );
    }

    /// <inheritdoc />
    public Result<Unit, IError> Verify(StepFactoryStore stepFactoryStore)
    {
        var r = Properties.Select(x => x.Value.Verify(stepFactoryStore))
            .Combine(_ => Unit.Default, ErrorList.Combine);

        return r;
    }

    /// <inheritdoc />
    public TextLocation? TextLocation { get; set; }

    /// <inheritdoc />
    public Type OutputType => typeof(Entity);

    /// <inheritdoc />
    public string Serialize(SerializeOptions options)
    {
        var sb = new StringBuilder();

        sb.Append('(');

        var results = new List<string>();

        foreach (var (key, value) in Properties)
        {
            var valueString = value.Serialize(options);

            if (value.ShouldBracketWhenSerialized)
                valueString = $"({valueString})";

            results.Add($"{key}: {valueString}");
        }

        sb.AppendJoin(",", results);

        sb.Append(')');

        return sb.ToString();
    }

    /// <inheritdoc />
    public IEnumerable<Requirement> RuntimeRequirements
    {
        get
        {
            return Properties.SelectMany(x => x.Value.RuntimeRequirements);
        }
    }

    /// <inheritdoc />
    public Maybe<ISCLObject> TryGetConstantValue()
    {
        var properties = new List<(EntityPropertyKey entityPropertyKey, ISCLObject value)>();

        foreach (var (key, step) in Properties)
        {
            var r = step.TryGetConstantValue();

            if (r.HasNoValue)
                return Maybe<ISCLObject>.None;

            properties.Add((key, r.Value));
        }

        return Entity.Create(properties);
    }

    /// <inheritdoc />
    public bool ShouldBracketWhenSerialized => false;
}
