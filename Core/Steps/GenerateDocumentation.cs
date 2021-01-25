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
    GenerateDocumentation : CompoundStep<Array<Entity>>
{
    /// <inheritdoc />
    protected override async Task<Result<Array<Entity>, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        var documented = stateMonad.StepFactoryStore
            .Dictionary
            .Values
            .Select(x => new StepWrapper(x))
            .ToList();

        var files = DocumentationCreator.CreateDocumentation(documented);

        var entities =
            files.Select(
                    x => Entity.Create(
                        ("FileName", x.fileName),
                        ("Title", x.title),
                        ("FileText", x.fileText),
                        ("Directory", x.directory),
                        ("Category", x.category)
                    )
                )
                .ToList();

        return new Array<Entity>(entities);
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory => GenerateDocumentationStepFactory.Instance;
}

/// <summary>
/// Generates documentation for all available steps.
/// </summary>
public sealed class
    GenerateDocumentationStepFactory : SimpleStepFactory<GenerateDocumentation, Array<Entity>>
{
    private GenerateDocumentationStepFactory() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static SimpleStepFactory<GenerateDocumentation, Array<Entity>> Instance { get; } =
        new GenerateDocumentationStepFactory();
}

}
