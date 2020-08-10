using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.NewProcesses
{
    /// <summary>
    /// A process that is not a constant or a variable reference.
    /// </summary>
    public sealed class CompoundFreezableProcess : IFreezableProcess
    {
        /// <summary>
        /// Creates a new CompoundFreezableProcess.
        /// </summary>
        public CompoundFreezableProcess(RunnableProcessFactory processFactory,
            IReadOnlyDictionary<string, IFreezableProcess> processArguments,
            IReadOnlyDictionary<string, IReadOnlyList<IFreezableProcess>> processListArguments)
        {
            ProcessFactory = processFactory;
            ProcessArguments = processArguments;
            ProcessListArguments = processListArguments;
        }


        /// <summary>
        /// The factory for this process.
        /// </summary>
        public RunnableProcessFactory ProcessFactory { get; }

        ///// <summary>
        ///// The type of Runnable process this will be mapped to.
        ///// </summary>
        //public Type ProcessType { get; }

        /// <summary>
        /// A dictionary mapping property names to property values.
        /// The values may be constants, references to variables, or processes.
        /// </summary>
        public IReadOnlyDictionary<string, IFreezableProcess> ProcessArguments { get; }


        /// <summary>
        /// A dictionary mapping property names to property list values.
        /// The values will be lists of be constants, references to variables, and processes.
        /// </summary>
        public IReadOnlyDictionary<string, IReadOnlyList<IFreezableProcess>> ProcessListArguments { get; }

        /// <inheritdoc />
        public Result<IRunnableProcess> TryFreeze(ProcessContext processContext) => ProcessFactory.TryFreeze(processContext, ProcessArguments, ProcessListArguments);

        /// <inheritdoc />
        public Result<IReadOnlyCollection<(string name, ITypeReference type)>> TryGetVariablesSet
        {
            get
            {
                var result = ProcessArguments.Values.Select(pa => pa.TryGetVariablesSet)
                    .Concat(ProcessListArguments.SelectMany(x => x.Value.Select(pa => pa.TryGetVariablesSet)))
                    .Combine().Map(x=>x.SelectMany(y=>y).ToList() as IReadOnlyCollection<(string name, ITypeReference type)>);

                return result;
            }
        }

        /// <inheritdoc />
        public string ProcessName => ProcessFactory.GetProcessName(ProcessArguments, ProcessListArguments);

        /// <inheritdoc />
        public Result<ITypeReference> TryGetOutputTypeReference()
        {
            return ProcessFactory.TryGetOutputTypeReference(ProcessArguments, ProcessListArguments);
        }

    }


    /// <summary>
    /// A factory for creating runnable processes.
    /// </summary>
    public abstract class RunnableProcessFactory
    {
        /// <summary>
        /// Tries to get a reference to the output type of this process.
        /// </summary>
        public abstract Result<ITypeReference> TryGetOutputTypeReference(
            IReadOnlyDictionary<string, IFreezableProcess> processArguments,
            IReadOnlyDictionary<string, IReadOnlyList<IFreezableProcess>> processListArguments);

        /// <summary>
        /// Unique name for this type of process.
        /// </summary>
        public abstract string TypeName { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return TypeName;
        }

        /// <summary>
        /// Gets the name of this particular instance of the process.
        /// </summary>
        public abstract string GetProcessName(IReadOnlyDictionary<string, IFreezableProcess> processArguments,
            IReadOnlyDictionary<string, IReadOnlyList<IFreezableProcess>> processListArguments);


        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        protected abstract Result<IRunnableProcess> TryCreateInstance(ProcessContext processContext,
            IReadOnlyDictionary<string, IFreezableProcess> processArguments,
            IReadOnlyDictionary<string, IReadOnlyList<IFreezableProcess>> processListArguments);

        /// <summary>
        /// Try to create the instance of this type and set all arguments.
        /// </summary>
        public Result<IRunnableProcess> TryFreeze(ProcessContext processContext,
            IReadOnlyDictionary<string, IFreezableProcess> processArguments,
            IReadOnlyDictionary<string, IReadOnlyList<IFreezableProcess>> processListArguments)
        {
            var instanceResult = TryCreateInstance(processContext, processArguments, processListArguments);

            if (instanceResult.IsFailure) return instanceResult;

            var runnableProcess = instanceResult.Value;

            var errors = new List<string>();

            var remainingProperties = instanceResult.Value.GetType().GetProperties()
                .Where(x => x.GetCustomAttribute<RunnableProcessPropertyAttribute>() != null)
                .ToDictionary(x => x.Name);

            var remainingListProperties = instanceResult.Value.GetType().GetProperties()
                .Where(x => x.GetCustomAttribute<RunnableProcessListPropertyAttribute>() != null)
                .ToDictionary(x => x.Name);


            foreach (var (argumentName, nestedProcess) in processArguments)
            {
                if (remainingProperties.Remove(argumentName, out var pi))
                {
                    var argumentFreezeResult = nestedProcess.TryFreeze(processContext);
                    if (argumentFreezeResult.IsFailure)
                        errors.Add(argumentFreezeResult.Error);
                    else
                    {
                        if (pi.PropertyType.IsInstanceOfType(argumentFreezeResult.Value))
                            pi.SetValue(runnableProcess, argumentFreezeResult.Value); //This could throw an exception but we don't expect it.
                        else
                            errors.Add($"'{pi.Name}' cannot take the value '{argumentFreezeResult.Value}'");
                    }
                }
                else
                    errors.Add($"The property '{argumentName}' does not exist on type '{GetType().Name}'.");
            }

            foreach (var (argumentName, nestedProcessList) in processListArguments)
            {
                if (remainingListProperties.Remove(argumentName, out var listInfo))
                {
                    var freezeResult = nestedProcessList.Select(x => x.TryFreeze(processContext)).Combine()
                        .Map(x => x.ToImmutableArray());
                    if (freezeResult.IsFailure)
                        errors.Add(freezeResult.Error);
                    else
                    {
                        var genericType = listInfo.PropertyType.GenericTypeArguments.Single();
                        var listType = typeof(List<>).MakeGenericType(genericType);

                        var list = Activator.CreateInstance(listType);

                        foreach (var process in freezeResult.Value)
                            if (genericType.IsInstanceOfType(process))
                            {
                                var addMethod = listType.GetMethod(nameof(List<object>.Add))!;
                                addMethod.Invoke(list, new object?[]{process});
                            }
                            else
                                errors.Add($"'{process.Name}' does not have the type '{genericType.Name}'");


                        listInfo.SetValue(runnableProcess, list);

                    }

                }
            }

            errors.AddRange(remainingProperties.Values
                .Where(property => property.GetCustomAttribute<RequiredAttribute>() != null)
                .Select(property => $"The property '{property.Name}' was not set on type '{GetType().Name}'."));

            errors.AddRange(remainingListProperties.Values
                .Where(property => property.GetCustomAttribute<RequiredAttribute>() != null)
                .Select(property => $"The property '{property.Name}' was not set on type '{GetType().Name}'."));


            if (errors.Any())
                return Result.Failure<IRunnableProcess>(string.Join("\r\n", errors));

            return Result.Success(runnableProcess);

        }


        /// <summary>
        /// Creates a typed generic IRunnableProcess with one type argument.
        /// </summary>
        protected static Result<IRunnableProcess> TryCreateGeneric(Type openGenericType, Type outputType)
        {
            var genericType = openGenericType.MakeGenericType(outputType);


            var r = Activator.CreateInstance(genericType);

            if (r is IRunnableProcess rp)
                return Result.Success(rp);

            return Result.Failure<IRunnableProcess>($"Could not create an instance of {outputType.Name}.");
        }

        /// <summary>
        /// Gets the name of the type, removing the backtick if it is a generic type.
        /// </summary>
        protected string FormatTypeName(Type type)
        {
            string friendlyName = type.Name;
            if (type.IsGenericType)
            {
                var iBacktick = friendlyName.IndexOf('`');
                if (iBacktick > 0) friendlyName = friendlyName.Remove(iBacktick);
            }

            return friendlyName;
        }
    }
}