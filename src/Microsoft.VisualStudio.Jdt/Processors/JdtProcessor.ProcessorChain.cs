// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.Jdt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The JdtProcessor chain
    /// </summary>
    internal abstract partial class JdtProcessor
    {
        private class JdtProcessorChain
        {
            // This is a a list of supported transformations
            // It is in order of execution
            private readonly List<JdtProcessor> processors = new List<JdtProcessor>()
            {
                // Supported transformations
                new JdtRecurse(),
                new JdtRemove(),
                new JdtReplace(),
                new JdtRename(),
                new JdtMerge(),
                new JdtDefault()
            };

            public JdtProcessorChain()
            {
                var validator = new JdtValidator();

                // The first step of a transformation is validating the verbs
                this.processors.Insert(0, validator);

                // The successor of each transform processor should be the next one on the list
                // The last processor defaults to the end of chain processor
                var processorsEnumerator = this.processors.GetEnumerator();
                processorsEnumerator.MoveNext();
                foreach (var successor in this.processors.Skip(1))
                {
                    if (!string.IsNullOrEmpty(successor.Verb))
                    {
                        // If the transformation has a corresponding verb,
                        // add it to the list of verbs to be validated
                        validator.ValidVerbs.Add(successor.Verb);
                    }

                    processorsEnumerator.Current.Successor = successor;
                    processorsEnumerator.MoveNext();
                }
            }

            public void Start(JObject source, JObject transform, JsonTransformationContextLogger logger)
            {
                if (source == null)
                {
                    throw new ArgumentNullException(nameof(source));
                }

                if (transform == null)
                {
                    throw new ArgumentNullException(nameof(transform));
                }

                this.processors.First().Process(source, transform, logger);
            }
        }

        /// <summary>
        /// Represents the end of the transformation chain
        /// </summary>
        private class JdtEndOfChain : JdtProcessor
        {
            private JdtEndOfChain()
            {
            }

            public static JdtEndOfChain Instance { get; } = new JdtEndOfChain();

            public override string Verb { get; } = null;

            internal override void Process(JObject source, JObject transform, JsonTransformationContextLogger logger)
            {
                // Do nothing, the chain is done
            }
        }
    }
}
