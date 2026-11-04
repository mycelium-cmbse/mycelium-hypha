// ------------------------------------------------------------------------------------------------
// <copyright file="IKnowledgeLookup.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Lookup
{
    /// <summary>
    /// Resolves a metamodel or model-library name to its generated page in an installed release and
    /// returns one page of it.
    /// </summary>
    /// <remarks>
    /// Read-only: it never fetches or generates. It is the one place that reads the generated indexes
    /// on behalf of a host (the CLI's <c>lookup</c> verb, an editor extension), so the index format
    /// stays private to this library.
    /// </remarks>
    public interface IKnowledgeLookup
    {
        /// <summary>Looks up a name.</summary>
        /// <exception cref="KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its generated
        /// files are invalid.
        /// </exception>
        LookupResult Lookup(LookupRequest request);
    }
}
