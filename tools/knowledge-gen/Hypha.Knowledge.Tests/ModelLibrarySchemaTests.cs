// ------------------------------------------------------------------------------------------------
// <copyright file="ModelLibrarySchemaTests.cs" company="Starion Group S.A.">
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
    using System.Linq;
    using System.Text;
    using System.Text.Json;

    using Hypha.Knowledge.ModelLibrary;

    /// <summary>
    /// Keeps the hand-written <c>model-library.schema.json</c> honest without pulling in a
    /// JSON-Schema validator.
    /// </summary>
    /// <remarks>
    /// Checks the shape the document actually produces against the <c>required</c> lists the schema
    /// declares, so adding a field to one and not the other fails here - the same technique
    /// <c>CrossReferenceSchemaTests</c> uses for <c>cross-references.schema.json</c>.
    /// </remarks>
    [TestFixture]
    public class ModelLibrarySchemaTests
    {
        private JsonDocument schema = null!;
        private JsonDocument produced = null!;

        [SetUp]
        public void SetUp()
        {
            var schemaFile = Repository.Layout!.ModelLibrarySchema;

            this.schema = JsonDocument.Parse(File.ReadAllText(schemaFile.FullName, Encoding.UTF8));

            var document = new ModelLibraryDocument(
                ModelLibraryDocument.CurrentSchemaVersion,
                "2026-05",
                new ModelLibraryCounts(Files: 1, Declarations: 1),
                new Dictionary<string, ModelLibraryDeclarationEntry>
                {
                    ["ScalarValues::Real"] = new ModelLibraryDeclarationEntry(
                        "datatype",
                        "packages/scalarvalues.md",
                        "sysml.library/Kernel Libraries/Kernel Data Type Library/ScalarValues.kerml"),
                });

            this.produced = JsonDocument.Parse(ModelLibraryFile.Render(document));
        }

        [TearDown]
        public void TearDown()
        {
            this.schema?.Dispose();
            this.produced?.Dispose();
        }

        private static IEnumerable<string> Keys(JsonElement element) =>
            element.EnumerateObject().Select(property => property.Name).Order(System.StringComparer.Ordinal);

        private static IEnumerable<string> Required(JsonElement element) =>
            element.GetProperty("required").EnumerateArray()
                .Select(item => item.GetString()!)
                .Order(System.StringComparer.Ordinal);

        [Test]
        public void The_document_has_exactly_the_required_top_level_keys() =>
            Assert.That(Keys(this.produced.RootElement), Is.EqualTo(Required(this.schema.RootElement)));

        [Test]
        public void The_counts_block_matches_the_schema() =>
            Assert.That(
                Keys(this.produced.RootElement.GetProperty("counts")),
                Is.EqualTo(Required(this.schema.RootElement.GetProperty("properties").GetProperty("counts"))));

        [Test]
        public void A_declaration_entry_matches_the_schema()
        {
            var entry = this.produced.RootElement.GetProperty("declarations").GetProperty("ScalarValues::Real");
            var definition = this.schema.RootElement.GetProperty("$defs").GetProperty("entry");

            Assert.That(Keys(entry), Is.EqualTo(Required(definition)));
        }
    }
}
