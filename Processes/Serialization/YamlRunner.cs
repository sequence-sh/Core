using System;
using System.IO;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Reductech.EDR.Processes.Serialization
{
    /// <summary>
    /// Runs processes from Yaml
    /// </summary>
    public class YamlRunner
    {
        /// <summary>
        /// Creates a new Yaml Runner
        /// </summary>
        public YamlRunner(IProcessSettings processSettings, ILogger logger,  ProcessFactoryStore processFactoryStore)
        {
            _processSettings = processSettings;
            _logger = logger;
            _processFactoryStore = processFactoryStore;
        }

        private readonly IProcessSettings _processSettings;
        private readonly ILogger _logger;
        private readonly ProcessFactoryStore _processFactoryStore;

        /// <summary>
        /// Run process defined in a yaml string.
        /// </summary>
        /// <param name="yamlString">Yaml representing the process.</param>
        /// <returns></returns>
        [UsedImplicitly]
        public Result RunProcessFromYamlString(string yamlString)
        {
            var result = YamlHelper.DeserializeFromYaml(yamlString, _processFactoryStore)
                    .Bind(x=>x.TryFreeze())
                    .BindCast<IRunnableProcess, IRunnableProcess<Unit>>()
                    .Bind(x=> x.Run(new ProcessState(_logger, _processSettings)));

            return result;

        }

        /// <summary>
        /// Run process defined in a yaml file.
        /// </summary>
        /// <param name="yamlPath">Path to the yaml file.</param>
        /// <returns></returns>
        [UsedImplicitly]
        public async Task<Result> RunProcessFromYaml(string yamlPath)
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
                return RunProcessFromYamlString(text);
            else
                return Result.Failure<string>("File is empty");
        }
    }
}
