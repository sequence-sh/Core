using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using CSharpFunctionalExtensions;
using Processes.conditions;
using Processes.enumerations;
using YamlDotNet.Serialization;

namespace Processes.process
{
    /// <summary>
    /// Performs a nested process once for each element in an enumeration
    /// </summary>
    public class ForEach : Process
    {
        /// <inheritdoc />
        public override IEnumerable<string> GetArgumentErrors()
        {
            return SubProcess.GetArgumentErrors(); //TODO look at this - its problematic. There seems to be no way to check the injected argument
        }

        /// <inheritdoc />
        public override string GetName() => $"Foreach in {Enumeration}, {SubProcess}";

        /// <summary>
        /// The enumeration to iterate through.
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 2)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public Enumeration Enumeration { get; set; }

        

        /// <summary>
        /// The process to run once for each element
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 5)]
        public Process SubProcess { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        /// <inheritdoc />
        public override async IAsyncEnumerable<Result<string>> Execute()
        {
            foreach (var injectionList in Enumeration.Elements.ToList())
            {
                var subProcess = SubProcess; //TODO if we ever try to run these in parallel we will need to clone the process

                foreach (var (element, injection) in injectionList)
                {

                    var property = subProcess.GetType().GetProperty(injection.PropertyToInject); //TODO handle dots and array indexes in this argument

                    if (property == null)
                    {
                        yield return Result.Failure<string>($"Could not find property '{injection.PropertyToInject}'");
                        break;
                    }

                    var (_, isFailure, value, error1) = injection.GetPropertyValue(element);

                    if (isFailure)
                    {
                        yield return Result.Failure<string>(error1);
                        break;
                    }

                    object v;
                    if (property.PropertyType == typeof(string))
                    {
                        v = value;
                    }
                    else
                    {
                        string? error = null;
                        try
                        {
                            v = Convert.ChangeType(value, property.PropertyType);
                        }
                        catch (InvalidCastException e)
                        {
                            v = "";
                            error = e.Message;
                        }

                        if (error != null)
                        {
                            yield return Result.Failure<string>($"Could not find property '{injection.PropertyToInject}'");
                            break;
                        }
                    }

                    property.SetValue(subProcess, v);
                }
                

                if (!(subProcess.Conditions ?? Enumerable.Empty<Condition>()).All(x => x.IsMet())) continue;

                var resultLines = subProcess.Execute();

                await foreach (var rl in resultLines)
                    yield return rl;
            }
        }
    }
}
