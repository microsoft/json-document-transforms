// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.Jdt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Defines extension methods used in JDT
    /// </summary>
    internal static class JdtExtensions
    {
        /// <summary>
        /// Gets all the properties within the object that correspond to JDT syntax
        /// </summary>
        /// <param name="objectToSearch">The object to search</param>
        /// <returns>An enumerable of properties that start with the JDT prefix</returns>
        internal static IEnumerable<JProperty> GetJdtProperties(this JObject objectToSearch)
        {
            if (objectToSearch == null)
            {
                throw new ArgumentNullException(nameof(objectToSearch));
            }

            return objectToSearch.Properties().Where(p => JdtUtilities.IsJdtSyntax(p.Name));
        }

        /// <summary>
        /// Checks if an exception is critical
        /// </summary>
        /// <param name="ex">The exception to check</param>
        /// <returns>True if the exception is critical and should not be caught</returns>
        internal static bool IsCriticalException(this Exception ex)
        {
            return ex is NullReferenceException
                || ex is OutOfMemoryException
                || ex is IndexOutOfRangeException
#if NET45
                || ex is StackOverflowException
                || ex is AccessViolationException
                || ex is System.Threading.ThreadAbortException
#endif
                ;
        }

        /// <summary>
        /// Clones a <see cref="JObject"/> preserving the line information
        /// </summary>
        /// <param name="objectToClone">The object to clone</param>
        /// <returns>A clone of the object with its line info</returns>
        internal static JObject CloneWithLineInfo(this JObject objectToClone)
        {
            var loadSettings = new JsonLoadSettings()
            {
                LineInfoHandling = LineInfoHandling.Load
            };

            using (var objectReader = objectToClone.CreateReader())
            {
                return JObject.Load(objectReader, loadSettings);
            }
        }
    }
}
