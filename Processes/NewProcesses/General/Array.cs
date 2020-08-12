using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.NewProcesses.General
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
    public class ArrayProcessFactory : RunnableProcessFactory
    {
        private ArrayProcessFactory() {}

        /// <summary>
        /// The ArrayProcessFactory.
        /// </summary>
        public static RunnableProcessFactory Instance { get; } = new ArrayProcessFactory();
        private Result<ITypeReference> GetMemberType(FreezableProcessData freezableProcessData)
        {
            var result =
            freezableProcessData.GetListArgument(nameof(Array<object>.Elements))
                .Bind(x => x.Select(r => r.TryGetOutputTypeReference()).Combine())
                .Bind(x => MultipleTypeReference.TryCreate(x, TypeName));


            return result;
        }

        /// <inheritdoc />
        public override Result<ITypeReference> TryGetOutputTypeReference(FreezableProcessData freezableProcessData) =>
            GetMemberType(freezableProcessData)
                .Map(x => new GenericTypeReference(typeof(List<>), new[] { x }) as ITypeReference);

        /// <inheritdoc />
        public override Type ProcessType => typeof(Array<>);


        /// <inheritdoc />
        public override ProcessNameBuilder ProcessNameBuilder { get; } = new ProcessNameBuilder($"[[{nameof(Array<object>.Elements)}]]");

        /// <inheritdoc />
        public override IEnumerable<Type> EnumTypes => ImmutableArray<Type>.Empty;

        /// <inheritdoc />
        protected override Result<IRunnableProcess> TryCreateInstance(ProcessContext processContext, FreezableProcessData freezableProcessData) =>
            GetMemberType(freezableProcessData)
                .Bind(processContext.TryGetTypeFromReference)
                .Bind(x => TryCreateGeneric(typeof(Array<>), x));



    }



}