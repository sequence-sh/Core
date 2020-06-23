using System;
using System.Collections.Generic;
using System.IO;
using CSharpFunctionalExtensions;
using JetBrains.Annotations;
using Reductech.EDR.Utilities.Processes.mutable;
using Reductech.EDR.Utilities.Processes.output;

namespace Reductech.EDR.Utilities.Processes
{
    /// <summary>
    /// Runs processes from Yaml
    /// </summary>
    public class YamlRunner
    {
        /// <summary>
        /// Creates a new Yaml Runner
        /// </summary>
        /// <param name="processSettings"></param>
        public YamlRunner(IProcessSettings processSettings)
        {
            _processSettings = processSettings;
        }

        private readonly IProcessSettings _processSettings;

        /// <summary>
        /// Run process defined in a yaml string.
        /// </summary>
        /// <param name="yamlString">Yaml representing the process.</param>
        /// <returns></returns>
        [UsedImplicitly]
        public async IAsyncEnumerable<Result<string>> RunProcessFromYamlString(string yamlString)
        {
            var yamlResult = YamlHelper.TryMakeFromYaml(yamlString);

            if (yamlResult.IsFailure)
            {
                yield return yamlResult.ConvertFailure<string>();
                yield break;
            }

            var (_, freezeFailure, immutableProcess, freezeError) = yamlResult.Value.TryFreeze<Unit>(_processSettings);

            if (freezeFailure)
                yield return Result.Failure<string>(freezeError);
            else
            {
                await foreach (var output in immutableProcess.Execute())
                {
                    var r = output.OutputType switch
                    {
                        OutputType.Error => Result.Failure<string>(output.Text),
                        OutputType.Warning => Result.Success(output.Text),
                        OutputType.Message => Result.Success(output.Text),
                        OutputType.Success => Result.Success(output.Text),
                        _ => throw new ArgumentOutOfRangeException()
                    };

                    yield return r;
                }
            }
        }

        /// <summary>
        /// Run process defined in a yaml file.
        /// </summary>
        /// <param name="yamlPath">Path to the yaml file.</param>
        /// <returns></returns>
        [UsedImplicitly]
        public async IAsyncEnumerable<Result<string>> RunProcessFromYaml(string yamlPath)
        {
            string? text;
            string? errorMessage;
            try
            {
                text = File.ReadAllText(yamlPath);
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
            {
                yield return Result.Failure<string>(errorMessage);
            }
            else if (!string.IsNullOrWhiteSpace(text))
            {
                var r = RunProcessFromYamlString(text);
                await foreach(var rl in r)
                    yield return rl;
            }
            else
            {
                yield return Result.Failure<string>("File is empty");
            }
        }
    }
}
