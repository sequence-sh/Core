using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Util;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Processes
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

        /// <summary>
        /// Tries to convert an object to a requirement
        /// </summary>
        public static Result<Requirement> TryConvert(object o)
        {
            if (o is Requirement req) return req;

            if (o is IReadOnlyDictionary<object, object> dictionary)
            {
                var requirement = new Requirement();

                var results = new List<Result>();


                foreach (var (key, value) in dictionary.Where(x => x.Value != null))
                {
                    var result = key switch
                    {
                        nameof(MinVersion) => TryConvertToVersion(value).Tap(x => requirement.MinVersion = x),
                        nameof(MaxVersion) => TryConvertToVersion(value).Tap(x => requirement.MaxVersion = x),
                        nameof(Name) => value.TryConvert<string>().Tap(x => requirement.Name = x),
                        nameof(Notes) => value.TryConvert<string>().Tap(x => requirement.Notes = x),
                        _ => Result.Failure($"Could not recognize property '{key}'.")
                    };

                    results.Add(result);
                }

                var r = results
                    .Combine().Bind<Requirement>(() => requirement);

                return r;

            }


            return Result.Failure<Requirement>($"Could not convert '{o}' to {nameof(Requirement)}");
        }

        private static Result<Version> TryConvertToVersion(object o)
        {
            if (o is Version ver) return ver;

            if (o is IReadOnlyDictionary<object, object> dictionary)
            {
                var major = -1;
                var minor = -1;
                var build = -1;
                var revision = -1;

                var results = new List<Result>();


                foreach (var (key, value) in dictionary.Where(x => x.Value != null))
                {
                    var result = key switch
                    {
                        nameof(Version.Major) => value.TryConvert<int>().Tap(x =>major = x),
                        nameof(Version.Minor) => value.TryConvert<int>().Tap(x =>minor = x),
                        nameof(Version.Build) => value.TryConvert<int>().Tap(x =>build = x),
                        nameof(Version.Revision) => value.TryConvert<int>().Tap(x =>revision = x),
                        nameof(Version.MajorRevision) => Result.Success(),
                        nameof(Version.MinorRevision) => Result.Success(),
                        _ => Result.Failure($"Could not recognize property '{key}'.")
                    };

                    results.Add(result);
                }

                var r = results
                    .Combine().Bind(() =>
                    {
                        Version version;

                        if (major < 0 || minor < 0)
                            return Result.Failure<Version>("Major and Minor versions must both be greater than zero");
                        if (build < 0) version = new Version(major, minor);
                        else if (revision < 0) version = new Version(major, minor, build);
                        else version = new Version(major, minor, build, revision);

                        return version;
                    });

                return r;

            }


            return Result.Failure<Version>($"Could not convert '{o}' to {nameof(Version)}");
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append(Name);
            if (MinVersion != null)
                sb.Append($" Version {MinVersion}");
            if (MaxVersion != null)
                sb.Append($" Version <= {MaxVersion}");

            if (Notes != null)
                sb.Append($" ({Notes})");

            return sb.ToString();
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