using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class CreateDirectoryTests : StepTestBase<CreateDirectory, Unit>
    {
        /// <inheritdoc />
        public CreateDirectoryTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Create Directory",
                    new CreateDirectory
                    {
                        Path = Constant("MyPath")
                    },
                    Unit.Default

                ).WithFileSystemAction(x=>
                    x.Setup(h=>h.CreateDirectory("MyPath"))
                        .Returns(Unit.Default));
        }
        }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {
                yield return new DeserializeCase("Create Directory", "CreateDirectory(Path = 'MyPath')", Unit.Default)
                    .WithFileSystemAction(x =>
                    x.Setup(h => h.CreateDirectory("MyPath"))
                        .Returns(Unit.Default));
            }
        }


        /// <inheritdoc />
        protected override IEnumerable<ErrorCase> ErrorCases {
            get
            {
                yield return new ErrorCase("Error returned", new CreateDirectory
                {
                    Path = Constant("MyPath")
                }, new ErrorBuilder("Test Error", ErrorCode.Test))
                    .WithFileSystemAction(x =>
                    x.Setup(h => h.CreateDirectory("MyPath"))
                        .Returns(new ErrorBuilder("Test Error", ErrorCode.Test)));

            } }
    }

    public class CreateFileTests : StepTestBase<CreateFile, Unit>
    {
        /// <inheritdoc />
        public CreateFileTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Create File",
                    new CreateFile
                    {
                        Path = Constant("My Path"),
                        Text = Constant("My Text")
                    }, Unit.Default)
                    .WithFileSystemAction(x=>x
                        .Setup(a=>a.CreateFileAsync("My Path", "My Text", It.IsAny<CancellationToken>()))
                        .ReturnsAsync(Unit.Default));
            }
        }

        /// <inheritdoc />
        protected override IEnumerable<ErrorCase> ErrorCases {
            get
            {
                yield return new ErrorCase("Error Returned",
                    new CreateFile
                    {
                        Path = Constant("My Path"),
                        Text = Constant("My Text")
                    }, new ErrorBuilder("Test Error", ErrorCode.Test))
                    .WithFileSystemAction(x => x
                        .Setup(a => a.CreateFileAsync("My Path", "My Text", It.IsAny<CancellationToken>()))
                        .ReturnsAsync(new ErrorBuilder("Test Error", ErrorCode.Test)));
            } }
    }

    public class DeleteItemTests : StepTestBase<DeleteItem, Unit>
    {
        /// <inheritdoc />
        public DeleteItemTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) {}

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Delete Directory",
                    new DeleteItem
                    {
                        Path = Constant("My Path")
                    },
                    Unit.Default, "Directory 'My Path' Deleted.")
                        .WithFileSystemAction(x=>x.Setup(a=>a.DoesDirectoryExist("My Path")).Returns(true))
                        .WithFileSystemAction(x=>x.Setup(a=>a.DeleteDirectory("My Path", true)).Returns(Unit.Default));

                yield return new StepCase("Delete File",
                    new DeleteItem
                    {
                        Path = Constant("My Path")
                    },
                    Unit.Default, "File 'My Path' Deleted.")
                        .WithFileSystemAction(x => x.Setup(a => a.DoesDirectoryExist("My Path")).Returns(false))
                        .WithFileSystemAction(x => x.Setup(a => a.DoesFileExist("My Path")).Returns(true))
                        .WithFileSystemAction(x => x.Setup(a => a.DeleteFile("My Path")).Returns(Unit.Default));

                yield return new StepCase("Item does not exist",
                    new DeleteItem
                    {
                        Path = Constant("My Path")
                    },
                    Unit.Default, "Item 'My Path' did not exist.")
                        .WithFileSystemAction(x => x.Setup(a => a.DoesDirectoryExist("My Path")).Returns(false))
                        .WithFileSystemAction(x => x.Setup(a => a.DoesFileExist("My Path")).Returns(false));

            }
        }

        /// <inheritdoc />
        protected override IEnumerable<ErrorCase> ErrorCases
        {
            get
            {
                yield return new ErrorCase("Could not delete file",
                    new DeleteItem
                    {
                        Path = Constant("My Path")
                    },
                    new ErrorBuilder("Test Error", ErrorCode.Test))
                    .WithFileSystemAction(x => x.Setup(a => a.DoesDirectoryExist("My Path")).Returns(true))
                        .WithFileSystemAction(x => x.Setup(a => a.DeleteDirectory("My Path", true))
                        .Returns(new ErrorBuilder("Test Error", ErrorCode.Test)));
            }
        }
    }

    public class DoesDirectoryExistTests : StepTestBase<DoesDirectoryExist, bool>
    {
        /// <inheritdoc />
        public DoesDirectoryExistTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Directory Exists",
                    new DoesDirectoryExist
                    {
                        Path = Constant("My Path")
                    },
                    true
                ).WithFileSystemAction(x=>x.Setup(a=>a.DoesDirectoryExist("My Path")).Returns(true));
            }
        }

    }

    public class DoesStringContainTests : StepTestBase<DoesStringContain, bool>
    {
        /// <inheritdoc />
        public DoesStringContainTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("True case sensitive",
                    new DoesStringContain
                    {
                        Superstring = Constant("Hello World"),
                        Substring = Constant("Hello")
                    }, true);

                yield return new StepCase("False case sensitive",
                    new DoesStringContain
                    {
                        Superstring = Constant("Hello World"),
                        Substring = Constant("hello")
                    }, false);

                yield return new StepCase("True case insensitive",
                    new DoesStringContain
                    {
                        Superstring = Constant("Hello World"),
                        Substring = Constant("hello"),
                        IgnoreCase = Constant(true)
                    }, true);

                yield return new StepCase("False case insensitive",
                    new DoesStringContain
                    {
                        Superstring = Constant("Hello World"),
                        Substring = Constant("Goodbye"),
                        IgnoreCase = Constant(true)
                    }, false);
            }
        }


    }

    public class ForEachEntityTests : StepTestBase<ForEachEntity, Unit> //TODO sort out entity stream serialization
    {
        /// <inheritdoc />
        public ForEachEntityTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("For each record",
                    new ForEachEntity
                    {
                        VariableName = new VariableName("Foo"),
                        Action = new Print<Entity>
                        {
                            Value = GetVariable<Entity>("Foo")
                        },EntityStream = Constant(EntityStream.Create(
                            new Entity(new KeyValuePair<string, string>("Foo", "Hello"), new KeyValuePair<string, string>("Bar", "World")),
                            new Entity(new KeyValuePair<string, string>("Foo", "Hello 2"), new KeyValuePair<string, string>("Bar", "World 2"))
                        ))
                    },
                    Unit.Default,
                    "Foo: Hello, Bar: World",
                    "Foo: Hello 2, Bar: World 2"
                ).WithExpectedFinalState("Foo", new Entity(new KeyValuePair<string, string>("Foo", "Hello 2"), new KeyValuePair<string, string>("Bar", "World 2")));
            }
        }
    }

    public class FromStreamTests : StepTestBase<FromStream, string>
    {
        /// <inheritdoc />
        public FromStreamTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("From Stream",
                    new FromStream
                    {
                        Stream = Constant<Stream>(new MemoryStream( Encoding.UTF8.GetBytes( "Hello World" )))
                    },"Hello World"
                    );
            }
        }

    }

    public class ReadCSVTests : StepTestBase<ReadCsv, EntityStream>
    {
        /// <inheritdoc />
        public ReadCSVTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new SequenceStepCase("Read CSV and print all lines",
                    new Sequence
                    {
                        Steps = new List<IStep<Unit>>
                        {
                            new ForEachEntity
                            {
                                VariableName = new VariableName("Foo"),
                                EntityStream = new ReadCsv
                            {
                                ColumnsToMap = Constant(new List<string> {"Foo", "Bar"}),
                                Delimiter = Constant(","),
                                TextStream = new ToStream
                                {Text = Constant(
                                    $@"Foo,Bar{Environment.NewLine}Hello,World{Environment.NewLine}Hello 2,World 2")}
                            },
                                Action = new Print<Entity>
                                {
                                    Value = new GetVariable<Entity> {VariableName = new VariableName("Foo")}
                                }
                            }
                      }
                    },
                    "Foo: Hello, Bar: World",
                    "Foo: Hello 2, Bar: World 2"
                ).WithExpectedFinalState("Foo", new Entity(new KeyValuePair<string, string>("Foo", "Hello 2"), new KeyValuePair<string, string>("Bar", "World 2")));


            }
        }


    }

    public class ReadFileTests : StepTestBase<ReadFile, Stream>
    {
        /// <inheritdoc />
        public ReadFileTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new SequenceStepCase("Print file text",
                    new Sequence
                    {
                        Steps = new List<IStep<Unit>>
                        {
                            new Print<string>
                            {
                                Value = new FromStream
                                {
                                    Stream = new ReadFile
                                    {
                                        FileName = Constant("File.txt"),
                                        Folder = Constant("MyFolder")
                                    }
                                }
                            }
                        }
                    },
                    "Hello World"

                ).WithFileSystemAction(x=>x.Setup(a=>a.ReadFile(
                    Path.Combine("MyFolder", "File.txt"))).Returns(new MemoryStream(Encoding.ASCII.GetBytes("Hello World"))));
            }
        }

        /// <inheritdoc />
        protected override IEnumerable<ErrorCase> ErrorCases
        {
            get
            {
                yield return new ErrorCase("Test Error",
                        new ReadFile
                        {
                            FileName = Constant("File.txt"),
                            Folder = Constant("MyFolder")
                        },
                        new ErrorBuilder("Test Error", ErrorCode.Test)
                    )
                    .WithFileSystemAction(x => x.Setup(a => a.ReadFile(
                      Path.Combine("MyFolder", "File.txt"))).Returns(new ErrorBuilder("Test Error", ErrorCode.Test)));
            }
        }
    }

    public class RunExternalProcessTests : StepTestBase<RunExternalProcess, Unit>
    {
        /// <inheritdoc />
        public RunExternalProcessTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get { yield return new StepCase("Do nothing external process",

                new RunExternalProcess
                {
                    Arguments = new Constant<List<string>>(new List<string>(){"Foo"}),
                    ProcessPath = new Constant<string>("Process.exe")
                },
Unit.Default, "My Message"
                )
                    .WithExternalProcessAction(x=>
                        x.Setup(a => a.RunExternalProcess("Process.exe",
                            It.IsAny<ILogger>(),
                            It.IsAny<IErrorHandler>(),
                            It.IsAny<IEnumerable<string>>()))
                            .Callback<string, ILogger, IErrorHandler, IEnumerable<string>>((a,b,c,d)=> b.LogInformation("My Message"))
                            .ReturnsAsync(Unit.Default)
                    )

                ;}
        }

    }

    public class ToStreamTests : StepTestBase<ToStream, Stream>
    {
        /// <inheritdoc />
        public ToStreamTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get { yield return new SequenceStepCase("To stream test",
                new Sequence
                {
                    Steps = new List<IStep<Unit>>
                    {
                        new Print<string>()
                        {
                            Value = new FromStream
                            {
                                Stream = new ToStream
                                {
                                    Text = Constant("Hello World")
                                }
                            }
                        }
                    }
                }, "Hello World"
            );}
        }

    }

    public class UnzipTests : StepTestBase<Unzip, Unit>
    {
        /// <inheritdoc />
        public UnzipTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get { throw new NotImplementedException(); }
        }


    }

    public class WriteFileTests : StepTestBase<WriteFile, Unit>
    {
        /// <inheritdoc />
        public WriteFileTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get { throw new NotImplementedException(); }
        }

    }


}
