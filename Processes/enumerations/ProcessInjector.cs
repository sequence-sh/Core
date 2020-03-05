using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Processes.process;

namespace Processes.enumerations
{
    internal interface IProcessInjector
    {
        Result Inject(Process process);
    }

    internal class ProcessInjector : IProcessInjector
    {
        private readonly List<(string element, Injection injection)> _injections;

        public void Add(string element, Injection injection)
        {
            _injections.Add((element, injection));
        }

        public ProcessInjector()
        {
            _injections = new List<(string element, Injection injection)>();
        }

        public ProcessInjector(IEnumerable<(string element, Injection injection)> injections)
        {
            _injections = injections.ToList();
        }

        public Result Inject(Process process)
        {
            foreach (var (element, injection) in _injections)
            {

                var property = process.GetType().GetProperty(injection.PropertyToInject); //TODO handle dots and array indexes in this argument

                if (property == null)
                {
                    return Result.Failure($"Could not find property '{injection.PropertyToInject}'");
                }

                var (_, isFailure, value, error1) = injection.GetPropertyValue(element);

                if (isFailure)
                {
                    return Result.Failure<string>(error1);
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
                        var underlyingType = Nullable.GetUnderlyingType(property.PropertyType) ??
                                           property.PropertyType;

                        v = Convert.ChangeType(value, underlyingType);
                    }
                    catch (InvalidCastException e)
                    {
                        v = "";
                        error = e.Message;
                    }

                    if (error != null)
                    {
                        return Result.Failure($"Could not cast '{value}' to type '{injection.PropertyToInject}'");
                    }
                }

                property.SetValue(process, v);
            }

            return Result.Success();
        }
    }
}