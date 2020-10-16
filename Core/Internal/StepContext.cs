using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// Gets the actual type from a type reference.
    /// </summary>
    public sealed class TypeResolver
    {

        private Dictionary<VariableName, ActualTypeReference> MyDictionary { get; } = new Dictionary<VariableName, ActualTypeReference>();

        /// <summary>
        /// The dictionary mapping VariableNames to ActualTypeReferences
        /// </summary>
        public IReadOnlyDictionary<VariableName, ActualTypeReference> Dictionary => MyDictionary;


        /// <summary>
        /// Tries to add another actual type.
        /// </summary>
        public Result TryAddType(VariableName variable, ActualTypeReference actualTypeReference)
        {
            if (MyDictionary.TryGetValue(variable, out var previous))
            {
                if(previous.Equals(actualTypeReference))
                    return Result.Success();

                return Result.Failure($"The type of {variable} is ambiguous.");
            }

            MyDictionary.Add(variable, actualTypeReference);
            return Result.Success();
        }
    }

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
        /// Gets the type referred to by a reference.
        /// </summary>
        public Result<Type> TryGetTypeFromReference(ITypeReference typeReference) => typeReference.TryGetActualTypeReference(TypeResolver).Map(x => x.Type);

        /// <summary>
        /// Tries to create a new StepContext.
        /// </summary>
        public static Result<StepContext> TryCreate1(params IFreezableStep[] freezableSteps)
        {
            var typeResolver = new TypeResolver();

            var remainingFreezableSteps = new Stack<IFreezableStep>(freezableSteps);
            var stepsForLater = new List<(IFreezableStep step, Result error)>();

            var changed = false;

            while (remainingFreezableSteps.Any())
            {
                var step = remainingFreezableSteps.Pop();

                var variablesSetResult = step.TryGetVariablesSet(typeResolver);

                var resolveResult = variablesSetResult.Bind(Resolve);

                if (resolveResult.IsSuccess)
                    changed = true;
                else
                    stepsForLater.Add((step, resolveResult));

                if (!remainingFreezableSteps.Any() && changed && stepsForLater.Any())
                {
                    remainingFreezableSteps = new Stack<IFreezableStep>(stepsForLater.Select(x=>x.step));
                    stepsForLater.Clear();
                    changed = false;
                }
            }

            Result Resolve(IEnumerable<(VariableName variableName, ITypeReference typeReference)> data)
            {
                var genericReferences = data.Where(x => x.typeReference is GenericTypeReference)
                    .SelectMany(x =>
                        ((GenericTypeReference)x.typeReference).ChildTypes.Select((t, i) =>
                           (variableName: x.variableName.CreateChild(i), typeReference: t)));

                var references = data.Concat(genericReferences);

                var result = references
                    .Select(x => (x.variableName, actualType: x.typeReference.TryGetActualTypeReference(typeResolver)))
                    .Select(x => x.actualType.Bind(y => typeResolver.TryAddType(x.variableName, y)))
                    .Combine();

                return result;
            }

            if (stepsForLater.Any())
                return stepsForLater.Select(x => x.error).Combine().ConvertFailure<StepContext>();

            return new StepContext(typeResolver);
        }
    }
}