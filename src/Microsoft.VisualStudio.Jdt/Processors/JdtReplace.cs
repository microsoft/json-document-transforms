// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License license. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.Jdt
{
    using System.Linq;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Represents the Replace transformation.
    /// </summary>
    internal class JdtReplace : JdtArrayProcessor
    {
        private readonly JdtAttributeValidator attributeValidator;

        /// <summary>
        /// Initializes a new instance of the <see cref="JdtReplace"/> class.
        /// </summary>
        public JdtReplace()
        {
            this.attributeValidator = new JdtAttributeValidator(JdtAttributes.Path, JdtAttributes.Value);
        }

        /// <inheritdoc/>
        public override string Verb { get; } = "replace";

        /// <inheritdoc/>
        protected override bool ProcessCore(JObject source, JToken transformValue, JsonTransformationContextLogger logger)
        {
            if (transformValue.Type == JTokenType.Object)
            {
                // If the value is an object, analyze the contents and perform the appropriate transform
                return this.ReplaceWithProperties(source, (JObject)transformValue, logger);
            }
            else
            {
                if (source.Root.Equals(source))
                {
                    // If trying to replace the root with a non-object token, throw
                    throw JdtException.FromLineInfo(Resources.ErrorMessage_ReplaceRoot, ErrorLocation.Transform, transformValue);
                }

                // If the value is not an object, simply replace the original node with the new value
                source.Replace(transformValue);

                // If the node is replaced, stop transformations on it
                return false;
            }
        }

        private bool ReplaceWithProperties(JObject source, JObject replaceObject, JsonTransformationContextLogger logger)
        {
            Dictionary<JdtAttributes, JToken> attributes = this.attributeValidator.ValidateAndReturnAttributes(replaceObject);

            // If there are attributes, handle them accordingly
            if (attributes.Any())
            {
                // If the object has attributes it must have both path and value
                JToken pathToken, valueToken;
                if (attributes.TryGetValue(JdtAttributes.Path, out pathToken) && attributes.TryGetValue(JdtAttributes.Value, out valueToken))
                {
                    if (pathToken.Type != JTokenType.String)
                    {
                        throw JdtException.FromLineInfo(Resources.ErrorMessage_PathContents, ErrorLocation.Transform, pathToken);
                    }

                    var tokensToReplace = source.SelectTokens(pathToken.ToString()).ToList();
                    if (!tokensToReplace.Any())
                    {
                        logger.LogWarning(Resources.WarningMessage_NoResults, ErrorLocation.Transform, pathToken);
                    }

                    foreach (JToken token in tokensToReplace)
                    {
                        bool replacedThisNode = false;

                        if (token.Root.Equals(token) && valueToken.Type != JTokenType.Object)
                        {
                            // If trying to replace the root object with a token that is not another object, throw
                            throw JdtException.FromLineInfo(Resources.ErrorMessage_ReplaceRoot, ErrorLocation.Transform, pathToken);
                        }

                        if (token.Equals(source))
                        {
                            // If the specified path is to the current node
                            replacedThisNode = true;
                        }

                        token.Replace(valueToken);

                        if (replacedThisNode)
                        {
                            // If the current node was replaced, stop executing transformations on this node
                            return false;
                        }
                    }
                }
                else
                {
                    // If either is not present, throw
                    throw JdtException.FromLineInfo(Resources.ErrorMessage_ReplaceAttributes, ErrorLocation.Transform, replaceObject);
                }

                // If we got here, transformations should continue
                return true;
            }
            else
            {
                // If there are no attributes, replace the current object with the given object
                // Here, the root can be replaced as the replace value is an object
                source.Replace(replaceObject);

                // If the node is replaced, stop transformations on it
                return false;
            }
        }
    }
}
