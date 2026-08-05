// ------------------------------------------------------------------------------------------------
// <copyright file="ExampleEdge.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.CrossReferences
{
    /// <summary>
    /// A worked example that declares the element.
    /// </summary>
    /// <param name="File">Path to the example page, relative to <c>knowledge/&lt;tag&gt;/</c>.</param>
    /// <param name="Provenance">Which tier this fact belongs to.</param>
    /// <param name="Method">How the edge was obtained.</param>
    public sealed record ExampleEdge(string File, string Provenance, string Method);
}
