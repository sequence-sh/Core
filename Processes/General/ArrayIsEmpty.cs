using System;
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
    /// Checks if an array is empty.
    /// </summary>
    public sealed class ArrayIsEmpty<T> : CompoundRunnableProcess<bool>
    {
        /// <summary>
        /// The array to check for emptiness.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<List<T>> Array { get; set; } = null!;

        /// <inheritdoc />
        public override Result<bool, IRunErrors> Run(ProcessState processState) => Array.Run(processState).Map(x => !x.Any());

        /// <inheritdoc />
        public override IRunnableProcessFactory RunnableProcessFactory => ArrayIsEmptyProcessFactory.Instance;
    }


    /// <summary>
    /// Checks if an array is empty.
    /// </summary>
    public sealed class ArrayIsEmptyProcessFactory : GenericProcessFactory
    {
        private ArrayIsEmptyProcessFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static GenericProcessFactory Instance { get; } = new ArrayIsEmptyProcessFactory();

        /// <inheritdoc />
        public override Type ProcessType => typeof(ArrayIsEmpty<>);

        /// <inheritdoc />
        public override string OutputTypeExplanation => nameof(Boolean);

        /// <inheritdoc />
        protected override ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference) => new ActualTypeReference(typeof(bool));

        /// <inheritdoc />
        protected override Result<ITypeReference> GetMemberType(FreezableProcessData freezableProcessData) =>
            freezableProcessData.GetArgument(nameof(ArrayCount<object>.Array))
                .Bind(x => x.TryGetOutputTypeReference())
                .BindCast<ITypeReference, GenericTypeReference>()
                .Map(x => x.ChildTypes)
                .BindSingle();
    }


}