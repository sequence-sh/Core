using System;
using Moq;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Parser;
using Reductech.EDR.Core.Parser;

namespace Reductech.EDR.Core.TestHarness
{
    public static class Extensions
    {
        public static T WithExpectedFinalState<T>(this T cws, string variableName, object value) where T : ICaseThatExecutes
        {
            if(value is string s)
                value = new StringStream(s);


            cws.ExpectedFinalState.Add(new VariableName(variableName), value);
            return cws;
        }

        public static T WithStepFactoryStore<T>(this T cws, StepFactoryStore stepFactoryStore) where T : ICaseThatExecutes
        {
            cws.StepFactoryStoreToUse = stepFactoryStore;
            return cws;
        }

        public static T WithSettings<T>(this T cws, ISettings settings) where T : ICaseThatExecutes
        {
            cws.Settings = settings;
            return cws;
        }


        public static T WithExternalProcessAction<T>(this T cws, Action<Mock<IExternalProcessRunner>> action) where T : ICaseThatExecutes
        {
            cws.AddExternalProcessRunnerAction(action);
            return cws;
        }

        public static T WithFileSystemAction<T>(this T cws, Action<Mock<IFileSystemHelper>> action) where T : ICaseThatExecutes
        {
            cws.AddFileSystemAction(action);
            return cws;
        }
    }
}
