using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.NewProcesses
{
    /// <summary>
    /// Keeps track of all variables in a Freezable context.
    /// </summary>
    public sealed class ProcessContext
    {
        /// <summary>
        /// Dictionary mapping variable names to types.
        /// </summary>
        public IReadOnlyDictionary<VariableName, Type> VariableTypesDictionary;

        private ProcessContext(IReadOnlyDictionary<VariableName, Type> variableTypesDictionary) => VariableTypesDictionary = variableTypesDictionary;

        /// <summary>
        /// Gets the type referred to by a reference.
        /// </summary>
        public Result<Type> TryGetTypeFromReference(ITypeReference typeReference)
        {
            switch (typeReference)
            {
                case ActualTypeReference actualType: return actualType.Type;
                case GenericTypeReference genericTypeReference:
                {
                    var result =
                        genericTypeReference.TypeArgumentReferences.Select(TryGetTypeFromReference).Combine()
                            .Map(x=>x.ToArray())
                            .OnSuccessTry(x=>genericTypeReference.GenericType.MakeGenericType(x));

                    return result;
                }
                case MultipleTypeReference multipleTypeReference:
                {
                    var result = multipleTypeReference.AllReferences
                        .Select(TryGetTypeFromReference)
                        .Combine()
                        .Map(x => x.Distinct())
                        .Ensure(x => x.Count() == 1, $"Type has multiple actual types.")
                        .Map(x => x.Single());

                    return result;
                }
                case VariableTypeReference typeReference1:
                {
                    return VariableTypesDictionary.TryFindOrFail(typeReference1.VariableName,
                        $"Variable '{typeReference1.VariableName}' is never set.");
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(typeReference));
            }
        }

        /// <summary>
        /// Tries to create a new ProcessContext.
        /// </summary>
        /// <param name="freezableProcesses"></param>
        /// <returns></returns>
        public static Result<ProcessContext> TryCreate(params IFreezableProcess[] freezableProcesses)
        {
            var result = freezableProcesses
                .Select(x => x.TryGetVariablesSet)
                .Combine()
                .Map(l=>l.SelectMany(y=>y))
                .Bind(ResolveTypes)
                .Map(dictionary=> new ProcessContext(dictionary));

            return result;

            static Result<IReadOnlyDictionary<VariableName, Type>> ResolveTypes(IEnumerable<(VariableName variableName, ITypeReference typeReference)> references)
            {
                var genericReferences = references.Where(x => x.typeReference is GenericTypeReference)
                    .SelectMany(x =>
                        ((GenericTypeReference) x.typeReference).ChildTypes.Select((t, i) =>
                            (variableName: x.variableName.CreateChild(i), typeReference: t)));


                var groups = references
                    .Concat(genericReferences)
                    .GroupBy(x => x.variableName).ToList();
                var groupingDictionary = new Dictionary<VariableName, ImmutableHashSet<ActualTypeReference>>();


                foreach (var remainingGroup in groups)
                {
                    var keys = remainingGroup.SelectMany(x=>x.typeReference.VariableTypeReferences).Select(x=>x.VariableName).Prepend(remainingGroup.Key).ToList();

                    var newSet = keys.Select(x =>
                            groupingDictionary.TryGetValue(x, out var hs) ? hs : Enumerable.Empty<ActualTypeReference>())
                        .Distinct()
                        .SelectMany(x => x)
                        .Concat(remainingGroup.SelectMany(x=>x.typeReference.ActualTypeReferences))
                        .ToImmutableHashSet(); //This is not super efficient, but who cares

                    //This set may have no elements - this is not a problem

                    foreach (var key in keys)
                        groupingDictionary[key] = newSet;
                }

                var r = groupingDictionary.Select(TryExtractType).Combine().Map(x => new Dictionary<VariableName, Type>(x) as IReadOnlyDictionary<VariableName, Type>);

                return r;

                static Result<KeyValuePair<VariableName, Type>> TryExtractType(KeyValuePair<VariableName, ImmutableHashSet<ActualTypeReference>> kvp)
                {
                    var (key, actualTypes) = kvp;
                    return actualTypes.Count switch
                    {
                        0 => Result.Failure<KeyValuePair<VariableName, Type>>($"The type '{key}' is never set."),
                        1 => new KeyValuePair<VariableName, Type>(key, actualTypes.Single().Type),
                        _ => Result.Failure<KeyValuePair<VariableName, Type>>(
                            $"The type '{key}' is set to more than one type - ({string.Join(", ", actualTypes.Select(x => x.Type.Name))})")
                    };
                }
            }

        }
    }
}