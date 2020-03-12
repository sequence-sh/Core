using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Utilities.Processes.injection
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
                var pathResult = InjectionParser.TryParse(injection.Property);

                if (pathResult.IsFailure)
                    return pathResult;

                var setResult = pathResult.Value.TrySetValue(process, element);

                if (setResult.IsFailure)
                    return setResult;
            }

            return Result.Success();
        }
    }
}