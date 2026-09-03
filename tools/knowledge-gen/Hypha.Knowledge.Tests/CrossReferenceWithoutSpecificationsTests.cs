// ------------------------------------------------------------------------------------------------
// <copyright file="CrossReferenceWithoutSpecificationsTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Tests
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    using Hypha.Knowledge;
    using Hypha.Knowledge.CrossReferences;
    using Hypha.Knowledge.Generation;
    using Hypha.Knowledge.Layout;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// #88's carry-forward mechanism no longer has a committed document to carry from - nothing is
    /// committed for any release any more (see #106) - so a release with no specification catalog
    /// extracted (the ordinary case for every install without a maintainer source checkout) must
    /// still generate rather than refuse (see #113).
    /// </summary>
    /// <remarks>
    /// The catalog is hidden by decorating the layout rather than by moving files about, so the test
    /// cannot leave the repository in a half-state if it fails.
    /// </remarks>
    [TestFixture]
    public class CrossReferenceWithoutSpecificationsTests
    {
        [Test]
        public async Task Generation_degrades_gracefully_instead_of_refusing()
        {
            using var provider = new ServiceCollection().AddHyphaKnowledge().BuildServiceProvider();

            var layout = provider.GetRequiredService<IKnowledgeLayout>();
            var tag = layout.DefaultTag;

            if (tag is null)
            {
                Assert.Ignore("no release is installed locally to generate for");
                return;
            }

            // CommittedCrossReferences is already always missing in this repository - nothing is
            // committed for any release any more - so forcing the specification catalog missing too
            // reproduces exactly what every real end-user's install has: neither source of
            // title-matched edges.
            var withoutCatalogOrCommitted = await Generate(provider, new WithoutSpecifications(layout), tag);

            Assert.That(withoutCatalogOrCommitted, Does.Contain("\"schemaVersion\""));
            Assert.That(
                withoutCatalogOrCommitted,
                Does.Not.Contain("\"title\""),
                "the licensing guarantee: no clause title may reach this file");
        }

        private static async Task<string> Generate(
            ServiceProvider provider, IKnowledgeLayout layout, string tag)
        {
            var generator = new CrossReferenceGenerator(
                layout,
                provider.GetRequiredService<Grammar.IGrammarParser>(),
                provider.GetRequiredService<Grammar.IGrammarLinks>(),
                provider.GetRequiredService<IKnowledgeReader>(),
                provider.GetRequiredService<ICrossReferenceBuilder>(),
                provider.GetRequiredService<ILogger<CrossReferenceGenerator>>());

            var result = await generator.GenerateAsync(tag);

            return await File.ReadAllTextAsync(result.Written[0].FullName);
        }

        /// <summary>A layout whose specification catalogs are all reported missing.</summary>
        private sealed class WithoutSpecifications(IKnowledgeLayout inner) : IKnowledgeLayout
        {
            public DirectoryInfo Root => inner.Root;

            public DirectoryInfo OutputRoot => inner.OutputRoot;

            public DirectoryInfo Sources => inner.Sources;

            public IReadOnlyList<string> InstalledTags => inner.InstalledTags;

            public string? DefaultTag => inner.DefaultTag;

            public FileInfo VersionManifest => inner.VersionManifest;

            public FileInfo SharedPrimitiveTypes => inner.SharedPrimitiveTypes;

            public FileInfo CrossReferenceSchema => inner.CrossReferenceSchema;

            public FileInfo ModelLibrarySchema => inner.ModelLibrarySchema;

            public DirectoryInfo ReleaseSources(string tag) => inner.ReleaseSources(tag);

            public DirectoryInfo Xmi(string tag) => inner.Xmi(tag);

            public DirectoryInfo TextualSources(string tag) => inner.TextualSources(tag);

            public DirectoryInfo Bnf(string tag) => inner.Bnf(tag);

            public DirectoryInfo ModelLibrarySources(string tag) => inner.ModelLibrarySources(tag);

            public FileInfo Grammar(string tag, string fileName) => inner.Grammar(tag, fileName);

            public DirectoryInfo Specifications(string tag) => inner.Specifications(tag);

            public DirectoryInfo Knowledge(string tag) => inner.Knowledge(tag);

            public DirectoryInfo Metamodel(string tag) => inner.Metamodel(tag);

            public FileInfo MetamodelIndex(string tag) => inner.MetamodelIndex(tag);

            public DirectoryInfo TextualNotation(string tag) => inner.TextualNotation(tag);

            public DirectoryInfo Examples(string tag) => inner.Examples(tag);

            public DirectoryInfo ModelLibrary(string tag) => inner.ModelLibrary(tag);

            public DirectoryInfo ModelLibraryPackages(string tag) => inner.ModelLibraryPackages(tag);

            public FileInfo ModelLibraryIndex(string tag) => inner.ModelLibraryIndex(tag);

            public FileInfo CommittedCrossReferences(string tag) => inner.CommittedCrossReferences(tag);

            public FileInfo CrossReferences(string tag) => inner.CrossReferences(tag);

            /// <summary>Always a path that does not exist, whatever is really on disk.</summary>
            public FileInfo SpecificationCatalog(string tag, string document) =>
                new(Path.Combine(inner.Knowledge(tag).FullName, "no-such-spec", document, "index.json"));
        }
    }
}
