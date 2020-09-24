using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Processes
{
    /// <summary>
    /// External settings for running the process.
    /// </summary>
    public interface IProcessSettings
    {

        /// <summary>
        /// Check that the requirement is met by these settings.
        /// </summary>
        Result<Unit, IRunErrors> CheckRequirement(string processName, Requirement requirement);
    }

    /// <summary>
    /// A settings object with no fields.
    /// </summary>
    public class EmptySettings : IProcessSettings
    {
        /// <summary>
        /// Gets the instance of EmptySettings.
        /// </summary>
        public static IProcessSettings Instance = new EmptySettings();


        private EmptySettings() {}

        /// <inheritdoc />
        public Result<Unit, IRunErrors> CheckRequirement(string processName, Requirement requirement) =>
            new RunError($"Requirement '{requirement}' not met.", processName, null,
                ErrorCode.RequirementsNotMet);
    }

}