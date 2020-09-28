using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Processes
{
    /// <summary>
    /// A settings object with no fields.
    /// </summary>
    public class EmptySettings : ISettings
    {
        /// <summary>
        /// Gets the instance of EmptySettings.
        /// </summary>
        public static ISettings Instance = new EmptySettings();


        private EmptySettings() {}

        /// <inheritdoc />
        public Result<Unit, IRunErrors> CheckRequirement(string processName, Requirement requirement) =>
            new RunError($"Requirement '{requirement}' not met.", processName, null,
                ErrorCode.RequirementsNotMet);
    }
}