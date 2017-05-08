// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.Jdt.Tests
{
    using System;
    using System.Text;

    /// <summary>
    /// Mock logger to test <see cref="JsonTransformation"/>
    /// </summary>
    public class JsonTransformationTestLogger : IJsonTransformationLogger
    {
        private readonly StringBuilder errorLog = new StringBuilder();

        private readonly StringBuilder warningLog = new StringBuilder();

        private readonly StringBuilder messageLog = new StringBuilder();

        /// <summary>
        /// Gets the text from the error log
        /// </summary>
        public string ErrorLogText
        {
            get
            {
                return this.errorLog.ToString();
            }
        }

        /// <summary>
        /// Gets the text from the warning log
        /// </summary>
        public string WarningLogText
        {
            get
            {
                return this.warningLog.ToString();
            }
        }

        /// <summary>
        /// Gets the text from the message log
        /// </summary>
        public string MessageLogText
        {
            get
            {
                return this.messageLog.ToString();
            }
        }

        /// <inheritdoc/>
        public void LogError(string message)
        {
            this.errorLog.AppendLine(message);
        }

        /// <inheritdoc/>
        public void LogError(string message, string fileName, int lineNumber, int linePosition)
        {
            this.errorLog.AppendLine(this.BuildLine(message, fileName, lineNumber, linePosition));
        }

        /// <inheritdoc/>
        public void LogErrorFromException(Exception ex)
        {
            this.errorLog.AppendLine($"Exception: {ex.Message}");
        }

        /// <inheritdoc/>
        public void LogErrorFromException(Exception ex, string fileName, int lineNumber, int linePosition)
        {
            this.errorLog.AppendLine(this.BuildLine($"Exception: {ex.Message}", fileName, lineNumber, linePosition));
        }

        /// <inheritdoc/>
        public void LogMessage(string message)
        {
            this.messageLog.AppendLine(message);
        }

        /// <inheritdoc/>
        public void LogWarning(string message)
        {
            this.warningLog.AppendLine(message);
        }

        /// <inheritdoc/>
        public void LogWarning(string message, string fileName)
        {
            this.warningLog.AppendLine($"{message} {fileName}");
        }

        /// <inheritdoc/>
        public void LogWarning(string message, string fileName, int lineNumber, int linePosition)
        {
            this.warningLog.AppendLine(this.BuildLine(message, fileName, lineNumber, linePosition));
        }

        private string BuildLine(string message, string fileName, int lineNumber, int linePosition)
        {
            string line = message;
            if (fileName != null)
            {
                line += " " + fileName;
            }

            line += $" {lineNumber} {linePosition}";

            return line;
        }
    }
}
