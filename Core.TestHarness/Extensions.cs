using System;
using Moq;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Core.TestHarness
{
    public static class Extensions
    {

        public static T WithInitialState<T>(this T cws, string variableName, object value) where T : ICaseThatRuns
        {
            cws.InitialState.Add(new VariableName(variableName), value);
            return cws;
        }

        public static T WithExpectedFinalState<T>(this T cws, string variableName, object value) where T : ICaseThatRuns
        {
            cws.ExpectedFinalState.Add(new VariableName(variableName), value);
            return cws;
        }

        public static T WithSettings<T>(this T cws, ISettings settings) where T : ICaseThatRuns
        {
            cws.Settings = settings;
            return cws;
        }

        public static T WithExternalProcess<T>(this T cws, Action<Mock<IExternalProcessRunner>> action) where T : ICaseThatRuns
        {
            cws.AddExternalProcessRunnerAction(action);
            return cws;
        }
    }
}
