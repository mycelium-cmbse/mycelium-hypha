// ------------------------------------------------------------------------------------------------
// <copyright file="ApiArtifacts.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// Which generated artifacts of one Systems Modeling API version are present on disk.
    /// </summary>
    /// <remarks>
    /// Part of the <c>hypha api list --json</c> contract: renaming a property is a breaking change for
    /// every host that reads it.
    /// </remarks>
    /// <param name="Pim"><c>pim/metamodel.json</c> is present.</param>
    /// <param name="OpenApi"><c>openapi/index.json</c> is present.</param>
    /// <param name="CrossReferences"><c>cross-references.json</c> is present.</param>
    /// <param name="Specification">
    /// The specification catalog (<c>spec/index.json</c>) is present. It is generated locally from the
    /// OMG PDF and is never distributed.
    /// </param>
    public sealed record ApiArtifacts(
        [property: JsonPropertyName("pim")] bool Pim,
        [property: JsonPropertyName("openApi")] bool OpenApi,
        [property: JsonPropertyName("crossReferences")] bool CrossReferences,
        [property: JsonPropertyName("specification")] bool Specification)
    {
        /// <summary>
        /// Every artifact generated from the redistributable inputs is present. Specification text is
        /// not part of this: an API version without the PDF is still usable.
        /// </summary>
        [JsonPropertyName("core")]
        public bool Core => this.Pim && this.OpenApi && this.CrossReferences;
    }
}
