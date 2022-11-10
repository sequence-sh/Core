using Reductech.Sequence.Core.Enums;
using Reductech.Sequence.Core.Internal.Logging;

namespace Reductech.Sequence.Core.Steps;

/// <summary>
/// For each entity in the stream, check that the value of the `ParentIdProperty` is the value of the `IdProperty` for at least one entity in the stream.
/// </summary>
[AllowConstantFolding]
[Alias("ValidateRelations")]
[SCLExample(
    $"EntityValidateRelations {ExampleEntities} 'id' 'parentid' 'Error'",
    ValidExampleEntities,
    ExpectedLogs = new[] { "The id '100' does not exist." }
)]
[SCLExample(
    $"EntityValidateRelations {ExampleEntities} 'id' 'parentid' 'Warning'",
    ExampleEntities,
    ExpectedLogs = new[] { "The id '100' does not exist." }
)]
[SCLExample(
    $"EntityValidateRelations {ExampleEntities} 'id' 'parentid' 'Skip'",
    ValidExampleEntities
)]
[SCLExample(
    $"EntityValidateRelations {ExampleEntities} 'id' 'parentid' 'Ignore'",
    ExampleEntities
)]
public class EntityValidateRelations : CompoundStep<Array<Entity>>
{
    private const string ExampleEntities =
        @"[('id': 1), ('id': 2 'parentid': 1), ('parentid': 1), ('id': 3 'parentid': 100), ('id': 4 'parentid': """")]";

    private const string ValidExampleEntities =
        @"[('id': 1), ('id': 2 'parentid': 1), ('parentid': 1), ('id': 4 'parentid': """")]";

    /// <inheritdoc />
    protected override async ValueTask<Result<Array<Entity>, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var data = await stateMonad
            .RunStepsAsync(
                EntityStream,
                IdProperty.WrapStringStream(),
                ParentIdProperty.WrapStringStream(),
                ErrorBehavior,
                IgnoreEmpty,
                cancellationToken
            );

        if (data.IsFailure)
            return data.ConvertFailure<Array<Entity>>();

        var (entities, idProperty, parentIdProperty, errorBehaviour, ignoreIfEmpty) = data.Value;

        var failedIds = new HashSet<ISCLObject>();
        var ids       = new HashSet<ISCLObject>();

        var idKey       = new EntityNestedKey(new EntityKey(idProperty));
        var parentIdKey = new EntityNestedKey(new EntityKey(parentIdProperty));

        ValueTask<Result<Unit, IError>> CheckEntity(Entity entity, CancellationToken ct)
        {
            var id = entity.TryGetProperty(idKey);

            if (id.TryGetValue(out var kvp))
            {
                ids.Add(kvp.Value);
            }

            var parentId = entity.TryGetProperty(parentIdKey);

            if (parentId.TryGetValue(out var parentKvp))
            {
                if (ignoreIfEmpty && parentKvp.Value.IsEmpty())
                {
                    //do nothing
                }
                else if (!ids.Contains(parentKvp.Value))
                {
                    failedIds.Add(parentKvp.Value);
                }
            }

            return new ValueTask<Result<Unit, IError>>(Unit.Default);
        }

        var evaluated = await entities.EnsureEvaluated(cancellationToken);

        if (evaluated.IsFailure)
            return evaluated.ConvertFailure<Array<Entity>>();

        entities = (Array<Entity>)evaluated.Value;

        var runResult = await entities.ForEach(CheckEntity, cancellationToken);

        if (runResult.IsFailure)
            return runResult.ConvertFailure<Array<Entity>>();

        Array<Entity> entitiesToReturn;
        failedIds.ExceptWith(ids);

        if (failedIds.Any())
        {
            #pragma warning disable CS1998
            async IAsyncEnumerable<Entity> Filter(Entity e)
                #pragma warning restore CS1998
            {
                var parentId = e.TryGetProperty(parentIdKey);

                if (parentId.HasValue)
                {
                    if (failedIds.Contains(parentId.Value.Value))
                        yield break;
                }

                yield return e;
            }

            switch (errorBehaviour.Value)
            {
                case Enums.ErrorBehavior.Fail:
                {
                    var errorBuilderList =
                        ErrorBuilderList.Combine(
                            failedIds
                                .Select(x => x.Serialize(SerializeOptions.Primitive))
                                .Select(fid => ErrorCode.IdNotPresent.ToErrorBuilder(fid))
                        );

                    var error = errorBuilderList.WithLocation(this);
                    return Result.Failure<Array<Entity>, IError>(error);
                }
                case Enums.ErrorBehavior.Error:
                {
                    foreach (var sclObject in failedIds)
                    {
                        var s = sclObject.Serialize(SerializeOptions.Primitive);
                        LogSituation.IdNotPresent.Log(stateMonad, this, s);
                    }

                    entitiesToReturn = entities.SelectMany(Filter);
                    break;
                }
                case Enums.ErrorBehavior.Warning:
                {
                    foreach (var sclObject in failedIds)
                    {
                        var s = sclObject.Serialize(SerializeOptions.Primitive);
                        LogSituation.IdNotPresent.Log(stateMonad, this, s);
                    }

                    entitiesToReturn = entities;
                    break;
                }
                case Enums.ErrorBehavior.Skip:
                {
                    entitiesToReturn = entities.SelectMany(Filter);
                    break;
                }
                case Enums.ErrorBehavior.Ignore:
                {
                    entitiesToReturn = entities;
                    break;
                }
                default: throw new ArgumentOutOfRangeException();
            }
        }

        else
        {
            entitiesToReturn = entities;
        }

        return entitiesToReturn;
    }

    /// <summary>
    /// The stream of entities to validate
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Array<Entity>> EntityStream { get; set; } = null!;

    /// <summary>
    /// The entity property which will be used to create a lookup table.
    /// </summary>

    [StepProperty(2)]
    [Required]
    [Alias("ExistsIn")]

    public IStep<StringStream> IdProperty { get; set; } = null!;

    /// <summary>
    /// The step will check that the value of this entity property exists in
    /// the lookup table.
    /// </summary>

    [StepProperty(3)]
    [Required]
    [Alias("LookupThat")]

    public IStep<StringStream> ParentIdProperty { get; set; } = null!;

    /// <summary>
    /// How to behave if an error occurs.
    /// </summary>
    [StepProperty(4)]
    [DefaultValueExplanation("Error")]
    public IStep<SCLEnum<ErrorBehavior>> ErrorBehavior { get; set; } =
        new SCLConstant<SCLEnum<ErrorBehavior>>(
            new SCLEnum<ErrorBehavior>(Enums.ErrorBehavior.Error)
        );

    /// <summary>
    /// If true, empty values will be ignored
    /// </summary>
    [StepProperty(5)]
    [DefaultValueExplanation("true")]
    public IStep<SCLBool> IgnoreEmpty { get; set; } =
        new SCLConstant<SCLBool>(SCLBool.True);

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<EntityValidateRelations, Array<Entity>>();
}
