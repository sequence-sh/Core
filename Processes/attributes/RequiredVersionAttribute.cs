using System;

namespace Reductech.EDR.Utilities.Processes
{
    /// <summary>
    /// Use this attribute to denote the required version of some software.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class RequiredVersionAttribute : Attribute
    {
        /// <summary>
        /// Create a new RequiredVersion attribute
        /// </summary>
        /// <param name="softwareName">e.g. "Nuix"</param>
        /// <param name="requiredVersion">e.g. "6.2"</param>
        public RequiredVersionAttribute(string softwareName, string requiredVersion)
        {
            SoftwareName = softwareName;
            RequiredVersion = new Version(requiredVersion);
        }

        /// <summary>
        /// The software whose version is required.
        /// </summary>
        public string SoftwareName { get; }

        /// <summary>
        /// The required version of the software.
        /// </summary>
        public Version RequiredVersion { get; }

        /// <summary>
        /// The required version in human readable form.
        /// </summary>
        public string Text => $"{SoftwareName} {RequiredVersion}";

        /// <inheritdoc />
        public override string ToString()
        {
            return Text;
        }
    }
}