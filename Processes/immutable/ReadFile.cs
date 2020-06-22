﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Reductech.EDR.Utilities.Processes.output;

namespace Reductech.EDR.Utilities.Processes.immutable
{
    /// <summary>
    /// Immutable process which reads a file.
    /// </summary>
    public class ReadFile : ImmutableProcess<string>
    {
        /// <summary>
        /// Creates a new ReadFile
        /// </summary>
        /// <param name="filePath"></param>
        public ReadFile(string filePath)
        {
            FilePath = filePath;
        }

        /// <summary>
        /// The path to the file.
        /// </summary>
        public string FilePath { get; }

        /// <inheritdoc />
#pragma warning disable 1998
        public override async IAsyncEnumerable<IProcessOutput<string>> Execute()
#pragma warning restore 1998
        {
            string? text;

            var errors = new List<string>();

            try
            {
                text = File.ReadAllText(FilePath);
            }
            catch (Exception e)
            {
                errors.Add("Could not read file.");
                errors.Add(e.Message);
                text = null;
            }


            if (errors.Any())
                foreach (var error in errors)
                    yield return ProcessOutput<string>.Error(error);
            else if (text != null)
                yield return ProcessOutput<string>.Success(text);
        }

        /// <inheritdoc />
        public override string Name => ProcessNameHelper.GetReadFileName();

        /// <inheritdoc />
        public override IProcessConverter? ProcessConverter => null;
    }
}
