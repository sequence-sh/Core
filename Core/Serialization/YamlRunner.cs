using System;
using System.IO;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Reductech.EDR.Core.Internal;
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
        /// <returns></returns>
        [UsedImplicitly]
        public Result RunSequenceFromYamlString(string yamlString)
        {
            var result = YamlMethods.DeserializeFromYaml(yamlString, _stepFactoryStore)
                    .Bind(x=>x.TryFreeze())
                    .BindCast<IStep, IStep<Unit>>()
                    .Bind(x=> x.Run(new StateMonad(_logger, _settings, ExternalProcessRunner.Instance))
                        .MapFailure(y=>y.AsString));

            return result;

        }

        /// <summary>
        /// Run step defined in a yaml file.
        /// </summary>
        /// <param name="yamlPath">Path to the yaml file.</param>
        /// <returns></returns>
        [UsedImplicitly]
        public async Task<Result> RunSequenceFromYaml(string yamlPath)
        {
            string? text;
            string? errorMessage;
            try
            {
                text = await File.ReadAllTextAsync(yamlPath);
                errorMessage = null;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            {
                errorMessage = e.Message;
                text = null;
            }
#pragma warning restore CA1031 // Do not catch general exception types

            if (errorMessage != null)
                return Result.Failure<string>(errorMessage);
            else if (!string.IsNullOrWhiteSpace(text))
                return RunSequenceFromYamlString(text);
            else
                return Result.Failure<string>("File is empty");
        }
    }
}
