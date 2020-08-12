using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.NewProcesses.General
{
    /// <summary>
    /// Do an action for each member of the list.
    /// </summary>
    public sealed class ForEach<T> : CompoundRunnableProcess<Unit>
    {
        /// <summary>
        /// The action to perform repeatedly.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<Unit> Action { get; set; } = null!;


        /// <summary>
        /// The name of the variable to loop over.
        /// </summary>
        [VariableName]
        [Required]
        public VariableName VariableName { get; set; }

        /// <summary>
        /// The elements to iterate over.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<List<T>> List { get; set; } = null!;

        /// <inheritdoc />
        public override Result<Unit> Run(ProcessState processState)
        {
            var elements = List.Run(processState);
            if (elements.IsFailure) return elements.ConvertFailure<Unit>();

            foreach (var element in elements.Value)
            {
                var setResult = processState.SetVariable(VariableName, element);
                if (setResult.IsFailure) return setResult.ConvertFailure<Unit>();

                var r = Action.Run(processState);
                if (r.IsFailure) return r;
            }

            return Unit.Default;
        }

        /// <inheritdoc />
        public override RunnableProcessFactory RunnableProcessFactory => ForeachProcessFactory.Instance;
    }

    /// <summary>
    /// Do an action for each member of the list.
    /// </summary>
    public sealed class ForeachProcessFactory : RunnableProcessFactory
    {
        private ForeachProcessFactory() { }

        public static RunnableProcessFactory Instance { get; } = new ForeachProcessFactory();

        /// <inheritdoc />
        public override Result<ITypeReference> TryGetOutputTypeReference(FreezableProcessData freezableProcessData) => new ActualTypeReference(typeof(Unit));

        /// <inheritdoc />
        public override Result<Maybe<ITypeReference>> GetTypeReferencesSet(VariableName variableName, FreezableProcessData freezableProcessData) =>
            freezableProcessData.GetArgument(nameof(ForEach<object>.List))
                .Bind(x => x.TryGetOutputTypeReference())
                .BindCast<ITypeReference, GenericTypeReference>()
                .Map(x=>x.ChildTypes)
                .BindSingle()
                .Map(Maybe<ITypeReference>.From);

        /// <inheritdoc />
        public override Type ProcessType => typeof(ForEach<>);

        /// <inheritdoc />
        public override ProcessNameBuilder ProcessNameBuilder => new ProcessNameBuilder($"Foreach [{nameof(ForEach<object>.VariableName)}] in [{nameof(ForEach<object>.List)}]; [{nameof(ForEach<object>.Action)}]");

        /// <inheritdoc />
        public override IEnumerable<Type> EnumTypes => ImmutableArray<Type>.Empty;

        private static Result<ITypeReference> GetMemberType(FreezableProcessData freezableProcessData) =>
            freezableProcessData.GetArgument(nameof(ForEach<object>.List))
                .Bind(x => x.TryGetOutputTypeReference());

        /// <inheritdoc />
        protected override Result<IRunnableProcess> TryCreateInstance(ProcessContext processContext, FreezableProcessData freezableProcessData) =>
            GetMemberType(freezableProcessData)
                .BindCast<ITypeReference, GenericTypeReference>()
                .Map(x => x.ChildTypes)
                .BindSingle()
                .Bind(processContext.TryGetTypeFromReference)

                .Bind(x => TryCreateGeneric(typeof(ForEach<>), x));
    }
}