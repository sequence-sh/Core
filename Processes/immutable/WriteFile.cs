using System;
using System.Collections.Generic;
using System.IO;
using Reductech.EDR.Utilities.Processes.output;

namespace Reductech.EDR.Utilities.Processes.immutable
{
    /// <summary>
    /// Writes the output of a process to a file. Will overwrite if necessary.
    /// </summary>
    public class WriteFile : ImmutableProcess<Unit>
    {
        /// <summary>
        /// A process which outputs the text to write.
        /// </summary>
        public readonly IImmutableProcess<string> TextProcess;

        /// <summary>
        /// The folder to create the file in.
        /// </summary>
        public readonly string Folder;

        /// <summary>
        /// The name of the file to write.
        /// </summary>
        public readonly string FileName;

        /// <summary>
        /// Creates a new WriteFile.
        /// </summary>
        public WriteFile(IImmutableProcess<string> textProcess, string folder, string fileName)
        {
            TextProcess = textProcess;
            Folder = folder;
            FileName = fileName;
        }


        /// <inheritdoc />
        public override string Name => ProcessNameHelper.GetWriteFileProcessName(TextProcess.Name);

        /// <inheritdoc />
        public override IProcessConverter? ProcessConverter => TextProcess.ProcessConverter;

        /// <inheritdoc />
        public override async IAsyncEnumerable<IProcessOutput<Unit>> Execute()
        {
            bool? successState = null;
            string? successValue = null;

            await foreach (var processOutput in TextProcess.Execute())
            {
                if (processOutput.OutputType == OutputType.Success && successState != null)
                {
                    successState = true;
                    successValue = processOutput.Value;
                }
                else
                {
                    if (processOutput.OutputType == OutputType.Error) successState = false;
                    yield return processOutput.ConvertTo<Unit>();
                }
            }

            switch (successState)
            {
                case true when successValue != null:
                {
                    var path = Path.Combine(Folder, FileName);

                    string? error;
                    try
                    {
                        File.WriteAllText(path, successValue);
                        error = null;
                    }
#pragma warning disable CA1031 // Do not catch general exception types
                        catch (Exception e)
                    {
                        error = e.Message;
                    }
#pragma warning restore CA1031 // Do not catch general exception types

                        if (error == null)
                        yield return ProcessOutput<Unit>.Success(Unit.Instance);
                    else yield return ProcessOutput<Unit>.Error(error);
                    break;
                }

                case false: //Do nothing
                    break;
                default:
                    yield return ProcessOutput<Unit>.Error("$Nested Process did not succeed");
                    break;
            }
        }
    }
}
