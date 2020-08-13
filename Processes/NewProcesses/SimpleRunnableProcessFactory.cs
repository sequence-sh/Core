using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.NewProcesses.General
{
    /// <summary>
    /// A process factory that uses default values for most properties.
    /// </summary>
    public abstract class SimpleRunnableProcessFactory<TProcess, TOutput> : RunnableProcessFactory where TProcess : IRunnableProcess, new ()
    {
        /// <inheritdoc />
        public override Result<ITypeReference> TryGetOutputTypeReference(FreezableProcessData freezableProcessData) => new ActualTypeReference(typeof(TOutput));

        /// <inheritdoc />
        public override IProcessNameBuilder ProcessNameBuilder => new DefaultProcessNameBuilder(TypeName);

        ///// <inheritdoc />
        //public override ProcessNameBuilder ProcessNameBuilder => new ProcessNameBuilder(ProcessNameTemplate);

        ///// <summary>
        ///// The template to use with the ProcessNameBuilder.
        ///// </summary>
        //protected abstract string ProcessNameTemplate { get; }

        /// <inheritdoc />
        public override Type ProcessType => typeof(TProcess);


        /// <inheritdoc />
        public override IEnumerable<Type> EnumTypes => ImmutableArray<Type>.Empty;

        /// <inheritdoc />
        protected override Result<IRunnableProcess> TryCreateInstance(ProcessContext processContext, FreezableProcessData freezableProcessData) => new TProcess();
    }
}