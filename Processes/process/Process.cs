using System.Collections.Generic;
using CSharpFunctionalExtensions;

namespace Processes.process
{
    /// <summary>
    /// A settings object with no fields.
    /// </summary>
    public class EmptySettings : IProcessSettings
    {

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
        /// Get errors in the properties of this process that may prevent it from running properly.
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<string> GetArgumentErrors();

        /// <summary>
        /// Get errors in the process settings.
        /// </summary>
        public abstract IEnumerable<string> GetSettingsErrors(IProcessSettings processSettings);

        /// <summary>
        /// The name of this process
        /// </summary>
        public abstract string GetName();

        /// <summary>
        /// Executes this process. Should only be called if all conditions are met
        /// </summary>
        /// <returns></returns>
        public abstract IAsyncEnumerable<Result<string>> Execute(IProcessSettings processSettings);

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