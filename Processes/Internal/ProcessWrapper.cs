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

            return new ProcessInvocation(freezeResult.Value, new ProcessState(_logger, _processSettings));
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
            public Result<object?> Execute() => Process.RunUntyped(ProcessState)!;
        }

        ///// <summary>
        ///// Gets an invocation of this process.
        ///// </summary>
        //public Result<Func<object?>, List<string?[]>> TryGetInvocation(IReadOnlyDictionary<string, string> dictionary)
        //{




        //    _processFactory.TryFreeze() .TryFreeze()

        //    if (!(Activator.CreateInstance(_processType) is IRunnableProcess<Unit> instance))
        //        return Result.Failure<Func<object?>, List<string?[]>>(new List<string?[]>{new []{"Instance must not be null"}});

        //    foreach (var property in RelevantProperties)
        //    {
        //        if (dictionary.TryGetValue(property.Name, out var v))
        //        {
        //            usedArguments.Add(property.Name);
        //            var (parsed, _, vObject ) = ArgumentHelpers.TryParseArgument(v, property.PropertyType);
        //            if (parsed)
        //                property.SetValue(instance, vObject);
        //            else
        //                errors.Add(new []{property.Name, property.PropertyType.Name, $"Could not parse '{v}'" });
        //        }
        //        else if (property.CustomAttributes.Any(att=>att.AttributeType == typeof(RequiredAttribute)))
        //            errors.Add(new []{property.Name, property.PropertyType.Name, "Is required"});
        //    }

        //    var extraArguments = dictionary.Keys.Where(k => !usedArguments.Contains(k)).ToList();
        //    errors.AddRange(extraArguments.Select(extraArgument => new[] {extraArgument, null, "Not a valid argument"}));

        //    if (errors.Any())
        //        return Result.Failure<Func<object?>, List<string?[]>>(errors);

        //    var processState = new ProcessState(_logger, _processSettings);

        //    var func = new Func<object?>(() => instance.Run(processState));

        //    return Result.Success<Func<object?>, List<string?[]>>(func);
        //}


    }
}