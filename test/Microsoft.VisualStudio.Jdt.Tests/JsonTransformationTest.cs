// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License license. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.Jdt.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Xunit;

    /// <summary>
    /// Test class for <see cref="JsonTransformation"/>.
    /// </summary>
    public class JsonTransformationTest
    {
        private static readonly string SimpleSourceString = @"{ 'A': 1 }";

        private readonly JsonTransformationTestLogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonTransformationTest"/> class.
        /// </summary>
        public JsonTransformationTest()
        {
            // xUnit creates a new instance of the class for each test, so a new logger is created
            this.logger = new JsonTransformationTestLogger();
        }

        /// <summary>
        /// Tests the error caused when an invalid verb is found.
        /// </summary>
        [Fact]
        public void InvalidVerb()
        {
            string transformString = @"{ 
                                         '@jdt.invalid': false 
                                       }";

            this.TryTransformTest(SimpleSourceString, transformString, false);

            Assert.Empty(this.logger.MessageLog);
            Assert.Empty(this.logger.WarningLog);

            // The error should be where at the location of the invalid verb
            LogHasSingleEntry(this.logger.ErrorLog, ErrorLocation.Transform.ToString(), 2, 56, true);
        }

        /// <summary>
        /// Tests the error caused by a verb having an invalid value.
        /// </summary>
        [Fact]
        public void InvalidVerbValue()
        {
            string transformString = @"{ 
                                         '@jdt.remove': 10 
                                       }";

            this.TryTransformTest(SimpleSourceString, transformString, false);

            Assert.Empty(this.logger.MessageLog);
            Assert.Empty(this.logger.WarningLog);

            // The error location should be at the location of the invalid value
            LogHasSingleEntry(this.logger.ErrorLog, ErrorLocation.Transform.ToString(), 2, 58, true);
        }

        /// <summary>
        /// Tests the error caused when an invalid attribute is found within a verb.
        /// </summary>
        [Fact]
        public void InvalidAttribute()
        {
            string transformString = @"{ 
                                         '@jdt.replace': { 
                                           '@jdt.invalid': false 
                                         } 
                                       }";

            this.TryTransformTest(SimpleSourceString, transformString, false);

            Assert.Empty(this.logger.MessageLog);
            Assert.Empty(this.logger.WarningLog);

            // The error location should be at the position of the invalid attribute
            LogHasSingleEntry(this.logger.ErrorLog, ErrorLocation.Transform.ToString(), 3, 58, true);
        }

        /// <summary>
        /// Tests the error caused when a required attribute is not found.
        /// </summary>
        [Fact]
        public void MissingAttribute()
        {
            string transformString = @"{ 
                                         '@jdt.rename': { 
                                           '@jdt.path': 'A' 
                                         } 
                                       }";

            this.TryTransformTest(SimpleSourceString, transformString, false);

            Assert.Empty(this.logger.MessageLog);
            Assert.Empty(this.logger.WarningLog);

            // The error location should be at the beginning of the object with the missing attribute
            LogHasSingleEntry(this.logger.ErrorLog, ErrorLocation.Transform.ToString(), 2, 57, true);
        }

        /// <summary>
        /// Tests the error caused when a verb object contains attributes and other objects.
        /// </summary>
        [Fact]
        public void MixedAttributes()
        {
            string transformString = @"{ 
                                         '@jdt.rename': { 
                                           '@jdt.path': 'A',
                                           '@jdt.value': 'Astar',
                                           'NotAttribute': true
                                         } 
                                       }";

            this.TryTransformTest(SimpleSourceString, transformString, false);

            Assert.Empty(this.logger.MessageLog);
            Assert.Empty(this.logger.WarningLog);

            // The error location should be at the beginning of the object with the mixed attribute
            LogHasSingleEntry(this.logger.ErrorLog, ErrorLocation.Transform.ToString(), 2, 57, true);
        }

        /// <summary>
        /// Tests the error caused when an attribute has an incorrect value.
        /// </summary>
        [Fact]
        public void WrongAttributeValue()
        {
            string transformString = @"{
                                         '@jdt.remove': { 
                                           '@jdt.path': false
                                         } 
                                       }";

            this.TryTransformTest(SimpleSourceString, transformString, false);

            Assert.Empty(this.logger.MessageLog);
            Assert.Empty(this.logger.WarningLog);

            // The error location should be at the position of the invalid value
            LogHasSingleEntry(this.logger.ErrorLog, ErrorLocation.Transform.ToString(), 3, 61, true);
        }

        /// <summary>
        /// Tests the error caused when a path attribute returns no result.
        /// </summary>
        [Fact]
        public void RemoveNonExistantNode()
        {
            string transformString = @"{
                                         '@jdt.remove': { 
                                           '@jdt.path': 'B'
                                         } 
                                       }";

            this.TryTransformTest(SimpleSourceString, transformString, true);

            Assert.Empty(this.logger.MessageLog);
            Assert.Empty(this.logger.ErrorLog);

            // The warning location should be at the position of the path value that yielded no results
            LogHasSingleEntry(this.logger.WarningLog, ErrorLocation.Transform.ToString(), 3, 59, false);
        }

        /// <summary>
        /// Tests the error caused when attempting to remove the root node.
        /// </summary>
        [Fact]
        public void RemoveRoot()
        {
            string transformString = @"{
                                         '@jdt.remove': true 
                                       }";

            this.TryTransformTest(SimpleSourceString, transformString, false);

            Assert.Empty(this.logger.MessageLog);
            Assert.Empty(this.logger.WarningLog);

            // The error location should be at the position of the remove value
            LogHasSingleEntry(this.logger.ErrorLog, ErrorLocation.Transform.ToString(), 2, 60, true);
        }

        /// <summary>
        /// Tests the error when a rename value is invalid.
        /// </summary>
        [Fact]
        public void InvalidRenameValue()
        {
            string transformString = @"{
                                         '@jdt.rename': { 
                                           'A': 10
                                         } 
                                       }";

            this.TryTransformTest(SimpleSourceString, transformString, false);

            Assert.Empty(this.logger.MessageLog);
            Assert.Empty(this.logger.WarningLog);

            // The error location should be at the position of the rename property
            LogHasSingleEntry(this.logger.ErrorLog, ErrorLocation.Transform.ToString(), 3, 47, true);
        }

        /// <summary>
        /// Tests the error caused when attempting to rename a non-existant node.
        /// </summary>
        [Fact]
        public void RenameNonExistantNode()
        {
            string transformString = @"{
                                         '@jdt.rename': { 
                                           'B': 'Bstar'
                                         } 
                                       }";

            this.TryTransformTest(SimpleSourceString, transformString, true);

            Assert.Empty(this.logger.MessageLog);
            Assert.Empty(this.logger.ErrorLog);

            // The position of the warning should be a the beginning of the rename property
            LogHasSingleEntry(this.logger.WarningLog, ErrorLocation.Transform.ToString(), 3, 47, false);
        }

        /// <summary>
        /// Test the error when attempting to replace the root with a non-object token.
        /// </summary>
        [Fact]
        public void ReplaceRoot()
        {
            string transformString = @"{
                                         '@jdt.replace': 10
                                       }";

            this.TryTransformTest(SimpleSourceString, transformString, false);

            Assert.Empty(this.logger.MessageLog);
            Assert.Empty(this.logger.WarningLog);

            // The position of the error should be at the value of replace that caused it
            LogHasSingleEntry(this.logger.ErrorLog, ErrorLocation.Transform.ToString(), 2, 59, true);
        }

        /// <summary>
        /// Tests that an exception is thrown when <see cref="JsonTransformation.Apply(Stream)"/> is called.
        /// </summary>
        [Fact]
        public void ThrowAndLogException()
        {
            string transformString = @"{ 
                                         '@jdt.invalid': false 
                                       }";
            using (Stream transformStream = this.GetStreamFromString(transformString))
            using (Stream sourceStream = this.GetStreamFromString(SimpleSourceString))
            {
                JsonTransformation transform = new JsonTransformation(transformStream, this.logger);
                Exception exception = Record.Exception(() => transform.Apply(sourceStream));
                Assert.NotNull(exception);
                Assert.IsType<JdtException>(exception);
                var jdtException = exception as JdtException;
                Assert.Contains("invalid", jdtException.Message);
                Assert.Equal(ErrorLocation.Transform, jdtException.Location);
                Assert.Equal(2, jdtException.LineNumber);
                Assert.Equal(56, jdtException.LinePosition);
            }
        }

        /// <summary>
        /// Tests that a transformation succeeds even if the source and transform files are read-only.
        /// </summary>
        [Fact]
        public void ReadOnly()
        {
            const string TransformSourceString = @"{
                                                        '@jdt.rename' : {
                                                            'A' : 'Astar'
                                                        }
                                                    }";

            // create temporary files to use for the source and transform
            using (ReadOnlyTempFile tempSourceFilePath = new ReadOnlyTempFile(SimpleSourceString))
            using (ReadOnlyTempFile tempTransformFilePath = new ReadOnlyTempFile(TransformSourceString))
            {
                // apply transform
                JsonTransformation transformation = new JsonTransformation(tempTransformFilePath.FilePath, this.logger);

                this.AssertTransformSucceeds(() => transformation.Apply(tempSourceFilePath.FilePath), true);
            }
        }

        private static void LogHasSingleEntry(List<JsonTransformationTestLogger.TestLogEntry> log, string fileName, int lineNumber, int linePosition, bool fromException)
        {
            Assert.Single(log);
            JsonTransformationTestLogger.TestLogEntry errorEntry = log.Single();
            Assert.Equal(fileName, errorEntry.FileName);
            Assert.Equal(lineNumber, errorEntry.LineNumber);
            Assert.Equal(linePosition, errorEntry.LinePosition);
            Assert.Equal(fromException, errorEntry.FromException);
        }

        private void TryTransformTest(string sourceString, string transformString, bool shouldTransformSucceed)
        {
            using (Stream transformStream = this.GetStreamFromString(transformString))
            using (Stream sourceStream = this.GetStreamFromString(sourceString))
            {
                JsonTransformation transform = new JsonTransformation(transformStream, this.logger);

                this.AssertTransformSucceeds(() => transform.Apply(sourceStream), shouldTransformSucceed);
            }
        }

        private void AssertTransformSucceeds(Func<Stream> applyTransformMethod, bool shouldTransformSucceed)
        {
            Stream result = null;

            Exception exception = Record.Exception(() => result = applyTransformMethod());

            if (shouldTransformSucceed)
            {
                Assert.NotNull(result);
                Assert.Null(exception);
            }
            else
            {
                Assert.Null(result);
                Assert.NotNull(exception);
                Assert.IsType<JdtException>(exception);
            }
        }

        private Stream GetStreamFromString(string s)
        {
            MemoryStream stringStream = new MemoryStream();
            StreamWriter stringWriter = new StreamWriter(stringStream);
            stringWriter.Write(s);
            stringWriter.Flush();
            stringStream.Position = 0;

            return stringStream;
        }
    }
}
