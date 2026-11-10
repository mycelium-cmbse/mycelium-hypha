// ------------------------------------------------------------------------------------------------
// <copyright file="ICrossReferenceDiffer.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Query
{
    using System;
    using System.Collections.Generic;

    using Hypha.Knowledge.CrossReferences;

    /// <summary>
    /// Compares two <c>cross-references.json</c> documents edge by edge, for the release diff.
    /// </summary>
    /// <remarks>
    /// Only elements found in both releases are compared: an element added or removed is a metamodel
    /// change, and its edges follow from it. An element renamed between the releases is compared under
    /// both of its names. Implementations must be safe to call from several threads at once.
    /// </remarks>
    public interface ICrossReferenceDiffer
    {
        /// <summary>Every change to the edges of the elements found in both documents.</summary>
        /// <param name="from">The older document.</param>
        /// <param name="to">The newer document.</param>
        /// <param name="renames">
        /// The elements renamed between the releases, older name to newer name, as the metamodel diff
        /// found them.
        /// </param>
        /// <returns>The changes, sorted by element, element kind, facet, edge, kind and then field.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="from"/>, <paramref name="to"/> or <paramref name="renames"/> is <c>null</c>.
        /// </exception>
        IReadOnlyList<CrossReferenceChange> Compare(
            CrossReferenceDocument from, CrossReferenceDocument to, IReadOnlyDictionary<string, string> renames);
    }
}
