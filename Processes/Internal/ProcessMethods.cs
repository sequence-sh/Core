using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Reductech.EDR.Processes.Internal.Documentation;
using Reductech.EDR.Processes.Serialization;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Processes.Internal
{
    /// <summary>
    /// Runs methods in the console
    /// </summary>
    public abstract class ProcessMethods
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
        /// Try to get process settings from the config file.
        /// </summary>
        /// <returns></returns>
        protected abstract Result<IProcessSettings> TryGetSettings();

        /// <summary>
        /// Generates documentation
        /// </summary>
        protected void GenerateDocumentationAbstract(string path)
        {
            var documentationCategories = ConnectorTypes.Append(typeof(IRunnableProcess))
                .Select(type => (docCategory: new DocumentationCategory(type.Assembly.GetName().Name!), type)).ToList();

            var documented = documentationCategories
                .SelectMany(x =>
                    DocumentationCreator.GetAllDocumented(x.docCategory, ProcessFactoryStore.CreateUsingReflection(x.type)))
                .Distinct().ToList();

            var lines = DocumentationCreator.CreateDocumentationLines(documented);


            File.WriteAllLines(path, lines);
        }

        /// <summary>
        /// Executes yaml
        /// </summary>
        protected void ExecuteAbstract(string? yaml, string? path)
        {
            if (string.IsNullOrWhiteSpace(yaml))
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    throw new ArgumentException($"Please provide either {nameof(yaml)} or {nameof(path)}");
                }

                ExecuteYamlFromPath(path);
            }

            else
            {
                if(string.IsNullOrWhiteSpace(path))
                    ExecuteYamlString(yaml);
                else
                    throw new ArgumentException($"Please provide only one of {nameof(yaml)} or {nameof(path)}");
            }
        }

        private void ExecuteYamlFromPath(string path)
        {
            var text = File.ReadAllText(path);

            ExecuteYamlString(text);
        }

        /// <summary>
        /// Runs a process defined in a yaml string
        /// </summary>
        private void ExecuteYamlString(string yaml)
        {
            var processFactoryStore = ProcessFactoryStore.CreateUsingReflection(ConnectorTypes.Append(typeof(IRunnableProcess)).ToArray());

            var freezeResult = YamlMethods.DeserializeFromYaml(yaml, processFactoryStore)
                .Bind(x => x.TryFreeze())
                .BindCast<IRunnableProcess, IRunnableProcess<Unit>>();

            if (freezeResult.IsFailure)
                Logger.LogError(freezeResult.Error);
            else
            {
                var settingsResult = TryGetSettings();

                if (settingsResult.IsFailure)
                    Logger.LogError(settingsResult.Error);
                else
                {
                    var processState = new ProcessState(Logger, settingsResult.Value, ExternalProcessRunner.Instance);

                    var runResult = freezeResult.Value.Run(processState);

                    if (runResult.IsFailure)
                        foreach (var runError in runResult.Error.AllErrors)
                            Logger.LogError(runError.Message);
                }
            }
        }


    }
}
