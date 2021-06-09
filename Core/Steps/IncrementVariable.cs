using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Increment an integer variable by a particular amount
/// </summary>
public sealed class IncrementVariable : CompoundStep<Unit>
{
    /// <summary>
    /// The variable to increment.
    /// </summary>
    [VariableName(1)]
    [Required]
    public VariableName Variable { get; set; }

    /// <summary>
    /// The amount to increment by.
    /// </summary>
    [StepProperty(2)]
    [DefaultValueExplanation("1")]
    public IStep<int> Amount { get; set; } = new IntConstant(1);

    /// <inheritdoc />
    protected override async Task<Result<Unit, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var variable = stateMonad.GetVariable<int>(Variable).MapError(x => x.WithLocation(this));

        if (variable.IsFailure)
            return variable.ConvertFailure<Unit>();

        var amount = await Amount.Run(stateMonad, cancellationToken);

        if (amount.IsFailure)
            return amount.ConvertFailure<Unit>();

        var r = await stateMonad.SetVariableAsync(
            Variable,
            variable.Value + amount.Value,
            false,
            this
        );

        return r;
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory => IncrementVariableStepFactory.Instance;

    /// <summary>
    /// Increment an integer variable by a particular amount
    /// </summary>
    private sealed class IncrementVariableStepFactory : SimpleStepFactory<IncrementVariable, Unit>
    {
        private IncrementVariableStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<IncrementVariable, Unit> Instance { get; } =
            new IncrementVariableStepFactory();

        /// <inheritdoc />
        public override IEnumerable<(VariableName variableName, TypeReference type)>
            GetVariablesSet(
                CallerMetadata callerMetadata,
                FreezableStepData freezableStepData,
                TypeResolver typeResolver)
        {
            var vn = freezableStepData.TryGetVariableName(
                nameof(IncrementVariable.Variable),
                StepType
            );

            if (vn.IsFailure)
                yield break;

            yield return (vn.Value, TypeReference.Actual.Integer);
        }
    }
}

}
