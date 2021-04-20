using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Create entities from a Json Stream
/// </summary>
public sealed class FromJsonArray : CompoundStep<Array<Entity>>
{
    /// <inheritdoc />
    protected override async Task<Result<Array<Entity>, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var text = await Stream.Run(stateMonad, cancellationToken)
            .Map(x => x.GetStringAsync());

        if (text.IsFailure)
            return text.ConvertFailure<Array<Entity>>();

        List<Entity>? entities;

        try
        {
            entities = JsonConvert.DeserializeObject<List<Entity>>(
                text.Value,
                EntityJsonConverter.Instance
            );
        }
        catch (Exception e)
        {
            stateMonad.Log(LogLevel.Error, e.Message, this);
            entities = null;
        }

        if (entities is null)
            return
                Result.Failure<Array<Entity>, IError>(
                    ErrorCode.CouldNotParse.ToErrorBuilder(text.Value, "JSON")
                        .WithLocation(this)
                );

        return entities.ToSCLArray();
    }

    /// <summary>
    /// Stream containing the Json data.
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<StringStream> Stream { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<FromJsonArray, Array<Entity>>();
}

}
