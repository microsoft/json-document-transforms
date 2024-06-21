// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.Jdt
{
    using System;
    using System.Linq;
    using System.Xml.Linq;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Represents the default JDT transformation.
    /// </summary>
    internal class JdtDefault : JdtProcessor
    {
        private JdtDefaultTransform defaultTransform = JdtDefaultTransform.Merge;

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

            if (source.Type == JTokenType.Object)
            {
                // JDT Verbs are not handled here
                foreach (JProperty transformNode in transform.Properties()
                .Where(p => !JdtUtilities.IsJdtSyntax(p.Name)))
                {
                    JToken nodeToTransform;
                    if (((JObject)source).TryGetValue(transformNode.Name, out nodeToTransform))
                    {
                        // If the node is present in both transform and source, analyze the types
                        // If both are objects, that is a recursive transformation, not handled here
                        if (nodeToTransform.Type == JTokenType.Array && transformNode.Value.Type == JTokenType.Array)
                        {
                            // If the original and transform are arrays, merge or replace the contents, depending on current mode
                            if (this.defaultTransform == JdtDefaultTransform.Merge)
                            {
                                ((JArray)nodeToTransform).Merge(transformNode.Value.DeepClone());
                            }
                            else
                            {
                                ((JArray)nodeToTransform).Replace(transformNode.Value.DeepClone());
                            }
                        }
                        else if (nodeToTransform.Type != JTokenType.Object || transformNode.Value.Type != JTokenType.Object)
                        {
                            // For non-arrays and non-objects, just replace them
                            source[transformNode.Name] = transformNode.Value.DeepClone();
                        }
                    }
                    else
                    {
                        var shouldResetMode = this.defaultTransform == JdtDefaultTransform.Merge;
                        if (shouldResetMode)
                        {
                            // While doing default transformation on an object, switch to replace by default
                            // This works as expected as long as a single level of JDT Verbs is used (more are unsupported anyway)
                            this.defaultTransform = JdtDefaultTransform.Replace;
                        }

                        // If the tranform node is an object, cleaning it and transforming it on itself resolves all remaining JDT Verbs
                        // Otherwise JDT Verbs would polute the source object
                        this.ProcessTransformAndCleanJdtProperties(transformNode.Value, logger);

                        if (shouldResetMode)
                        {
                            this.defaultTransform = JdtDefaultTransform.Merge;
                        }

                        // If the node is not present in the original, add it
                        ((JObject)source).Add(transformNode.DeepClone());
                    }
                }
            }
            else if (!transform.Properties().Where(p => JdtUtilities.IsJdtSyntax(p.Name)).Any())
            {
                source.Replace(transform.DeepClone());
            }

            this.Successor.Process(source, transform, logger);
        }

        /// <summary>
        /// Cleans JDT Value properties and processes itself as a transform.
        /// </summary>
        /// <param name="token">JObject or JArray.</param>
        /// <param name="logger">Logger.</param>
        /// <returns>The same reference.</returns>
        private JToken ProcessTransformAndCleanJdtProperties(JToken token, JsonTransformationContextLogger logger)
        {
            if (token.Type == JTokenType.Array)
            {
                foreach (var item in ((JArray)token).Children())
                {
                    this.ProcessTransformAndCleanJdtProperties(item, logger);
                }
            }
            else if (token.Type == JTokenType.Object)
            {
                var jObject = (JObject)token;
                var selfTransform = (JObject)jObject.DeepClone();
                foreach (JProperty node in jObject.Properties().ToArray())
                {
                    if (JdtUtilities.IsJdtSyntax(node.Name))
                    {
                        // Rename any JDT verb nodes
                        jObject.Remove(node.Name);
                    }
                    else if (JdtUtilities.IsJdtInlineSyntax(node.Name))
                    {
                        // Rename any JDT inline verb nodes and replace them with empty JObjects
                        // As JDT inline verbs are supported in merge or replace, this is sufficient
                        node.Replace(new JProperty(JdtUtilities.GetJdtInlineKey(node.Name), new JObject()));
                    }
                }

                ProcessTransform(token, selfTransform, logger);

                foreach (JProperty node in jObject.Properties().ToArray())
                {
                    this.ProcessTransformAndCleanJdtProperties(node.Value, logger);
                }
            }

            return token;
        }
    }
}
