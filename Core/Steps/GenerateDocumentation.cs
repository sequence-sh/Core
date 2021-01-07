using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Documentation;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Generates documentation for all available steps.
/// </summary>
public sealed class
    GenerateDocumentation : CompoundStep<StringStream> //TODO maybe output a list of entities
{
    /// <inheritdoc />
    protected override async Task<Result<StringStream, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        var documented = stateMonad.StepFactoryStore
            .Dictionary
            .Values
            .Select(x => new StepWrapper(x))
            .ToList();

        var lines = DocumentationCreator.CreateDocumentationLines(documented);

        var document = string.Join(Environment.NewLine, lines);

        return new StringStream(document);
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory => GenerateDocumentationStepFactory.Instance;
}

/// <summary>
/// Generates documentation for all available steps.
/// </summary>
public sealed class
    GenerateDocumentationStepFactory : SimpleStepFactory<GenerateDocumentation, StringStream>
{
    private GenerateDocumentationStepFactory() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static SimpleStepFactory<GenerateDocumentation, StringStream> Instance { get; } =
        new GenerateDocumentationStepFactory();
}

}
