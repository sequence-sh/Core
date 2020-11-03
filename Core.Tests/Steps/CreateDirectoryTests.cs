using System;
using System.Collections.Generic;
using System.IO;
using Reductech.EDR.Core.Entities;
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
                throw new NotImplementedException();
            }
        }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {
                throw new NotImplementedException();
            }
        }
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
                throw new NotImplementedException();
            }
        }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {
                throw new NotImplementedException();
            }

        }

    }

    public class DeleteItemTests : StepTestBase<DeleteItem, Unit>
    {
        /// <inheritdoc />
        public DeleteItemTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get { throw new NotImplementedException(); }
        }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get { throw new NotImplementedException(); }

        }

    }

    public class DirectoryExistsTests : StepTestBase<DirectoryExists, bool>
    {
        /// <inheritdoc />
        public DirectoryExistsTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get { throw new NotImplementedException(); }
        }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get { throw new NotImplementedException(); }

        }

    }

    public class DoesFileContainTests : StepTestBase<DoesFileContain, bool>
    {
        /// <inheritdoc />
        public DoesFileContainTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get { throw new NotImplementedException(); }
        }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get { throw new NotImplementedException(); }

        }

    }

    public class ForEachRecordTests : StepTestBase<ForEachRecord, Unit>
    {
        /// <inheritdoc />
        public ForEachRecordTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get { throw new NotImplementedException(); }
        }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get { throw new NotImplementedException(); }

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
            get { throw new NotImplementedException(); }
        }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get { throw new NotImplementedException(); }

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
            get { throw new NotImplementedException(); }
        }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get { throw new NotImplementedException(); }

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
            get { throw new NotImplementedException(); }
        }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get { throw new NotImplementedException(); }

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
            get { throw new NotImplementedException(); }
        }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get { throw new NotImplementedException(); }

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
            get { throw new NotImplementedException(); }
        }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get { throw new NotImplementedException(); }

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

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
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

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get { throw new NotImplementedException(); }

        }

    }


}
