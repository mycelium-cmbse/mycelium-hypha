// ------------------------------------------------------------------------------------------------
// <copyright file="KnowledgeLookupException.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Lookup
{
    using System;

    /// <summary>
    /// A lookup could not be answered: the release is not installed, its knowledge has not been
    /// generated, or the generated files are not what the generators write.
    /// </summary>
    /// <remarks>
    /// Distinct from a name that does not resolve, which is a normal <see cref="LookupResult"/>. The
    /// message says what to do about it, so a host can show it as is.
    /// </remarks>
    public sealed class KnowledgeLookupException : InvalidOperationException
    {
        /// <summary>Initializes a new instance of the <see cref="KnowledgeLookupException"/> class.</summary>
        public KnowledgeLookupException()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="KnowledgeLookupException"/> class.</summary>
        /// <param name="message">What went wrong and what to do about it, fit to show a user as is.</param>
        public KnowledgeLookupException(string message)
            : base(message)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="KnowledgeLookupException"/> class.</summary>
        /// <param name="message">What went wrong and what to do about it, fit to show a user as is.</param>
        /// <param name="innerException">The failure that caused this one, such as a JSON or I/O error.</param>
        public KnowledgeLookupException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
