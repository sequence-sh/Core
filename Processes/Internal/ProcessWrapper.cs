using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Reductech.EDR.Processes.Serialization;
using Reductech.Utilities.InstantConsole;

namespace Reductech.EDR.Processes.Internal
{
    /// <summary>
    /// A wrapper for a runnable process.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class ProcessWrapper<T> : YamlObjectWrapper, IRunnable where T : IProcessSettings
    {
        private readonly RunnableProcessFactory _processFactory;
        private readonly T _processSettings;
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new ProcessWrapper.
        /// </summary>
        public ProcessWrapper(RunnableProcessFactory processFactory, T processSettings, ILogger logger, DocumentationCategory category)
            : base(processFactory.ProcessType, category)
        {
            _processFactory = processFactory;
            _processSettings = processSettings;
            _logger = logger;
        }

        Result<IInvocation, IReadOnlyCollection<DisplayError>> IRunnable.TryGetInvocation(IReadOnlyDictionary<string, string> arguments)
        {
            var dict = arguments
                .ToDictionary(x => x.Key,
                    x => new ProcessMember(new ConstantFreezableProcess(x)));


            var fpd = new FreezableProcessData(dict);


            var freezableProcess = new CompoundFreezableProcess(_processFactory, fpd, null);

            var freezeResult = freezableProcess.TryFreeze();
            if (freezeResult.IsFailure)
                return Result.Failure<IInvocation, IReadOnlyCollection<DisplayError>>(new[]
                {
                    new DisplayError("Parameter", null, freezeResult.Error) //TODO named parameters in errors
                });

            return new ProcessInvocation(freezeResult.Value, new ProcessState(_logger, _processSettings, ExternalProcessRunner.Instance));
        }

        /// <summary>
        /// An invocation of a process.
        /// </summary>
        private class ProcessInvocation : IInvocation
        {
            private ProcessState ProcessState { get; }
            private IRunnableProcess Process { get; }


            public ProcessInvocation(IRunnableProcess process, ProcessState processState)
            {
                ProcessState = processState;
                Process = process;
            }

            /// <inheritdoc />
            public Result<object?> Execute() => Process.Verify(ProcessState.ProcessSettings).Bind(_ => Process.Run<object>(ProcessState));
        }

    }
}