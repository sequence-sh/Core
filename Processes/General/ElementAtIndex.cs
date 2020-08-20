using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Gets the array element at a particular index.
    /// </summary>
    public sealed class ElementAtIndex<T> : CompoundRunnableProcess<T>
    {
        /// <summary>
        /// The array to check.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<List<T>> Array { get; set; } = null!;

        /// <summary>
        /// The index to get the element at.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<int> Index { get; set; } = null!;

        /// <inheritdoc />
        public override Result<T> Run(ProcessState processState) =>
            Array.Run(processState)
                .Compose(() => Index.Run(processState))
                .Ensure(x => x.Item2 >= 0 && x.Item2 < x.Item1.Count, "Index was out of the range of the array.")
                .Map(x=>x.Item1[x.Item2]);

        /// <inheritdoc />
        public override RunnableProcessFactory RunnableProcessFactory => ElementAtIndexProcessFactory.Instance;
    }


    /// <summary>
    /// Gets the array element at a particular index.
    /// </summary>
    public sealed class ElementAtIndexProcessFactory : GenericProcessFactory
    {
        private ElementAtIndexProcessFactory() { }

        public static GenericProcessFactory Instance { get; } = new ElementAtIndexProcessFactory();

        /// <inheritdoc />
        public override Type ProcessType => typeof(ElementAtIndex<>);

        /// <inheritdoc />
        protected override ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference) => memberTypeReference;

        /// <inheritdoc />
        protected override Result<ITypeReference> GetMemberType(FreezableProcessData freezableProcessData) =>
            freezableProcessData.GetArgument(nameof(ElementAtIndex<object>.Array))
                .Bind(x => x.TryGetOutputTypeReference())
                .BindCast<ITypeReference, GenericTypeReference>()
                .Map(x => x.ChildTypes)
                .BindSingle();
    }
}