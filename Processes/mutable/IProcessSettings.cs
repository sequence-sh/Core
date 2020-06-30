namespace Reductech.EDR.Processes.Mutable
{
    /// <summary>
    /// External settings for running the process.
    /// </summary>
    public interface IProcessSettings
    {

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


        private EmptySettings()
        {

        }

    }

}