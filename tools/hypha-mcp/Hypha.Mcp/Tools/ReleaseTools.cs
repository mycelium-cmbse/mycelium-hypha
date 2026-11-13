// ------------------------------------------------------------------------------------------------
// <copyright file="ReleaseTools.cs" company="Starion Group S.A.">
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

    using Hypha.Knowledge.Query;

    using ModelContextProtocol;
    using ModelContextProtocol.Server;

    /// <summary>
    /// The tools about the installed releases themselves: what is installed, and what changed between two.
    /// </summary>
    [McpServerToolType]
    public sealed class ReleaseTools
    {
        /// <summary>Answers the questions.</summary>
        private readonly IKnowledgeQuery query;

        /// <summary>Initializes a new instance of the <see cref="ReleaseTools"/> class.</summary>
        /// <param name="query">Answers the questions.</param>
        /// <exception cref="ArgumentNullException"><paramref name="query"/> is <c>null</c>.</exception>
        public ReleaseTools(IKnowledgeQuery query)
        {
            ArgumentNullException.ThrowIfNull(query);

            this.query = query;
        }

        /// <summary>Lists the installed releases and API versions.</summary>
        /// <returns>The inventory as JSON.</returns>
        /// <exception cref="McpException">A manifest could not be read; the message says why.</exception>
        [McpServerTool(Name = "hypha_release_status", Title = "Release status", ReadOnly = true, Idempotent = true, OpenWorld = false, Destructive = false)]
        [Description(
            "Lists the SysML v2 / KerML releases and Systems Modeling API versions installed on this machine, which "
            + "one answers by default, and which artifacts each has generated (metamodel, textual notation, model "
            + "library, specification text, cross-references). Call it first when unsure what is installed.")]
        public string ReleaseStatus() =>
            Answer.Of(() => this.query.Releases(), QueryJsonContext.Readable.ReleaseInventory);

        /// <summary>Compares two installed releases.</summary>
        /// <param name="from">The older release.</param>
        /// <param name="to">The newer release.</param>
        /// <param name="element">Only this element, or <c>null</c> for every change.</param>
        /// <returns>The changes, with their grounding, as JSON.</returns>
        /// <exception cref="McpException">The releases could not be compared; the message says why.</exception>
        [McpServerTool(Name = "hypha_diff", Title = "Release diff", ReadOnly = true, Idempotent = true, OpenWorld = false, Destructive = false)]
        [Description(
            "Lists what changed in the metamodel and the cross-references between two installed SysML v2 / KerML "
            + "releases: metaclasses and members added, removed or changed.")]
        public string Diff(
            [Description("The older release, such as 2026-02.")] string from,
            [Description("The newer release, such as 2026-05.")] string to,
            [Description("Only this element, by its name in either release.")] string? element = null) =>
            Answer.Of(() => this.query.Diff(from, to, element));
    }
}
