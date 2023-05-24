// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.Jdt
{
    using System.Linq;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Represents the Merge transformation.
    /// </summary>
    internal class JdtMerge : JdtArrayProcessor
    {
        private readonly JdtAttributeValidator attributeValidator;

        /// <summary>
        /// Initializes a new instance of the <see cref="JdtMerge"/> class.
        /// </summary>
        public JdtMerge()
        {
            // Merge accepts path and value attributes
            this.attributeValidator = new JdtAttributeValidator(JdtAttributes.Path, JdtAttributes.Value);
        }

        /// <inheritdoc/>
        public override string Verb { get; } = "merge";

        /// <inheritdoc/>
        protected override bool ProcessCore(JObject source, JToken transformValue, JsonTransformationContextLogger logger)
        {
            if (transformValue.Type == JTokenType.Object)
            {
                // If both source and transform are objects,
                // analyze the contents and perform the appropriate transforms
                this.MergeWithObject(source, (JObject)transformValue, logger);
            }
            else
            {
                // If the transformation is trying to replace the root, throw
                if (source.Root.Equals(source))
                {
                    throw JdtException.FromLineInfo(Resources.ErrorMessage_ReplaceRoot, ErrorLocation.Transform, transformValue);
                }

                // If the transform value is not an object, then simply replace it with the new token
                source.Replace(transformValue);
            }

            // Do not halt transformations
            return true;
        }

        private void MergeWithObject(JObject source, JObject mergeObject, JsonTransformationContextLogger logger)
        {
            var attributes = this.attributeValidator.ValidateAndReturnAttributes(mergeObject);

            // If there are attributes, handle them accordingly
            if (attributes.Any())
            {
                // If the object has attributes it must have both path and value
                // TO DO: Accept value without path
                JToken pathToken, valueToken;
                if (attributes.TryGetValue(JdtAttributes.Path, out pathToken) && attributes.TryGetValue(JdtAttributes.Value, out valueToken))
                {
                    if (pathToken.Type != JTokenType.String)
                    {
                        throw JdtException.FromLineInfo(Resources.ErrorMessage_PathContents, ErrorLocation.Transform, mergeObject);
                    }

                    var tokensToMerge = source.SelectTokens(pathToken.ToString()).ToList();
                    if (!tokensToMerge.Any())
                    {
                        logger.LogWarning(Resources.WarningMessage_NoResults, ErrorLocation.Transform, pathToken);
                    }

                    foreach (JToken token in tokensToMerge)
                    {
                        // Perform the merge for each element found through the path
                        if (token.Type == JTokenType.Object && valueToken.Type == JTokenType.Object)
                        {
                            // If they are both objects, start a new transformation
                            ProcessTransform((JObject)token, (JObject)valueToken, logger);
                        }
                        else if (token.Type == JTokenType.Array && valueToken.Type == JTokenType.Array)
                        {
                            // If they are both arrays, add the new values to the original
                            ((JArray)token).Merge(valueToken.DeepClone());
                        }
                        else
                        {
                            // If the transformation is trying to replace the root, throw
                            if (token.Root.Equals(token))
                            {
                                throw JdtException.FromLineInfo(Resources.ErrorMessage_ReplaceRoot, ErrorLocation.Transform, mergeObject);
                            }

                            // If they are primitives or have different values, perform a replace
                            token.Replace(valueToken);
                        }
                    }
                }
                else
                {
                    // If either is not present, throw
                    throw JdtException.FromLineInfo(Resources.ErrorMessage_MergeAttributes, ErrorLocation.Transform, mergeObject);
                }
            }
            else
            {
                // If the merge object does not contain attributes,
                // simply execute the transform with that object
                ProcessTransform(source, mergeObject, logger);
            }
        }
    }
}
