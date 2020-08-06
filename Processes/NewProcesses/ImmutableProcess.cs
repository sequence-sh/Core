namespace Reductech.EDR.Processes.NewProcesses
{
    /// <summary>
    /// A process that can be run.
    /// </summary>
    public interface IRunnableProcess
    {
        /// <summary>
        /// The name of this process.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Convert this RunnableProcess into a FreezableProcess.
        /// </summary>
        IFreezableProcess Unfreeze();
    }
}
