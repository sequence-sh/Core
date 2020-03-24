using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes.immutable;

namespace Reductech.EDR.Utilities.Processes.mutable
{
    /// <summary>
    /// A settings object with no fields.
    /// </summary>
    public class EmptySettings : IProcessSettings
    {
        /// <summary>
        /// Gets the instance of EmptySettings.
        /// </summary>
        public static IProcessSettings Instance = new EmptySettings();


        private EmptySettings()
        {

        }

    }

    /// <summary>
    /// External settings for running the process.
    /// </summary>
    public interface IProcessSettings
    {

    }

    /// <summary>
    /// A process. Can contain one or more steps
    /// </summary>
    public abstract class Process
    {
        /// <summary>
        /// The name of this process
        /// </summary>
        public abstract string GetName();

        /// <summary>
        /// Executes this process. Should only be called if all conditions are met
        /// </summary>
        /// <returns></returns>
        public abstract Result<ImmutableProcess, ErrorList> TryFreeze(IProcessSettings processSettings);

        /// <summary>
        /// String representation of this process
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return GetName();
        }
    }
}