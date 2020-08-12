using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.NewProcesses
{
    /// <summary>
    /// Gets the value of a named variable.
    /// </summary>
    public class GetVariableProcessFactory : RunnableProcessFactory
    {
        private GetVariableProcessFactory() {}

        public static RunnableProcessFactory Instance { get; } = new GetVariableProcessFactory();

        /// <inheritdoc />
        public override Result<ITypeReference> TryGetOutputTypeReference(FreezableProcessData freezableProcessData) =>
            freezableProcessData.GetVariableName(nameof(GetVariable<object>.VariableName))
                .Map(x => new VariableTypeReference(x) as ITypeReference);

        /// <inheritdoc />
        public override Type ProcessType => typeof(GetVariable<>);

        /// <inheritdoc />
        public override ProcessNameBuilder ProcessNameBuilder => new ProcessNameBuilder($"<[{nameof(GetVariable<object>.VariableName)}]>");

        /// <inheritdoc />
        public override IEnumerable<Type> EnumTypes => ImmutableArray<Type>.Empty;


        /// <inheritdoc />
        protected override Result<IRunnableProcess> TryCreateInstance(ProcessContext processContext, FreezableProcessData freezableProcessData) =>
            TryGetOutputTypeReference(freezableProcessData)
                .Bind(processContext.TryGetTypeFromReference)
                .Bind(x => TryCreateGeneric(typeof(GetVariable<>), x));


        /// <summary>
        /// Gets the value of a named variable.
        /// </summary>
        public sealed class GetVariable<T> : CompoundRunnableProcess<T>
        {
            /// <summary>
            /// Necessary Parameterless constructor
            /// </summary>
            public GetVariable(){}


            public GetVariable(VariableName variableName) => VariableName = variableName;


            /// <inheritdoc />
            public override Result<T> Run(ProcessState processState) => processState.GetVariable<T>(VariableName);

            /// <inheritdoc />
            public override RunnableProcessFactory RunnableProcessFactory => Instance;

            [VariableName]
            [Required]
            public VariableName VariableName { get; set; }
        }
    }
}