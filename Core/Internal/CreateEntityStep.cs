using System.Text;

namespace Reductech.Sequence.Core.Internal;

/// <summary>
/// A step that creates and returns an entity.
/// </summary>
public record CreateEntityStep
    (IReadOnlyDictionary<EntityPropertyKey, IStep> Properties) : IStep<Entity>
{
    /// <inheritdoc />
    public async ValueTask<Result<Entity, IError>> Run(
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
    public ValueTask<Result<ISCLObject, IError>> RunUntyped(
        IStateMonad stateMonad,
        CancellationToken cancellationToken) =>
        Run(stateMonad, cancellationToken).Map(x => x as ISCLObject);

    /// <inheritdoc />
    public string Name => "Create Entity";

    /// <inheritdoc />
    public async ValueTask<Result<T, IError>> Run<T>(
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
    public void Format(
        IndentationStringBuilder indentationStringBuilder,
        FormattingOptions options,
        Stack<Comment> remainingComments)
    {
        indentationStringBuilder.AppendPrecedingComments(remainingComments, TextLocation);

        if (Properties.Count <= 1)
        {
            indentationStringBuilder.Append("(");
            AppendProperties(false);
            indentationStringBuilder.AppendContainingComments(remainingComments, TextLocation);
            indentationStringBuilder.Append(")");
        }

        else
        {
            indentationStringBuilder.AppendLine("(");
            indentationStringBuilder.Indent();

            AppendProperties(true);

            indentationStringBuilder.AppendContainingComments(remainingComments, TextLocation);

            indentationStringBuilder.AppendLineMaybe();
            indentationStringBuilder.UnIndent();
            indentationStringBuilder.Append(")");
        }

        void AppendProperties(bool appendLine)
        {
            var propertiesList = Properties.ToList();
            var longestKey     = Properties.Keys.Select(x => x.AsString.Length).Max();

            indentationStringBuilder.AppendJoin(
                "",
                false,
                propertiesList,
                pair =>
                {
                    var (key, value) = pair;

                    indentationStringBuilder.AppendPrecedingComments(
                        remainingComments,
                        value.TextLocation
                    );

                    if (appendLine)
                        indentationStringBuilder.AppendLineMaybe();

                    indentationStringBuilder.Append($"{$"'{key}'".PadRight(longestKey + 2)}: ");

                    if (value.ShouldBracketWhenSerialized)
                        indentationStringBuilder.Append("(");

                    value.Format(
                        indentationStringBuilder,
                        options,
                        remainingComments
                    );

                    if (value.ShouldBracketWhenSerialized)
                        indentationStringBuilder.Append(")");
                }
            );
        }
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
    /// <inheritdoc />
    public async ValueTask<Maybe<ISCLObject>> TryGetConstantValueAsync(
        IReadOnlyDictionary<VariableName, ISCLObject> variableValues,
        StepFactoryStore sfs)
    {
        var properties = new List<(EntityPropertyKey entityPropertyKey, ISCLObject value)>();

        foreach (var (key, step) in Properties)
        {
            var r = await step.TryGetConstantValueAsync(variableValues, sfs);

            if (r.HasNoValue)
                return Maybe<ISCLObject>.None;

            properties.Add((key, r.Value));
        }

        return Entity.Create(properties);
    }

    /// <inheritdoc />
    public bool HasConstantValue(IEnumerable<VariableName> providedVariables)
    {
        foreach (var (_, step) in Properties)
        {
            // ReSharper disable once PossibleMultipleEnumeration
            if (!step.HasConstantValue(providedVariables))
                return false;
        }

        return true;
    }

    /// <inheritdoc />
    public IEnumerable<(IStep Step, IStepParameter Parameter, IStep Value)> GetParameterValues()
    {
        foreach (var property in Properties)
        {
            foreach (var parameterValue in property.Value.GetParameterValues())
            {
                yield return parameterValue;
            }
        }
    }

    /// <summary>
    /// IF this step has a constant value. Convert it to a constant
    /// </summary>
    public IStep FoldIfConstant(
        StepFactoryStore sfs,
        IReadOnlyDictionary<VariableName, InjectedVariable> injectedVariables)
    {
        var constantValue = TryGetConstantValueAsync(
                ImmutableDictionary<VariableName, ISCLObject>.Empty,
                sfs
            )
            .Result.Bind(x => x.MaybeAs<Entity>());

        if (constantValue.HasNoValue)
            return this;

        return new SCLConstant<Entity>(constantValue.Value);
    }

    /// <inheritdoc />
    public bool ShouldBracketWhenSerialized => false;
}
