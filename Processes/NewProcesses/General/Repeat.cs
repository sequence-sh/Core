using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.NewProcesses.General
{
    /// <summary>
    /// Creates an array by repeating an element.
    /// </summary>
    public sealed class Repeat<T> : CompoundRunnableProcess<List<T>>
    {
        /// <summary>
        /// The element to repeat.
        /// </summary>
        public IRunnableProcess<T> Element { get; set; } = null!;

        /// <summary>
        /// The number of times to repeat the element
        /// </summary>
        public IRunnableProcess<int> Number { get; set; } = null!;

        /// <inheritdoc />
        public override Result<List<T>> Run(ProcessState processState) =>
            Element.Run(processState).Compose(() => Number.Run(processState))
                .Map(x => Enumerable.Repeat(x.Item1, x.Item2).ToList());

        /// <inheritdoc />
        public override RunnableProcessFactory RunnableProcessFactory => RepeatProcessFactory.Instance;
    }

    /// <summary>
    /// Creates an array by repeating an element.
    /// </summary>
    public sealed class RepeatProcessFactory : GenericProcessFactory
    {
        private RepeatProcessFactory() { }

        public static GenericProcessFactory Instance { get; } = new RepeatProcessFactory();

        /// <inheritdoc />
        public override Type ProcessType => typeof(Repeat<>);

        /// <inheritdoc />
        protected override ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference) => new GenericTypeReference(typeof(List<>), new []{memberTypeReference});

        /// <inheritdoc />
        protected override Result<ITypeReference> GetMemberType(FreezableProcessData freezableProcessData) =>
            freezableProcessData.GetArgument(nameof(Repeat<object>.Element))
                .Bind(x => x.TryGetOutputTypeReference());
    }
}