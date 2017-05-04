// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.Jdt
{
    using System;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Base for a processor that handles array values
    /// </summary>
    internal abstract class JdtArrayProcessor : JdtProcessor
    {
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

            JToken transformValue;
            if (transform.TryGetValue(this.FullVerb, out transformValue))
            {
                if (!this.Transform(source, transformValue, logger))
                {
                    // If the transformation returns false,
                    // it performed an operation that halts transforms
                    return;
                }
            }

            this.Successor.Process(source, transform, logger);
        }

        /// <summary>
        /// The core transformation logic. Arrays are treated as the transform values
        /// </summary>
        /// <param name="source">Object to be transformed</param>
        /// <param name="transformValue">Value of the transform</param>
        /// <param name="logger">The transformation context logger</param>
        /// <returns>True if transforms should continue</returns>
        protected abstract bool ProcessCore(JObject source, JToken transformValue, JsonTransformationContextLogger logger);

        /// <summary>
        /// Performs the initial logic of processing arrays.
        /// Arrays cause the transform to be applied to each value in them
        /// </summary>
        /// <param name="source">Object to be transformed</param>
        /// <param name="transformValue">Value of the transform</param>
        /// <param name="logger">The transformation context logger</param>
        /// <returns>True if transforms should continue</returns>
        private bool Transform(JObject source, JToken transformValue, JsonTransformationContextLogger logger)
        {
            if (transformValue.Type == JTokenType.Array)
            {
                // If the value is an array, perform the transformation for each object in the array
                // From here, arrays are handled as the transformation value
                foreach (JToken arrayValue in (JArray)transformValue)
                {
                    if (!this.ProcessCore(source, arrayValue, logger))
                    {
                        // If the core transformation indicates a halt, we halt
                        return true;
                    }
                }

                // If we are not told to stop, we continue with transformations
                return true;
            }
            else
            {
                // If it is not an array, perform the transformation as normal
                return this.ProcessCore(source, transformValue, logger);
            }
        }
    }
}
