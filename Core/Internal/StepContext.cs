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
        public Result<Type> TryGetTypeFromReference(ITypeReference typeReference)
        {
            return typeReference.TryGetActualTypeReference(TypeResolver).Map(x => x.Type);

            //switch (typeReference)
            //{
            //    case ActualTypeReference actualType: return actualType.Type;
            //    case GenericTypeReference genericTypeReference:
            //    {
            //        var result =
            //            genericTypeReference.TypeArgumentReferences.Select(TryGetTypeFromReference).Combine()
            //                .Map(x=>x.ToArray())
            //                .OnSuccessTry(x=>genericTypeReference.GenericType.MakeGenericType(x));

            //        return result;
            //    }
            //    case MultipleTypeReference multipleTypeReference:
            //    {
            //        var result = multipleTypeReference.AllReferences
            //            .Select(TryGetTypeFromReference)
            //            .Combine()
            //            .Map(x => x.Distinct())
            //            .Ensure(x => x.Count() == 1, "Type has multiple actual types.")
            //            .Map(x => x.Single());

            //        return result;
            //    }
            //    case VariableTypeReference typeReference1:
            //    {
            //        return TypeResolver.TryFindOrFail(typeReference1.VariableName,
            //            $"Variable '{typeReference1.VariableName}' is never set.");
            //    }
            //    default:
            //        throw new ArgumentOutOfRangeException(nameof(typeReference));
            //}
        }

        /// <summary>
        /// Tries to create a new StepContext.
        /// </summary>
        public static Result<StepContext> TryCreate1(params IFreezableStep[] freezableSteps)
        {
            var typeResolver = new TypeResolver();

            var remainingFreezableSteps = new Stack<IFreezableStep>(freezableSteps);
            var stepsForLater = new List<IFreezableStep>();

            while (remainingFreezableSteps.Any())
            {
                var step = remainingFreezableSteps.Pop();
                var stuff = step.TryGetVariablesSet();
            }


        }


        /// <summary>
        /// Tries to create a new StepContext.
        /// </summary>
        public static Result<StepContext> TryCreate(params IFreezableStep[] freezableSteps)
        {
            var result = freezableSteps
                .Select(x => x.TryGetVariablesSet())
                .Combine()
                .Map(l=>l.SelectMany(y=>y))
                .Bind(ResolveTypes)
                .Map(dictionary=> new StepContext(dictionary));

            return result;

            static Result<TypeResolver> ResolveTypes(IEnumerable<(VariableName variableName, ITypeReference typeReference)> references)
            {
                var genericReferences = references.Where(x => x.typeReference is GenericTypeReference)
                    .SelectMany(x =>
                        ((GenericTypeReference) x.typeReference).ChildTypes.Select((t, i) =>
                            (variableName: x.variableName.CreateChild(i), typeReference: t)));

                var remainingReferences = new Stack<(VariableName variableName, ITypeReference typeReference)>(genericReferences.Concat(references).Reverse());
                var referencesForLater = new List<(VariableName variableName, ITypeReference typeReference)>();
                var anyChanged = false;
                var typeResolver = new TypeResolver();


                while (remainingReferences.Any())
                {
                    var (variableName, typeReference) = remainingReferences.Pop();

                    var r =
                        typeReference.TryGetActualTypeReference(typeResolver)
                            .Bind(tr=> typeResolver.TryAddType(variableName, tr));

                    if (r.IsSuccess) anyChanged = true;
                    else
                        referencesForLater.Add((variableName, typeReference));


                    if (!remainingReferences.Any() && anyChanged && referencesForLater.Any())
                    {
                        remainingReferences = new Stack<(VariableName, ITypeReference)>(referencesForLater);
                        referencesForLater.Clear();
                    }
                }

                if (referencesForLater.Any())
                {
                    var error = referencesForLater
                        .Select(x => x.typeReference.TryGetActualTypeReference(typeResolver))
                        .Select(x => x.ConvertFailure<TypeResolver>())
                        .First();

                    return error;
                }

                return typeResolver;


                //var groups = references
                //    .Concat(genericReferences)
                //    .GroupBy(x => x.variableName).ToList();


                //var groupingDictionary = new Dictionary<VariableName, ImmutableHashSet<ActualTypeReference>>();


                //foreach (var remainingGroup in groups)
                //{
                //    var keys = remainingGroup
                //        .SelectMany(x=>x.typeReference.VariableTypeReferences)
                //        .Select(x=>x.VariableName)
                //        .Prepend(remainingGroup.Key).ToList();


                //    foreach (var (variableName, typeReference) in remainingGroup)
                //    {
                //        var referenceResult = typeReference.TryGetActualTypeReference(typeResolver);
                //        if (referenceResult.IsSuccess)
                //        {

                //        }

                //    }



                //    var newSet = keys.Select(x =>
                //            groupingDictionary.TryGetValue(x, out var hs) ? hs : Enumerable.Empty<ActualTypeReference>())
                //        .Distinct()
                //        .SelectMany(x => x)
                //        .Concat(remainingGroup.SelectMany(x=>x.typeReference.ActualTypeReferences))
                //        .ToImmutableHashSet(); //This is not super efficient, but who cares

                //    //This set may have no elements - this is not a problem

                //    foreach (var key in keys)
                //        groupingDictionary[key] = newSet;
                //}

                //var r = groupingDictionary.Select(TryExtractType)
                //    .Combine()
                //    .Map(x => new Dictionary<VariableName, Type>(x) as IReadOnlyDictionary<VariableName, Type>);

                //return r;

                //static Result<KeyValuePair<VariableName, Type>> TryExtractType(KeyValuePair<VariableName, ImmutableHashSet<ActualTypeReference>> kvp)
                //{
                //    var (key, actualTypes) = kvp;
                //    return actualTypes.Count switch
                //    {
                //        0 => Result.Failure<KeyValuePair<VariableName, Type>>($"The type '{key}' is never set."),
                //        1 => new KeyValuePair<VariableName, Type>(key, actualTypes.Single().Type),
                //        _ => Result.Failure<KeyValuePair<VariableName, Type>>(
                //            $"The type '{key}' is set to more than one type - ({string.Join(", ", actualTypes.Select(x => x.Type.Name))})")
                //    };
                //}
            }

        }
    }
}