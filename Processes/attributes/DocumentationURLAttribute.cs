﻿using System;

namespace Reductech.EDR.Utilities.Processes.attributes
{
    /// <summary>
    /// Points to a documentation page for this parameter.
    /// </summary>
    public sealed class DocumentationURLAttribute : Attribute
    {
        /// <summary>
        /// Creates a new DocumentationURLAttribute.
        /// </summary>
        /// <param name="documentationURL"></param>
        public DocumentationURLAttribute(string documentationURL)
        {
            DocumentationURL = documentationURL;
        }

        /// <summary>
        /// The url to the documentation.
        /// </summary>
        public string DocumentationURL { get; }
    }
}