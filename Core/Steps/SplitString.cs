﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Splits a string.
    /// </summary>
    public sealed class SplitString : CompoundStep<List<string>>
    {
        /// <summary>
        /// The string to split.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<string> String { get; set; } = null!;

        /// <summary>
        /// The delimiter to use.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<string> Delimiter { get; set; } = null!;

        /// <inheritdoc />
        public override async Task<Result<List<string>, IRunErrors>>  Run(StateMonad stateMonad, CancellationToken cancellationToken)
        {
            return await String.Run(stateMonad, cancellationToken).Compose(() => Delimiter.Run(stateMonad, cancellationToken))
                .Map(x => x.Item1.Split(new[] {x.Item2}, StringSplitOptions.None).ToList());
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => SplitStringStepFactory.Instance;
    }

    /// <summary>
    /// Splits a string.
    /// </summary>
    public class SplitStringStepFactory : SimpleStepFactory<SplitString, List<string>>
    {
        private SplitStringStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<SplitString, List<string>> Instance { get; } = new SplitStringStepFactory();
    }
}