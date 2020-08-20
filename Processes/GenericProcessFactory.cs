using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes
{
    /// <summary>
    /// Process factory for generic types.
    /// </summary>
    public abstract class GenericProcessFactory : RunnableProcessFactory
    {
        /// <inheritdoc />
        public override Result<ITypeReference> TryGetOutputTypeReference(FreezableProcessData freezableProcessData) => GetMemberType(freezableProcessData).Map(GetOutputTypeReference);

        /// <summary>
        /// Gets the output type from the member type.
        /// </summary>
        protected abstract ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference);

        /// <inheritdoc />
        public override IProcessNameBuilder ProcessNameBuilder => new DefaultProcessNameBuilder(TypeName);

        /// <inheritdoc />
        public override IEnumerable<Type> EnumTypes => ImmutableList<Type>.Empty;

        /// <inheritdoc />
        protected override Result<ICompoundRunnableProcess> TryCreateInstance(ProcessContext processContext, FreezableProcessData freezableProcessData) =>
            GetMemberType(freezableProcessData)
                .Bind(processContext.TryGetTypeFromReference)
                .Bind(x => TryCreateGeneric(ProcessType, x));

        /// <summary>
        /// Gets the type
        /// </summary>
        protected abstract Result<ITypeReference> GetMemberType(FreezableProcessData freezableProcessData);
    }
}