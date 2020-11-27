using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Serialization;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Reductech.Utilities.Testing;
// ReSharper disable once RedundantUsingDirective
using Xunit;
using Xunit.Abstractions;
using Entity = Reductech.EDR.Core.Entities.Entity;

namespace Reductech.EDR.Core.Tests
{
    /// <summary>
    /// These are not really tests but ways to quickly and easily run steps
    /// </summary>
    public class ExampleTests
    {
        public ExampleTests(ITestOutputHelper testOutputHelper) => TestOutputHelper = testOutputHelper;

        public ITestOutputHelper TestOutputHelper { get; }

        //[Theory]
        //[InlineData(@"C:\Users\wainw\source\repos\Reductech\edr\Examples\Sort.yml")]
        //[InlineData(@"C:\Users\wainw\source\repos\Reductech\edr\Examples\MapFieldNames.yml")]
        public async Task RunYamlSequenceFromFile(string path)
        {
            var yaml = await File.ReadAllTextAsync(path);

            var sfs = StepFactoryStore.CreateUsingReflection();

            var stepResult = YamlMethods.DeserializeFromYaml(yaml, sfs).Bind(x => x.TryFreeze());

            stepResult.ShouldBeSuccessful(x => x.AsString);

            var monad = new StateMonad(new TestLogger(), EmptySettings.Instance, ExternalProcessRunner.Instance, FileSystemHelper.Instance, sfs);

            var r = await stepResult.Value.Run<Unit>(monad, CancellationToken.None);

            r.ShouldBeSuccessful(x => x.AsString);
        }


        //[Fact]
        public async Task RunYamlSequence()
        {
            const string yaml = @"
- <Folder> = 'C:\Users\wainw\source\repos\Reductech\edr\Examples'
- <SourceFile> = 'Dinosaurs.csv'
- <TargetFile> = 'SortedDinosaurs.csv'
- <EntityStream> = ReadCSV(Stream = ReadFile(Folder = <Folder>, FileName = <SourceFile>))
- <EntityStream> = SortEntities(EntityStream = <EntityStream>, SortBy = GetProperty(Entity =<Entity>, Property = 'Name'))
- <WriteStream> = WriteCSV(Entities = <EntityStream>)
- WriteFile(Folder = <Folder>, Text = <WriteStream>, FileName =<TargetFile>)";


            var sfs = StepFactoryStore.CreateUsingReflection();

            var stepResult = YamlMethods.DeserializeFromYaml(yaml, sfs).Bind(x=>x.TryFreeze());

            stepResult.ShouldBeSuccessful(x=>x.AsString);

            var monad = new StateMonad(new TestLogger(), EmptySettings.Instance, ExternalProcessRunner.Instance, FileSystemHelper.Instance, sfs);

            var r = await stepResult.Value.Run<Unit>(monad, CancellationToken.None);

            r.ShouldBeSuccessful(x => x.AsString);
        }


       //[Fact]
        public async Task RunObjectSequence()
        {
            var step = new WriteFile
            {
                Path = new PathCombine{Paths = new Constant<List<string>>(new List<string>{"MyFile.txt"})},
                Stream = new WriteCSV
                {
                    Entities = new MapFieldNames
                    {
                        EntityStream = new Constant<EntityStream>(EntityStream.Create(new Entity(
                        new KeyValuePair<string, EntityValue>("Foo", EntityValue.Create("Hello")),
                        new KeyValuePair<string, EntityValue>("Bar", EntityValue.Create("World"))))),
                        Mappings = new Constant<Entity>(new Entity(new KeyValuePair<string, EntityValue>("Foo", EntityValue.Create("Foot"))))

                    }
                }
            };

            var yaml = await step.Unfreeze().SerializeToYamlAsync(CancellationToken.None);

            TestOutputHelper.WriteLine(yaml);

            var monad = new StateMonad(new TestLogger(), EmptySettings.Instance, ExternalProcessRunner.Instance, FileSystemHelper.Instance, StepFactoryStore.CreateUsingReflection() );

            var r = await step.Run(monad, CancellationToken.None);

            r.ShouldBeSuccessful(x=>x.AsString);
        }


    }
}
