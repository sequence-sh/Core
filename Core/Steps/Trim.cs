using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Trims a string.
    /// </summary>
    public sealed class Trim : CompoundStep<string>
    {

        /// <summary>
        /// The string to change the case of.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<string> String { get; set; } = null!;

        /// <summary>
        /// The side to trim.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<TrimSide> Side { get; set; } = null!;

        /// <inheritdoc />
        public override async Task<Result<string, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            return await String.Run(stateMonad, cancellationToken).Compose(() => Side.Run(stateMonad, cancellationToken))
                .Map(x => TrimString(x.Item1, x.Item2));
        }

        private static string TrimString(string s, TrimSide side) =>
            side switch
            {
                TrimSide.Left => s.TrimStart(),
                TrimSide.Right => s.TrimEnd(),
                TrimSide.Both => s.Trim(),
                _ => throw new ArgumentOutOfRangeException(nameof(side), side, null)
            };

        /// <inheritdoc />
        public override IStepFactory StepFactory => TrimStepFactory.Instance;
    }


    /// <summary>
    /// Trims a string.
    /// </summary>
    public sealed class TrimStepFactory : SimpleStepFactory<Trim, string>
    {
        private TrimStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<Trim, string> Instance { get; } = new TrimStepFactory();

        /// <inheritdoc />
        public override IEnumerable<Type> EnumTypes => new[] { typeof(TrimSide) };
    }
}