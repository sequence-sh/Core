using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Documentation;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Parser;

namespace Reductech.EDR.Core.Steps
{

    /// <summary>
    /// Generates documentation for all available steps.
    /// </summary>
    public sealed class GenerateDocumentation : CompoundStep<List<StringStream>> //TODO maybe output a list of entities
    {
        /// <inheritdoc />
        public override async Task<Result<List<StringStream>, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            var documented = stateMonad.StepFactoryStore
                .Dictionary
                .Values
                .Select(x => new StepWrapper(x))
                .ToList();

            var lines = DocumentationCreator.CreateDocumentationLines(documented).Select(x=> new StringStream(x)).ToList();

            //var text = string.Match(Environment.NewLine, lines);
            //TODO allow multiple files somehow

            return lines;
        }


        /// <inheritdoc />
        public override IStepFactory StepFactory => GenerateDocumentationStepFactory.Instance;
    }

    /// <summary>
    /// Generates documentation for all available steps.
    /// </summary>
    public sealed class GenerateDocumentationStepFactory : SimpleStepFactory<GenerateDocumentation, List<StringStream>>
    {
        private GenerateDocumentationStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<GenerateDocumentation, List<StringStream>> Instance { get; } = new GenerateDocumentationStepFactory();
    }
}
