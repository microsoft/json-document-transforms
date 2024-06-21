// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.Jdt
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Memory logger.
    /// </summary>
    public class JsonTransformationMemoryLogger : IJsonTransformationLogger
    {
        /// <summary>
        /// Gets the error log.
        /// </summary>
        public List<LogEntry> ErrorLog { get; } = new List<LogEntry>();

        /// <summary>
        /// Gets the warning log.
        /// </summary>
        public List<LogEntry> WarningLog { get; } = new List<LogEntry>();

        /// <summary>
        /// Gets the message log.
        /// </summary>
        public List<LogEntry> MessageLog { get; } = new List<LogEntry>();

        /// <inheritdoc/>
        public void LogError(string message)
        {
            this.ErrorLog.Add(new LogEntry()
            {
                Message = message,
            });
        }

        /// <inheritdoc/>
        public void LogError(string message, string fileName, int lineNumber, int linePosition)
        {
            this.ErrorLog.Add(new LogEntry()
            {
                Message = message,
                FileName = fileName,
                LineNumber = lineNumber,
                LinePosition = linePosition,
            });
        }

        /// <inheritdoc/>
        public void LogErrorFromException(Exception ex)
        {
            this.ErrorLog.Add(new LogEntry()
            {
                Exception = ex,
                Message = ex.Message,
            });
        }

        /// <inheritdoc/>
        public void LogErrorFromException(Exception ex, string fileName, int lineNumber, int linePosition)
        {
            this.ErrorLog.Add(new LogEntry()
            {
                Exception = ex,
                Message = ex.Message,
                FileName = fileName,
                LineNumber = lineNumber,
                LinePosition = linePosition,
            });
        }

        /// <inheritdoc/>
        public void LogMessage(string message)
        {
            this.MessageLog.Add(new LogEntry()
            {
                Message = message,
            });
        }

        /// <inheritdoc/>
        public void LogMessage(string message, string fileName, int lineNumber, int linePosition)
        {
            this.MessageLog.Add(new LogEntry()
            {
                Message = message,
                FileName = fileName,
                LineNumber = lineNumber,
                LinePosition = linePosition,
            });
        }

        /// <inheritdoc/>
        public void LogWarning(string message)
        {
            this.WarningLog.Add(new LogEntry()
            {
                Message = message,
            });
        }

        /// <inheritdoc/>
        public void LogWarning(string message, string fileName)
        {
            this.WarningLog.Add(new LogEntry()
            {
                Message = message,
                FileName = fileName,
                LineNumber = 0,
                LinePosition = 0,
            });
        }

        /// <inheritdoc/>
        public void LogWarning(string message, string fileName, int lineNumber, int linePosition)
        {
            this.WarningLog.Add(new LogEntry()
            {
                Message = message,
                FileName = fileName,
                LineNumber = lineNumber,
                LinePosition = linePosition,
            });
        }

        /// <summary>
        /// A simple entry for the logger.
        /// Corresponds to an error, warning or message.
        /// </summary>
        public struct LogEntry
        {
            /// <summary>
            /// Gets or sets the log exception.
            /// </summary>
            public Exception Exception { get; set; }

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
        }
    }
}
