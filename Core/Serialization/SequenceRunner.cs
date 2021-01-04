using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Parser;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Serialization
{
    /// <summary>
    /// Runs processes from Text
    /// </summary>
    public class SCLRunner
    {
        /// <summary>
        /// Creates a new SCL Runner
        /// </summary>
        public SCLRunner(ISettings settings,
            ILogger logger,
            IExternalProcessRunner externalProcessRunner,
            IFileSystemHelper fileSystemHelper,
            StepFactoryStore stepFactoryStore)
        {
            _settings = settings;
            _logger = logger;
            _externalProcessRunner = externalProcessRunner;
            _stepFactoryStore = stepFactoryStore;
            _fileSystemHelper = fileSystemHelper;
        }

        private readonly ISettings _settings;
        private readonly ILogger _logger;
        private readonly IExternalProcessRunner _externalProcessRunner;
        private readonly IFileSystemHelper _fileSystemHelper;
        private readonly StepFactoryStore _stepFactoryStore;

        /// <summary>
        /// Run step defined in an SCL string.
        /// </summary>
        /// <param name="text">SCL representing the step.</param>
        /// <param name="cancellationToken">Cancellation ErrorLocation</param>
        /// <returns></returns>
        [UsedImplicitly]
        public async Task<Result<Unit, IError>> RunSequenceFromTextAsync(string text, CancellationToken cancellationToken)
        {
            var stepResult = SCLParsing.ParseSequence(text)
                    .Bind(x => x.TryFreeze(_stepFactoryStore))
                    .Bind(ConvertToUnitStep);

            if (stepResult.IsFailure)
                return stepResult.ConvertFailure<Unit>();

            using var stateMonad = new StateMonad(_logger, _settings, _externalProcessRunner, _fileSystemHelper, _stepFactoryStore);

            var runResult = await stepResult.Value.Run(stateMonad, cancellationToken);

            return runResult;
        }

        /// <summary>
        /// Converts the step to a unit step for running.
        /// </summary>
        public static Result<IStep<Unit>, IError> ConvertToUnitStep(IStep step)
        {
            if (step is IStep<Unit> unitStep)
            {
                return Result.Success<IStep<Unit>, IError>(unitStep);
            }

            return new SingleError("SCL must represent a step with return type Unit", ErrorCode.InvalidCast, new StepErrorLocation(step));
        }

        /// <summary>
        /// Run step defined in an SCL file.
        /// </summary>
        /// <param name="path">Path to the SCL file.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns></returns>
        [UsedImplicitly]
        public async Task<Result<Unit, IError>> RunSequenceFromPathAsync(string path, CancellationToken cancellationToken)
        {
            Result<string, IError> result;
            try
            {
                result = await File.ReadAllTextAsync(path, cancellationToken);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            {
                result = new SingleError(e, ErrorCode.ExternalProcessError, EntireSequenceLocation.Instance);
            }
#pragma warning restore CA1031 // Do not catch general exception types

            var result2 = await result.Bind(x=> RunSequenceFromTextAsync(x, cancellationToken));
            return result2;
        }
    }
}
