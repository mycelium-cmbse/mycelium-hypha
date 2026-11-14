// ------------------------------------------------------------------------------------------------
// <copyright file="ReleaseIndexes.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Query
{
    using System.Linq;

    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Lookup;
    using Hypha.Knowledge.Metamodel;
    using Hypha.Knowledge.ModelLibrary;
    using Hypha.Knowledge.TextualNotation;

    /// <summary>
    /// Reads a release's metamodel, model-library and worked-example indexes, for <see cref="KnowledgeQuery"/> and
    /// <see cref="KnowledgeLookup"/> alike.
    /// </summary>
    /// <remarks>
    /// The <see cref="IDocumentCache"/> checks a document is whole only when it first reads it, so every
    /// reader of one file has to apply the same check; reading through here is what keeps them the same.
    /// </remarks>
    internal static class ReleaseIndexes
    {
        /// <summary>Reads a release's metamodel <c>index.json</c>.</summary>
        /// <param name="documents">The document cache.</param>
        /// <param name="layout">Where the release's knowledge lives.</param>
        /// <param name="release">The release.</param>
        /// <returns>The index.</returns>
        /// <exception cref="KnowledgeLookupException">
        /// The release has not generated its metamodel, or the index is invalid or has an empty entry.
        /// </exception>
        public static MetamodelLookup Metamodel(IDocumentCache documents, IKnowledgeLayout layout, string release) =>
            documents.Get(
                layout.MetamodelIndex(release),
                ReadContracts.MetamodelIndex,
                document => document.Entries is not null && document.Entries.Values.All(entry => entry is not null),
                ReleaseAxis.Language.NotGenerated("metamodel", release),
                ReleaseAxis.Language.GenerateCommand(release));

        /// <summary>Reads a release's model-library <c>index.json</c>.</summary>
        /// <param name="documents">The document cache.</param>
        /// <param name="layout">Where the release's knowledge lives.</param>
        /// <param name="release">The release.</param>
        /// <returns>The index.</returns>
        /// <exception cref="KnowledgeLookupException">
        /// The release has not generated its model library, or the index is invalid or has an empty entry.
        /// </exception>
        public static ModelLibraryDocument ModelLibrary(IDocumentCache documents, IKnowledgeLayout layout, string release) =>
            documents.Get(
                layout.ModelLibraryIndex(release),
                ReadContracts.ModelLibrary,
                document => document.Declarations is not null && document.Declarations.Values.All(entry => entry is not null),
                ReleaseAxis.Language.NotGenerated("model library", release),
                ReleaseAxis.Language.GenerateCommand(release));

        /// <summary>Reads a release's worked-example <c>index.json</c>.</summary>
        /// <param name="documents">The document cache.</param>
        /// <param name="layout">Where the release's knowledge lives.</param>
        /// <param name="release">The release.</param>
        /// <returns>The index.</returns>
        /// <exception cref="KnowledgeLookupException">
        /// The release has not generated its example index (a release generated before it existed needs
        /// generating again), or the index is invalid or has an empty entry.
        /// </exception>
        public static ExampleDocument Examples(IDocumentCache documents, IKnowledgeLayout layout, string release) =>
            documents.Get(
                layout.ExamplesIndex(release),
                ReadContracts.Examples,
                document => document.Examples is not null && document.Examples.Values.All(entry => entry is not null),
                ReleaseAxis.Language.NotGenerated("worked-example index", release),
                ReleaseAxis.Language.GenerateCommand(release));
    }
}
