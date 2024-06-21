// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.Jdt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Expands the simplified inline JDT verbs in the transformation.
    /// </summary>
    internal class JdtExpander : JdtProcessor
    {
        /// <summary>
        /// Gets set of the valid verbs for the transformation.
        /// </summary>
        public HashSet<string> ValidVerbs { get; } = new HashSet<string>();

        /// <inheritdoc/>
        public override string Verb { get; } = null;

        /// <inheritdoc/>
        public override bool Expandable { get; } = false;

        /// <inheritdoc/>
        internal override void Process(JToken source, JObject transform, JsonTransformationContextLogger logger)
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
                .Where(p => JdtUtilities.IsJdtInlineSyntax(p.Name)).ToList())
            {
                string verb = JdtUtilities.GetJdtInlineSyntax(transformNode.Name);
                if (verb != null)
                {
                    if (!this.ValidVerbs.Contains(verb))
                    {
                        throw JdtException.FromLineInfo(string.Format(Resources.ErrorMessage_InvalidInlineVerb, verb), ErrorLocation.Transform, transformNode);
                    }

                    var newValue = new JObject(
                        new JProperty(
                            JdtUtilities.JdtSyntaxPrefix + verb,
                            new JArray(transformNode.Value)));

                    transformNode.Replace(new JProperty(JdtUtilities.GetJdtInlineKey(transformNode.Name), newValue));
                }
            }

            this.Successor.Process(source, transform, logger);
        }
    }
}
