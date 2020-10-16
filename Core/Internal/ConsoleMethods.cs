using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Reductech.EDR.Core.Internal.Documentation;
using Reductech.EDR.Core.Serialization;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// Runs methods in the console
    /// </summary>
    public abstract class ConsoleMethods
    {
        /// <summary>
        /// One type for each additional connector.
        /// </summary>
        protected abstract IEnumerable<Type> ConnectorTypes { get; }

        /// <summary>
        /// The logger. Most probably a console logger.
        /// </summary>
        protected abstract ILogger Logger { get; }


        /// <summary>
        /// Try to get step settings from the config file.
        /// </summary>
        /// <returns></returns>
        protected abstract Result<ISettings> TryGetSettings();

        /// <summary>
        /// Generates documentation
        /// </summary>
        protected void GenerateDocumentationAbstract(string path)
        {
            var documentationCategories = ConnectorTypes.Append(typeof(IStep))
                .Select(type => (docCategory: type.Assembly.GetName().Name!, type)).ToList();

            var documented = documentationCategories
                .SelectMany(x =>
                    DocumentationCreator.GetAllDocumented(x.docCategory, StepFactoryStore.CreateUsingReflection(x.type)))
                .Distinct().ToList();

            var lines = DocumentationCreator.CreateDocumentationLines(documented);


            File.WriteAllLines(path, lines);
        }

        /// <summary>
        /// Executes yaml
        /// </summary>
        protected async Task ExecuteAbstractAsync(string? yaml, string? path, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(yaml))
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    throw new ArgumentException($"Please provide either {nameof(yaml)} or {nameof(path)}");
                }

                await ExecuteYamlFromPathAsync(path, cancellationToken);
            }

            else
            {
                if(string.IsNullOrWhiteSpace(path))
                    await ExecuteYamlStringAsync(yaml, cancellationToken);
                else
                    throw new ArgumentException($"Please provide only one of {nameof(yaml)} or {nameof(path)}");
            }
        }

        private async Task ExecuteYamlFromPathAsync(string path, CancellationToken cancellationToken)
        {
            var text = await File.ReadAllTextAsync(path, cancellationToken);

            await ExecuteYamlStringAsync(text, cancellationToken);
        }

        /// <summary>
        /// Runs a step defined in a yaml string
        /// </summary>
        private async Task ExecuteYamlStringAsync(string yaml, CancellationToken cancellationToken)
        {
            var stepFactoryStore = StepFactoryStore.CreateUsingReflection(ConnectorTypes.Append(typeof(IStep)).ToArray());

            var freezeResult = YamlMethods.DeserializeFromYaml(yaml, stepFactoryStore)
                .Bind(x => x.TryFreeze())
                .BindCast<IStep, IStep<Unit>>();

            if (freezeResult.IsFailure)
                Logger.LogError(freezeResult.Error);
            else
            {
                var settingsResult = TryGetSettings();

                if (settingsResult.IsFailure)
                    Logger.LogError(settingsResult.Error);
                else
                {
                    var stateMonad = new StateMonad(Logger, settingsResult.Value, ExternalProcessRunner.Instance);

                    var runResult = await freezeResult.Value.Run(stateMonad, cancellationToken);

                    if (runResult.IsFailure)
                        foreach (var runError in runResult.Error.AllErrors)
                            Logger.LogError(runError.Message);
                }
            }
        }


    }
}
