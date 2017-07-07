// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.Jdt
{
    using System;
    using System.IO;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Represents a transformation based on a JSON file using JDT
    /// </summary>
    public class JsonTransformation
    {
        private readonly JsonTransformationContextLogger logger;

        private JObject transformObject;
        private JsonLoadSettings loadSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonTransformation"/> class.
        /// </summary>
        /// <param name="transformFile">The path to the file that specifies the transformation</param>
        public JsonTransformation(string transformFile)
            : this(transformFile, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonTransformation"/> class with an external logger.
        /// </summary>
        /// <param name="transformFile">The path to the file that specifies the transformation</param>
        /// <param name="logger">The external logger</param>
        public JsonTransformation(string transformFile, IJsonTransformationLogger logger)
        {
            if (string.IsNullOrEmpty(transformFile))
            {
                throw new ArgumentNullException(nameof(transformFile));
            }

            this.logger = new JsonTransformationContextLogger(transformFile, logger);

            using (FileStream transformStream = File.Open(transformFile, FileMode.Open))
            {
                this.SetTransform(transformStream);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonTransformation"/> class.
        /// </summary>
        /// <param name="transform">The stream containing the JSON that specifies the transformation</param>
        public JsonTransformation(Stream transform)
            : this(transform, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonTransformation"/> class with an external logger.
        /// </summary>
        /// <param name="transform">The stream containing the JSON that specifies the transformation</param>
        /// /// <param name="logger">The external logger</param>
        public JsonTransformation(Stream transform, IJsonTransformationLogger logger)
        {
            if (transform == null)
            {
                throw new ArgumentNullException(nameof(transform));
            }

            this.logger = new JsonTransformationContextLogger(logger);

            this.SetTransform(transform);
        }

        /// <summary>
        /// Transforms a JSON object
        /// </summary>
        /// <param name="sourceFile">The object to be transformed</param>
        /// <returns>The stream with the result of the transform</returns>
        public Stream Apply(string sourceFile)
        {
            if (string.IsNullOrEmpty(sourceFile))
            {
                throw new ArgumentNullException(nameof(sourceFile));
            }

            // Open the file as streams and apply the transforms
            using (Stream sourceStream = File.Open(sourceFile, FileMode.Open))
            {
                return this.ApplyWithSourceName(sourceStream, sourceFile);
            }
        }

        /// <summary>
        /// Transforms a JSON object
        /// </summary>
        /// <param name="source">The object to be transformed</param>
        /// <returns>The stream with the result of the transform</returns>
        public Stream Apply(Stream source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return this.ApplyWithSourceName(source, null);
        }

        private Stream ApplyWithSourceName(Stream source, string sourceName)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            this.logger.SourceFile = sourceName;
            Stream result;

            using (StreamReader sourceStreamReader = new StreamReader(source))
            using (JsonTextReader sourceReader = new JsonTextReader(sourceStreamReader))
            {
                result = null;
                JObject sourceObject = null;

                try
                {
                    // The JObject corresponding to the streams with line info
                    sourceObject = JObject.Load(sourceReader, this.loadSettings);

                    // Execute the transforms
                    JdtProcessor.ProcessTransform(sourceObject, this.transformObject, this.logger);

                    // Save the result to a memory stream
                    // Don't close the stream of the streamwriter so data isn't lost
                    // User should handle the close
                    result = new MemoryStream();
                    StreamWriter streamWriter = new StreamWriter(result);
                    JsonTextWriter jsonWriter = new JsonTextWriter(streamWriter)
                    {
                        Formatting = Formatting.Indented
                    };

                    // Writes the changes in the source object to the stream
                    // and resets it so the user can read the stream
                    sourceObject.WriteTo(jsonWriter);
                    streamWriter.Flush();
                    result.Position = 0;

                    return result;
                }
                catch (Exception ex) when (!ex.IsCriticalException())
                {
                    this.logger.LogErrorFromException(ex);
                    throw;
                }
            }
        }

        private void SetTransform(Stream transformStream)
        {
            this.loadSettings = new JsonLoadSettings()
            {
                CommentHandling = CommentHandling.Ignore,
                LineInfoHandling = JdtUtilities.GetLineInfoHandling()
            };

            using (StreamReader transformStreamReader = new StreamReader(transformStream))
            using (JsonTextReader transformReader = new JsonTextReader(transformStreamReader))
            {
                this.transformObject = JObject.Load(transformReader, this.loadSettings);
            }
        }
    }
}
