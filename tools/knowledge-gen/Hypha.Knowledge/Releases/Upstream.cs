// ------------------------------------------------------------------------------------------------
// <copyright file="Upstream.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Releases
{
    /// <summary>
    /// The two upstream repositories a hypha release is assembled from.
    /// </summary>
    /// <remarks>
    /// A hypha version is one <b>tag name that resolves in both</b>. The tag is the only version
    /// identifier used: the model URI inside the XMI tracks neither the release nor the content.
    /// </remarks>
    public static class Upstream
    {
        /// <summary>Specification PDFs, grammar and the textual model sources.</summary>
        public const string ReleaseRepository = "Systems-Modeling/SysML-v2-Release";

        /// <summary>The metamodel UML models, in XMI form.</summary>
        public const string PilotRepository = "Systems-Modeling/SysML-v2-Pilot-Implementation";
    }
}
