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
// ReSharper disable once RedundantUsingDirective
using Xunit.Abstractions;
using Xunit.Sdk;

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
        [InlineData(@"C:\Users\wainw\source\repos\Reductech\edr\Examples\Sort.yml")]
        [InlineData(@"C:\Users\wainw\source\repos\Reductech\edr\Examples\EntityMapProperties.yml")]
        [InlineData(@"C:\Users\wainw\source\repos\Reductech\edr\Examples\ChangeCase.yml")]
        public async Task RunYamlSequenceFromFile(string path)
        {
            var yaml = await File.ReadAllTextAsync(path);

            TestOutputHelper.WriteLine(yaml);

            var sfs = StepFactoryStore.CreateUsingReflection();

            var stepResult = SequenceParsing.ParseSequence(yaml).Bind(x => x.TryFreeze(sfs));

            if(stepResult.IsFailure)
                throw new XunitException(
                    string.Join(", ",
                    stepResult.Error.GetAllErrors().Select(x=> x.Message + " " + x.Location.AsString)));


            var monad = new StateMonad(new TestLogger(), EmptySettings.Instance, ExternalProcessRunner.Instance, FileSystemHelper.Instance, sfs);

            var r = await stepResult.Value.Run<Unit>(monad, CancellationToken.None);

            r.ShouldBeSuccessful(x => x.AsString);
        }


        [Fact(Skip = "manual")]
        [Trait("Category", "Integration")]
        public async Task RunYamlSequence()
        {
            const string yaml = @"
- <Folder> = 'C:\Users\wainw\source\repos\Reductech\edr\Examples'
- <SourceFile> = 'Dinosaurs.csv'
- <TargetFile> = 'MappedDinosaurs.csv'
- <EntityStream> = FromCSV(Stream = ReadFile(Path = PathCombine(Paths = [<Folder>, <SourceFile>])))
- <EntityStream> = EntityMapProperties(EntityStream = <EntityStream>, Mappings = (Name = 'Dinosaur'))
- <WriteStream> = ToCSV(Entities = <EntityStream>)
- FileWrite(Path = PathCombine(Paths = [<Folder>, <TargetFile>]), Stream = <WriteStream>)";


            var sfs = StepFactoryStore.CreateUsingReflection();

            var stepResult = SequenceParsing.ParseSequence(yaml).Bind(x=>x.TryFreeze(sfs));

            stepResult.ShouldBeSuccessful(x=>x.AsString);

            var monad = new StateMonad(new TestLogger(), EmptySettings.Instance, ExternalProcessRunner.Instance, FileSystemHelper.Instance, sfs);

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
                    new SetVariable<EntityStream>
                    {
                        Variable = new VariableName("EntityStream"),
                        Value = new FromCSV{Stream = new ReadFile{Path = new Constant<StringStream>(@"C:\Users\wainw\source\repos\Reductech\edr\Examples\Dinosaurs.csv")}}
                    },

                    new SetVariable<Entity>()
                    {
                        Variable = new VariableName("Schema"),
                        Value = new Constant<Entity>(new Schema
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
                        Path = new PathCombine{Paths = new Constant<List<StringStream>>(new List<StringStream>{"MyFile.txt"})},
                        Stream = new ToCSV
                        {
                            Entities = new EnforceSchema()
                            {
                                EntityStream = new GetVariable<EntityStream>(){Variable = new VariableName("EntityStream")},
                                Schema = new GetVariable<Entity>(){Variable = new VariableName("Schema")}
                            }
                        }
                    }
                },
                FinalStep = new DoNothing()
            };

            var yaml = await step.SerializeAsync(CancellationToken.None);

            TestOutputHelper.WriteLine(yaml);

            var monad = new StateMonad(new TestLogger(), EmptySettings.Instance, ExternalProcessRunner.Instance, FileSystemHelper.Instance, StepFactoryStore.CreateUsingReflection() );

            var r = await step.Run(monad, CancellationToken.None);

            r.ShouldBeSuccessful(x=>x.AsString);
        }


    }
}
