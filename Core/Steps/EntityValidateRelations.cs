using Reductech.Sequence.Core.Enums;
using Reductech.Sequence.Core.Internal.Logging;

namespace Reductech.Sequence.Core.Steps;

/// <summary>
/// Rename entity properties.
/// </summary>
[AllowConstantFolding]
[SCLExample(
    $"EntityValidateRelations {ExampleEntities} 'id' 'parentid' 'Error'",
    ValidExampleEntities,
    ExpectedLogs = new[] { "The id '4' does not exist." }
)]
[SCLExample(
    $"EntityValidateRelations {ExampleEntities} 'id' 'parentid' 'Warning'",
    ExampleEntities,
    ExpectedLogs = new[] { "The id '4' does not exist." }
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
        @"[('id': 1), ('id': 2 'parentid': 1), ('parentid': 1), ('id': 3 'parentid': 4)]";

    private const string ValidExampleEntities =
        @"[('id': 1), ('id': 2 'parentid': 1), ('parentid': 1)]";

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
                cancellationToken
            );

        if (data.IsFailure)
            return data.ConvertFailure<Array<Entity>>();

        var (entities, idProperty, parentIdProperty, errorBehaviour) = data.Value;

        var failedIds = new HashSet<ISCLObject>();
        var ids       = new HashSet<ISCLObject>();

        var idKey       = new EntityNestedKey(new EntityKey(idProperty));
        var parentIdKey = new EntityNestedKey(new EntityKey(parentIdProperty));

        ValueTask<Result<Unit, IError>> CheckEntity(Entity entity, CancellationToken ct)
        {
            var id = entity.TryGetProperty(idKey);

            if (id.HasValue)
            {
                ids.Add(id.Value.Value);
            }

            var parentId = entity.TryGetProperty(parentIdKey);

            if (parentId.HasValue)
            {
                if (!ids.Contains(parentId.Value.Value))
                    failedIds.Add(parentId.Value.Value);
            }

            return new ValueTask<Result<Unit, IError>>(Unit.Default);
        }

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
    /// The stream of entities to change the field names of.
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Array<Entity>> EntityStream { get; set; } = null!;

    /// <summary>
    /// The name of the Entity Id property
    /// </summary>
    [StepProperty(2)]
    [Required]
    public IStep<StringStream> IdProperty { get; set; } = null!;

    /// <summary>
    /// The name of the Entity ParentId property
    /// </summary>
    [StepProperty(3)]
    [Required]
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

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<EntityValidateRelations, Array<Entity>>();
}
