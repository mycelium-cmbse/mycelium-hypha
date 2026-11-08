// ------------------------------------------------------------------------------------------------
// <copyright file="ReleaseOperationLockException.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Releases
{
    using System;

    /// <summary>
    /// The release-operation lock could not be taken: another process or VS Code window is changing
    /// releases, or a lock it left behind cannot be safely recovered.
    /// </summary>
    /// <remarks>
    /// The message says what to do about it, so a host can show it as is.
    /// </remarks>
    public sealed class ReleaseOperationLockException : InvalidOperationException
    {
        /// <summary>Initializes a new instance of the <see cref="ReleaseOperationLockException"/> class.</summary>
        public ReleaseOperationLockException()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ReleaseOperationLockException"/> class.</summary>
        /// <param name="message">What went wrong and what to do about it, fit to show a user as is.</param>
        public ReleaseOperationLockException(string message)
            : base(message)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ReleaseOperationLockException"/> class.</summary>
        /// <param name="message">What went wrong and what to do about it, fit to show a user as is.</param>
        /// <param name="innerException">The failure that caused this one, such as a JSON error.</param>
        public ReleaseOperationLockException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
