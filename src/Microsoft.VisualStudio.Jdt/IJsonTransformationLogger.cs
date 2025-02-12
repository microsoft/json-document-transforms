// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.Jdt
{
    using System;

    /// <summary>
    /// External logger.
    /// </summary>
    public interface IJsonTransformationLogger
    {
        /// <summary>
        /// Logs a message.
        /// </summary>
        /// <param name="message">The message text.</param>
        void LogMessage(string message);

        /// <summary>
        /// Logs a message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="fileName">The full path to the file that caused the message. Can be null.</param>
        /// <param name="lineNumber">The line that caused the message.</param>
        /// <param name="linePosition">The position in the line that caused the message.</param>
        void LogMessage(string message, string fileName, int lineNumber, int linePosition);

        /// <summary>
        /// Logs a warning.
        /// </summary>
        /// <param name="message">The warning message.</param>
        void LogWarning(string message);

        /// <summary>
        /// Logs a warning.
        /// </summary>
        /// <param name="message">The warning message.</param>
        /// <param name="fileName">The full path to the file that caused the warning. Can be null.</param>
        void LogWarning(string message, string fileName);

        /// <summary>
        /// Logs a warning.
        /// </summary>
        /// <param name="message">The warning message.</param>
        /// <param name="fileName">The full path to the file that caused the warning. Can be null.</param>
        /// <param name="lineNumber">The line that caused the warning.</param>
        /// <param name="linePosition">The position in the line that caused the warning.</param>
        void LogWarning(string message, string fileName, int lineNumber, int linePosition);

        /// <summary>
        /// Logs an error.
        /// </summary>
        /// <param name="message">The error message.</param>
        void LogError(string message);

        /// <summary>
        /// Logs an error.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="fileName">The full path to the file that caused the exception. Can be null.</param>
        /// <param name="lineNumber">The line that caused the exception.</param>
        /// <param name="linePosition">The position in the line that caused the exception.</param>
        void LogError(string message, string fileName, int lineNumber, int linePosition);

        /// <summary>
        /// Logs an error from an exception.
        /// </summary>
        /// <param name="ex">The exception that caused the error.</param>
        void LogErrorFromException(Exception ex);

        /// <summary>
        /// Logs an error from an exception.
        /// </summary>
        /// <param name="ex">The exception that caused the error.</param>
        /// <param name="fileName">The full path to the file that caused the exception. Can be null.</param>
        /// <param name="lineNumber">The line that caused the exception.</param>
        /// <param name="linePosition">The position in the line that caused the exception.</param>
        void LogErrorFromException(Exception ex, string fileName, int lineNumber, int linePosition);
    }
}
