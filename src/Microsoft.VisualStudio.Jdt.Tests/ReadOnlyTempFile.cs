// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.Jdt.Tests
{
    using System;
    using System.IO;

    /// <summary>
    /// Read-Only Temp File.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    internal class ReadOnlyTempFile : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyTempFile"/> class.
        /// </summary>
        /// <param name="contents">The contents.</param>
        public ReadOnlyTempFile(string contents)
        {
            // create temp file
            this.FilePath = Path.GetTempFileName();

            // write contents
            File.WriteAllText(this.FilePath, contents);

            // set the file as read-only
            File.SetAttributes(this.FilePath, FileAttributes.ReadOnly);
        }

        /// <summary>
        /// Gets the file path.
        /// </summary>
        /// <value>The file path.</value>
        public string FilePath { get; private set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // delete file
            if (this.FilePath == null)
            {
                // nothing to delete
                return;
            }

            if (!File.Exists(this.FilePath))
            {
                // nothing to delete
                return;
            }

            // remove read-only attribute if it exists
            FileAttributes attributes = File.GetAttributes(this.FilePath);
            if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
            {
                // read-only attribute exists
                // remove read-only attribute
                File.SetAttributes(this.FilePath, attributes ^ FileAttributes.ReadOnly);
            }

            // delete the file
            File.Delete(this.FilePath);
        }
    }
}
