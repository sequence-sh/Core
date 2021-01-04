using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// Keeps track of all variables in a Freezable context.
    /// </summary>
    public sealed class StepContext
    {
        /// <summary>
        /// Dictionary mapping variable names to types.
        /// </summary>
        public  TypeResolver TypeResolver { get; }


        private StepContext(TypeResolver typeResolver) => TypeResolver = typeResolver;

        /// <summary>
        /// Try to Clone this context with additional set variables.
        /// </summary>
        public  Result<StepContext, IErrorBuilder> TryClone(params (VariableName vn, ITypeReference typeReference)[] extraVariables)
        {
            var newTypeResolver = TypeResolver.Copy();

            foreach (var (vn, typeReference) in extraVariables)
            {
                var r = newTypeResolver.TryAddType(vn, typeReference);
                if (r.IsFailure) return r.ConvertFailure<StepContext>();
            }

            return new StepContext(newTypeResolver);
        }

        /// <summary>
        /// Gets the type referred to by a reference.
        /// </summary>
        public Result<Type, IErrorBuilder> TryGetTypeFromReference(ITypeReference typeReference) => typeReference.TryGetActualTypeReference(TypeResolver).Map(x => x.Type);

        /// <summary>
        /// Tries to create a new StepContext.
        /// </summary>
        public static Result<StepContext, IError> TryCreate(StepFactoryStore stepFactoryStore, IFreezableStep topLevelStep)
        {
            var typeResolver = new TypeResolver(stepFactoryStore);

            int? numberUnresolved = null;

            while (true)
            {
                var unresolvableVariableNames = new List<VariableName>();
                var errors = new List<IError>();

                var result = topLevelStep.GetVariablesSet(typeResolver);
                if (result.IsFailure) return result.ConvertFailure<StepContext>();

                foreach (var (variableName, maybe) in result.Value)
                {
                    if(maybe.HasNoValue)
                        unresolvableVariableNames.Add(variableName);
                    else
                    {
                        var addResult = typeResolver.TryAddType(variableName, maybe.Value);
                        if (addResult.IsFailure) errors.Add(addResult.Error.WithLocation(EntireSequenceLocation.Instance));
                        else if(!addResult.Value) unresolvableVariableNames.Add(variableName);
                    }
                }

                if (errors.Any())
                    return Result.Failure<StepContext, IError>(ErrorList.Combine(errors));

                if (!unresolvableVariableNames.Any())
                    break; //We've resolved everything. Yey!

                if (numberUnresolved == null || numberUnresolved > unresolvableVariableNames.Count)
                    numberUnresolved = unresolvableVariableNames.Count; //We have improved this number and can try again
                else
                {
                    var error =
                        ErrorList.Combine(
                            unresolvableVariableNames.Distinct().Select(x =>
                                new SingleError($"Could not resolve variable {x}",
                                    ErrorCode.CouldNotResolveVariable, EntireSequenceLocation.Instance))
                        );

                    return Result.Failure<StepContext, IError>(error);
                }
            }

            return new StepContext(typeResolver);
        }
    }
}