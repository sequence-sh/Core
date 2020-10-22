using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Serialization
{
    /// <summary>
    /// Runs processes from Yaml
    /// </summary>
    public class YamlRunner
    {
        /// <summary>
        /// Creates a new Yaml Runner
        /// </summary>
        public YamlRunner(ISettings settings, ILogger logger,  StepFactoryStore stepFactoryStore)
        {
            _settings = settings;
            _logger = logger;
            _stepFactoryStore = stepFactoryStore;
        }

        private readonly ISettings _settings;
        private readonly ILogger _logger;
        private readonly StepFactoryStore _stepFactoryStore;

        /// <summary>
        /// Run step defined in a yaml string.
        /// </summary>
        /// <param name="yamlString">Yaml representing the step.</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns></returns>
        [UsedImplicitly]
        public async Task<Result<Unit, IError>> RunSequenceFromYamlStringAsync(string yamlString, CancellationToken cancellationToken)
        {
            var stepResult = YamlMethods.DeserializeFromYaml(yamlString, _stepFactoryStore)
                    .Bind(x => x.TryFreeze());

            if (stepResult.IsFailure)
                return stepResult.ConvertFailure<Unit>();

            if (stepResult.Value is IStep<Unit> unitStep)
            {
                var stateMonad = new StateMonad(_logger, _settings, ExternalProcessRunner.Instance, _stepFactoryStore);

                var runResult = await unitStep.Run(stateMonad, cancellationToken);

                return runResult;
            }

            return new SingleError("Yaml must represent a step with return type Unit", ErrorCode.InvalidCast, new StepErrorLocation(stepResult.Value));
        }

        /// <summary>
        /// Run step defined in a yaml file.
        /// </summary>
        /// <param name="yamlPath">Path to the yaml file.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns></returns>
        [UsedImplicitly]
        public async Task<Result<Unit, IError>> RunSequenceFromYamlPathAsync(string yamlPath, CancellationToken cancellationToken)
        {
            Result<string, IError> result;
            try
            {
                result = await File.ReadAllTextAsync(yamlPath, cancellationToken);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            {
                result = new SingleError(e, ErrorCode.ExternalProcessError, EntireSequenceLocation.Instance);
            }
#pragma warning restore CA1031 // Do not catch general exception types

            var result2 = await result.Bind(x=> RunSequenceFromYamlStringAsync(x, cancellationToken));
            return result2;
        }
    }
}
