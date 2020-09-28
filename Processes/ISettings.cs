using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Processes
{
    /// <summary>
    /// External settings for running the step.
    /// </summary>
    public interface ISettings
    {

        /// <summary>
        /// Check that the requirement is met by these settings.
        /// </summary>
        Result<Unit, IRunErrors> CheckRequirement(string processName, Requirement requirement);
    }
}