// ------------------------------------------------------------------------------------------------
// <copyright file="ReleaseArtifacts.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Layout
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// Which generated artifacts of one release are present on disk.
    /// </summary>
    /// <remarks>
    /// Part of the <c>hypha list --json</c> contract: renaming a property is a breaking change for
    /// every host that reads it.
    /// </remarks>
    /// <param name="Metamodel"><c>metamodel/index.json</c> is present.</param>
    /// <param name="TextualNotation">
    /// <c>textual-notation/index.md</c> and the worked-example index <c>textual-notation/examples/index.json</c>
    /// are present.
    /// </param>
    /// <param name="ModelLibrary"><c>model-library/index.json</c> is present.</param>
    /// <param name="CrossReferences"><c>cross-references.json</c> is present.</param>
    /// <param name="Specification">
    /// Both specification catalogs (<c>spec/kerml</c> and <c>spec/sysml2</c>) are present. They are
    /// generated locally from the OMG PDFs and are never distributed.
    /// </param>
    public sealed record ReleaseArtifacts(
        [property: JsonPropertyName("metamodel")] bool Metamodel,
        [property: JsonPropertyName("textualNotation")] bool TextualNotation,
        [property: JsonPropertyName("modelLibrary")] bool ModelLibrary,
        [property: JsonPropertyName("crossReferences")] bool CrossReferences,
        [property: JsonPropertyName("specification")] bool Specification)
    {
        /// <summary>
        /// Every artifact generated from the redistributable inputs is present. Specification text is
        /// not part of this: a release without the PDFs is still usable.
        /// </summary>
        [JsonPropertyName("core")]
        public bool Core => this.Metamodel && this.TextualNotation && this.ModelLibrary && this.CrossReferences;
    }
}
