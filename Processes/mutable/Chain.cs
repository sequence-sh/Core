using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes.immutable;
using Reductech.EDR.Utilities.Processes.mutable.injection;
using Reductech.EDR.Utilities.Processes.output;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Utilities.Processes.mutable
{
    /// <summary>
    /// An immutable chain.
    /// </summary>
    /// <typeparam name="TFinal"></typeparam>
    public class ImmutableChainProcess<TFinal> : ImmutableProcess<TFinal>
    {
        /// <summary>
        /// Creates a new chain process.
        /// </summary>
        /// <param name="chain"></param>
        public ImmutableChainProcess(IChainLink<Unit, TFinal> chain)
        {
            Chain = chain;
        }

        /// <inheritdoc />
        public override string Name => Chain.Name;

        /// <inheritdoc />
        public override IProcessConverter? ProcessConverter => null;

        /// <inheritdoc />
        public override IAsyncEnumerable<IProcessOutput<TFinal>> Execute()
        {
            return Chain.Execute(Unit.Instance);
        }

        /// <summary>
        /// The chain to execute.
        /// </summary>
        public IChainLink<Unit, TFinal> Chain { get; }
    }

    /// <summary>
    /// A chain link where the output type is not defined.
    /// </summary>
    public interface IChainLink<in TInput, out TFinal>
    {
        /// <summary>
        /// Execute this link and all subsequent links.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        IAsyncEnumerable<IProcessOutput<TFinal>> Execute(TInput input);

        /// <summary>
        /// The name of this link in the chain. Will include the names from subsequent links.
        /// </summary>
        string Name { get; }
    }


    /// <summary>
    /// The final link in the chain.
    /// </summary>
    public class ImmutableFinalChainLink<TInput, TFinal, TProcess> : IChainLink<TInput, TFinal> where TProcess : ImmutableProcess<TFinal>
    {
        /// <summary>
        /// The final link in the chain.
        /// </summary>
        /// <param name="processFactory"></param>
        public ImmutableFinalChainLink(ProcessFactory<TInput, TFinal, TProcess> processFactory)
        {
            ProcessFactory = processFactory;
        }

        /// <summary>
        /// Gets the process.
        /// </summary>
        public ProcessFactory<TInput, TFinal, TProcess> ProcessFactory { get; }

        /// <inheritdoc />
        public async IAsyncEnumerable<IProcessOutput<TFinal>> Execute(TInput input)
        {
            var (_, isFailure, process, errorList) = ProcessFactory.TryCreate(input);

            if (isFailure)
                foreach (var errorLine in errorList)
                    yield return ProcessOutput<TFinal>.Error(errorLine);
            else
                await foreach (var line in process.Execute())
                    yield return line;
        }

        /// <inheritdoc />
        public string Name => ProcessFactory.Name;
    }


    /// <summary>
    /// A link in a chain. Accepts input of a particular type and
    /// </summary>
    public class ImmutableChainLink<TInput, TOutput, TFinal, TProcess> : IChainLink<TInput, TFinal> where TProcess : ImmutableProcess<TOutput>
    {
        /// <summary>
        /// Creates a new ImmutableChainLink
        /// </summary>
        public ImmutableChainLink(ProcessFactory<TInput, TOutput, TProcess> processFactory, IChainLink<TOutput, TFinal> nextLink)
        {
            ProcessFactory = processFactory;
            NextLink = nextLink;
        }

        /// <summary>
        /// Gets the process.
        /// </summary>
        public ProcessFactory<TInput, TOutput, TProcess> ProcessFactory { get; }

        /// <summary>
        /// The next link in the chain.
        /// </summary>
        public IChainLink<TOutput, TFinal> NextLink { get; }

        /// <inheritdoc />
        public string Name => ProcessNameHelper.GetChainName(ProcessFactory.Name, NextLink.Name);

        /// <inheritdoc />
        public override string ToString()
        {
            return Name;
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<IProcessOutput<TFinal>> Execute(TInput input)
        {
            var (_, isFailure, process, errorList) = ProcessFactory.TryCreate(input);

            if (isFailure)
                foreach (var errorLine in errorList)
                    yield return ProcessOutput<TFinal>.Error(errorLine);
            else
            {
                var failed = false;
                IProcessOutput<TOutput>? processOutput = null;

                await foreach (var output in process.Execute())
                {
                    switch (output.OutputType)
                    {
                        case OutputType.Error: failed = true; yield return output.ConvertTo<TFinal>(); break;
                        case OutputType.Warning: yield return output.ConvertTo<TFinal>(); break;
                        case OutputType.Message: yield return output.ConvertTo<TFinal>(); break;
                        case OutputType.Success: processOutput = output;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                if(failed) yield break;
                if (processOutput == null)
                    yield return ProcessOutput<TFinal>.Error($"'{Name}' did not return success.");
                else
                    await foreach (var nextOutput in NextLink.Execute(processOutput.Value))
                        yield return nextOutput;
            }
        }
    }

    /// <summary>
    /// Sets properties on processes and freezes them. For use in Chains.
    /// </summary>
    public class ProcessFactory<TInput, TOutput, TImmutableProcess, TProcess> :
        ProcessFactory<TInput, TOutput, TImmutableProcess>
        where TImmutableProcess : ImmutableProcess<TOutput>
        where TProcess : Process
    {
        /// <summary>
        /// Creates a new process factory.
        /// </summary>
        /// <param name="process"></param>
        /// <param name="processSettings"></param>
        /// <param name="setProcess"></param>
        public ProcessFactory(TProcess process, IProcessSettings processSettings, Func<TInput, TProcess, Result<TProcess, ErrorList>> setProcess)
        {
            Process = process;
            ProcessSettings = processSettings;
            SetProcess = setProcess;
        }

        /// <inheritdoc />
        protected override Result<TImmutableProcess, ErrorList> GetProcess(TInput input)
        {
            var output = SetProcess(input, Process).Bind(x=>x.TryFreeze(ProcessSettings));

            if (output.IsFailure)
                return output.ConvertFailure<TImmutableProcess>();

            if (output.Value is TImmutableProcess process)
                return Result.Success<TImmutableProcess, ErrorList>(process);

            return Result.Failure<TImmutableProcess, ErrorList>(new ErrorList($"'{output.Value.Name}' does not have output type '{typeof(TOutput).Name}'."));
        }

        /// <inheritdoc />
        public override string Name => Process.GetName();

        /// <summary>
        /// The mutable process.
        /// </summary>
        public TProcess Process { get; }

        /// <summary>
        /// A function that sets the property on the process and returns the new process.
        /// </summary>
        public Func<TInput, TProcess, Result<TProcess, ErrorList>> SetProcess { get; }

        /// <summary>
        /// The process settings to use.
        /// </summary>
        public IProcessSettings ProcessSettings { get; }
    }


    /// <summary>
    /// Creates a process from an input.
    /// </summary>
    public abstract class ProcessFactory<TInput, TOutput, TImmutableProcess> where TImmutableProcess : ImmutableProcess<TOutput>
    {

        /// <summary>
        /// Sets the property on the process and freezes is.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        protected abstract Result<TImmutableProcess, ErrorList> GetProcess(TInput input);

        /// <summary>
        /// The name of the process.
        /// </summary>
        public abstract string Name { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Tries to create a new process from the input.
        /// </summary>
        public Result<TImmutableProcess, ErrorList> TryCreate(TInput input)
        {
            var mutableProcess = GetProcess(input);

            return mutableProcess;
        }
    }

    /// <summary>
    /// A series of processes where the result of each process is fed into the following process.
    /// </summary>
    public class Chain : Process
    {
        /// <summary>
        /// The process for this step in the chain.
        /// </summary>
        [YamlMember(Order = 1)]
        [Required]
        public Process Process { get; set; }

        /// <summary>
        /// The next step in the chain.
        /// </summary>
        [YamlMember(Order = 2)]
        public ChainLink? Into { get; set; }

        /// <inheritdoc />
        public override string GetReturnTypeInfo()
        {
            if (Into == null)
                return Process.GetReturnTypeInfo();
            else return Into.GetReturnTypeInfo();
        }

        /// <inheritdoc />
        public override string GetName()
        {
            return ProcessNameHelper.GetChainName(Process.GetName(), Into?.GetName());
        }

        /// <inheritdoc />
        public override Result<ImmutableProcess, ErrorList> TryFreeze(IProcessSettings processSettings)
        {
            return TryFreeze<Unit>(processSettings); //TODO improve this
        }

        private Result<ImmutableProcess, ErrorList> TryFreeze<TInput>(IProcessSettings processSettings)
        {
            throw new NotImplementedException();
        }


        /// <inheritdoc />
        public override IEnumerable<string> GetRequirements()
        {
            return Into == null ?
                Process.GetRequirements() :
                Process.GetRequirements().Concat(Into.GetRequirements()).Distinct();
        }

    }

    /// <summary>
    /// A step in the chain other than the first.
    /// </summary>
    public class ChainLink : Chain
    {
        /// <summary>
        /// The injection to inject the result of the previous method.
        /// </summary>
        [YamlMember(Order = 3)]
        [Required]
        public Injection Inject { get; set; }
    }
}
