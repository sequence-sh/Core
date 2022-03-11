using Reductech.Sequence.Core.Internal.Parser;

namespace Reductech.Sequence.Core.Steps;

/// <summary>
/// Runs SCL defined in a StringStream
/// </summary>
public sealed class RunSCL : CompoundStep<Unit>
{
    /// <inheritdoc />
    protected override async Task<Result<Unit, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var sclResult = await SCL.Run(stateMonad, cancellationToken).Map(x => x.GetStringAsync());

        if (sclResult.IsFailure)
            return sclResult.ConvertFailure<Unit>();

        List<VariableName> variablesToExport;

        if (Export is null)
        {
            variablesToExport = new List<VariableName>();
        }
        else
        {
            var exportResult = await Export.Run(stateMonad, cancellationToken)
                .Bind(x => x.GetElementsAsync(cancellationToken));

            if (exportResult.IsFailure)
                return exportResult.ConvertFailure<Unit>();

            variablesToExport = exportResult.Value.Select(x => x.GetString())
                .Select(x => new VariableName(x))
                .ToList();
        }

        var stepResult = SCLParsing.TryParseStep(sclResult.Value)
            .Bind(
                x => x.TryFreeze(
                    new CallerMetadata(Name, nameof(SCL), TypeReference.Unit.Instance),
                    stateMonad.StepFactoryStore,
                    ImmutableDictionary<VariableName, ISCLObject>.Empty
                )
            );

        if (stepResult.IsFailure)
            return stepResult.ConvertFailure<Unit>();

        await using var monad2 = new ScopedStateMonad(
            stateMonad,
            ImmutableDictionary<VariableName, ISCLObject>.Empty,
            Maybe<VariableName>.None
        );

        await stepResult.Value.Run<Unit>(monad2, cancellationToken);

        foreach (var variable in variablesToExport)
        {
            var value = monad2.GetVariable<ISCLObject>(variable);

            var valueV = value.IsSuccess ? value.Value : SCLNull.Instance;

            await monad2.RemoveVariableAsync(
                variable,
                false,
                this
            ); //Remove the variable to prevent it being disposed

            await stateMonad.SetVariableAsync(variable, valueV, true, this, cancellationToken);
        }

        return Unit.Default;
    }

    /// <summary>
    /// The SCL to run
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<StringStream> SCL { get; set; } = null!;

    /// <summary>
    /// The names of variables to export
    /// </summary>
    [StepProperty(2)]
    [DefaultValueExplanation("Empty")]
    public IStep<Array<StringStream>>? Export { get; set; } = null;

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } = RunSCLStepFactory.Instance;

    private sealed class RunSCLStepFactory : SimpleStepFactory<RunSCL, Unit>
    {
        private RunSCLStepFactory() { }
        public static SimpleStepFactory<RunSCL, Unit> Instance { get; } = new RunSCLStepFactory();

        /// <inheritdoc />
        public override IEnumerable<UsedVariable> GetVariablesUsed(
            CallerMetadata callerMetadata,
            FreezableStepData freezableStepData,
            TypeResolver typeResolver)
        {
            var export = freezableStepData.TryGetStep(nameof(Export), StepType)
                    .Bind(
                        x => x.TryFreeze(
                            new CallerMetadata(
                                TypeName,
                                nameof(Export),
                                new TypeReference.Array(TypeReference.Actual.String)
                            ),
                            typeResolver
                        )
                    )
                ;

            if (export.IsSuccess)
            {
                var ev = export.Value.TryGetConstantValue(
                    ImmutableDictionary<VariableName, ISCLObject>.Empty
                );

                if (ev.HasValue && ev.GetValueOrThrow() is IArray { IsEvaluated: true } nestedArray)
                {
                    var list = nestedArray.ListIfEvaluated().Value;

                    foreach (var entityValue in list)
                    {
                        var name = entityValue.Serialize(SerializeOptions.Primitive);

                        yield return new(
                            new VariableName(name),
                            TypeReference.Any.Instance,
                            true,
                            freezableStepData.Location
                        );
                    }
                }
            }

            foreach (var pair in base.GetVariablesUsed(
                         callerMetadata,
                         freezableStepData,
                         typeResolver
                     ))
            {
                yield return pair;
            }
        }
    }
}
