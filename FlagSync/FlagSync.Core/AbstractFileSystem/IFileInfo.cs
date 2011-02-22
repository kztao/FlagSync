﻿using System;

namespace FlagSync.Core.AbstractFileSystem
{
    public interface IFileInfo : IFileSystemInfo
    {
        /// <summary>
        /// Gets the last write time.
        /// </summary>
        /// <value>The last write time.</value>
        DateTime LastWriteTime { get; }

        /// <summary>
        /// Gets the length of the file.
        /// </summary>
        /// <value>The length of the file.</value>
        long Length { get; }

        /// <summary>
        /// Gets the directory of the file.
        /// </summary>
        /// <value>The directory of the file.</value>
        IDirectoryInfo Directory { get; }
    }
}