// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.Jdt
{
    using System.Linq;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Represents the Remove transformation.
    /// </summary>
    internal class JdtRemove : JdtArrayProcessor
    {
        private readonly JdtAttributeValidator attributeValidator;

        /// <summary>
        /// Initializes a new instance of the <see cref="JdtRemove"/> class.
        /// </summary>
        public JdtRemove()
        {
            // Remove only accepts the path attribute
            this.attributeValidator = new JdtAttributeValidator(JdtAttributes.Path);
        }

        /// <inheritdoc/>
        public override string Verb { get; } = "remove";

        /// <inheritdoc/>
        protected override bool ProcessCore(JToken source, JToken transformValue, JsonTransformationContextLogger logger)
        {
            switch (transformValue.Type)
            {
                case JTokenType.String:
                    if (source.Type == JTokenType.Object)
                    {
                        // If the value is just a string, remove that node
                        if (!((JObject)source).Remove(transformValue.ToString()))
                        {
                            logger.LogWarning(Resources.WarningMessage_UnableToRemove, ErrorLocation.Transform, transformValue);
                        }
                    }

                    break;
                case JTokenType.Boolean:
                    if ((bool)transformValue)
                    {
                        if (source.Root.Equals(source))
                        {
                            throw JdtException.FromLineInfo(Resources.ErrorMessage_RemoveRoot, ErrorLocation.Transform, transformValue);
                        }

                        // If the transform value is true, remove the entire node
                        return this.RemoveThisNode(source, logger);
                    }

                    break;
                case JTokenType.Object:
                    // If the value is an object, verify the attributes within and perform the remove
                    return this.RemoveWithAttributes(source, (JObject)transformValue, logger);
                default:
                    throw JdtException.FromLineInfo(string.Format(Resources.ErrorMessage_InvalidRemoveValue, transformValue.Type.ToString()), ErrorLocation.Transform, transformValue);
            }

            // If nothing indicates a halt, continue with transforms
            return true;
        }

        private bool RemoveWithAttributes(JToken source, JObject removeObject, JsonTransformationContextLogger logger)
        {
            var attributes = this.attributeValidator.ValidateAndReturnAttributes(removeObject);

            // The remove attribute only accepts objects if they have only the path attribute
            JToken pathToken;
            if (attributes.TryGetValue(JdtAttributes.Path, out pathToken))
            {
                if (pathToken.Type == JTokenType.String)
                {
                    var tokensToRemove = source.SelectTokens(pathToken.ToString()).ToList();
                    if (!tokensToRemove.Any())
                    {
                        logger.LogWarning(Resources.WarningMessage_NoResults, ErrorLocation.Transform, pathToken);
                    }

                    // Removes all of the tokens specified by the path
                    foreach (JToken token in tokensToRemove)
                    {
                        if (token.Equals(source))
                        {
                            if (source.Root.Equals(source))
                            {
                                throw JdtException.FromLineInfo(Resources.ErrorMessage_RemoveRoot, ErrorLocation.Transform, removeObject);
                            }

                            // If the path specifies the current node
                            if (!this.RemoveThisNode(source, logger))
                            {
                                // Halt transformations
                                return false;
                            }
                        }
                        else
                        {
                            if (token.Parent.Type == JTokenType.Property)
                            {
                                // If the token is the value of a property,
                                // the property must be removed
                                token.Parent.Remove();
                            }
                            else
                            {
                                // If the token is a property or an element in an array,
                                // it must be removed directly
                                token.Remove();
                            }
                        }
                    }
                }
                else
                {
                    throw JdtException.FromLineInfo(Resources.ErrorMessage_PathContents, ErrorLocation.Transform, pathToken);
                }
            }
            else
            {
                throw JdtException.FromLineInfo(Resources.ErrorMessage_RemoveAttributes, ErrorLocation.Transform, removeObject);
            }

            // If nothing indicates a halt, continue transforms
            return true;
        }

        private bool RemoveThisNode(JToken nodeToRemove, JsonTransformationContextLogger logger)
        {
            var parent = (JProperty)nodeToRemove.Parent;
            if (parent == null)
            {
                // If the node can't be removed, log a warning pointing to the node in the source
                logger.LogWarning(Resources.WarningMessage_UnableToRemove, ErrorLocation.Source, nodeToRemove);
                return true;
            }
            else
            {
                // If the node is removed, transformations should be halted
                parent.Value = null;
                return false;
            }
        }
    }
}
