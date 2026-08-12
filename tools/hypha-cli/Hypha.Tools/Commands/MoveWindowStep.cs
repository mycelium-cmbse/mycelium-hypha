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

        /// <summary>Re-bless the metamodel-gen test fixtures that follow the default release.</summary>
        ReblessFixtures,

        /// <summary>Drop the releases that fall outside the window.</summary>
        Evict,

        /// <summary>Confirm the committed knowledge base still regenerates byte-identical.</summary>
        Verify,
    }
}
