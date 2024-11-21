// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License license. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.Jdt
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// The file that caused the exception.
    /// </summary>
    public enum ErrorLocation
    {
        /// <summary>
        /// Represents no location set.
        /// </summary>
        None,

        /// <summary>
        /// Represents the source file.
        /// </summary>
        Source,

        /// <summary>
        /// Represents the transform file.
        /// </summary>
        Transform,
    }

    /// <summary>
    /// Exception thrown on JDT error.
    /// </summary>
    [Serializable]
    public class JdtException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JdtException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public JdtException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JdtException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="location">The file that generated the exception.</param>
        public JdtException(string message, ErrorLocation location)
            : this(message)
        {
            this.Location = location;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JdtException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="location">The file that generated the exception.</param>
        /// <param name="lineNumber">The line that caused the error.</param>
        /// <param name="linePosition">The position in the lite that caused the error.</param>
        public JdtException(string message, ErrorLocation location, int lineNumber, int linePosition)
            : this(message, location)
        {
            this.LineNumber = lineNumber;
            this.LinePosition = linePosition;
        }

        /// <summary>
        /// Gets the line number of the exception.
        /// </summary>
        public int LineNumber { get; }

        /// <summary>
        /// Gets the line position of the exception.
        /// </summary>
        public int LinePosition { get; }

        /// <summary>
        /// Gets the name of the file that generated the exception.
        /// </summary>
        public ErrorLocation Location { get; } = ErrorLocation.None;

        /// <summary>
        /// Returns a <see cref="JdtException"/> with line info.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="location">The file that generated the exception.</param>
        /// <param name="lineInfo">The line info of the object that caused the error.</param>
        /// <returns>A new instance of <see cref="JdtException"/>.</returns>
        internal static JdtException FromLineInfo(string message, ErrorLocation location, IJsonLineInfo lineInfo)
        {
            return new JdtException(message, location, lineInfo?.LineNumber ?? 0, lineInfo?.LinePosition ?? 0);
        }
    }
}
