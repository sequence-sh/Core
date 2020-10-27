using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Core.TestHarness
{
    public static class Extensions
    {

        public static T WithInitialState<T>(this T cws, string variableName, object value) where T : ICaseWithState
        {
            cws.InitialState.Add(new VariableName(variableName), value);
            return cws;
        }

        public static T WithExpectedFinalState<T>(this T cws, string variableName, object value) where T : ICaseWithState
        {
            cws.ExpectedFinalState.Add(new VariableName(variableName), value);
            return cws;
        }
    }
}
