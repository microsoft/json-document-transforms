// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License license. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.Jdt.Tests
{
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.VisualStudio.Jdt;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Xunit;

    /// <summary>
    /// Test class for JDT transformations.
    /// </summary>
    public class TransformTest
    {
        // Directory for test inputs, that are JSON files
        private static readonly string TestInputDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + "\\test\\Microsoft.VisualStudio.Jdt.Tests\\Inputs\\";

        // Directory for Default transformation tests
        private static readonly string DefaultTestDirectory = TestInputDirectory + "Default\\";

        // Directory for Remove transformation test
        private static readonly string RemoveTestDirectory = TestInputDirectory + "Remove\\";

        // Directory for Rename transformation test
        private static readonly string RenameTestDirectory = TestInputDirectory + "Rename\\";

        // Directory for Replace transformation test
        private static readonly string ReplaceTestDirectory = TestInputDirectory + "Replace\\";

        // Directory for Merge transformation test
        private static readonly string MergeTestDirectory = TestInputDirectory + "Merge\\";

        /// <summary>
        /// Gets inputs for the Default transformation.
        /// </summary>
        public static IEnumerable<object[]> GetDefaultInputs
        {
            get
            {
                // Gets inputs from Default transformation test directory
                return GetInputs(DefaultTestDirectory);
            }
        }

        /// <summary>
        /// Gets inputs for the Remove transformation.
        /// </summary>
        public static IEnumerable<object[]> GetRemoveInputs
        {
            get
            {
                return GetInputs(RemoveTestDirectory);
            }
        }

        /// <summary>
        /// Gets inputs for the Rename transformation.
        /// </summary>
        public static IEnumerable<object[]> GetRenameInputs
        {
            get
            {
                return GetInputs(RenameTestDirectory);
            }
        }

        /// <summary>
        /// Gets inputs for the Replace transformation.
        /// </summary>
        public static IEnumerable<object[]> GetReplaceInputs
        {
            get
            {
                return GetInputs(ReplaceTestDirectory);
            }
        }

        /// <summary>
        /// Gets inputs for the Merge transformation.
        /// </summary>
        public static IEnumerable<object[]> GetMergeInputs
        {
            get
            {
                return GetInputs(MergeTestDirectory);
            }
        }

        /// <summary>
        /// Tests the Default transformation.
        /// </summary>
        /// <param name="testFileName">Name of the test being performed.
        /// Corresponds to a group of files in the input folder.</param>
        [Theory]
        [MemberData(nameof(GetDefaultInputs))]
        public void Default(string testFileName)
        {
            BaseTransformTest(DefaultTestDirectory, testFileName);
        }

        /// <summary>
        /// Tests the Remove transformation.
        /// </summary>
        /// <param name="testFileName">Name of the test being performed.
        /// Corresponds to a group of files in the input folder.</param>
        [Theory]
        [MemberData(nameof(GetRemoveInputs))]
        public void Remove(string testFileName)
        {
            BaseTransformTest(RemoveTestDirectory, testFileName);
        }

        /// <summary>
        /// Tests the Rename transformation.
        /// </summary>
        /// <param name="testFileName">Name of the test being performed.
        /// Corresponds to a group of files in the input folder.</param>
        [Theory]
        [MemberData(nameof(GetRenameInputs))]
        public void Rename(string testFileName)
        {
            BaseTransformTest(RenameTestDirectory, testFileName);
        }

        /// <summary>
        /// Tests the Replace transformation.
        /// </summary>
        /// <param name="testFileName">Name of the test being performed.
        /// Corresponds to a group of files in the input folder.</param>
        [Theory]
        [MemberData(nameof(GetReplaceInputs))]
        public void Replace(string testFileName)
        {
            BaseTransformTest(ReplaceTestDirectory, testFileName);
        }

        /// <summary>
        /// Tests the Merge transformation.
        /// </summary>
        /// <param name="testFileName">Name of the test being performed.
        /// Corresponds to a group of files in the input folder.</param>
        [Theory]
        [MemberData(nameof(GetMergeInputs))]
        public void Merge(string testFileName)
        {
            BaseTransformTest(MergeTestDirectory, testFileName);
        }

        private static IEnumerable<object[]> GetInputs(string testDirectory)
        {
            // Each transform file in the test input folder will correspond to one test
            foreach (string file in Directory.GetFiles(testDirectory, "*.Transform.json"))
            {
                // Transform files are called {TestCategory}.{TestName}.Transform.json
                // Expected results files are called {TestCategory}.{TestName}.Expected.json
                // Source files are called {TestCategory}.Source.json
                // This returns {TestCategory}.{TestName} so the test can find the files
                yield return new object[] { Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(file)) };
            }
        }

        private static void BaseTransformTest(string inputsDirectory, string testName)
        {
            // Removes the test name to find the source file
            string sourceName = Path.GetFileNameWithoutExtension(testName);

            var transformation = new JsonTransformation(inputsDirectory + testName + ".Transform.json");

            // Read the resulting stream into a JObject to compare
            using (Stream result = transformation.Apply(inputsDirectory + sourceName + ".Source.json"))
            using (StreamReader streamReader = new StreamReader(result))
            using (JsonTextReader jsonReader = new JsonTextReader(streamReader))
            {
                var expected = JObject.Parse(File.ReadAllText(inputsDirectory + testName + ".Expected.json"));

                var transformed = JObject.Load(jsonReader);

                Assert.True(JObject.DeepEquals(expected, transformed));
            }
        }
    }
}
