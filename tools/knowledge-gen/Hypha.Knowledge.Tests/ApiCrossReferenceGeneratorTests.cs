// ------------------------------------------------------------------------------------------------
// <copyright file="ApiCrossReferenceGeneratorTests.cs" company="Starion Group S.A.">
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
    using Hypha.Knowledge.Api.CrossReferences;
    using Hypha.Knowledge.Generation;
    using Hypha.Knowledge.Layout;

    using Microsoft.Extensions.Logging.Abstractions;

    /// <summary>
    /// Tests for <see cref="ApiCrossReferenceGenerator"/> over temporary folders, with the real
    /// builder and hand-written inputs - no network, and nothing written into the repository.
    /// </summary>
    [TestFixture]
    public class ApiCrossReferenceGeneratorTests
    {
        private const string Tag = "1.0";

        /// <summary>
        /// An invented <c>pim/metamodel.json</c>, with members the cross-references do not read.
        /// </summary>
        private const string PimJson = """
            {
              "schemaVersion": "1.0.0",
              "classes": [
                {
                  "name": "Project",
                  "isAbstract": false,
                  "ownedAttributes": [
                    { "name": "name", "type": "String", "lower": 1, "upper": 1, "isDerived": false },
                    { "name": "commits", "type": "Commit", "lower": 0, "upper": -1, "isDerived": true }
                  ],
                  "inheritedAttributes": [
                    { "name": "id", "lower": 1, "upper": 1, "isDerived": false, "inheritedFrom": "Record" }
                  ],
                  "ownedOperations": []
                },
                { "name": "Commit", "ownedAttributes": [ { "name": "message", "lower": 1, "upper": 1, "isDerived": false } ] },
                {
                  "name": "ProjectService",
                  "ownedOperations": [ { "name": "getProjects", "parameters": [] }, { "name": "mergeProject" } ]
                },
                { "name": "ElementService", "ownedOperations": [ { "name": "getById" } ] }
              ],
              "enumerations": [ { "name": "ChangeKind", "literals": [ "added" ] } ]
            }
            """;

        private DirectoryInfo root = null!;
        private DirectoryInfo outputRoot = null!;
        private ApiLayout layout = null!;
        private ApiCrossReferenceGenerator generator = null!;

        [SetUp]
        public void SetUp()
        {
            var unique = Guid.NewGuid().ToString("N");
            this.root = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"hypha-api-xref-{unique}-in"));
            this.outputRoot = Directory.CreateDirectory(
                Path.Combine(Path.GetTempPath(), $"hypha-api-xref-{unique}-out"));

            this.layout = new ApiLayout(new KnowledgeLayout(this.root, this.outputRoot));
            this.generator = new ApiCrossReferenceGenerator(
                this.layout, new ApiCrossReferenceBuilder(), NullLogger<ApiCrossReferenceGenerator>.Instance);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var directory in new[] { this.root, this.outputRoot })
            {
                directory.Refresh();
                if (directory.Exists)
                {
                    directory.Delete(recursive: true);
                }
            }
        }

        [Test]
        public void Describes_itself_as_the_last_api_artifact()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.generator.Artifact, Is.EqualTo("cross-references"));
                Assert.That(this.generator.Order, Is.EqualTo(40));
            });
        }

        [Test]
        public async Task Skips_with_a_reason_when_the_pim_knowledge_is_absent()
        {
            // The two other inputs alone are not enough.
            this.SeedOpenApiIndex();
            this.SeedSpecification();

            var result = await this.generator.GenerateAsync(Tag);

            Assert.Multiple(() =>
            {
                Assert.That(result.Outcome, Is.EqualTo(GenerationOutcome.Skipped));
                Assert.That(result.Reason, Does.Contain("PIM"));
                Assert.That(result.Reason, Does.Contain(Tag));
                Assert.That(this.layout.CrossReferences(Tag).Exists, Is.False);
            });
        }

        [Test]
        public async Task Generates_from_the_pim_alone()
        {
            this.SeedPim();

            var result = await this.generator.GenerateAsync(Tag);
            var document = this.ReadDocument();

            Assert.Multiple(() =>
            {
                Assert.That(result.Outcome, Is.EqualTo(GenerationOutcome.Generated));
                Assert.That(result.Written, Has.Count.EqualTo(1));
                Assert.That(result.Written[0].FullName, Is.EqualTo(this.layout.CrossReferences(Tag).FullName));

                Assert.That(document.ApiVersion, Is.EqualTo(Tag));
                Assert.That(document.Sources, Is.EqualTo(new ApiCrossReferenceSources(true, false, false)));
                Assert.That(
                    document.Types.Keys,
                    Is.EqualTo(new[] { "ChangeKind", "Commit", "ElementService", "Project", "ProjectService" }));
                Assert.That(document.Types["ChangeKind"].Kind, Is.EqualTo("enumeration"));
                Assert.That(
                    document.Operations.Keys,
                    Is.EqualTo(new[]
                    {
                        "ElementService.getById", "ProjectService.getProjects", "ProjectService.mergeProject",
                    }));
                Assert.That(document.Types.Values.Select(type => type.Schema), Has.All.Null);
                Assert.That(document.Types.Values.SelectMany(type => type.Clauses), Is.Empty);
                Assert.That(document.Counts, Is.EqualTo(new ApiCrossReferenceCounts(5, 0, 0, 3, 0)));
            });
        }

        [Test]
        public async Task Reads_the_openapi_index_when_it_is_present()
        {
            this.SeedPim();
            this.SeedOpenApiIndex();

            await this.generator.GenerateAsync(Tag);
            var document = this.ReadDocument();

            Assert.Multiple(() =>
            {
                Assert.That(document.Sources, Is.EqualTo(new ApiCrossReferenceSources(true, true, false)));
                Assert.That(
                    document.Types["Commit"].Schema,
                    Is.EqualTo(new ApiSchemaEdge("Commit", ApiCrossReferenceDocument.SameNameMethod, null, null)));
                Assert.That(document.Types["Commit"].PsmOnly, Is.EqualTo(new[] { "@id" }));
                Assert.That(
                    document.Types["Project"].Schema!.RequestMethod,
                    Is.EqualTo(ApiCrossReferenceDocument.TypeConstantMethod));
            });
        }

        [Test]
        public async Task Reads_the_clause_catalog_and_the_clause_files_when_they_are_present()
        {
            this.SeedPim();
            this.SeedOpenApiIndex();
            this.SeedSpecification();

            await this.generator.GenerateAsync(Tag);
            var document = this.ReadDocument();

            Assert.Multiple(() =>
            {
                Assert.That(document.Sources, Is.EqualTo(new ApiCrossReferenceSources(true, true, true)));

                // The title comes from the catalog, the definition and the tables from the files.
                Assert.That(
                    document.Types["Project"].Clauses,
                    Is.EqualTo(new[]
                    {
                        new ApiClauseEdge("7.1", ApiCrossReferenceDocument.TitleMethod),
                        new ApiClauseEdge("7.10", ApiCrossReferenceDocument.BoldMentionMethod),
                    }));
                Assert.That(
                    document.Types["Project"].Schema!.ResponseMethod,
                    Is.EqualTo(ApiCrossReferenceDocument.MappingTableMethod));
                Assert.That(
                    document.Types["Project"].Members!.Select(member => member.Name),
                    Is.EqualTo(new[] { "commits", "id", "name" }));
                Assert.That(
                    document.Types["Project"].Members!.Single(member => member.Name == "id").InheritedFrom,
                    Is.EqualTo("Record"));
                Assert.That(
                    document.Types["Project"].Members!.Single(member => member.Name == "commits").Multiplicity,
                    Is.EqualTo("[0..*]"));
                Assert.That(
                    document.Operations["ProjectService.getProjects"].Clauses,
                    Is.EqualTo(new[] { new ApiClauseEdge("7.10", ApiCrossReferenceDocument.SignatureMethod) }));
                Assert.That(
                    document.Operations["ProjectService.getProjects"].Endpoints,
                    Is.EqualTo(new[] { new ApiEndpointEdge("GET", "/projects", "listProjects") }));
                Assert.That(document.Operations["ProjectService.mergeProject"].MappingClause, Is.EqualTo("8.2"));
                Assert.That(document.ModelMapping, Has.Count.EqualTo(4));
            });
        }

        [Test]
        public async Task A_clause_the_catalog_lists_but_whose_file_is_missing_is_left_out()
        {
            this.SeedPim();
            this.SeedSpecification();
            File.Delete(Path.Combine(this.layout.SpecificationText(Tag).FullName, "07.1-project.md"));

            var result = await this.generator.GenerateAsync(Tag);

            Assert.Multiple(() =>
            {
                Assert.That(result.Outcome, Is.EqualTo(GenerationOutcome.Generated));
                Assert.That(
                    this.ReadDocument().Types["Project"].Clauses.Select(edge => edge.Clause),
                    Is.EqualTo(new[] { "7.10" }));
            });
        }

        [Test]
        public async Task A_catalog_without_entries_counts_as_no_specification()
        {
            this.SeedPim();
            var catalog = this.layout.SpecificationCatalog(Tag);
            catalog.Directory!.Create();
            File.WriteAllText(catalog.FullName, "{ \"document\": \"api\" }\n");

            await this.generator.GenerateAsync(Tag);

            Assert.That(this.ReadDocument().Sources.Specification, Is.False);
        }

        [Test]
        public async Task The_written_file_carries_no_clause_text()
        {
            this.SeedPim();
            this.SeedOpenApiIndex();
            this.SeedSpecification();

            await this.generator.GenerateAsync(Tag);

            var json = File.ReadAllText(this.layout.CrossReferences(Tag).FullName);

            Assert.Multiple(() =>
            {
                foreach (var word in new[] { "wombat", "quokka", "narwhal", "axolotl", "capybara", "Zanzibar" })
                {
                    Assert.That(json, Does.Not.Contain(word), word);
                }
            });
        }

        [Test]
        public async Task The_file_is_written_with_line_feeds_and_without_a_byte_order_mark()
        {
            this.SeedPim();
            this.SeedOpenApiIndex();
            this.SeedSpecification();

            await this.generator.GenerateAsync(Tag);

            var bytes = File.ReadAllBytes(this.layout.CrossReferences(Tag).FullName);

            Assert.Multiple(() =>
            {
                Assert.That(bytes, Does.Not.Contain((byte)'\r'));
                Assert.That(bytes[0], Is.EqualTo((byte)'{'));
                Assert.That(bytes[^1], Is.EqualTo((byte)'\n'));
            });
        }

        [Test]
        public async Task Two_runs_are_byte_identical()
        {
            this.SeedPim();
            this.SeedOpenApiIndex();
            this.SeedSpecification();

            await this.generator.GenerateAsync(Tag);
            var first = File.ReadAllBytes(this.layout.CrossReferences(Tag).FullName);

            await this.generator.GenerateAsync(Tag);
            var second = File.ReadAllBytes(this.layout.CrossReferences(Tag).FullName);

            Assert.Multiple(() =>
            {
                Assert.That(first, Is.Not.Empty);
                Assert.That(second, Is.EqualTo(first));
            });
        }

        [Test]
        public async Task The_order_the_catalog_lists_the_clauses_in_does_not_change_the_output()
        {
            this.SeedPim();
            this.SeedOpenApiIndex();
            this.SeedSpecification();

            await this.generator.GenerateAsync(Tag);
            var first = File.ReadAllBytes(this.layout.CrossReferences(Tag).FullName);

            this.SeedSpecification(reversed: true);
            await this.generator.GenerateAsync(Tag);
            var second = File.ReadAllBytes(this.layout.CrossReferences(Tag).FullName);

            Assert.That(second, Is.EqualTo(first));
        }

        private ApiCrossReferenceDocument ReadDocument() =>
            ApiJson.Read<ApiCrossReferenceDocument>(File.ReadAllText(this.layout.CrossReferences(Tag).FullName));

        private void SeedPim()
        {
            var file = this.layout.PimGraph(Tag);
            file.Directory!.Create();
            File.WriteAllText(file.FullName, PimJson);
        }

        private void SeedOpenApiIndex()
        {
            var file = this.layout.OpenApiIndex(Tag);
            file.Directory!.Create();
            File.WriteAllText(file.FullName, ApiJson.Render(ApiCrossReferenceBuilderTests.OpenApi));
        }

        /// <summary>
        /// Writes the clause catalog and one file per clause, as the specification extraction lays
        /// them out: <c>entries</c> keyed by clause number, each naming its title and its file.
        /// </summary>
        private void SeedSpecification(bool reversed = false)
        {
            var folder = this.layout.SpecificationText(Tag);
            folder.Create();

            var clauses = new[]
            {
                (File: "07.1-project.md", Clause: ApiCrossReferenceBuilderTests.ProjectClause),
                (File: "07.10-projectservice.md", Clause: ApiCrossReferenceBuilderTests.ServiceClause),
                (File: "08.1-mapping.md", Clause: ApiCrossReferenceBuilderTests.ModelMappingClause),
                (File: "08.2-mapping.md", Clause: ApiCrossReferenceBuilderTests.EndpointMappingClause),
            };

            foreach (var (file, clause) in clauses)
            {
                File.WriteAllText(Path.Combine(folder.FullName, file), clause.Text);
            }

            var entries = (reversed ? Enumerable.Reverse(clauses) : clauses)
                .Select(entry =>
                    $"\"{entry.Clause.Number}\": {{ \"title\": \"{entry.Clause.Title}\", \"file\": \"{entry.File}\" }}");

            File.WriteAllText(
                this.layout.SpecificationCatalog(Tag).FullName,
                $"{{ \"document\": \"api\", \"entries\": {{ {string.Join(", ", entries)} }} }}\n");
        }
    }
}
