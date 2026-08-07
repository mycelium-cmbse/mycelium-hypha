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
    /// The claim from #88, checked on the real knowledge base rather than on synthetic data: the
    /// cross-references come out the same whether or not the OMG specifications are present.
    /// </summary>
    /// <remarks>
    /// The catalog is hidden by decorating the layout rather than by moving files about, so the test
    /// cannot leave the repository in a half-state if it fails.
    /// </remarks>
    [TestFixture]
    public class CrossReferenceWithoutSpecificationsTests
    {
        [Test]
        public async Task The_same_document_is_produced_without_the_specification_catalog()
        {
            using var provider = new ServiceCollection().AddHyphaKnowledge().BuildServiceProvider();

            var layout = provider.GetRequiredService<IKnowledgeLayout>();
            var tag = layout.DefaultTag;

            if (tag is null || !layout.SpecificationCatalog(tag, "kerml").Exists)
            {
                Assert.Ignore("the specification catalog has not been extracted");
                return;
            }

            var withCatalog = await Generate(provider, layout, tag);
            var withoutCatalog = await Generate(provider, new WithoutSpecifications(layout), tag);

            Assert.That(withoutCatalog, Is.EqualTo(withCatalog));
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

            public DirectoryInfo Xmi(string tag) => inner.Xmi(tag);

            public DirectoryInfo TextualSources(string tag) => inner.TextualSources(tag);

            public DirectoryInfo Bnf(string tag) => inner.Bnf(tag);

            public FileInfo Grammar(string tag, string fileName) => inner.Grammar(tag, fileName);

            public DirectoryInfo Specifications(string tag) => inner.Specifications(tag);

            public DirectoryInfo Knowledge(string tag) => inner.Knowledge(tag);

            public DirectoryInfo Metamodel(string tag) => inner.Metamodel(tag);

            public FileInfo MetamodelIndex(string tag) => inner.MetamodelIndex(tag);

            public DirectoryInfo TextualNotation(string tag) => inner.TextualNotation(tag);

            public DirectoryInfo Examples(string tag) => inner.Examples(tag);

            public FileInfo CommittedCrossReferences(string tag) => inner.CommittedCrossReferences(tag);

            public FileInfo CrossReferences(string tag) => inner.CrossReferences(tag);

            /// <summary>Always a path that does not exist, whatever is really on disk.</summary>
            public FileInfo SpecificationCatalog(string tag, string document) =>
                new(Path.Combine(inner.Knowledge(tag).FullName, "no-such-spec", document, "index.json"));
        }
    }
}
