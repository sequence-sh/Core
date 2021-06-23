using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Divergic.Logging.Xunit;
using Microsoft.Extensions.Logging;
using Reductech.EDR.Core.Abstractions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Parser;
using Reductech.EDR.Core.Internal.Serialization;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Reductech.EDR.Core.Tests
{

/// <summary>
/// These are not really tests but ways to quickly and easily run steps
/// </summary>
[AutoTheory.UseTestOutputHelper]
public partial class ExampleTests
{
    #pragma warning disable xUnit1004 // Test methods should not be skipped
    [Theory(Skip = "skip")]
    #pragma warning restore xUnit1004 // Test methods should not be skipped
    [Trait("Category", "Integration")]
    [InlineData(@"C:\Users\wainw\source\repos\Reductech\edr\Examples\Sort.scl")]
    [InlineData(@"C:\Users\wainw\source\repos\Reductech\edr\Examples\EntityMapProperties.scl")]
    [InlineData(@"C:\Users\wainw\source\repos\Reductech\edr\Examples\ChangeCase.scl")]
    public async Task RunSCLFromFile(string path)
    {
        var scl = await File.ReadAllTextAsync(path);

        TestOutputHelper.WriteLine(scl);

        var sfs = StepFactoryStore.Create();

        var stepResult = SCLParsing.TryParseStep(scl)
            .Bind(x => x.TryFreeze(SCLRunner.RootCallerMetadata, sfs));

        if (stepResult.IsFailure)
            throw new XunitException(
                string.Join(
                    ", ",
                    stepResult.Error.GetAllErrors()
                        .Select(x => x.Message + " " + x.Location.AsString())
                )
            );

        var monad = new StateMonad(
            TestOutputHelper.BuildLogger(),
            sfs,
            ExternalContext.Default,
            new Dictionary<string, object>()
        );

        var r = await stepResult.Value.Run<Unit>(monad, CancellationToken.None);

        r.ShouldBeSuccessful();
    }

    [Fact(Skip = "true")]
    //[Fact]
    [Trait("Category", "Integration")]
    public async Task RunSCLSequence()
    {
        const string scl = @"
- <root> = 'Documentation'
- <docs> = GenerateDocumentation

# Create a directory for each connector
- <docs>
  | ArrayDistinct (From <Entity> 'Directory')
  | ForEach (
      CreateDirectory (PathCombine [<root>, (From <Entity> 'Directory')])
    )

# Export all steps to .\<root>\ConnectorName\StepName.md
- <docs>
  | Foreach (
      FileWrite
        (From <Entity> 'FileText')
        (PathCombine [<root>, (From <Entity> 'Directory'), (From <Entity> 'FileName')])
    )

";

        var logger =
            TestOutputHelper.BuildLogger(new LoggingConfig() { LogLevel = LogLevel.Information });

        var sfs = StepFactoryStore.Create();

        var runner = new SCLRunner(
            logger,
            sfs,
            ExternalContext.Default
        );

        var r = await runner.RunSequenceFromTextAsync(
            scl,
            new Dictionary<string, object>(),
            CancellationToken.None
        );

        r.ShouldBeSuccessful();
    }

    //#pragma warning disable xUnit1004 // Test methods should not be skipped
    //[Fact(Skip = "skip")]
    //#pragma warning restore xUnit1004 // Test methods should not be skipped
    //[Trait("Category", "Integration")]
    //public async Task RunObjectSequence()
    //{
    //    var step = new Sequence<Unit>()
    //    {
    //        InitialSteps = new List<IStep<Unit>>
    //        {
    //            new SetVariable<Array<Entity>>
    //            {
    //                Variable = new VariableName("EntityStream"),
    //                Value = new FromCSV
    //                {
    //                    Stream = new FileRead
    //                    {
    //                        Path = new StringConstant(
    //                            @"C:\Users\wainw\source\repos\Reductech\edr\Examples\Dinosaurs.csv"
    //                        )
    //                    }
    //                }
    //            },
    //            new SetVariable<Entity>()
    //            {
    //                Variable = new VariableName("Schema"),
    //                Value = Constant(
    //                    new Schema
    //                    {
    //                        ExtraProperties = ExtraPropertyBehavior.Fail,
    //                        Name            = "Dinosaur",
    //                        Properties = new Dictionary<string, SchemaProperty>
    //                        {
    //                            {
    //                                "Name", new SchemaProperty { Type = SCLType.String }
    //                            },
    //                            {
    //                                "ArrayLength",
    //                                new SchemaProperty { Type = SCLType.Double }
    //                            },
    //                            { "Period", new SchemaProperty { Type = SCLType.String } },
    //                        }
    //                    }.ConvertToEntity()
    //                )
    //            },
    //            new FileWrite
    //            {
    //                Path = new PathCombine { Paths = Array("MyFile.txt") },
    //                Stream = new ToCSV
    //                {
    //                    Entities = new EnforceSchema()
    //                    {
    //                        EntityStream =
    //                            new GetVariable<Array<Entity>>()
    //                            {
    //                                Variable =
    //                                    new VariableName("EntityStream")
    //                            },
    //                        Schema = new GetVariable<Entity>()
    //                        {
    //                            Variable = new VariableName("Schema")
    //                        }
    //                    }
    //                }
    //            }
    //        },
    //        FinalStep = new DoNothing()
    //    };

    //    var scl = step.Serialize();

    //    TestOutputHelper.WriteLine(scl);

    //    var monad = new StateMonad(
    //        TestOutputHelper.BuildLogger(),
    //        SCLSettings.EmptySettings,
    //        StepFactoryStore.CreateFromAssemblies(),
    //        ExternalContext.Default,
    //        new Dictionary<string, object>()
    //    );

    //    var r = await (step as IStep<Unit>).Run(monad, CancellationToken.None);

    //    r.ShouldBeSuccessful();
    //}
}

}
