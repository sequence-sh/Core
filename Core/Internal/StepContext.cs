using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.Util;

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
        /// Gets the type referred to by a reference.
        /// </summary>
        public Result<Type, IErrorBuilder> TryGetTypeFromReference(ITypeReference typeReference) => typeReference.TryGetActualTypeReference(TypeResolver).Map(x => x.Type);

        /// <summary>
        /// Tries to create a new StepContext.
        /// </summary>
        public static Result<StepContext, IError> TryCreate(params IFreezableStep[] freezableSteps)
        {
            var typeResolver = new TypeResolver();

            var remainingFreezableSteps = new Stack<IFreezableStep>(freezableSteps);
            var stepsForLater = new List<(IFreezableStep step, IError error)>();

            var changed = false;

            while (remainingFreezableSteps.Any())
            {
                var step = remainingFreezableSteps.Pop();

                if (step is CompoundFreezableStep seq && seq.StepFactory == SequenceStepFactory.Instance)
                {
                    var stepsResult = seq.FreezableStepData.GetListArgument(nameof(Sequence.Steps));

                    if (stepsResult.IsFailure)
                        return stepsResult.MapError(x => x.WithLocation(seq)).ConvertFailure<StepContext>();

                    foreach (var freezableStep in stepsResult.Value)
                        remainingFreezableSteps.Push(freezableStep);

                    continue;
                }

                var variablesSetResult = step.TryGetVariablesSet(typeResolver);

                var resolveResult = variablesSetResult.Bind(Resolve);

                if (resolveResult.IsSuccess)
                    changed = true;
                else
                    stepsForLater.Add((step, resolveResult.Error));

                if (!remainingFreezableSteps.Any() && changed && stepsForLater.Any())
                {
                    remainingFreezableSteps = new Stack<IFreezableStep>(stepsForLater.Select(x=>x.step));
                    stepsForLater.Clear();
                    changed = false;
                }
            }

            Result<Unit, IError> Resolve(IEnumerable<(VariableName variableName, ITypeReference typeReference)> data)
            {
                var genericReferences = data.Where(x => x.typeReference is GenericTypeReference)
                    .SelectMany(x =>
                        ((GenericTypeReference)x.typeReference).ChildTypes.Select((t, i) =>
                           (variableName: x.variableName.CreateChild(i), typeReference: t)));

                var references = data.Concat(genericReferences);

                var result = references
                    .Select(x => (x.variableName, actualType: x.typeReference.TryGetActualTypeReference(typeResolver)))
                    .Select(x => x.actualType.Bind(y => typeResolver.TryAddType(x.variableName, y)))
                    .Combine(ErrorBuilderList.Combine)
                    .Map(_=> Unit.Default)
                    .MapError(x=>x.WithLocation(EntireSequenceLocation.Instance));

                return result;
            }

            if (stepsForLater.Any())
            {
                var error = ErrorList.Combine(stepsForLater.Select(x => x.error));
                return error;
            }

            return new StepContext(typeResolver);
        }
    }
}