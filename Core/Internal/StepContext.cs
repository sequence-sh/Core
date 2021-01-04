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

            //var remainingFreezableSteps = new Stack<IFreezableStep>(freezableSteps);
            //var stepsForLater = new List<(IFreezableStep step, IError error)>();

            //var changed = false;

            //while (remainingFreezableSteps.Any())
            //{
            //    var step = remainingFreezableSteps.Pop();

            //    if (step is CompoundFreezableStep seq && seq.StepName == SequenceStepFactory.Instance.TypeName)
            //    {
            //        var stepsResult = seq.FreezableStepData.GetStepList(nameof(Sequence<object>.InitialSteps), nameof(Sequence<object>));

            //        if (stepsResult.IsFailure)
            //            return stepsResult.ConvertFailure<StepContext>();

            //        foreach (var freezableStep in stepsResult.Value)
            //            remainingFreezableSteps.Push(freezableStep);

            //        var finalStepResult = seq.FreezableStepData.GetStep(nameof(Sequence<object>.FinalStep), nameof(Sequence<object>));

            //        if (finalStepResult.IsFailure)
            //            return finalStepResult.ConvertFailure<StepContext>();

            //        remainingFreezableSteps.Push(finalStepResult.Value);

            //        continue;
            //    }

            //    var variablesSetResult = step.GetVariablesSet(typeResolver);
            //    if (variablesSetResult.IsFailure) return variablesSetResult.ConvertFailure<StepContext>();

            //    var resolveResult = variablesSetResult.Bind(Resolve);

            //    if (resolveResult.IsSuccess)
            //        changed = true;
            //    else
            //        stepsForLater.Add((step, resolveResult.Error));

            //    if (!remainingFreezableSteps.Any() && changed && stepsForLater.Any())
            //    {
            //        remainingFreezableSteps = new Stack<IFreezableStep>(stepsForLater.Select(x=>x.step));
            //        stepsForLater.Clear();
            //        changed = false;
            //    }
            //}

            //static Result<Unit, IError> Resolve(IEnumerable<(VariableName variableName, ITypeReference typeReference)> data, TypeResolver typeResolver)
            //{
            //    var references = data.SelectMany(GetAllSetReferences);

            //    var result = references
            //        .Select(x => (x.variableName, actualType: x.typeReference.TryGetActualTypeReference(typeResolver)))
            //        .Select(x => x.actualType.Bind(y => typeResolver.TryAddType(x.variableName, y)))
            //        .Combine(ErrorBuilderList.Combine)
            //        .Map(_=> Unit.Default)
            //        .MapError(x=>x.WithLocation(EntireSequenceLocation.Instance));

            //    return result;

            //    static IEnumerable<(VariableName variableName, ITypeReference typeReference)> GetAllSetReferences((VariableName variableName, ITypeReference typeReference) pair)
            //    {
            //        yield return pair;

            //        if (pair.typeReference is GenericTypeReference gtr)
            //        {
            //            foreach (var pair2 in gtr.ChildTypes.Select((t, i) => (variableName: pair.variableName.CreateChild(i), typeReference: t)))
            //                yield return pair2;
            //        }
            //    }
            //}

            //if (stepsForLater.Any())
            //{
            //    var error = ErrorList.Combine(stepsForLater.Select(x => x.error));
            //    return Result.Failure<StepContext, IError>(error);
            //}

            //return new StepContext(typeResolver);
        }
    }
}