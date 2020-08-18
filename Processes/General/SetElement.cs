//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using CSharpFunctionalExtensions;

//namespace Reductech.EDR.Processes.NewProcesses.General //TODO set element
//{
//    /// <summary>
//    /// Sets a particular element of an array
//    /// </summary>
//    public sealed class SetElement<T> : CompoundRunnableProcess<Unit>
//    {
//        /// <summary>
//        /// The array to modify.
//        /// </summary>
//        [VariableName]
//        [Required]
//        public VariableName Array { get; set; }


//        /// <summary>
//        /// The element to set in the array.
//        /// </summary>
//        [RunnableProcessProperty]
//        [Required]
//        public IRunnableProcess<T> Element { get; set; } = null!;

//        /// <summary>
//        /// The index to set the element at.
//        /// </summary>
//        [RunnableProcessProperty]
//        [Required]
//        public IRunnableProcess<int> Index { get; set; } = null!;

//        /// <inheritdoc />
//        public override Result<Unit> Run(ProcessState processState)
//        {
//            return Array.Run(processState)
//                .Compose(()=> Element.Run(processState))
//                .Compose(()=> Index.Run(processState))
//                .Ensure(x=> x.Item2>= 0 && x.Item2 < x.Item1.Item1.Count, "Index was outside the bounds of the array")
//                .Map()

//        }

//        /// <inheritdoc />
//        public override RunnableProcessFactory RunnableProcessFactory => SetElementProcessFactory.Instance;
//    }

//    /// <summary>
//    /// Sets a particular element of an array
//    /// </summary>
//    public sealed class SetElementProcessFactory : GenericProcessFactory
//    {
//        private SetElementProcessFactory() {}

//        public static GenericProcessFactory Instance { get; } = new SetElementProcessFactory();

//        /// <inheritdoc />
//        public override Type ProcessType => typeof(SetElement<>);

//        /// <inheritdoc />
//        protected override ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference) => new ActualTypeReference(typeof(Unit));

//        /// <inheritdoc />
//        protected override Result<ITypeReference> GetMemberType(FreezableProcessData freezableProcessData) =>



//            freezableProcessData.GetArgument(nameof(SetElement<object>.Array))
//                .Bind(x => x.TryGetOutputTypeReference())
//                .BindCast<ITypeReference, GenericTypeReference>()
//                .Map(x => x.ChildTypes)
//                .BindSingle()
//                .Compose(() => freezableProcessData.GetArgument(nameof(SetElement<object>.Element))
//                    .Bind(x => x.TryGetOutputTypeReference()))
//                .Bind(x=>MultipleTypeReference.TryCreate(new[]{x.Item1, x.Item2}, TypeName));
//    }
//}