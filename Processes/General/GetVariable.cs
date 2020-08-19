using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;

namespace Reductech.EDR.Processes.General
{

    /// <summary>
    /// Gets the value of a named variable.
    /// </summary>
    public sealed class GetVariable<T> : CompoundRunnableProcess<T>
    {
        /// <summary>
        /// Necessary Parameterless constructor
        /// </summary>
        public GetVariable() { }

        public GetVariable(VariableName variableName) => VariableName = variableName;


        /// <inheritdoc />
        public override Result<T> Run(ProcessState processState) => processState.GetVariable<T>(VariableName);

        /// <inheritdoc />
        public override RunnableProcessFactory RunnableProcessFactory => GetVariableProcessFactory.Instance;

        [VariableName]
        [Required]
        public VariableName VariableName { get; set; }
    }

    /// <summary>
    /// Gets the value of a named variable.
    /// </summary>
    public class GetVariableProcessFactory : GenericProcessFactory
    {
        private GetVariableProcessFactory() { }

        public static GenericProcessFactory Instance { get; } = new GetVariableProcessFactory();


        /// <inheritdoc />
        public override IProcessNameBuilder ProcessNameBuilder => new ProcessNameBuilderFromTemplate($"<[{nameof(GetVariable<object>.VariableName)}]>");

        /// <inheritdoc />
        public override Type ProcessType => typeof(GetVariable<>);

        /// <inheritdoc />
        protected override ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference) => memberTypeReference;

        /// <inheritdoc />
        protected override Result<ITypeReference> GetMemberType(FreezableProcessData freezableProcessData) =>
            freezableProcessData.GetVariableName(nameof(GetVariable<object>.VariableName))
                .Map(x => new VariableTypeReference(x) as ITypeReference);


        /// <inheritdoc />
        public override Maybe<ICustomSerializer> CustomSerializer { get; } = Maybe<ICustomSerializer>.From(new CustomSerializer(new VariableNameComponent(nameof(GetVariable<object>.VariableName))));

    }

}