// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License license. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.Jdt
{
    using System.Linq;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Represents the Rename transformation.
    /// </summary>
    internal class JdtRename : JdtArrayProcessor
    {
        private readonly JdtAttributeValidator attributeValidator;

        /// <summary>
        /// Initializes a new instance of the <see cref="JdtRename"/> class.
        /// </summary>
        public JdtRename()
        {
            // Rename accepts the path and value attributes
            this.attributeValidator = new JdtAttributeValidator(JdtAttributes.Path, JdtAttributes.Value);
        }

        /// <inheritdoc/>
        public override string Verb { get; } = "rename";

        /// <inheritdoc/>
        protected override bool ProcessCore(JObject source, JToken transformValue, JsonTransformationContextLogger logger)
        {
            if (transformValue.Type != JTokenType.Object)
            {
                // Rename only accepts objects, either with properties or direct renames
                throw JdtException.FromLineInfo(string.Format(Resources.ErrorMessage_InvalidRenameValue, transformValue.Type.ToString()), ErrorLocation.Transform, transformValue);
            }
            else
            {
                // Try and get attributes from the object
                var renameObject = (JObject)transformValue;
                Dictionary<JdtAttributes, JToken> attributes = this.attributeValidator.ValidateAndReturnAttributes(renameObject);

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

                        if (valueToken.Type != JTokenType.String)
                        {
                            throw JdtException.FromLineInfo(Resources.ErrorMessage_ValueContents, ErrorLocation.Transform, valueToken);
                        }

                        var tokensToRename = source.SelectTokens(pathToken.ToString()).ToList();
                        if (!tokensToRename.Any())
                        {
                            logger.LogWarning(Resources.WarningMessage_NoResults, ErrorLocation.Transform, pathToken);
                        }

                        // If the values are correct, rename each token found with the given path
                        foreach (JToken token in tokensToRename)
                        {
                            if (!this.RenameNode(token, valueToken.ToString()))
                            {
                                throw JdtException.FromLineInfo(Resources.ErrorMessage_RenameNode, ErrorLocation.Transform, renameObject);
                            }
                        }
                    }
                    else
                    {
                        // If either is not present, throw
                        throw JdtException.FromLineInfo(Resources.ErrorMessage_RenameAttributes, ErrorLocation.Transform, renameObject);
                    }
                }
                else
                {
                    // If the object does not contain attributes, each property is a rename to execute
                    // where the key is the old name and the value must be a string with the new name of the node
                    foreach (JProperty renameOperation in renameObject.Properties())
                    {
                        if (renameOperation.Value.Type != JTokenType.String)
                        {
                            throw JdtException.FromLineInfo(Resources.ErrorMessage_ValueContents, ErrorLocation.Transform, renameOperation);
                        }

                        // TO DO: Warning if the node is not found
                        JToken nodeToRename;
                        if (source.TryGetValue(renameOperation.Name, out nodeToRename))
                        {
                            if (!this.RenameNode(nodeToRename, renameOperation.Value.ToString()))
                            {
                                throw JdtException.FromLineInfo(Resources.ErrorMessage_RenameNode, ErrorLocation.Transform, renameOperation);
                            }
                        }
                        else
                        {
                            logger.LogWarning(string.Format(Resources.WarningMessage_NodeNotFound, renameOperation.Name), ErrorLocation.Transform, renameOperation);
                        }
                    }
                }
            }

            // Do not halt transformations
            return true;
        }

        private bool RenameNode(JToken nodeToRename, string newName)
        {
            // We can only rename tokens belonging to a property
            // This excludes objects from arrays and the root object
            JProperty parent = nodeToRename.Parent as JProperty;

            if (parent == null)
            {
                return false;
            }

            // Replace with a new property of identical value and new name
            parent.Replace(new JProperty(newName, nodeToRename));
            return true;
        }
    }
}
