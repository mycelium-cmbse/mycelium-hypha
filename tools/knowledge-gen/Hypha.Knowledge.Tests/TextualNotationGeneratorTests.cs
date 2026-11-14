// ------------------------------------------------------------------------------------------------
// <copyright file="TextualNotationGeneratorTests.cs" company="Starion Group S.A.">
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
    using System.Text.Json;
    using System.Threading.Tasks;

    using Hypha.Knowledge.CrossReferences;
    using Hypha.Knowledge.Generation;
    using Hypha.Knowledge.Grammar;
    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.TextualNotation;

    using Microsoft.Extensions.Logging.Abstractions;

    [TestFixture]
    public class TextualNotationGeneratorTests
    {
        private const string Tag = "2026-05";

        private const string Vehicle = "sysml-src-examples-vehicle-example-vehicle";

        private const string Camera = "kerml-src-examples-camera";

        private DirectoryInfo root = null!;
        private KnowledgeLayout layout = null!;

        [SetUp]
        public void SetUp()
        {
            this.root = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"hypha-notation-{Guid.NewGuid():N}"));
            this.layout = new KnowledgeLayout(this.root);

            // An upper-case extension, a folder with a space, and a library model that is no example.
            this.WriteModel("sysml/src/examples/Vehicle Example/Vehicle.sysml", "part def Vehicle;\n");
            this.WriteModel("kerml/src/examples/Camera.KERML", "class Camera;\n");
            this.WriteModel("sysml.library/Kernel/Base.kerml", "package Base;\n");

            var index = this.layout.MetamodelIndex(Tag);
            index.Directory!.Create();
            File.WriteAllText(
                index.FullName,
                """
                {
                  "schemaVersion": "2.1.0",
                  "modelVersionUri": "https://www.omg.org/spec/SysML/20250201",
                  "entries": {
                    "PartDefinition": { "kind": "class", "qualifiedName": "SysML::Systems::Parts::PartDefinition", "file": "elements/PartDefinition.md" },
                    "Class": { "kind": "class", "qualifiedName": "KerML::Kernel::Classes::Class", "file": "elements/Class.md" }
                  }
                }
                """);
        }

        [TearDown]
        public void TearDown()
        {
            this.root.Refresh();
            if (this.root.Exists)
            {
                this.root.Delete(recursive: true);
            }
        }

        [Test]
        public async Task The_example_index_names_each_model_by_its_page_and_its_own_name()
        {
            var result = await this.Generator().GenerateAsync(Tag);

            var file = this.layout.ExamplesIndex(Tag);
            using var json = JsonDocument.Parse(File.ReadAllText(file.FullName));
            var document = json.RootElement;
            var examples = document.GetProperty("examples");
            var vehicle = examples.GetProperty(Vehicle);

            Assert.Multiple(() =>
            {
                Assert.That(result.Written.Select(written => written.FullName), Does.Contain(file.FullName));
                Assert.That(document.GetProperty("schemaVersion").GetString(), Is.EqualTo(ExampleDocument.CurrentSchemaVersion));
                Assert.That(document.GetProperty("tag").GetString(), Is.EqualTo(Tag));
                Assert.That(examples.EnumerateObject().Select(example => example.Name), Is.EqualTo(new[] { Camera, Vehicle }), "ordinal, and no library model");
                Assert.That(vehicle.GetProperty("name").GetString(), Is.EqualTo("Vehicle"));
                Assert.That(vehicle.GetProperty("language").GetString(), Is.EqualTo("SysML"));
                Assert.That(vehicle.GetProperty("source").GetString(), Is.EqualTo("sysml/src/examples/Vehicle Example/Vehicle.sysml"));
                Assert.That(vehicle.GetProperty("file").GetString(), Is.EqualTo(Vehicle + ".md"));
                Assert.That(vehicle.GetProperty("elements").EnumerateArray().Select(element => element.GetString()), Is.EqualTo(new[] { "PartDefinition" }));
                Assert.That(examples.GetProperty(Camera).GetProperty("language").GetString(), Is.EqualTo("KerML"));
                Assert.That(File.ReadAllText(file.FullName), Does.Not.Contain("\r"));
            });
        }

        [Test]
        public async Task Every_indexed_example_points_at_a_written_page_whose_front_matter_agrees()
        {
            await this.Generator().GenerateAsync(Tag);

            using var json = JsonDocument.Parse(File.ReadAllText(this.layout.ExamplesIndex(Tag).FullName));

            Assert.Multiple(() =>
            {
                foreach (var example in json.RootElement.GetProperty("examples").EnumerateObject())
                {
                    var page = new FileInfo(Path.Combine(this.layout.Examples(Tag).FullName, example.Value.GetProperty("file").GetString()!));
                    Assert.That(page.Exists, Is.True, example.Name);

                    var front = FrontMatter(page);
                    var elements = example.Value.GetProperty("elements").EnumerateArray().Select(element => element.GetString());

                    Assert.That(front["name"], Is.EqualTo(example.Value.GetProperty("name").GetString()), example.Name);
                    Assert.That(front["source"], Is.EqualTo(example.Value.GetProperty("source").GetString()), example.Name);
                    Assert.That(front["elements"], Is.EqualTo($"[{string.Join(", ", elements)}]"), example.Name);
                }
            });
        }

        [Test]
        public async Task The_index_outlives_the_sweep_of_stale_pages_and_names_only_the_models_left()
        {
            await this.Generator().GenerateAsync(Tag);
            File.Delete(Path.Combine(this.layout.TextualSources(Tag).FullName, "kerml", "src", "examples", "Camera.KERML"));

            await this.Generator().GenerateAsync(Tag);

            using var json = JsonDocument.Parse(File.ReadAllText(this.layout.ExamplesIndex(Tag).FullName));
            Assert.Multiple(() =>
            {
                Assert.That(json.RootElement.GetProperty("examples").EnumerateObject().Select(example => example.Name), Is.EqualTo(new[] { Vehicle }));
                Assert.That(File.Exists(Path.Combine(this.layout.Examples(Tag).FullName, Camera + ".md")), Is.False);
            });
        }

        private static Dictionary<string, string> FrontMatter(FileInfo page) =>
            File.ReadLines(page.FullName)
                .Skip(1)
                .TakeWhile(line => line != "---")
                .Select(line => line.Split(": ", 2))
                .ToDictionary(parts => parts[0], parts => parts[1], StringComparer.Ordinal);

        private TextualNotationGenerator Generator() =>
            new(
                this.layout,
                new ModelCatalog(),
                new SurfaceForms(),
                new NotationRenderer(),
                new GrammarParser(),
                new KnowledgeReader(),
                NullLogger<TextualNotationGenerator>.Instance);

        private void WriteModel(string relative, string text)
        {
            var file = new FileInfo(Path.Combine(this.layout.TextualSources(Tag).FullName, relative.Replace('/', Path.DirectorySeparatorChar)));
            file.Directory!.Create();
            File.WriteAllText(file.FullName, text);
        }
    }
}
