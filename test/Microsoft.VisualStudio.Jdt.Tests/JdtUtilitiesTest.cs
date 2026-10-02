// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.Jdt.Tests
{
    using Microsoft.VisualStudio.Jdt;
    using TUnit.Core;
    using Xunit;

    /// <summary>
    /// Test class for <see cref="JdtUtilities"/>.
    /// </summary>
    public class JdtUtilitiesTest
    {
        /// <summary>
        /// Tests <see cref="JdtUtilities.IsJdtSyntax(string)"/> with invalid JSON syntax.
        /// </summary>
        /// <param name="key">Key to test.</param>
        [Test]
        [Arguments(null)]
        [Arguments("")]
        [Arguments("string")]
        [Arguments("jdt.Verb")]
        [Arguments("@jdtverb")]
        [Arguments("@jdt")]
        [Arguments("@JDT.WrongCase")]
        public void IsJdtSyntaxInvalid(string key)
        {
            Assert.False(JdtUtilities.IsJdtSyntax(key));
        }

        /// <summary>
        /// Tests <see cref="JdtUtilities.IsJdtSyntax(string)"/> with valid JSON syntax.
        /// </summary>
        /// <param name="key">Key to test.</param>
        [Test]
        [Arguments("@jdt.NotAVerb")]
        [Arguments("@jdt.Remove")]
        [Arguments("@jdt.merge")]
        [Arguments("@jdt.")]
        [Arguments("@jdt.  ")]
        public void IsJdtSyntaxValid(string key)
        {
            Assert.True(JdtUtilities.IsJdtSyntax(key));
        }

        /// <summary>
        /// Tests <see cref="JdtUtilities.GetJdtSyntax(string)"/> with invalid JSON syntax.
        /// </summary>
        /// <param name="key">Key to test.</param>
        [Test]
        [Arguments(null)]
        [Arguments("")]
        [Arguments("string")]
        [Arguments("jdt.Verb")]
        [Arguments("@jdtverb")]
        [Arguments("@jdt")]
        [Arguments("@JDT.WrongCase")]
        public void GetInvalidJdtSyntax(string key)
        {
            Assert.Null(JdtUtilities.GetJdtSyntax(key));
        }

        /// <summary>
        /// Tests <see cref="JdtUtilities.GetJdtSyntax(string)"/> with valid JSON syntax.
        /// </summary>
        [Test]
        public void GetValidJdtSyntax()
        {
            Assert.Equal(JdtUtilities.GetJdtSyntax("@jdt."), string.Empty);
            Assert.Equal(" ", JdtUtilities.GetJdtSyntax("@jdt. "));
            Assert.Equal("verb", JdtUtilities.GetJdtSyntax("@jdt.verb"));
            Assert.Equal("NotAVerb", JdtUtilities.GetJdtSyntax("@jdt.NotAVerb"));
        }
    }
}
