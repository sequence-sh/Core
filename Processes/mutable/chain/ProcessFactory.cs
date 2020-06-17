using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes.immutable;
using Reductech.EDR.Utilities.Processes.mutable.injection;

namespace Reductech.EDR.Utilities.Processes.mutable.chain
{

    /// <summary>
    /// Creates a process from an input.
    /// </summary>
    public abstract class ProcessFactory<TInput, TOutput, TImmutableProcess>
        where TImmutableProcess : ImmutableProcess<TOutput>
    {

        /// <summary>
        /// Sets the property on the process and freezes is.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        protected abstract Result<TImmutableProcess> GetProcess(TInput input);

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
        public Result<TImmutableProcess> TryCreate(TInput input)
        {
            var mutableProcess = GetProcess(input);

            return mutableProcess;
        }
    }

    /// <summary>
    /// Creates a process from an input.
    /// </summary>
    public abstract class ProcessFactory<TInput, TOutput, TImmutableProcess, TProcess> :
        ProcessFactory<TInput, TOutput, TImmutableProcess>
        where TImmutableProcess : ImmutableProcess<TOutput>
        where TProcess : Process
    {
        /// <summary>
        /// Creates a new ProcessFactory.
        /// </summary>
        protected ProcessFactory(TProcess process, IProcessSettings processSettings)
        {
            Process = process;
            ProcessSettings = processSettings;
        }

        /// <inheritdoc />
        public override string Name => Process.GetName();

        /// <summary>
        /// The mutable process.
        /// </summary>
        public TProcess Process { get; }

        /// <summary>
        /// The process settings to use.
        /// </summary>
        public IProcessSettings ProcessSettings { get; }
    }

    /// <summary>
    /// For use in Chains.
    /// </summary>
    public class UnitProcessFactory<TOutput, TImmutableProcess, TProcess> : ProcessFactory<Unit, TOutput, TImmutableProcess, TProcess>
        where TImmutableProcess : ImmutableProcess<TOutput>
        where TProcess : Process
    {
        /// <inheritdoc />
        public UnitProcessFactory(TProcess process, IProcessSettings processSettings) : base(process, processSettings)
        {
        }//TODO freeze the process immediately - fail fast

        /// <inheritdoc />
        protected override Result<TImmutableProcess> GetProcess(Unit input)
        {
            var frozenProcess = Process.TryFreeze(ProcessSettings);

            if (frozenProcess.IsFailure)
                return frozenProcess.ConvertFailure<TImmutableProcess>();

            if (frozenProcess.Value is TImmutableProcess process)
                return Result.Success<TImmutableProcess>(process);

            return Result.Failure<TImmutableProcess>(new ErrorList($"'{frozenProcess.Value.Name}' does not have output type '{typeof(TOutput).Name}'."));
        }
    }

    /// <summary>
    /// Sets properties on processes and freezes them. For use in Chains.
    /// </summary>
    public class InjectionProcessFactory<TInput, TOutput, TImmutableProcess, TProcess> :
        ProcessFactory<TInput, TOutput, TImmutableProcess, TProcess>
        where TImmutableProcess : ImmutableProcess<TOutput>
        where TProcess : Process
    {
        /// <summary>
        /// Creates a new InjectionProcessFactory.
        /// </summary>
        public InjectionProcessFactory(TProcess process, IProcessSettings processSettings, Injection injection) : base(process, processSettings)
        {
            Injection = injection;
        }

        /// <inheritdoc />
        protected override Result<TImmutableProcess> GetProcess(TInput input)
        {
            var injectionResult = Injection.TryInject(input.ToString(), Process);
            if (injectionResult.IsFailure)
                return Result.Failure<TImmutableProcess>(new ErrorList(injectionResult.Error));

            var frozenProcess = Process.TryFreeze(ProcessSettings);

            if (frozenProcess.IsFailure)
                return frozenProcess.ConvertFailure<TImmutableProcess>();

            if (frozenProcess.Value is TImmutableProcess process)
                return Result.Success<TImmutableProcess>(process);

            return Result.Failure<TImmutableProcess>(new ErrorList($"'{frozenProcess.Value.Name}' does not have output type '{typeof(TOutput).Name}'."));
        }

        /// <summary>
        /// The injection to inject values into the process.
        /// </summary>
        public Injection Injection { get; }
    }
}