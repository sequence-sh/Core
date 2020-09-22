using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Serialization;

namespace Reductech.EDR.Processes.General
{

    /// <summary>
    /// Gets the value of a named variable.
    /// </summary>
    public sealed class GetVariable<T> : CompoundRunnableProcess<T>
    {
        /// <inheritdoc />
        public override Result<T, IRunErrors> Run(ProcessState processState) => processState.GetVariable<T>(VariableName, Name);

        /// <inheritdoc />
        public override IRunnableProcessFactory RunnableProcessFactory => GetVariableProcessFactory.Instance;

        /// <summary>
        /// The name of the variable to get.
        /// </summary>
        [VariableName] [Required]
        public VariableName VariableName { get; set; }
    }

    /// <summary>
    /// Gets the value of a named variable.
    /// </summary>
    public class GetVariableProcessFactory : GenericProcessFactory
    {
        private GetVariableProcessFactory() { }

        /// <summary>
        /// The Instance.
        /// </summary>
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
        public override IProcessSerializer Serializer { get; } = new ProcessSerializer(
            new VariableNameComponent(nameof(GetVariable<object>.VariableName)));



        /// <summary>
        /// Create a freezable GetVariable process.
        /// </summary>
        public static IFreezableProcess CreateFreezable(VariableName variableName)
        {
            var dict = new Dictionary<string, ProcessMember>
            {
                {nameof(GetVariable<object>.VariableName), new ProcessMember(variableName)}
            };

            var fpd = new FreezableProcessData(dict);

            return new CompoundFreezableProcess(Instance, fpd, null);
        }

    }

}