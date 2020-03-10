using System.Collections.Generic;
using CSharpFunctionalExtensions;
using JetBrains.Annotations;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Utilities.Processes.Tests
{
    public class EmitProcess : Process
    {
        [UsedImplicitly]
        [YamlMember]
        public string? Term { get; set; }

        [UsedImplicitly]
        [YamlMember]
        public int? Number { get; set; }

        public override IEnumerable<string> GetArgumentErrors()
        {
            yield break;
        }

        public override IEnumerable<string> GetSettingsErrors(IProcessSettings processSettings)
        {
            yield break;
        }

        public override string GetName()
        {
            return "Emit";
        }


#pragma warning disable 1998
        public override async IAsyncEnumerable<Result<string>> Execute(IProcessSettings processSettings)
#pragma warning restore 1998
        {
            yield return Result.Success(Term + Number);
        }
    }
}