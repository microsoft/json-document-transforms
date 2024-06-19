// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.Jdt
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Linq;
    using System.Xml.Linq;

    /// <summary>
    /// Represents the default JDT transformation.
    /// </summary>
    internal class JdtDefault : JdtProcessor
    {
        private DefaultMode mode = DefaultMode.Merge;

        private enum DefaultMode
        {
            Merge,
            Replace,
        }

        /// <inheritdoc/>
        public override string Verb { get; } = null;

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
                            if (this.mode == DefaultMode.Merge)
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
                            // TO DO: Verify if object has JDT verbs. They shouldn't be allowed here because they won't be processed
                            // If the contents are different, execute the replace
                            source[transformNode.Name] = transformNode.Value.DeepClone();
                        }
                    }
                    else
                    {
                        var shouldResetMode = this.mode == DefaultMode.Merge;
                        if (shouldResetMode)
                        {
                            // While doing default transformation on an object, switch to replace by default
                            // This works as expected as long as a single level of JDT Verbs is used (more are unsupported anyway)
                            this.mode = DefaultMode.Replace;
                        }

                        // If the tranform node is an object, cleaning it and transforming it on itself resolves all remaining JDT Verbs
                        // Otherwise JDT Verbs would polute the source object
                        this.ProcessTransformAndCleanJdtProperties(transformNode.Value, logger);

                        if (shouldResetMode)
                        {
                            this.mode = DefaultMode.Merge;
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
                foreach (JProperty node in jObject.Properties()
                    .Where(p => JdtUtilities.IsJdtSyntax(p.Name)).ToArray())
                {
                    jObject.Remove(node.Name);
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
