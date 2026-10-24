// ------------------------------------------------------------------------------------------------
// <copyright file="OpenApiGeneratorTests.cs" company="Starion Group S.A.">
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

    using Hypha.Knowledge.Api;
    using Hypha.Knowledge.Api.OpenApi;
    using Hypha.Knowledge.Generation;
    using Hypha.Knowledge.Layout;

    using Microsoft.Extensions.Logging.Abstractions;

    /// <summary>
    /// Tests for <see cref="OpenApiGenerator"/> over temporary folders, on the invented document of
    /// <see cref="OpenApiReaderTests"/> - no network, and nothing written into the repository.
    /// </summary>
    [TestFixture]
    public class OpenApiGeneratorTests
    {
        private const string Tag = "1.0";

        private DirectoryInfo root = null!;
        private DirectoryInfo outputRoot = null!;
        private DirectoryInfo secondOutputRoot = null!;

        [SetUp]
        public void SetUp()
        {
            var unique = Guid.NewGuid().ToString("N");
            this.root = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"hypha-openapi-{unique}-in"));
            this.outputRoot = Directory.CreateDirectory(
                Path.Combine(Path.GetTempPath(), $"hypha-openapi-{unique}-out"));
            this.secondOutputRoot = Directory.CreateDirectory(
                Path.Combine(Path.GetTempPath(), $"hypha-openapi-{unique}-out2"));
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var directory in new[] { this.root, this.outputRoot, this.secondOutputRoot })
            {
                directory.Refresh();
                if (directory.Exists)
                {
                    directory.Delete(recursive: true);
                }
            }
        }

        [Test]
        public void Describes_itself_as_the_openapi_artifact()
        {
            var generator = this.Generator(this.outputRoot);

            Assert.Multiple(() =>
            {
                Assert.That(generator.Artifact, Is.EqualTo("openapi"));
                Assert.That(generator.Order, Is.EqualTo(20));
            });
        }

        [Test]
        public async Task Skips_with_a_reason_when_the_openapi_document_is_absent()
        {
            var result = await this.Generator(this.outputRoot).GenerateAsync(Tag);

            Assert.Multiple(() =>
            {
                Assert.That(result.Outcome, Is.EqualTo(GenerationOutcome.Skipped));
                Assert.That(result.Reason, Does.Contain(Tag));
                Assert.That(result.Reason, Does.Contain("hypha api fetch"));
                Assert.That(result.Written, Is.Empty);
                Assert.That(this.Layout(this.outputRoot).OpenApiKnowledge(Tag).Exists, Is.False);
            });
        }

        [Test]
        public async Task Writes_a_page_for_each_api_schema_and_none_for_a_metamodel_one()
        {
            this.SeedDocument();

            var result = await this.Generator(this.outputRoot).GenerateAsync(Tag);

            var schemas = new DirectoryInfo(
                Path.Combine(this.Layout(this.outputRoot).OpenApiKnowledge(Tag).FullName, "schemas"));

            Assert.Multiple(() =>
            {
                Assert.That(result.Outcome, Is.EqualTo(GenerationOutcome.Generated));
                Assert.That(
                    schemas.GetFiles().Select(file => file.Name).Order(StringComparer.Ordinal),
                    Is.EqualTo(new[]
                    {
                        "Choice.md", "Colour.md", "Gadget.md", "Identified.md", "OnlyInExample.md", "Part.md",
                        "Problem.md", "Shared.md", "Tagging.md", "Token.md", "Widget.md", "WidgetDraft.md",
                        "WidgetRequest.md",
                    }));
                Assert.That(File.Exists(Path.Combine(schemas.FullName, "Element.md")), Is.False);
                Assert.That(File.Exists(Path.Combine(schemas.FullName, "Anonymous.md")), Is.False);
            });
        }

        [Test]
        public async Task Writes_the_endpoints_and_both_indexes_and_reports_every_file()
        {
            this.SeedDocument();

            var result = await this.Generator(this.outputRoot).GenerateAsync(Tag);

            var output = this.Layout(this.outputRoot).OpenApiKnowledge(Tag);

            Assert.Multiple(() =>
            {
                Assert.That(File.Exists(Path.Combine(output.FullName, "endpoints.md")), Is.True);
                Assert.That(File.Exists(Path.Combine(output.FullName, "index.md")), Is.True);
                Assert.That(File.Exists(Path.Combine(output.FullName, "index.json")), Is.True);

                // 13 schema pages, the endpoints, and the two indexes - and nothing outside the output.
                Assert.That(result.Written, Has.Count.EqualTo(16));
                Assert.That(
                    result.Written.Select(file => file.FullName).Order(StringComparer.Ordinal),
                    Is.EqualTo(
                        output.GetFiles("*", SearchOption.AllDirectories)
                            .Select(file => file.FullName)
                            .Order(StringComparer.Ordinal)));
            });
        }

        [Test]
        public async Task Writes_nothing_beside_the_input_it_reads()
        {
            this.SeedDocument();

            await this.Generator(this.outputRoot).GenerateAsync(Tag);

            Assert.That(
                this.root.GetFiles("*", SearchOption.AllDirectories).Select(file => file.Name),
                Is.EqualTo(new[] { ApiInputs.OpenApi }));
        }

        [Test]
        public async Task Removes_the_page_of_a_schema_that_is_no_longer_in_the_document()
        {
            this.SeedDocument();
            var schemas = Directory.CreateDirectory(
                Path.Combine(this.Layout(this.outputRoot).OpenApiKnowledge(Tag).FullName, "schemas"));
            var stale = Path.Combine(schemas.FullName, "Removed.md");
            File.WriteAllText(stale, "# Removed\n");

            await this.Generator(this.outputRoot).GenerateAsync(Tag);

            Assert.Multiple(() =>
            {
                Assert.That(File.Exists(stale), Is.False);
                Assert.That(File.Exists(Path.Combine(schemas.FullName, "Widget.md")), Is.True);
            });
        }

        [Test]
        public async Task The_json_index_reads_back_as_the_document_it_was_written_from()
        {
            this.SeedDocument();

            await this.Generator(this.outputRoot).GenerateAsync(Tag);

            var index = ApiJson.Read<OpenApiIndexDocument>(
                File.ReadAllText(this.Layout(this.outputRoot).OpenApiIndex(Tag).FullName));
            var model = new OpenApiReader().Read(OpenApiReaderTests.Document);

            Assert.Multiple(() =>
            {
                Assert.That(index.SchemaVersion, Is.EqualTo(OpenApiIndexDocument.CurrentSchemaVersion));
                Assert.That(index.ApiVersion, Is.EqualTo(Tag));
                Assert.That(index.Title, Is.EqualTo("Toy Widget API"));
                Assert.That(index.Version, Is.EqualTo("9.9"));
                Assert.That(index.OpenApi, Is.EqualTo("3.1.0"));
                Assert.That(index.Counts, Is.EqualTo(new OpenApiIndexCounts(15, 13, 3)));
                Assert.That(index.Schemas.Keys, Is.EqualTo(model.Schemas.Select(schema => schema.Name)));

                var widget = index.Schemas["Widget"];
                Assert.That(widget.File, Is.EqualTo("schemas/Widget.md"));
                Assert.That(widget.Namespace, Is.EqualTo(OpenApiNames.ApiNamespace));
                Assert.That(widget.Kind, Is.EqualTo(OpenApiNames.ObjectKind));
                Assert.That(widget.Role, Is.EqualTo(OpenApiNames.ResponseRole));
                Assert.That(
                    widget.Properties,
                    Is.EqualTo(model.Schemas.Single(schema => schema.Name == "Widget").Properties));
                Assert.That(widget.Variants, Is.Null);
                Assert.That(index.Schemas["Choice"].Variants, Is.EqualTo(new[] { "Part", "Gadget" }));

                Assert.That(index.Endpoints, Has.Count.EqualTo(3));
                Assert.That(index.Endpoints[0].OperationId, Is.EqualTo("getWidgets"));
                Assert.That(index.Endpoints[0].Parameters, Is.EqualTo(model.Endpoints[0].Parameters));
                Assert.That(index.Endpoints[0].Responses, Is.EqualTo(model.Endpoints[0].Responses));
                Assert.That(index.Endpoints[1].Request, Is.EqualTo("WidgetDraft"));
            });
        }

        [Test]
        public async Task A_metamodel_schema_is_indexed_by_name_without_a_file_or_its_members()
        {
            this.SeedDocument();

            await this.Generator(this.outputRoot).GenerateAsync(Tag);

            var json = File.ReadAllText(this.Layout(this.outputRoot).OpenApiIndex(Tag).FullName);
            var index = ApiJson.Read<OpenApiIndexDocument>(json);
            using var raw = JsonDocument.Parse(json);
            var element = raw.RootElement.GetProperty("schemas").GetProperty("Element");

            Assert.Multiple(() =>
            {
                Assert.That(index.Schemas["Element"].Namespace, Is.EqualTo(OpenApiNames.MetamodelNamespace));
                Assert.That(index.Schemas["Element"].File, Is.Null);
                Assert.That(index.Schemas["Element"].Properties, Is.Null);

                // Absent rather than null: the members are left out of the file altogether.
                Assert.That(element.TryGetProperty("file", out _), Is.False);
                Assert.That(element.TryGetProperty("properties", out _), Is.False);
                Assert.That(element.TryGetProperty("variants", out _), Is.False);
                Assert.That(element.GetProperty("role").GetString(), Is.EqualTo(OpenApiNames.ResponseRole));
                Assert.That(
                    raw.RootElement.GetProperty("schemas").GetProperty("Widget").TryGetProperty("file", out _),
                    Is.True);
            });
        }

        [Test]
        public async Task Every_file_is_written_with_line_feeds_and_without_a_byte_order_mark()
        {
            this.SeedDocument();

            var result = await this.Generator(this.outputRoot).GenerateAsync(Tag);

            Assert.Multiple(() =>
            {
                foreach (var file in result.Written)
                {
                    var bytes = File.ReadAllBytes(file.FullName);

                    Assert.That(bytes, Does.Not.Contain((byte)'\r'), file.Name);
                    Assert.That(bytes[0], Is.Not.EqualTo(0xEF), file.Name);
                    Assert.That(bytes[^1], Is.EqualTo((byte)'\n'), file.Name);
                }
            });
        }

        [Test]
        public async Task Two_runs_into_independent_folders_are_byte_identical()
        {
            this.SeedDocument();

            await this.Generator(this.outputRoot).GenerateAsync(Tag);
            await this.Generator(this.secondOutputRoot).GenerateAsync(Tag);

            var first = Snapshot(this.Layout(this.outputRoot).OpenApiKnowledge(Tag));
            var second = Snapshot(this.Layout(this.secondOutputRoot).OpenApiKnowledge(Tag));

            Assert.Multiple(() =>
            {
                Assert.That(first, Has.Count.EqualTo(16));
                Assert.That(second.Keys, Is.EqualTo(first.Keys));

                foreach (var (path, bytes) in first)
                {
                    Assert.That(second[path], Is.EqualTo(bytes), path);
                }
            });
        }

        [Test]
        public async Task Running_again_over_its_own_output_changes_nothing()
        {
            this.SeedDocument();
            var generator = this.Generator(this.outputRoot);

            await generator.GenerateAsync(Tag);
            var first = Snapshot(this.Layout(this.outputRoot).OpenApiKnowledge(Tag));

            await generator.GenerateAsync(Tag);
            var second = Snapshot(this.Layout(this.outputRoot).OpenApiKnowledge(Tag));

            Assert.Multiple(() =>
            {
                Assert.That(second.Keys, Is.EqualTo(first.Keys));

                foreach (var (path, bytes) in first)
                {
                    Assert.That(second[path], Is.EqualTo(bytes), path);
                }
            });
        }

        private static SortedDictionary<string, byte[]> Snapshot(DirectoryInfo folder)
        {
            var files = new SortedDictionary<string, byte[]>(StringComparer.Ordinal);

            foreach (var file in folder.GetFiles("*", SearchOption.AllDirectories))
            {
                files[Path.GetRelativePath(folder.FullName, file.FullName).Replace('\\', '/')] =
                    File.ReadAllBytes(file.FullName);
            }

            return files;
        }

        private ApiLayout Layout(DirectoryInfo output) => new(new KnowledgeLayout(this.root, output));

        private OpenApiGenerator Generator(DirectoryInfo output) => new(
            this.Layout(output), new OpenApiReader(), new OpenApiRenderer(), NullLogger<OpenApiGenerator>.Instance);

        private void SeedDocument()
        {
            var source = this.Layout(this.outputRoot).OpenApi(Tag);
            source.Directory!.Create();
            File.WriteAllText(source.FullName, OpenApiReaderTests.Document);
        }
    }
}
