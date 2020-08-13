using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.NewProcesses.General
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
        public IRunnableProcess<List<T>> Array { get; set; } = null!;

        /// <summary>
        /// The element to look for.
        /// </summary>
        public IRunnableProcess<T> Element { get; set; } = null!;

        /// <inheritdoc />
        public override Result<int> Run(ProcessState processState) =>
            Array.Run(processState).Compose(() => Element.Run(processState))
                .Map(x => x.Item1.IndexOf(x.Item2));

        /// <inheritdoc />
        public override RunnableProcessFactory RunnableProcessFactory => FirstIndexOfProcessFactory.Instance;
    }

    /// <summary>
    /// Gets the first index of an element in an array.
    /// </summary>
    public sealed class FirstIndexOfElementProcessFactory : GenericProcessFactory
    {
        private FirstIndexOfElementProcessFactory() { }

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