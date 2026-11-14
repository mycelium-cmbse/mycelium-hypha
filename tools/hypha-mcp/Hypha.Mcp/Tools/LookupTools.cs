// ------------------------------------------------------------------------------------------------
// <copyright file="LookupTools.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Mcp.Tools
{
    using System;
    using System.ComponentModel;

    using Hypha.Knowledge.Lookup;

    using ModelContextProtocol;
    using ModelContextProtocol.Server;

    /// <summary>
    /// The tool that reads an element page or a standard-library declaration a page at a time, through
    /// <see cref="IKnowledgeLookup"/>.
    /// </summary>
    [McpServerToolType]
    public sealed class LookupTools
    {
        /// <summary>Reads the pages.</summary>
        private readonly IKnowledgeLookup lookup;

        /// <summary>Initializes a new instance of the <see cref="LookupTools"/> class.</summary>
        /// <param name="lookup">Reads the pages.</param>
        /// <exception cref="ArgumentNullException"><paramref name="lookup"/> is <c>null</c>.</exception>
        public LookupTools(IKnowledgeLookup lookup)
        {
            ArgumentNullException.ThrowIfNull(lookup);

            this.lookup = lookup;
        }

        /// <summary>Reads one page of an element or declaration.</summary>
        /// <param name="kind">What to look up.</param>
        /// <param name="name">The name.</param>
        /// <param name="startLine">The first line to return, 1-based; <c>null</c> for the start.</param>
        /// <param name="maxLines">How many lines the page holds.</param>
        /// <param name="tag">The release; the default release when <c>null</c>.</param>
        /// <returns>The page, or the candidates for a name that is not exact, as JSON.</returns>
        /// <exception cref="McpException">The lookup could not be run; the message says why.</exception>
        [McpServerTool(Name = "hypha_lookup", Title = "Look up", ReadOnly = true, Idempotent = true, OpenWorld = false, Destructive = false)]
        [Description(
            "Reads a SysML v2 or KerML metamodel element page ('metamodel', by name such as PartUsage), a "
            + "standard-library declaration ('model-library', by qualified name such as ISQBase::mass) or a worked "
            + "example model the release ships ('examples', by its page name, or by its model's name such as Camera), "
            + "a page at a time. A name that is not exact returns the candidates instead. When 'nextLine' is set, call again with "
            + "it as 'startLine' for the rest.")]
        public string Lookup(
            [Description("What to look up: metamodel, model-library or examples.")] KnowledgeKind kind,
            [Description(
                "The element's name, such as PartUsage, the declaration's qualified name, such as ISQBase::mass, or the "
                + "example's page name (the file name hypha_syntax cross-reference gives, without .md) or model name.")]
            string name,
            [Description("The first line to return, 1-based. The start of the page (for a declaration, its declaration) when omitted.")] int? startLine = null,
            [Description("How many lines one page holds, at most 2000.")] int maxLines = LookupRequest.DefaultMaxLines,
            [Description("The installed release to answer from, such as 2026-05. The default release when omitted.")] string? tag = null) =>
            Answer.Of(
                () => this.lookup.Lookup(new LookupRequest(kind, name, tag, startLine, maxLines)),
                McpJsonContext.Readable.LookupResult);
    }
}
