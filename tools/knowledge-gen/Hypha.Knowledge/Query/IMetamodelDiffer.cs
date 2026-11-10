// ------------------------------------------------------------------------------------------------
// <copyright file="IMetamodelDiffer.cs" company="Starion Group S.A.">
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

    using Hypha.Knowledge.Metamodel;

    /// <summary>
    /// Compares two <c>metamodel.json</c> documents field by field, for the release diff.
    /// </summary>
    /// <remarks>
    /// An element or member is matched on its XMI id first, so a rename is reported as one, and on its
    /// name when the ids differ or are missing. Inherited members are not compared. Implementations must
    /// be safe to call from several threads at once.
    /// </remarks>
    public interface IMetamodelDiffer
    {
        /// <summary>Every change between two documents.</summary>
        /// <param name="from">The older document.</param>
        /// <param name="to">The newer document.</param>
        /// <returns>The changes, sorted by element, element kind, member, member kind, kind and then field.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="from"/> or <paramref name="to"/> is <c>null</c>.</exception>
        IReadOnlyList<MetamodelChange> Compare(MetamodelDocument from, MetamodelDocument to);
    }
}
