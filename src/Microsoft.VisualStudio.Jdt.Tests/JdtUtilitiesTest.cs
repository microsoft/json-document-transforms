// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License license. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.Jdt.Tests
{
    using Microsoft.VisualStudio.Jdt;
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
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("string")]
        [InlineData("jdt.Verb")]
        [InlineData("@jdtverb")]
        [InlineData("@jdt")]
        [InlineData("@JDT.WrongCase")]
        public void IsJdtSyntaxInvalid(string key)
        {
            Assert.False(JdtUtilities.IsJdtSyntax(key));
        }

        /// <summary>
        /// Tests <see cref="JdtUtilities.IsJdtSyntax(string)"/> with valid JSON syntax.
        /// </summary>
        /// <param name="key">Key to test.</param>
        [Theory]
        [InlineData("@jdt.NotAVerb")]
        [InlineData("@jdt.Remove")]
        [InlineData("@jdt.merge")]
        [InlineData("@jdt.")]
        [InlineData("@jdt.  ")]
        public void IsJdtSyntaxValid(string key)
        {
            Assert.True(JdtUtilities.IsJdtSyntax(key));
        }

        /// <summary>
        /// Tests <see cref="JdtUtilities.GetJdtSyntax(string)"/> with invalid JSON syntax.
        /// </summary>
        /// <param name="key">Key to test.</param>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("string")]
        [InlineData("jdt.Verb")]
        [InlineData("@jdtverb")]
        [InlineData("@jdt")]
        [InlineData("@JDT.WrongCase")]
        public void GetInvalidJdtSyntax(string key)
        {
            Assert.Null(JdtUtilities.GetJdtSyntax(key));
        }

        /// <summary>
        /// Tests <see cref="JdtUtilities.GetJdtSyntax(string)"/> with valid JSON syntax.
        /// </summary>
        [Fact]
        public void GetValidJdtSyntax()
        {
            Assert.Equal(JdtUtilities.GetJdtSyntax("@jdt."), string.Empty);
            Assert.Equal(" ", JdtUtilities.GetJdtSyntax("@jdt. "));
            Assert.Equal("verb", JdtUtilities.GetJdtSyntax("@jdt.verb"));
            Assert.Equal("NotAVerb", JdtUtilities.GetJdtSyntax("@jdt.NotAVerb"));
        }
    }
}
