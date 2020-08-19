using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Serialization;

namespace Reductech.EDR.Processes.General
{

    /// <summary>
    /// Gets the value of a named variable.
    /// </summary>
    public sealed class SetVariable<T> : CompoundRunnableProcess<Unit>
    {
        /// <summary>
        /// Necessary Parameterless constructor
        /// </summary>
        public SetVariable() { }

        /// <inheritdoc />
        public SetVariable(VariableName variableName, IRunnableProcess<T> value)
        {
            VariableName = variableName;
            Value = value;
        }

        /// <inheritdoc />
        public override Result<Unit> Run(ProcessState processState) =>
            Value.Run(processState)
                .Bind(x => processState.SetVariable(VariableName, x))
                .Map(() => Unit.Default);

        /// <inheritdoc />
        public override RunnableProcessFactory RunnableProcessFactory => SetVariableProcessFactory.Instance;

        /// <summary>
        /// The name of the variable to set.
        /// </summary>
        [VariableName]
        [Required]
        public VariableName VariableName { get; set; }

        /// <summary>
        /// The value to set the variable to.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<T> Value { get; set; } = null!;
    }

    /// <summary>
    /// Sets the value of a named variable.
    /// </summary>
    public class SetVariableProcessFactory : RunnableProcessFactory
    {
        private SetVariableProcessFactory() { }

        public static RunnableProcessFactory Instance { get; } = new SetVariableProcessFactory();

        /// <inheritdoc />
        public override Result<ITypeReference> TryGetOutputTypeReference(FreezableProcessData freezableProcessData) => new ActualTypeReference(typeof(Unit));

        /// <inheritdoc />
        public override Type ProcessType => typeof(SetVariable<>);


        /// <inheritdoc />
        public override IProcessNameBuilder ProcessNameBuilder => new ProcessNameBuilderFromTemplate($"[{nameof(SetVariable<object>.VariableName)}] = [{nameof(SetVariable<object>.Value)}]");

        /// <inheritdoc />
        public override IEnumerable<Type> EnumTypes => ImmutableArray<Type>.Empty;


        /// <inheritdoc />
        public override Result<Maybe<ITypeReference>> GetTypeReferencesSet(VariableName variableName, FreezableProcessData freezableProcessData)
        {
            var result = freezableProcessData.GetArgument(nameof(SetVariable<object>.Value))
                .Bind(x => x.TryGetOutputTypeReference())
                .Map(Maybe<ITypeReference>.From);

            return result;
        }

        /// <inheritdoc />
        protected override Result<IRunnableProcess> TryCreateInstance(ProcessContext processContext, FreezableProcessData freezableProcessData) =>
            freezableProcessData.GetVariableName(nameof(SetVariable<object>.VariableName))
                .Bind(x => processContext.TryGetTypeFromReference(new VariableTypeReference(x)))
                .Bind(x => TryCreateGeneric(typeof(SetVariable<>), x));


        /// <inheritdoc />
        public override Maybe<ICustomSerializer> CustomSerializer { get; } = Maybe<ICustomSerializer>.From(
            new CustomSerializer(
                new VariableNameComponent(nameof(SetVariable<object>.VariableName)),
                new FixedStringComponent("=", FixedStringComponent.SpaceType.Optional),
                new AnyPrimitiveComponent(nameof(SetVariable<object>.Value))
                ));
    }
}