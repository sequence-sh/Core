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
        public IReadOnlyDictionary<string, Type> VariableTypesDictionary;

        private ProcessContext(IReadOnlyDictionary<string, Type> variableTypesDictionary)
        {
            VariableTypesDictionary = variableTypesDictionary;
        }

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

            static Result<IReadOnlyDictionary<string, Type>> ResolveTypes(IEnumerable<(string variableName, ITypeReference typeReference)> references)
            {
                var groups = references
                    .Concat(references.Where(x=>x.typeReference is GenericTypeReference)
                        .SelectMany(x=>
                            ((GenericTypeReference) x.typeReference).ChildTypes.Select((t,i)=>
                                (variableName: x.variableName + "ARG" + i,typeReference: t) )))



                    .GroupBy(x => x.variableName).ToList();
                var groupingDictionary = new Dictionary<string, ImmutableHashSet<ActualTypeReference>>();


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

                var r = groupingDictionary.Select(TryExtractType).Combine().Map(x => new Dictionary<string, Type>(x) as IReadOnlyDictionary<string, Type>);

                return r;

                static Result<KeyValuePair<string, Type>> TryExtractType(KeyValuePair<string, ImmutableHashSet<ActualTypeReference>> kvp)
                {
                    var (key, actualTypes) = kvp;
                    return actualTypes.Count switch
                    {
                        0 => Result.Failure<KeyValuePair<string, Type>>($"The type '{key}' is never set."),
                        1 => new KeyValuePair<string, Type>(key, actualTypes.Single().Type),
                        _ => Result.Failure<KeyValuePair<string, Type>>(
                            $"The type '{key}' is set to more than one type - ({string.Join(", ", actualTypes.Select(x => x.Type.Name))})")
                    };
                }
            }

        }
    }
}