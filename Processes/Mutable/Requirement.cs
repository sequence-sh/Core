using System;
using System.ComponentModel.DataAnnotations;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Processes.Mutable
{
    /// <summary>
    /// A requirement of a process.
    /// </summary>
    public sealed class Requirement
    {
        /// <summary>
        /// The name of the required software.
        /// </summary>
        [YamlMember(Order = 1)]
        [Required]
#pragma warning disable 8618
        public string Name { get; set; }
#pragma warning restore 8618

        /// <summary>
        /// The minimum required version. Inclusive.
        /// </summary>
        [YamlMember(Order = 1)]
        public Version? MinVersion { get; set; }

        /// <summary>
        /// The The version above the highest allowed version.
        /// </summary>
        [YamlMember(Order = 1)]
        public Version? MaxVersion { get; set; }

        /// <summary>
        /// Notes on the requirement.
        /// </summary>
        public string? Notes { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return ToTuple.ToString()!;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is Requirement r && ToTuple.Equals(r.ToTuple);
        }

        /// <inheritdoc />
        public override int GetHashCode() => ToTuple.GetHashCode();

        private object ToTuple => (Name, MinVersion, MaxVersion, Notes);
    }
}