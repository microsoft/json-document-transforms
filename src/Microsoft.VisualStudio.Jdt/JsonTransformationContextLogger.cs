// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.Jdt
{
    using System;
    using System.Runtime.ExceptionServices;
    using Newtonsoft.Json;

    /// <summary>
    /// Logger wrapper for JDT transformations.
    /// </summary>
    internal class JsonTransformationContextLogger
    {
        private readonly IJsonTransformationLogger externalLogger = null;

        private string sourceFile = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonTransformationContextLogger"/> class.
        /// </summary>
        /// <param name="extLogger">External logger to be used. Can be null.</param>
        internal JsonTransformationContextLogger(IJsonTransformationLogger extLogger)
        {
            this.externalLogger = extLogger;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonTransformationContextLogger"/> class.
        /// </summary>
        /// <param name="transformationFile">The file that specifies the transformations.</param>
        /// <param name="extLogger">External logger to be used. Can be null.</param>
        internal JsonTransformationContextLogger(string transformationFile, IJsonTransformationLogger extLogger)
            : this(extLogger)
        {
            if (string.IsNullOrEmpty(transformationFile))
            {
                throw new ArgumentNullException(nameof(transformationFile));
            }

            this.TransformFile = transformationFile;
        }

        /// <summary>
        /// Gets or sets the source file of the current transformation.
        /// </summary>
        internal string SourceFile
        {
            get
            {
                return this.sourceFile ?? Resources.DefaultSourceFileName;
            }

            set
            {
                this.sourceFile = value;
            }
        }

        /// <summary>
        /// Gets the transformation file of the current transformation.
        /// </summary>
        internal string TransformFile { get; } = Resources.DefaultTransformFileName;

        /// <summary>
        /// Gets a value indicating whether the logger has logged errrors.
        /// </summary>
        internal bool HasLoggedErrors { get; private set; }

        /// <summary>
        /// Logs an error from an internal exception.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        internal void LogErrorFromException(Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            // Only log if an external logger has been provided
            if (this.externalLogger != null)
            {
                this.HasLoggedErrors = true;

                // First, attempt to convert to a JdtException that contains lineinfo and error location
                JdtException jdtException = exception as JdtException;
                if (jdtException != null)
                {
                    this.externalLogger.LogErrorFromException(jdtException, this.LocationPath(jdtException.Location), jdtException.LineNumber, jdtException.LinePosition);
                }
                else
                {
                    // JsonReader exceptions are caused by loading errors and contain file and line info
                    JsonReaderException readerException = exception as JsonReaderException;
                    if (readerException != null)
                    {
                        this.externalLogger.LogErrorFromException(readerException, readerException.Path, readerException.LineNumber, readerException.LinePosition);
                    }
                    else
                    {
                        // If the exception does not have any additional info on it
                        this.externalLogger.LogErrorFromException(exception);
                    }
                }
            }
            else
            {
                ExceptionDispatchInfo.Capture(exception).Throw();
            }
        }

        /// <summary>
        /// Logs a warning according to the line info.
        /// </summary>
        /// <param name="message">The warning message.</param>
        /// <param name="location">The file that caused the warning.</param>
        /// <param name="lineInfo">The information of the line that caused the warning.</param>
        internal void LogWarning(string message, ErrorLocation location, IJsonLineInfo lineInfo)
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (this.externalLogger != null)
            {
                if (lineInfo != null && lineInfo.HasLineInfo())
                {
                    this.externalLogger.LogWarning(message, this.LocationPath(location), lineInfo.LineNumber, lineInfo.LinePosition);
                }
                else
                {
                    this.externalLogger.LogWarning(message, this.LocationPath(location));
                }
            }
        }

        private string LocationPath(ErrorLocation location)
        {
            switch (location)
            {
                case ErrorLocation.Source:
                    return this.SourceFile;
                case ErrorLocation.Transform:
                    return this.TransformFile;
                default:
                    return null;
            }
        }
    }
}
