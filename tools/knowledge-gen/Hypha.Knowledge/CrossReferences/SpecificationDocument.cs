// ------------------------------------------------------------------------------------------------
// <copyright file="SpecificationDocument.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.CrossReferences
{
    /// <summary>
    /// One of the specifications the clause identifiers refer to.
    /// </summary>
    /// <param name="Document">The specification's name, e.g. <c>KerML</c>.</param>
    /// <param name="Version">Its version, e.g. <c>1.0</c>.</param>
    public sealed record SpecificationDocument(string Document, string Version);
}
