using System;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace Reductech.EDR.Processes.NewProcesses.General
{

    /// <summary>
    /// Prints a value to the log.
    /// </summary>
    public sealed class Print<T> : CompoundRunnableProcess<Unit>
    {
        /// <inheritdoc />
        public override Result<Unit> Run(ProcessState processState)
        {
            var r = Value.Run(processState);
            if (r.IsFailure) return r.ConvertFailure<Unit>();

            processState.Logger.LogInformation(r.Value.ToString());

            return Result.Success(Unit.Default);
        }

        /// <summary>
        /// The Value to Print.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<T> Value { get; set; } = null!;

        /// <inheritdoc />
        public override RunnableProcessFactory RunnableProcessFactory => PrintProcessFactory.Instance;
    }

    /// <summary>
    /// Prints a value to the log.
    /// </summary>
    public sealed class PrintProcessFactory : GenericProcessFactory
    {
        private PrintProcessFactory() { }

        public static GenericProcessFactory Instance { get; } = new PrintProcessFactory();

        /// <inheritdoc />
        public override Type ProcessType => typeof(Print<>);

        /// <inheritdoc />
        protected override ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference) => new ActualTypeReference(typeof(Unit));

        /// <inheritdoc />
        public override IProcessNameBuilder ProcessNameBuilder { get; } = new ProcessNameBuilderFromTemplate($"Print '[{nameof(Print<object>.Value)}]'");

        /// <inheritdoc />
        protected override Result<ITypeReference> GetMemberType(FreezableProcessData freezableProcessData) =>
            freezableProcessData.GetArgument(nameof(Print<object>.Value))
                .Bind(x => x.TryGetOutputTypeReference());
    }


    ///// <summary>
    ///// Prints a value to the log.
    ///// </summary>
    //public sealed class PrintProcessFactory : RunnableProcessFactory
    //{
    //    private PrintProcessFactory() { }

    //    public static RunnableProcessFactory Instance { get; } = new PrintProcessFactory();

    //    /// <inheritdoc />
    //    public override Result<ITypeReference> TryGetOutputTypeReference(FreezableProcessData freezableProcessData) => new ActualTypeReference(typeof(Unit));

    //    /// <inheritdoc />
    //    public override Type ProcessType  => typeof(Print<>);

    //    /// <inheritdoc />
    //    public override ProcessNameBuilder ProcessNameBuilder { get; } = new ProcessNameBuilder($"Print '[{nameof(Print<object>.Value)}]'");

    //    /// <inheritdoc />
    //    public override IEnumerable<Type> EnumTypes => ImmutableArray<Type>.Empty;


    //    /// <inheritdoc />
    //    protected override Result<IRunnableProcess> TryCreateInstance(ProcessContext processContext, FreezableProcessData freezableProcessData) =>
    //        freezableProcessData.GetArgument(nameof(Print<object>.Value))
    //            .Bind(x => x.TryGetOutputTypeReference())
    //            .Bind(processContext.TryGetTypeFromReference)
    //            .Bind(x => TryCreateGeneric(typeof(Print<>), x));
    //}
}
