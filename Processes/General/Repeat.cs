﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Creates an array by repeating an element.
    /// </summary>
    public sealed class Repeat<T> : CompoundRunnableProcess<List<T>>
    {
        /// <summary>
        /// The element to repeat.
        /// </summary>
        [RunnableProcessPropertyAttribute]
        [Required]
        public IRunnableProcess<T> Element { get; set; } = null!;

        /// <summary>
        /// The number of times to repeat the element
        /// </summary>
        [RunnableProcessPropertyAttribute]
        [Required]
        public IRunnableProcess<int> Number { get; set; } = null!;

        /// <inheritdoc />
        public override Result<List<T>, IRunErrors> Run(ProcessState processState) =>
            Element.Run(processState).Compose(() => Number.Run(processState))
                .Map(x => Enumerable.Repeat(x.Item1, x.Item2).ToList());

        /// <inheritdoc />
        public override IRunnableProcessFactory RunnableProcessFactory => RepeatProcessFactory.Instance;
    }

    /// <summary>
    /// Creates an array by repeating an element.
    /// </summary>
    public sealed class RepeatProcessFactory : GenericProcessFactory
    {
        private RepeatProcessFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static GenericProcessFactory Instance { get; } = new RepeatProcessFactory();

        /// <inheritdoc />
        public override Type ProcessType => typeof(Repeat<>);

        /// <inheritdoc />
        public override string OutputTypeExplanation => "List<T>";

        /// <inheritdoc />
        protected override ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference) => new GenericTypeReference(typeof(List<>), new []{memberTypeReference});

        /// <inheritdoc />
        protected override Result<ITypeReference> GetMemberType(FreezableProcessData freezableProcessData) =>
            freezableProcessData.GetArgument(nameof(Repeat<object>.Element))
                .Bind(x => x.TryGetOutputTypeReference());
    }
}