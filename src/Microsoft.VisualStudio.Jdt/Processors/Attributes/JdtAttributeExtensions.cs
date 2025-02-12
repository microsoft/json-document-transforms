// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.Jdt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Implements extensions for <see cref="JdtAttributes"/>.
    /// </summary>
    internal static class JdtAttributeExtensions
    {
        /// <summary>
        /// Get the full name of an attribute, with the JDT prefix.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <returns>A string with the full name of the requested attribute.</returns>
        internal static string FullName(this JdtAttributes attribute)
        {
            if (attribute == JdtAttributes.None)
            {
                return JdtUtilities.JdtSyntaxPrefix;
            }

            return JdtUtilities.JdtSyntaxPrefix + Enum.GetName(typeof(JdtAttributes), attribute).ToLower();
        }

        /// <summary>
        /// Gets a <see cref="JdtAttributes"/> from an enumerable based on name.
        /// </summary>
        /// <param name="collection">The enumerable to search.</param>
        /// <param name="name">The name of the attribute.</param>
        /// <returns>The attribute with that name of <see cref="JdtAttributes.None"/> if no attribute was found.</returns>
        internal static JdtAttributes GetByName(this IEnumerable<JdtAttributes> collection, string name)
        {
            // The default value for the enum is 0, which is None
            return collection.SingleOrDefault(a => a.FullName().Equals(name));
        }
    }
}
