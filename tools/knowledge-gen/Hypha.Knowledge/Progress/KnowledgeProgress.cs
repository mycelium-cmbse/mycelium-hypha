// ------------------------------------------------------------------------------------------------
// <copyright file="KnowledgeProgress.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Progress
{
    /// <summary>
    /// How far a fetch or a generation has come: one unit of one step finished. The CLI shows it as a
    /// progress display, and the MCP server sends it as a progress notification.
    /// </summary>
    /// <param name="Operation"><see cref="Fetch"/>, <see cref="ApiFetch"/> or <see cref="Generate"/>.</param>
    /// <param name="Step">
    /// What the units belong to: for a fetch, the kind of input (<c>metamodel</c>, <c>textual</c>,
    /// <c>specifications</c>); for an API fetch, <c>documents</c>; for a generation, the release or API
    /// version being generated.
    /// </param>
    /// <param name="Done">
    /// How many of <paramref name="Total"/> have finished, a file kept because it was already there and
    /// an artifact skipped for want of its inputs included: each is one unit done, not zero.
    /// </param>
    /// <param name="Total">The step's units: its files, documents or artifacts, known before the first one finishes.</param>
    /// <param name="Message">
    /// What just finished, for a person, without the step: such as <c>3 of 12 files</c> for a fetch or
    /// <c>textual-notation: 312 files</c> for a generation.
    /// </param>
    public readonly record struct KnowledgeProgress(string Operation, string Step, int Done, int Total, string Message)
    {
        /// <summary>The <see cref="Operation"/> of fetching a release's inputs.</summary>
        public const string Fetch = "fetch";

        /// <summary>The <see cref="Operation"/> of fetching an API version's inputs.</summary>
        public const string ApiFetch = "api-fetch";

        /// <summary>The <see cref="Operation"/> of generating a release's or an API version's knowledge.</summary>
        public const string Generate = "generate";
    }
}
