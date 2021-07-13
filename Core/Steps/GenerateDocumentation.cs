using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Documentation;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Generates documentation for all available steps.
/// </summary>
[SCLExample(
    @"
- <root> = 'edr/steps'
- <docs> = (GenerateDocumentation)['AllPages']
- <docs> | ForEach (
    - <path> = $""{<root>}/{<>['Directory']}/{<>['FileName']}""
    - log <path>
)",
    Description = "Logs all the file paths"
    //ExecuteInTests = false
)]
public sealed class
    GenerateDocumentation : CompoundStep<Entity>
{
    /// <inheritdoc />
    protected override async Task<Result<Entity, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        var documented = stateMonad.StepFactoryStore
            .Dictionary
            .GroupBy(x => x.Value, x => x.Key)
            .Select(x => new StepWrapper(x))
            .ToList();

        var creationResult = DocumentationCreator.CreateDocumentation(documented);

        return creationResult.ConvertToEntity();
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<GenerateDocumentation, Entity>();
}

}
