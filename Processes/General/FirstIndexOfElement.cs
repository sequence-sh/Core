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
    /// Gets the first index of an element in an array.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class FirstIndexOfElement<T> : CompoundRunnableProcess<int>
    {
        /// <summary>
        /// The array to check.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<List<T>> Array { get; set; } = null!;

        /// <summary>
        /// The element to look for.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<T> Element { get; set; } = null!;

        /// <inheritdoc />
        public override Result<int, IRunErrors> Run(ProcessState processState) =>
            Array.Run(processState).Compose(() => Element.Run(processState))
                .Map(x => x.Item1.IndexOf(x.Item2));

        /// <inheritdoc />
        public override IRunnableProcessFactory RunnableProcessFactory => FirstIndexOfElementProcessFactory.Instance;
    }

    /// <summary>
    /// Gets the first index of an element in an array.
    /// </summary>
    public sealed class FirstIndexOfElementProcessFactory : GenericProcessFactory
    {
        private FirstIndexOfElementProcessFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static GenericProcessFactory Instance { get; } = new FirstIndexOfElementProcessFactory();

        /// <inheritdoc />
        public override Type ProcessType => typeof(FirstIndexOfElement<>);

        /// <inheritdoc />
        protected override ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference) => new ActualTypeReference(typeof(int));

        /// <inheritdoc />
        protected override Result<ITypeReference> GetMemberType(FreezableProcessData freezableProcessData) =>
            freezableProcessData.GetArgument(nameof(FirstIndexOfElement<object>.Array))
                .Bind(x => x.TryGetOutputTypeReference())
                .BindCast<ITypeReference, GenericTypeReference>()
                .Map(x => x.ChildTypes)
                .BindSingle();
    }
}