// ------------------------------------------------------------------------------------------------
// <copyright file="MoveWindowStep.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Commands
{
    /// <summary>The steps <c>move-window</c> runs, in order.</summary>
    public enum MoveWindowStep
    {
        /// <summary>Download the release's inputs from both upstreams.</summary>
        Fetch,

        /// <summary>Regenerate the metamodel, textual notation and cross-references for the release.</summary>
        Generate,

        /// <summary>Re-extract the specification text from the release's PDFs.</summary>
        ExtractSpecifications,

        /// <summary>
        /// Re-bless the <c>Hypha.MetamodelGen.Tests</c> <c>Expected/</c> golden files against its
        /// committed XMI fixture (a fixed regression fixture, independent of the release just fetched).
        /// </summary>
        ReblessFixtures,

        /// <summary>Drop the locally-installed releases that fall outside <c>--keep</c>.</summary>
        Evict,

        /// <summary>Confirm the regenerated knowledge base is self-consistent (see CLAUDE.md).</summary>
        Verify,
    }
}
