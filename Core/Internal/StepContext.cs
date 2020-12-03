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
        public static Result<StepContext, IError> TryCreate(StepFactoryStore stepFactoryStore, params IFreezableStep[] freezableSteps)
        {
            var typeResolver = new TypeResolver(stepFactoryStore);

            var remainingFreezableSteps = new Stack<IFreezableStep>(freezableSteps);
            var stepsForLater = new List<(IFreezableStep step, IError error)>();

            var changed = false;

            while (remainingFreezableSteps.Any())
            {
                var step = remainingFreezableSteps.Pop();

                if (step is CompoundFreezableStep seq && seq.StepName == SequenceStepFactory.Instance.TypeName)
                {
                    var stepsResult = seq.FreezableStepData.GetStepList(nameof(Sequence<object>.Steps), nameof(Sequence<object>));

                    if (stepsResult.IsFailure)
                        return stepsResult.ConvertFailure<StepContext>();

                    foreach (var freezableStep in stepsResult.Value)
                        remainingFreezableSteps.Push(freezableStep);

                    var finalStepResult = seq.FreezableStepData.GetStep(nameof(Sequence<object>.FinalStep), nameof(Sequence<object>));

                    if (finalStepResult.IsFailure)
                        return finalStepResult.ConvertFailure<StepContext>();

                    remainingFreezableSteps.Push(finalStepResult.Value);

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
                var references = data.SelectMany(GetAllSetReferences);

                var result = references
                    .Select(x => (x.variableName, actualType: x.typeReference.TryGetActualTypeReference(typeResolver)))
                    .Select(x => x.actualType.Bind(y => typeResolver.TryAddType(x.variableName, y)))
                    .Combine(ErrorBuilderList.Combine)
                    .Map(_=> Unit.Default)
                    .MapError(x=>x.WithLocation(EntireSequenceLocation.Instance));

                return result;

                static IEnumerable<(VariableName variableName, ITypeReference typeReference)> GetAllSetReferences((VariableName variableName, ITypeReference typeReference) pair)
                {
                    yield return pair;

                    if (pair.typeReference is GenericTypeReference gtr)
                    {
                        foreach (var pair2 in gtr.ChildTypes.Select((t, i) => (variableName: pair.variableName.CreateChild(i), typeReference: t)))
                            yield return pair2;
                    }
                }


            }

            if (stepsForLater.Any())
            {
                var error = ErrorList.Combine(stepsForLater.Select(x => x.error));
                return Result.Failure<StepContext, IError>(error);
            }

            return new StepContext(typeResolver);
        }
    }
}