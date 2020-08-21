using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Counts the elements in an array.
    /// </summary>
    public sealed class ArrayCount<T> : CompoundRunnableProcess<int>
    {
        /// <summary>
        /// The array to count.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<List<T>> Array { get; set; } = null!;

        /// <inheritdoc />
        public override Result<int, IRunErrors> Run(ProcessState processState) => Array.Run(processState).Map(x => x.Count);

        /// <inheritdoc />
        public override IRunnableProcessFactory RunnableProcessFactory => ArrayCountProcessFactory.Instance;
    }

    /// <summary>
    /// Counts the elements in an array.
    /// </summary>
    public sealed class ArrayCountProcessFactory : GenericProcessFactory
    {
        private ArrayCountProcessFactory() { }

        public static GenericProcessFactory Instance { get; } = new ArrayCountProcessFactory();

        /// <inheritdoc />
        public override Type ProcessType => typeof(ArrayCount<>);

        /// <inheritdoc />
        protected override ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference) => new ActualTypeReference(typeof(int));

        /// <inheritdoc />
        protected override Result<ITypeReference> GetMemberType(FreezableProcessData freezableProcessData) =>
            freezableProcessData.GetArgument(nameof(ArrayCount<object>.Array))
                .Bind(x => x.TryGetOutputTypeReference())
                .BindCast<ITypeReference, GenericTypeReference>()
                .Map(x => x.ChildTypes)
                .BindSingle();
    }
}