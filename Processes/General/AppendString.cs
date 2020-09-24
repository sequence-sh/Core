using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Appends a string to an existing string variable.
    /// </summary>
    public sealed class AppendString : CompoundRunnableProcess<Unit>
    {
        /// <summary>
        /// The variable to append to.
        /// </summary>
        [VariableName]
        [Required]
        public VariableName Variable { get; set; }


        /// <summary>
        /// The string to append.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<string> String { get; set; } = null!;

        /// <inheritdoc />
        public override Result<Unit, IRunErrors> Run(ProcessState processState)
        {
            var currentValue = processState.GetVariable<string>(Variable, Name);
            if (currentValue.IsFailure)
                return currentValue.ConvertFailure<Unit>();


            var str = String.Run(processState);
            if (str.IsFailure)
                return str.ConvertFailure<Unit>();

            var value = currentValue.Value + str.Value;

            var r = processState.SetVariable(Variable, value);
            if (r.IsFailure)
                return r.ConvertFailure<Unit>();

            return Unit.Default;
        }

        /// <inheritdoc />
        public override IRunnableProcessFactory RunnableProcessFactory => AppendStringProcessFactory.Instance;
    }

    /// <summary>
    /// Appends a string to an existing string variable.
    /// </summary>
    public sealed class AppendStringProcessFactory : SimpleRunnableProcessFactory<AppendString, Unit>
    {
        private AppendStringProcessFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleRunnableProcessFactory<AppendString, Unit> Instance { get; } = new AppendStringProcessFactory();

        /// <inheritdoc />
        public override Result<Maybe<ITypeReference>> GetTypeReferencesSet(VariableName variableName, FreezableProcessData freezableProcessData) => Maybe<ITypeReference>.From(new ActualTypeReference(typeof(string)));


        /// <inheritdoc />
        public override IProcessNameBuilder ProcessNameBuilder => new ProcessNameBuilderFromTemplate($"Append [{nameof(AppendString.String)}] to [{nameof(AppendString.Variable)}]");
    }
}