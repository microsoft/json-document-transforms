// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.Jdt.Tests
{
    using System;
    using System.IO;

    /// <summary>
    /// Shared Read Lock Temp File.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    internal class SharedReadLockTempFile : IDisposable
    {
        private readonly FileStream readFileStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="SharedReadLockTempFile"/> class.
        /// </summary>
        /// <param name="contents">The contents.</param>
        public SharedReadLockTempFile(string contents)
        {
            // create temp file
            this.FilePath = Path.GetTempFileName();

            // write contents
            File.WriteAllText(this.FilePath, contents);

            // set the file as read-only
            this.readFileStream = File.Open(this.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
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
            if ((this.FilePath == null) || !File.Exists(this.FilePath))
            {
                // nothing to delete
                return;
            }

            this.readFileStream.Dispose();

            // delete the file
            File.Delete(this.FilePath);
        }
    }
}
