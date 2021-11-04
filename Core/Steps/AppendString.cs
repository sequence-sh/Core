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
/// Appends a string to an existing string variable.
/// </summary>
[SCLExample(
    "- <var> = 'hello'\r\n- AppendString <var> ' world'\r\n- log <var>",
    null,
    null,
    "hello world"
)]
public sealed class AppendString : CompoundStep<Unit>
{
    /// <summary>
    /// The variable to append to.
    /// </summary>
    [VariableName(1)]
    [Required]
    public VariableName Variable { get; set; }

    /// <summary>
    /// The string to append.
    /// </summary>
    [StepProperty(2)]
    [Required]
    public IStep<StringStream> String { get; set; } = null!;

    /// <inheritdoc />
    protected override async Task<Result<Unit, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var str = await String.Run(stateMonad, cancellationToken)
            .Map(x => x.GetStringAsync());

        if (str.IsFailure)
            return str.ConvertFailure<Unit>();

        var currentValue = stateMonad.GetVariable<StringStream>(Variable)
            .MapError(x => x.WithLocation(this));

        if (currentValue.IsFailure)
            return currentValue.ConvertFailure<Unit>();

        var newValue = await currentValue.Value.GetStringAsync() + str.Value;

        var r = await stateMonad.SetVariableAsync(
            Variable,
            new StringStream(newValue),
            false,
            this,
            cancellationToken
        );

        if (r.IsFailure)
            return r.ConvertFailure<Unit>();

        return Unit.Default;
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory => AppendStringStepFactory.Instance;

    private sealed class AppendStringStepFactory : SimpleStepFactory<AppendString, Unit>
    {
        private AppendStringStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<AppendString, Unit> Instance { get; } =
            new AppendStringStepFactory();

        /// <inheritdoc />
        public override IEnumerable<UsedVariable> GetVariablesUsed(
            CallerMetadata callerMetadata,
            FreezableStepData freezableStepData,
            TypeResolver typeResolver)
        {
            var vn = freezableStepData.TryGetVariableName(nameof(AppendString.Variable), StepType);

            if (vn.IsFailure)
                yield break;

            yield return new(
                vn.Value,
                TypeReference.Actual.String,
                freezableStepData.Location
            );
        }
    }
}

}
