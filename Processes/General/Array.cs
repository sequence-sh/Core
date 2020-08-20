using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.General
{

    /// <summary>
    /// Represents an ordered collection of objects.
    /// </summary>
    public sealed class Array<T> : CompoundRunnableProcess<List<T>>
    {
        /// <inheritdoc />
        public override Result<List<T>> Run(ProcessState processState)
        {
            var result = Elements.Select(x => x.Run(processState)).Combine().Map(x => x.ToList());

            return result;
        }

        /// <inheritdoc />
        public override RunnableProcessFactory RunnableProcessFactory => ArrayProcessFactory.Instance;

        /// <summary>
        /// The elements of this array.
        /// </summary>
        [RunnableProcessListProperty]
        [Required]
        public IReadOnlyList<IRunnableProcess<T>> Elements { get; set; } = null!;
    }

    /// <summary>
    /// The factory for creating Arrays.
    /// </summary>
    public class ArrayProcessFactory : GenericProcessFactory
    {
        private ArrayProcessFactory() {}

        public static GenericProcessFactory Instance { get; } = new ArrayProcessFactory();

        /// <inheritdoc />
        public override Type ProcessType => typeof(Array<>);

        /// <inheritdoc />
        protected override ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference) => new GenericTypeReference(typeof(List<>), new []{memberTypeReference});

        /// <inheritdoc />
        public override IProcessNameBuilder ProcessNameBuilder => new ProcessNameBuilderFromTemplate($"[[{nameof(Array<object>.Elements)}]]");

        /// <inheritdoc />
        protected override Result<ITypeReference> GetMemberType(FreezableProcessData freezableProcessData)
        {
            var result =
            freezableProcessData.GetListArgument(nameof(Array<object>.Elements))
                .Bind(x => x.Select(r => r.TryGetOutputTypeReference()).Combine())
                .Bind(x => MultipleTypeReference.TryCreate(x, TypeName));


            return result;
        }
    }
}