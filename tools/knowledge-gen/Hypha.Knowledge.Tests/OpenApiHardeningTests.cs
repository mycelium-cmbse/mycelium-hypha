// ------------------------------------------------------------------------------------------------
// <copyright file="OpenApiHardeningTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Tests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Api;
    using Hypha.Knowledge.Api.OpenApi;
    using Hypha.Knowledge.Layout;

    using Microsoft.Extensions.Logging.Abstractions;

    /// <summary>
    /// Suite of tests for what the OpenAPI knowledge does with a document that is unlike the published
    /// one: parameters declared on the path rather than the operation, and schema names that cannot be
    /// a page name.
    /// </summary>
    [TestFixture]
    public class OpenApiHardeningTests
    {
        private const string Tag = "2026-04";

        private DirectoryInfo workspace = null!;
        private ApiLayout layout = null!;
        private OpenApiGenerator generator = null!;

        [SetUp]
        public void SetUp()
        {
            this.workspace = Directory.CreateDirectory(
                Path.Combine(Path.GetTempPath(), $"hypha-openapi-hardening-{Guid.NewGuid():N}"));
            this.layout = new ApiLayout(new KnowledgeLayout(this.workspace));
            this.generator = new OpenApiGenerator(
                this.layout, new OpenApiReader(), new OpenApiRenderer(), NullLogger<OpenApiGenerator>.Instance);
        }

        [TearDown]
        public void TearDown()
        {
            this.workspace.Refresh();
            if (this.workspace.Exists)
            {
                this.workspace.Delete(recursive: true);
            }
        }

        [Test]
        public void Parameters_declared_on_the_path_apply_to_every_operation_on_it()
        {
            var model = new OpenApiReader().Read(
                """
                {
                  "paths": {
                    "/widgets/{widgetId}": {
                      "parameters": [
                        { "name": "widgetId", "in": "path", "required": true, "schema": { "type": "string" } }
                      ],
                      "get": {
                        "operationId": "readWidget",
                        "parameters": [ { "name": "depth", "in": "query", "schema": { "type": "integer" } } ]
                      },
                      "delete": { "operationId": "dropWidget" }
                    }
                  },
                  "components": { "schemas": {} }
                }
                """);

            Assert.Multiple(() =>
            {
                Assert.That(
                    model.Endpoints.Single(endpoint => endpoint.Method == "GET").Parameters.Select(parameter => parameter.Name),
                    Is.EqualTo(new[] { "widgetId", "depth" }),
                    "the path's parameters, then the operation's own");
                Assert.That(
                    model.Endpoints.Single(endpoint => endpoint.Method == "DELETE").Parameters.Select(parameter => parameter.Name),
                    Is.EqualTo(new[] { "widgetId" }));
            });
        }

        [TestCase("../Escape")]
        [TestCase("Sub/Widget")]
        [TestCase(".hidden")]
        [TestCase("Wid get")]
        public void A_schema_whose_name_cannot_be_a_page_name_is_refused_and_nothing_is_written(string name)
        {
            this.Seed(name);

            Assert.Multiple(() =>
            {
                Assert.That(
                    () => this.generator.GenerateAsync(Tag),
                    Throws.InvalidOperationException.With.Message.Contains(name));
                Assert.That(
                    Directory.Exists(this.layout.OpenApiKnowledge(Tag).FullName)
                        ? Directory.GetFiles(this.layout.OpenApiKnowledge(Tag).FullName, "*", SearchOption.AllDirectories)
                        : [],
                    Is.Empty);
                Assert.That(
                    Directory.GetFiles(this.workspace.FullName, "*.md", SearchOption.AllDirectories),
                    Is.Empty,
                    "nothing may be written outside the schemas folder either");
            });
        }

        [Test]
        public void Two_schemas_whose_names_differ_only_by_case_are_refused()
        {
            // On a case-insensitive disk they would be one file: two index entries, one page.
            this.Seed("Widget", "widget");

            Assert.That(
                () => this.generator.GenerateAsync(Tag),
                Throws.InvalidOperationException.With.Message.Contains("differ only by case"));
        }

        [Test]
        public async Task A_metamodel_schema_with_an_unusual_name_does_not_stop_generation()
        {
            // Only schemas that get a page have to be usable as a file name.
            var file = this.layout.OpenApi(Tag);
            file.Directory!.Create();
            await File.WriteAllTextAsync(
                file.FullName,
                """
                {
                  "paths": {},
                  "components": { "schemas": {
                    "Odd/Name": { "$id": "https://example.org/spec/KerML/20990101/Odd", "type": "object" },
                    "Widget": { "$id": "https://example.org/spec/SystemsModelingAPI/20990101/Widget", "type": "object" }
                  } }
                }
                """);

            await this.generator.GenerateAsync(Tag);

            Assert.That(
                File.Exists(Path.Combine(this.layout.OpenApiKnowledge(Tag).FullName, "schemas", "Widget.md")), Is.True);
        }

        private void Seed(params string[] apiSchemaNames)
        {
            var schemas = string.Join(
                ",\n",
                apiSchemaNames.Select(name =>
                    $$"""    "{{name}}": { "$id": "https://example.org/spec/SystemsModelingAPI/20990101/X", "type": "object" }"""));

            var file = this.layout.OpenApi(Tag);
            file.Directory!.Create();
            File.WriteAllText(
                file.FullName,
                "{ \"paths\": {}, \"components\": { \"schemas\": {\n" + schemas + "\n} } }");
        }
    }
}
