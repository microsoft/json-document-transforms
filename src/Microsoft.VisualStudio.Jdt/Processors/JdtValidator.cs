// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.Jdt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Validates the JDT verbs in the transformation.
    /// </summary>
    internal class JdtValidator : JdtProcessor
    {
        /// <summary>
        /// Gets set of the valid verbs for the transformation.
        /// </summary>
        public HashSet<string> ValidVerbs { get; } = new HashSet<string>();

        /// <inheritdoc/>
        public override string Verb { get; } = null;

        /// <inheritdoc/>
        internal override void Process(JObject source, JObject transform, JsonTransformationContextLogger logger)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (transform == null)
            {
                throw new ArgumentNullException(nameof(transform));
            }

            foreach (JProperty transformNode in transform.Properties()
                .Where(p => JdtUtilities.IsJdtSyntax(p.Name)))
            {
                string verb = JdtUtilities.GetJdtSyntax(transformNode.Name);
                if (verb != null)
                {
                    if (!this.ValidVerbs.Contains(verb))
                    {
                        throw JdtException.FromLineInfo(string.Format(Resources.ErrorMessage_InvalidVerb, verb), ErrorLocation.Transform, transformNode);
                    }
                }
            }

            this.Successor.Process(source, transform, logger);
        }
    }
}
