// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License license. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.Jdt.Tests
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Mock logger to test <see cref="JsonTransformation"/>.
    /// </summary>
    public class JsonTransformationTestLogger : IJsonTransformationLogger
    {
        /// <summary>
        /// Gets the error log.
        /// </summary>
        public List<TestLogEntry> ErrorLog { get; } = new List<TestLogEntry>();

        /// <summary>
        /// Gets the warning log.
        /// </summary>
        public List<TestLogEntry> WarningLog { get; } = new List<TestLogEntry>();

        /// <summary>
        /// Gets the message log.
        /// </summary>
        public List<TestLogEntry> MessageLog { get; } = new List<TestLogEntry>();

        /// <inheritdoc/>
        public void LogError(string message)
        {
            this.ErrorLog.Add(new TestLogEntry()
            {
                Message = message,
                FromException = false,
            });
        }

        /// <inheritdoc/>
        public void LogError(string message, string fileName, int lineNumber, int linePosition)
        {
            this.ErrorLog.Add(new TestLogEntry()
            {
                Message = message,
                FileName = fileName,
                LineNumber = lineNumber,
                LinePosition = linePosition,
                FromException = false,
            });
        }

        /// <inheritdoc/>
        public void LogErrorFromException(Exception ex)
        {
            this.ErrorLog.Add(new TestLogEntry()
            {
                Message = ex.Message,
                FromException = false,
            });
        }

        /// <inheritdoc/>
        public void LogErrorFromException(Exception ex, string fileName, int lineNumber, int linePosition)
        {
            this.ErrorLog.Add(new TestLogEntry()
            {
                Message = ex.Message,
                FileName = fileName,
                LineNumber = lineNumber,
                LinePosition = linePosition,
                FromException = true,
            });
        }

        /// <inheritdoc/>
        public void LogMessage(string message)
        {
            this.MessageLog.Add(new TestLogEntry()
            {
                Message = message,
                FromException = false,
            });
        }

        /// <inheritdoc/>
        public void LogMessage(string message, string fileName, int lineNumber, int linePosition)
        {
            this.MessageLog.Add(new TestLogEntry()
            {
                Message = message,
                FileName = fileName,
                LineNumber = lineNumber,
                LinePosition = linePosition,
                FromException = false,
            });
        }

        /// <inheritdoc/>
        public void LogWarning(string message)
        {
            this.WarningLog.Add(new TestLogEntry()
            {
                Message = message,
                FromException = false,
            });
        }

        /// <inheritdoc/>
        public void LogWarning(string message, string fileName)
        {
            this.WarningLog.Add(new TestLogEntry()
            {
                Message = message,
                FileName = fileName,
                LineNumber = 0,
                LinePosition = 0,
                FromException = false,
            });
        }

        /// <inheritdoc/>
        public void LogWarning(string message, string fileName, int lineNumber, int linePosition)
        {
            this.WarningLog.Add(new TestLogEntry()
            {
                Message = message,
                FileName = fileName,
                LineNumber = lineNumber,
                LinePosition = linePosition,
                FromException = false,
            });
        }

        /// <summary>
        /// An test entry for the logger.
        /// Corresponds to an error, warning or message.
        /// </summary>
        public struct TestLogEntry
        {
            /// <summary>
            /// Gets or sets the log message.
            /// </summary>
            public string Message { get; set; }

            /// <summary>
            /// Gets or sets the file that caused the entry.
            /// </summary>
            public string FileName { get; set; }

            /// <summary>
            /// Gets or sets the line in the file.
            /// </summary>
            public int LineNumber { get; set; }

            /// <summary>
            /// Gets or sets the position in the line.
            /// </summary>
            public int LinePosition { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether whether the entry was caused from an exception.
            /// </summary>
            public bool FromException { get; set; }
        }
    }
}
