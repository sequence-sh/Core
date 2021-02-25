using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Internal.Serialization;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Gets the value of a named variable.
/// </summary>
public sealed class SetVariable<T> : CompoundStep<Unit>
{
    /// <inheritdoc />
    protected override async Task<Result<Unit, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        return await Value.Run(stateMonad, cancellationToken)
            .Bind(x => stateMonad.SetVariableAsync(Variable, x, true, this));
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory => SetVariableStepFactory.Instance;

    /// <summary>
    /// The name of the variable to set.
    /// </summary>
    [VariableName(1)]
    [Required]
    public VariableName Variable { get; set; }

    /// <summary>
    /// The value to set the variable to.
    /// </summary>
    [StepProperty(2)]
    [Required]
    public IStep<T> Value { get; set; } = null!;
}

/// <summary>
/// Sets the value of a named variable.
/// </summary>
public class SetVariableStepFactory : GenericStepFactory
{
    private SetVariableStepFactory() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static GenericStepFactory Instance { get; } = new SetVariableStepFactory();

    /// <inheritdoc />
    public override Type StepType => typeof(SetVariable<>);

    /// <inheritdoc />
    public override string OutputTypeExplanation => nameof(Unit);

    /// <inheritdoc />
    protected override ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference) =>
        new ActualTypeReference(typeof(Unit));

    /// <inheritdoc />
    protected override Result<ITypeReference, IError> GetMemberType(
        FreezableStepData freezableStepData,
        TypeResolver typeResolver)
    {
        var r =
            freezableStepData
                .TryGetStep(nameof(SetVariable<object>.Value), StepType)
                .Bind(x => x.TryGetOutputTypeReference(typeResolver));

        return r;
    }

    /// <inheritdoc />
    public override IEnumerable<(VariableName variableName, Maybe<ITypeReference>)> GetVariablesSet(
        FreezableStepData freezableStepData,
        TypeResolver typeResolver)
    {
        var vn = freezableStepData.TryGetVariableName(
            nameof(SetVariable<object>.Variable),
            StepType
        );

        if (vn.IsFailure)
            yield break;

        var memberType = GetMemberType(freezableStepData, typeResolver);

        if (memberType.IsFailure)
            yield return (vn.Value, Maybe<ITypeReference>.None);
        else
            yield return (vn.Value, Maybe<ITypeReference>.From(memberType.Value));
    }

    /// <inheritdoc />
    public override IStepSerializer Serializer => new StepSerializer(
        TypeName,
        new StepComponent(nameof(SetVariable<object>.Variable)),
        SpaceComponent.Instance,
        new FixedStringComponent("="),
        SpaceComponent.Instance,
        new StepComponent(nameof(SetVariable<object>.Value))
    );
}

}
