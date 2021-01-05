using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Parser;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Reductech.Utilities.Testing;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests
{
    /// <summary>
    /// These are not really tests but ways to quickly and easily run steps
    /// </summary>
    public class ExampleTests
    {
        public ExampleTests(ITestOutputHelper testOutputHelper) => TestOutputHelper = testOutputHelper;

        public ITestOutputHelper TestOutputHelper { get; }

        [Theory(Skip = "Manual")]
        [Trait("Category", "Integration")]
        [InlineData(@"C:\Users\wainw\source\repos\Reductech\edr\Examples\Sort.scl")]
        [InlineData(@"C:\Users\wainw\source\repos\Reductech\edr\Examples\EntityMapProperties.scl")]
        [InlineData(@"C:\Users\wainw\source\repos\Reductech\edr\Examples\ChangeCase.scl")]
        public async Task RunSCLFromFile(string path)
        {
            var scl = await File.ReadAllTextAsync(path);

            TestOutputHelper.WriteLine(scl);

            var sfs = StepFactoryStore.CreateUsingReflection();

            var stepResult = SCLParsing.ParseSequence(scl).Bind(x => x.TryFreeze(sfs));

            if(stepResult.IsFailure)
                throw new XunitException(
                    string.Join(", ",
                    stepResult.Error.GetAllErrors().Select(x=> x.Message + " " + x.Location.AsString)));


            var monad = new StateMonad(new TestInformationLogger(), EmptySettings.Instance, ExternalProcessRunner.Instance, FileSystemHelper.Instance, sfs);

            var r = await stepResult.Value.Run<Unit>(monad, CancellationToken.None);

            r.ShouldBeSuccessful(x => x.AsString);
        }


        [Fact(Skip = "Manual")]
        [Trait("Category", "Integration")]
        public async Task RunSCLSequence()
        {
            const string scl = @"
- <Converted> = ReadFile Path: 'C:\Users\wainw\source\repos\Reductech\core\TestCSV.csv'
  | FromCSV
  | EntityMap Function: (
      EntitySetValue
        Entity: <Entity>
        Property: 'Name'
        Value: (StringToCase String: (EntityGetValue <Entity> 'Name') Case: TextCase.Upper)
    )
  | ToCSV
- FileWrite Path: 'C:\Users\wainw\source\repos\Reductech\core\TestCSV2.csv' Stream: <Converted>";


            var sfs = StepFactoryStore.CreateUsingReflection();

            var stepResult = SCLParsing.ParseSequence(scl).Bind(x=>x.TryFreeze(sfs));

            stepResult.ShouldBeSuccessful(x=>x.AsString);

            var monad = new StateMonad(new TestInformationLogger(), EmptySettings.Instance, ExternalProcessRunner.Instance, FileSystemHelper.Instance, sfs);

            var r = await stepResult.Value.Run<Unit>(monad, CancellationToken.None);

            r.ShouldBeSuccessful(x => x.AsString);
        }


        [Fact(Skip = "Manual")]
        [Trait("Category", "Integration")]
        public async Task RunObjectSequence()
        {
            var step = new Sequence<Unit>()
            {
                InitialSteps = new List<IStep<Unit>>
                {
                    new SetVariable<Array<Entity>>
                    {
                        Variable = new VariableName("EntityStream"),
                        Value = new FromCSV{Stream = new FileRead{Path = new StringConstant(@"C:\Users\wainw\source\repos\Reductech\edr\Examples\Dinosaurs.csv")}}
                    },

                    new SetVariable<Entity>()
                    {
                        Variable = new VariableName("Schema"),
                        Value = Constant(new Schema
                                {
                                    AllowExtraProperties = false,
                                    Name = "Dinosaur",
                                    Properties = new Dictionary<string, SchemaProperty>()
                                    {
                                        {"Name", new SchemaProperty{Type = SchemaPropertyType.String}},
                                        {"ArrayLength", new SchemaProperty{Type = SchemaPropertyType.Double}},
                                        {"Period", new SchemaProperty{Type = SchemaPropertyType.String}},
                                    }
                                }.ConvertToEntity())
                    },

                    new FileWrite
                    {
                        Path = new PathCombine{Paths = Array("MyFile.txt")},
                        Stream = new ToCSV
                        {
                            Entities = new EnforceSchema()
                            {
                                EntityStream = new GetVariable<Array<Entity>>(){Variable = new VariableName("EntityStream")},
                                Schema = new GetVariable<Entity>(){Variable = new VariableName("Schema")}
                            }
                        }
                    }
                },
                FinalStep = new DoNothing()
            };

            var scl = step.Serialize();

            TestOutputHelper.WriteLine(scl);

            var monad = new StateMonad(new TestInformationLogger(), EmptySettings.Instance, ExternalProcessRunner.Instance, FileSystemHelper.Instance, StepFactoryStore.CreateUsingReflection() );

            var r = await (step as IStep<Unit>).Run(monad, CancellationToken.None);

            r.ShouldBeSuccessful(x=>x.AsString);
        }


    }
}
