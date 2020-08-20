﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;

namespace Reductech.EDR.Processes
{
    /// <summary>
    /// A runnable process that is not a constant.
    /// </summary>
    public interface ICompoundRunnableProcess : IRunnableProcess
    {
        /// <summary>
        /// The factory used to create processes of this type.
        /// </summary>
        RunnableProcessFactory RunnableProcessFactory { get; }

        /// <summary>
        /// Configuration for this process.
        /// </summary>
        ProcessConfiguration? ProcessConfiguration { get; set; }
    }

    /// <summary>
    /// A runnable process that is not a constant.
    /// </summary>
    public abstract class CompoundRunnableProcess<T> : IRunnableProcess<T>, ICompoundRunnableProcess
    {
        /// <inheritdoc />
        public abstract Result<T> Run(ProcessState processState);

        /// <summary>
        /// The factory used to create processes of this type.
        /// </summary>
        public abstract RunnableProcessFactory RunnableProcessFactory { get; }

        /// <inheritdoc />
        public string Name => RunnableProcessFactory.ProcessNameBuilder.GetFromArguments(FreezableProcessData);

        /// <inheritdoc />
        public override string ToString() => RunnableProcessFactory.TypeName;

        /// <summary>
        /// Configuration for this process.
        /// </summary>
        public ProcessConfiguration? ProcessConfiguration { get; set; }


        private FreezableProcessData FreezableProcessData
        {
            get
            {
                var variableNames = GetType().GetProperties()
                .Where(x => x.GetCustomAttribute<VariableNameAttribute>() != null)
                .Select(x => (x.Name, variableName: (VariableName)x.GetValue(this)!))
                .Where(x => x.variableName != null)

                .ToDictionary(x => x.Name, x => new ProcessMember(x.variableName)  );


                var arguments  = GetType().GetProperties()
                 .Where(x => x.GetCustomAttribute<RunnableProcessPropertyAttribute>() != null)
                 .Select(x => (x.Name, process: x.GetValue(this) as IRunnableProcess))
                 .Where(x => x.process != null)
                 .ToDictionary(x => x.Name, x => new ProcessMember(x.process!.Unfreeze()));

                var listArguments = GetType()
                .GetProperties()
                .Where(x => x.GetCustomAttribute<RunnableProcessListPropertyAttribute>() != null)
                .Select(x => (x.Name, list: x.GetValue(this) as IEnumerable<IRunnableProcess>))
                .Where(x => x.list != null)
                .ToDictionary(x => x.Name,
                    x => new ProcessMember( x.list.Select(y => y.Unfreeze()).ToList()));


                var processMembers = variableNames.Concat(arguments).Concat(listArguments)
                    .ToDictionary(x => x.Key, x => x.Value);


                return new FreezableProcessData(processMembers, ProcessConfiguration);
            }
        }

        /// <inheritdoc />
        public IFreezableProcess Unfreeze() => new CompoundFreezableProcess(RunnableProcessFactory,FreezableProcessData);

        /// <inheritdoc />
        public Result<object> RunUntyped(ProcessState processState) => Run(processState);
    }
}