// ------------------------------------------------------------------------------------------------
// <copyright file="DiffSide.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Query
{
    /// <summary>One of the two releases a diff compares.</summary>
    /// <param name="Release">The release tag.</param>
    /// <param name="Source">
    /// The generated file read, relative to the output root, with <c>/</c> separators:
    /// <c>knowledge/2026-05/metamodel/metamodel.json</c>, for example.
    /// </param>
    /// <param name="SourceXmiSha256">The digest of the XMI the file was generated from, as the file records it.</param>
    public sealed record DiffSide(string Release, string Source, string SourceXmiSha256);
}
