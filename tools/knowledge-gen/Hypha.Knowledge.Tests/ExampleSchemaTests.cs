// ------------------------------------------------------------------------------------------------
// <copyright file="ExampleSchemaTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.Json;

    using Hypha.Knowledge.TextualNotation;

    // knowledge/examples.schema.json is hand-written; this keeps it and the generated document in step.
    [TestFixture]
    public class ExampleSchemaTests
    {
        private JsonDocument schema = null!;
        private JsonDocument produced = null!;

        [SetUp]
        public void SetUp()
        {
            this.schema = JsonDocument.Parse(File.ReadAllText(Repository.Layout!.ExamplesSchema.FullName, Encoding.UTF8));

            var document = new ExampleDocument(
                ExampleDocument.CurrentSchemaVersion,
                "2026-05",
                new SortedDictionary<string, ExampleIndexEntry>(StringComparer.Ordinal)
                {
                    ["kerml-src-examples-camera"] = new("Camera", "KerML", "kerml/src/examples/Camera.kerml", "kerml-src-examples-camera.md", ["Class"]),
                });

            this.produced = JsonDocument.Parse(ExampleFile.Render(document));
        }

        [TearDown]
        public void TearDown()
        {
            this.schema?.Dispose();
            this.produced?.Dispose();
        }

        [Test]
        public void The_document_has_exactly_the_required_top_level_keys() =>
            Assert.That(Keys(this.produced.RootElement), Is.EqualTo(Required(this.schema.RootElement)));

        [Test]
        public void An_example_has_exactly_the_keys_the_schema_requires()
        {
            var example = this.produced.RootElement.GetProperty("examples").GetProperty("kerml-src-examples-camera");
            var definition = this.schema.RootElement.GetProperty("$defs").GetProperty("example");

            Assert.Multiple(() =>
            {
                Assert.That(Keys(example), Is.EqualTo(Required(definition)));
                Assert.That(
                    definition.GetProperty("properties").GetProperty("language").GetProperty("enum").EnumerateArray().Select(value => value.GetString()),
                    Does.Contain(example.GetProperty("language").GetString()));
            });
        }

        [Test]
        public void The_file_ends_with_one_line_feed_and_has_no_carriage_return()
        {
            var text = ExampleFile.Render(new ExampleDocument(ExampleDocument.CurrentSchemaVersion, "2026-05", new Dictionary<string, ExampleIndexEntry>()));

            Assert.Multiple(() =>
            {
                Assert.That(text, Does.Not.Contain("\r"));
                Assert.That(text, Does.EndWith("}\n").And.Not.EndWith("\n\n"));
            });
        }

        private static List<string> Keys(JsonElement element) =>
            [.. element.EnumerateObject().Select(property => property.Name).Order(StringComparer.Ordinal)];

        private static List<string> Required(JsonElement definition) =>
            [.. definition.GetProperty("required").EnumerateArray().Select(value => value.GetString()!).Order(StringComparer.Ordinal)];
    }
}
