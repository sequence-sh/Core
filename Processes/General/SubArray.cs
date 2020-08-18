//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using CSharpFunctionalExtensions;

//namespace Reductech.EDR.Processes.NewProcesses.General
//{
//TODO subarray
//    /// <summary>
//    /// Creates a subArray from an array
//    /// </summary>
//    public sealed class SubArray<T> : CompoundRunnableProcess<List<T>>
//    {
//        /// <summary>
//        /// The array to modify.
//        /// </summary>
//        [RunnableProcessProperty]
//        [Required]
//        public IRunnableProcess<List<T>> Array { get; set; } = null!;


//        /// <summary>
//        /// The index of the first element of the SubArray.
//        /// </summary>
//        [RunnableProcessProperty]
//        [Required]
//        public IRunnableProcess<int> Index { get; set; } = null!;

//        /// <summary>
//        /// The length of the sub array.
//        /// </summary>
//        [RunnableProcessProperty]
//        [Required]
//        public IRunnableProcess<int> Length { get; set; } = null!;

//        /// <inheritdoc />
//        public override Result<List<T>> Run(ProcessState processState)
//        {
//            throw new System.NotImplementedException();
//        }

//        /// <inheritdoc />
//        public override RunnableProcessFactory RunnableProcessFactory { get; }
//    }

//    public class SubArrayProcessFactory : GenericProcessFactory
//    {
//        private SubArrayProcessFactory() { }

//        public static GenericProcessFactory Instance { get; } = new SubArrayProcessFactory();

//        /// <inheritdoc />
//        public override Type ProcessType => typeof(SubArray<>);

//        /// <inheritdoc />
//        protected override ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference)
//        {
//            throw new NotImplementedException();
//        }

//        /// <inheritdoc />
//        protected override Result<ITypeReference> GetMemberType(FreezableProcessData freezableProcessData)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}