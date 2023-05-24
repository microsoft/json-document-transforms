// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.Jdt
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Utilities class for handling JSON files.
    /// </summary>
    public static class JdtUtilities
    {
        /// <summary>
        /// The prefix for all JDT syntax.
        /// </summary>
        internal const string JdtSyntaxPrefix = "@jdt.";

        /// <summary>
        /// The cached line info handling to use, based on Newtonsoft.Json version
        /// https://github.com/JamesNK/Newtonsoft.Json/issues/1249.
        /// </summary>
        private static LineInfoHandling? lineInfoHandling = null;

        /// <summary>
        /// Wheter the given key corresponds to a JDT verb.
        /// </summary>
        /// <param name="key">The JSON key to analyze.</param>
        /// <returns>True if the key corresponds to a verb.</returns>
        public static bool IsJdtSyntax(string key)
        {
            // If the key is empty of does not start with the correct prefix,
            // it is not a valid verb
            return !string.IsNullOrEmpty(key) && key.StartsWith(JdtSyntaxPrefix);
        }

        /// <summary>
        /// Gets the JDT syntax in the key.
        /// </summary>
        /// <param name="key">The JDT key, in the correct syntax.</param>
        /// <returns>The string property. Null if the property does is not JDT syntax.</returns>
        public static string GetJdtSyntax(string key)
        {
            // If the key does not start with the correct prefix, it is not a JDT verb
            // If it is a JDT verb, remove the prefix
            return IsJdtSyntax(key) ? key.Substring(JdtSyntaxPrefix.Length) : null;
        }

        /// <summary>
        /// Gets the <see cref="LineInfoHandling"/> depending on the Newtonsoft version
        /// This is due to a bug in previous versions of JSON.Net that loaded line info on ignore and vice-versa
        /// See https://github.com/JamesNK/Newtonsoft.Json/pull/1250.
        /// </summary>
        /// <returns>The correct line info handling.</returns>
        internal static LineInfoHandling GetLineInfoHandling()
        {
            if (lineInfoHandling == null)
            {
                try
                {
                    string newtonsoftLocation = typeof(JObject).GetTypeInfo().Assembly.Location;
                    FileVersionInfo fileVersion = FileVersionInfo.GetVersionInfo(newtonsoftLocation);

                    // The version the line ending bug was fixed in. We want to target a lower
                    // version of Newtonsoft to give consumers more flexibility in what version
                    // they consume theirselves. However, we still want line endings to work in
                    // both new and old versions.
                    if (new Version(fileVersion.ProductVersion) < new Version("10.0.2"))
                    {
                        lineInfoHandling = LineInfoHandling.Ignore;
                    }
                    else
                    {
                        lineInfoHandling = LineInfoHandling.Load;
                    }
                }
                catch (Exception e) when (!e.IsCriticalException())
                {
                    // we will default to the "correct" value in the instance of any issues.
                    lineInfoHandling = LineInfoHandling.Load;
                }
            }

            return lineInfoHandling.Value;
        }
    }
}
