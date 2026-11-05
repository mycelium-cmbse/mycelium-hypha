// ------------------------------------------------------------------------------------------------
// <copyright file="LookupRequest.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Lookup
{
    /// <summary>What to look up, in which release, and which page of its source to return.</summary>
    /// <param name="Kind">The index to resolve the name against.</param>
    /// <param name="Name">
    /// A short name (<c>PartUsage</c>, <c>Real</c>), a qualified name
    /// (<c>ScalarValues::Real</c>) or a trailing part of one (<c>Actions::ActionUsage</c>).
    /// </param>
    /// <param name="Tag">The release; <c>null</c> uses the default release.</param>
    /// <param name="StartLine">
    /// The first line to return, 1-based. <c>null</c> starts at the declaration for a model-library
    /// entry and at line 1 for a metamodel entry.
    /// </param>
    /// <param name="MaxLines">How many lines one page holds.</param>
    public sealed record LookupRequest(
        KnowledgeKind Kind, string Name, string? Tag = null, int? StartLine = null, int MaxLines = LookupRequest.DefaultMaxLines)
    {
        /// <summary>The default page size: enough for most element pages without flooding a context window.</summary>
        public const int DefaultMaxLines = 160;

        /// <summary>The largest page a caller may ask for.</summary>
        public const int LargestPage = 2000;
    }
}
