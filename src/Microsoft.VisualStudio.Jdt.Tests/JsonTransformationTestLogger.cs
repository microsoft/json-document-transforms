// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.Jdt.Tests
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Mock logger to test <see cref="JsonTransformation"/>
    /// </summary>
    public class JsonTransformationTestLogger : IJsonTransformationLogger
    {
        /// <summary>
        /// Gets the error log
        /// </summary>
        public List<TestLogEntry> ErrorLog { get; } = new List<TestLogEntry>();

        /// <summary>
        /// Gets the warning log
        /// </summary>
        public List<TestLogEntry> WarningLog { get; } = new List<TestLogEntry>();

        /// <summary>
        /// Gets the message log
        /// </summary>
        public List<TestLogEntry> MessageLog { get; } = new List<TestLogEntry>();

        /// <inheritdoc/>
        public void LogError(string message)
        {
            this.ErrorLog.Add(new TestLogEntry(message, null, 0, 0, false));
        }

        /// <inheritdoc/>
        public void LogError(string message, string fileName, int lineNumber, int linePosition)
        {
            this.ErrorLog.Add(new TestLogEntry(message, fileName, lineNumber, linePosition, false));
        }

        /// <inheritdoc/>
        public void LogErrorFromException(Exception ex)
        {
            this.ErrorLog.Add(new TestLogEntry(ex.Message, null, 0, 0, true));
        }

        /// <inheritdoc/>
        public void LogErrorFromException(Exception ex, string fileName, int lineNumber, int linePosition)
        {
            this.ErrorLog.Add(new TestLogEntry(ex.Message, fileName, lineNumber, linePosition, true));
        }

        /// <inheritdoc/>
        public void LogMessage(string message)
        {
            this.MessageLog.Add(new TestLogEntry(message, null, 0, 0, false));
        }

        /// <inheritdoc/>
        public void LogWarning(string message)
        {
            this.WarningLog.Add(new TestLogEntry(message, null, 0, 0, false));
        }

        /// <inheritdoc/>
        public void LogWarning(string message, string fileName)
        {
            this.WarningLog.Add(new TestLogEntry(message, fileName, 0, 0, false));
        }

        /// <inheritdoc/>
        public void LogWarning(string message, string fileName, int lineNumber, int linePosition)
        {
            this.WarningLog.Add(new TestLogEntry(message, fileName, lineNumber, linePosition, false));
        }

        /// <summary>
        /// An test entry for the logger.
        /// Corresponds to an error, warning or message
        /// </summary>
        public struct TestLogEntry
        {
            /// <summary>
            /// The log message
            /// </summary>
            public string Message;

            /// <summary>
            /// The file that caused the entry
            /// </summary>
            public string FileName;

            /// <summary>
            /// The line in the file
            /// </summary>
            public int LineNumber;

            /// <summary>
            /// The position in the line
            /// </summary>
            public int LinePosition;

            /// <summary>
            /// Whether the entry was caused from an exception
            /// </summary>
            public bool FromException;

            /// <summary>
            /// Initializes a new instance of the <see cref="TestLogEntry"/> struct.
            /// </summary>
            /// <param name="message">The entry message</param>
            /// <param name="file">The file that caused the entry</param>
            /// <param name="lineNumber">The line in the file</param>
            /// <param name="linePosition">The position in the line</param>
            /// <param name="fromException">Whether the entry was caused by an exception</param>
            public TestLogEntry(string message, string file, int lineNumber, int linePosition, bool fromException)
            {
                this.Message = message;
                this.FileName = file;
                this.LineNumber = lineNumber;
                this.LinePosition = linePosition;
                this.FromException = fromException;
            }
        }
    }
}
