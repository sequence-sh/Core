using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Trims a string.
    /// </summary>
    public sealed class Trim : CompoundRunnableProcess<string>
    {

        /// <summary>
        /// The string to change the case of.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<string> String { get; set; } = null!;

        /// <summary>
        /// The side to trim.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<TrimSide> Side { get; set; } = null!;

        /// <inheritdoc />
        public override Result<string, IRunErrors> Run(ProcessState processState) =>
            String.Run(processState).Compose(() => Side.Run(processState))
                .Map(x => TrimString(x.Item1, x.Item2));

        private static string TrimString(string s, TrimSide side) =>
            side switch
            {
                TrimSide.Left => s.TrimStart(),
                TrimSide.Right => s.TrimEnd(),
                TrimSide.Both => s.Trim(),
                _ => throw new ArgumentOutOfRangeException(nameof(side), side, null)
            };

        /// <inheritdoc />
        public override IRunnableProcessFactory RunnableProcessFactory => TrimProcessFactory.Instance;
    }

    /// <summary>
    /// Trims a string.
    /// </summary>
    public sealed class TrimProcessFactory : SimpleRunnableProcessFactory<Trim, string>
    {
        private TrimProcessFactory() { }

        public static SimpleRunnableProcessFactory<Trim, string> Instance { get; } = new TrimProcessFactory();

        /// <inheritdoc />
        public override IEnumerable<Type> EnumTypes => new[] { typeof(TrimSide) };
    }


    /// <summary>
    /// The side of the string to trim.
    /// </summary>
    public enum TrimSide
    {
        Left,
        Right,
        Both
    }
}