﻿using System.Collections.Generic;
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

        /// <inheritdoc />
        public override Result<ITypeReference> TryGetOutputTypeReference(IReadOnlyDictionary<string, IFreezableProcess> processArguments, IReadOnlyDictionary<string, IReadOnlyList<IFreezableProcess>> processListArguments) =>
            GetMemberType(processListArguments)
                .Map(x => new GenericTypeReference(typeof(List<>), new[] {x}) as ITypeReference);

        private Result<ITypeReference> GetMemberType(IReadOnlyDictionary<string, IReadOnlyList<IFreezableProcess>> processListArguments)
        {
            var result =
            processListArguments.TryFindOrFail(nameof(Array<object>.Elements), "Array elements not set.")
                .Bind(x => x.Select(r => r.TryGetOutputTypeReference()).Combine())
                .Bind(x => MultipleTypeReference.TryCreate(x, TypeName));


            return result;
        }

        /// <inheritdoc />
        public override string TypeName => typeof(Array<>).Name;

        /// <inheritdoc />
        public override string GetProcessName(IReadOnlyDictionary<string, IFreezableProcess> processArguments, IReadOnlyDictionary<string, IReadOnlyList<IFreezableProcess>> processListArguments)
        {
            var elementsCount = processListArguments.TryGetValue(nameof(Array<object>.Elements), out var elements)? elements.Count : 0;

            return NameHelper.GetArrayName(elementsCount);
        }

        /// <inheritdoc />
        protected override Result<IRunnableProcess> TryCreateInstance(ProcessContext processContext, IReadOnlyDictionary<string, IFreezableProcess> processArguments,
            IReadOnlyDictionary<string, IReadOnlyList<IFreezableProcess>> processListArguments) =>
            GetMemberType(processListArguments)
                .Bind(processContext.TryGetTypeFromReference)
                .Bind(x => TryCreateGeneric(typeof(Array<>), x));
    }

}