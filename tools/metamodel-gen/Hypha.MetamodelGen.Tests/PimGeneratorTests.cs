// ------------------------------------------------------------------------------------------------
// <copyright file="PimGeneratorTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.MetamodelGen.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge;
    using Hypha.Knowledge.Api;
    using Hypha.Knowledge.Generation;
    using Hypha.MetamodelGen.Generation;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging.Abstractions;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="PimGenerator"/>: what it writes for an installed API version,
    /// where it writes it, and what it does with a version it cannot generate.
    /// </summary>
    /// <remarks>
    /// Run end to end over the hand-written fixture (see <see cref="PimFixture"/>), laid out in a
    /// temporary folder the way <c>hypha api fetch</c> would have left it. Nothing is read from or
    /// written to the repository's own <c>sources/</c> and <c>knowledge/</c>, other than the shared
    /// primitive types being copied out of it.
    /// </remarks>
    [TestFixture]
    public class PimGeneratorTests
    {
        private const string Tag = PimFixture.Tag;

        private DirectoryInfo workspace = null!;
        private ApiLayout layout = null!;
        private PimGenerator generator = null!;

        [SetUp]
        public void SetUp()
        {
            if (Repository.Layout is null)
            {
                Assert.Ignore("The tests are not running inside a checkout.");
            }

            this.workspace = Directory.CreateDirectory(
                Path.Combine(Path.GetTempPath(), $"hypha-pim-{Guid.NewGuid():N}"));

            this.layout = PimFixture.Install(this.workspace);
            this.generator = new PimGenerator(this.layout, NullLogger<PimGenerator>.Instance);
        }

        [TearDown]
        public void TearDown()
        {
            this.workspace?.Refresh();
            if (this.workspace is { Exists: true })
            {
                this.workspace.Delete(recursive: true);
            }
        }

        [Test]
        public void The_pim_is_the_first_api_artifact()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.generator.Artifact, Is.EqualTo("pim"));
                Assert.That(this.generator.Order, Is.EqualTo(10));
            });
        }

        [Test]
        public async Task The_pim_knowledge_is_generated_in_the_shape_of_the_metamodel_knowledge()
        {
            var result = await this.generator.GenerateAsync(Tag);

            var pim = this.layout.Pim(Tag);

            Assert.Multiple(() =>
            {
                Assert.That(result.Outcome, Is.EqualTo(GenerationOutcome.Generated));
                Assert.That(result.Reason, Is.Null);

                Assert.That(this.Relative(pim), Is.EqualTo($"knowledge/api/{Tag}/pim"));

                foreach (var expected in new[]
                {
                    "index.md",
                    "index.json",
                    "metamodel.json",
                    "diagrams/ProjectDataVersioning.md",
                    "elements/Record.md",
                    "elements/Project.md",
                    "elements/Commit.md",
                    "elements/Data.md",
                    "elements/Query.md",
                    "elements/UUID.md",
                    "elements/Timestamp.md",
                    "elements/ProjectState.md",
                    "elements/String.md",
                })
                {
                    Assert.That(File.Exists(Path.Combine(pim.FullName, expected)), Is.True, expected);
                }

                Assert.That(this.layout.PimGraph(Tag).Exists, Is.True, "the graph is where the layout says it is");
            });
        }

        [Test]
        public async Task The_result_reports_exactly_the_files_under_the_pim_folder()
        {
            var result = await this.generator.GenerateAsync(Tag);

            var onDisk = this.layout.Pim(Tag)
                .EnumerateFiles("*", SearchOption.AllDirectories)
                .Select(file => file.FullName)
                .OrderBy(path => path, StringComparer.Ordinal);

            Assert.Multiple(() =>
            {
                Assert.That(result.Written, Is.Not.Empty);
                Assert.That(
                    result.Written.Select(file => file.FullName).OrderBy(path => path, StringComparer.Ordinal),
                    Is.EqualTo(onDisk));
                Assert.That(
                    Directory.EnumerateFileSystemEntries(this.layout.Knowledge(Tag).FullName).Select(Path.GetFileName),
                    Is.EqualTo(new[] { "pim" }),
                    "the other API artifacts' folders are not this generator's to create");
            });
        }

        [Test]
        public async Task The_pages_call_a_class_a_class_and_the_model_a_model()
        {
            // The PIM is an ordinary model: calling its Commit a metaclass would be wrong.
            await this.generator.GenerateAsync(Tag);

            Assert.Multiple(() =>
            {
                Assert.That(this.Read("index.md"), Does.StartWith("# Systems Modeling API PIM model index\n"));
                Assert.That(this.Read("index.md"), Does.Contain(" classes across "));
                Assert.That(this.Read("elements/Record.md"), Does.Contain(" package · abstract class\n"));
                Assert.That(this.Read("elements/Project.md"), Does.Contain(" package · concrete class\n"));
                Assert.That(
                    this.Read("diagrams/ProjectDataVersioning.md"),
                    Does.Contain(" classes in this package").And.Contain("class-typed features"));

                foreach (var page in new[]
                {
                    "index.md",
                    "diagrams/ProjectDataVersioning.md",
                    "elements/Record.md",
                    "elements/Project.md",
                    "elements/Commit.md",
                    "elements/Data.md",
                    "elements/UUID.md",
                    "elements/ProjectState.md",
                })
                {
                    Assert.That(this.Read(page), Does.Not.Contain("metaclass"), page);
                }
            });
        }

        [Test]
        public async Task A_class_page_gives_the_full_effective_feature_set()
        {
            await this.generator.GenerateAsync(Tag);

            var record = this.Read("elements/Record.md");
            var project = this.Read("elements/Project.md");

            Assert.Multiple(() =>
            {
                Assert.That(record, Does.StartWith("---\nname: Record\n"));
                Assert.That(record, Does.Contain("\nfully qualified name: FixtureApi::ProjectDataVersioning::Record\n"));
                Assert.That(record, Does.Contain("\nisAbstract: true\n"));
                Assert.That(record, Does.Contain("\nspecializedBy: [Commit, Project]\n"));
                Assert.That(record, Does.Contain("A Record is anything the service keeps under an identifier."));
                Assert.That(record, Does.Contain("\n### alias\n\n`+` [String](String.md) · `[0..*]`\n"));
                Assert.That(
                    record, Does.Contain("\n### id\n\n`+` [UUID](UUID.md) · `[1..1]`\n"),
                    "a feature typed by a data type links to that data type's page");
                Assert.That(record, Does.Contain("\n### label\n\n`+` [String](String.md) · `[0..1]` · *derived*\n"));

                Assert.That(project, Does.Contain("\ngeneralizes: [Record]\n"));
                Assert.That(project, Does.Contain("\n## Generalizations\n\n- [Record](Record.md)\n"));
                Assert.That(project, Does.Contain("\n### commit\n\n`+` [Commit](Commit.md) · `[0..*]` · *composite*\n"));
                Assert.That(project, Does.Contain("\n### headCommit\n\n`+` [Commit](Commit.md) · `[0..1]`\n"));
                Assert.That(project, Does.Contain("\nSubsets [commit](#commit)\n"));
                Assert.That(project, Does.Contain("\n### state\n\n`+` [ProjectState](ProjectState.md) · `[1..1]`\n"));

                Assert.That(project, Does.Contain("\n## Inherited features\n"));
                Assert.That(project, Does.Contain("\n| alias | [String](String.md) | [0..*] | [Record](Record.md) |  |\n"));
                Assert.That(project, Does.Contain("\n| id | [UUID](UUID.md) | [1..1] | [Record](Record.md) |  |\n"));
                Assert.That(
                    project, Does.Contain("\n| label | [String](String.md) | [0..1] | [Record](Record.md) | derived |\n"));

                Assert.That(project, Does.Contain("\n## Operations\n\n### commitAt\n"));
                Assert.That(
                    project,
                    Does.Contain("\n`commitAt(moment : Timestamp [1..1]) : Commit [1..1]` · *query*\n"));
            });
        }

        [Test]
        public async Task A_feature_typed_by_an_interface_links_to_the_interface()
        {
            await this.generator.GenerateAsync(Tag);

            Assert.That(
                this.Read("elements/Commit.md"),
                Does.Contain("\n### change\n\n`+` [Data](Data.md) · `[0..*]`\n"));
        }

        [Test]
        public async Task The_interface_and_the_data_types_get_pages_of_their_own()
        {
            // What the metamodels do not have, and so what the metamodel generators never wrote.
            await this.generator.GenerateAsync(Tag);

            var data = this.Read("elements/Data.md");
            var uuid = this.Read("elements/UUID.md");

            Assert.Multiple(() =>
            {
                Assert.That(data, Does.Contain("\nkind: interface\n"));
                Assert.That(data, Does.Contain("\n`ProjectDataVersioning` package · interface\n"));
                Assert.That(data, Does.Contain("\n## Realized by\n"));
                Assert.That(data, Does.Contain("\n- [Project](Project.md)\n"));
                Assert.That(data, Does.Contain("\n`identity() : UUID [1..1]` · *query*\n"));

                Assert.That(uuid, Does.Contain("\nkind: datatype\n"));
                Assert.That(uuid, Does.Contain("\n`ProjectDataVersioning` package · data type\n"));
                Assert.That(this.Read("elements/Timestamp.md"), Does.Contain("\nkind: datatype\n"));

                Assert.That(this.Read("elements/ProjectState.md"), Does.Contain("\nkind: enumeration\n"));
                Assert.That(this.Read("elements/ProjectState.md"), Does.Contain("\n- `open`\n- `archived`\n"));
                Assert.That(this.Read("elements/String.md"), Does.Contain("\nkind: primitive\n"));
            });
        }

        [Test]
        public async Task The_index_lists_every_kind_of_element_and_links_each_to_its_page()
        {
            await this.generator.GenerateAsync(Tag);

            var index = this.Read("index.md");
            var pim = this.layout.Pim(Tag);

            Assert.Multiple(() =>
            {
                Assert.That(index, Does.Contain("\n## ProjectDataVersioning\n"));
                Assert.That(index, Does.Contain("\n[Class diagram](diagrams/ProjectDataVersioning.md)\n"));
                Assert.That(
                    index,
                    Does.Contain("\n- [Commit](elements/Commit.md) — A Commit is a Record of one change to a Project.\n"));
                Assert.That(index, Does.Contain("\n## Interfaces\n\n- [Data](elements/Data.md)"));
                Assert.That(index, Does.Contain("\n## Data types\n\n- [Timestamp](elements/Timestamp.md)\n- [UUID](elements/UUID.md)"));
                Assert.That(index, Does.Contain("\n## Enumeration types\n\n- [ProjectState](elements/ProjectState.md)"));
                Assert.That(index, Does.Contain("\n## Primitive types\n"));

                var links = System.Text.RegularExpressions.Regex.Matches(index, @"\]\(([^)]+\.md)\)")
                    .Select(match => match.Groups[1].Value)
                    .ToList();

                Assert.That(links, Is.Not.Empty);

                foreach (var link in links)
                {
                    Assert.That(File.Exists(Path.Combine(pim.FullName, link)), Is.True, $"index.md links to {link}");
                }
            });
        }

        [Test]
        public async Task The_lookup_names_every_element_and_points_each_at_a_page_that_exists()
        {
            await this.generator.GenerateAsync(Tag);

            var pim = this.layout.Pim(Tag);

            using var lookup = JsonDocument.Parse(this.Read("index.json"));
            var entries = lookup.RootElement.GetProperty("entries");

            string Kind(string name) => entries.GetProperty(name).GetProperty("kind").GetString()!;

            Assert.Multiple(() =>
            {
                Assert.That(lookup.RootElement.GetProperty("metamodel").GetString(), Is.EqualTo(PimFixture.ModelName));

                Assert.That(Kind("Record"), Is.EqualTo("class"));
                Assert.That(Kind("Data"), Is.EqualTo("interface"));
                Assert.That(Kind("Query"), Is.EqualTo("interface"));
                Assert.That(Kind("UUID"), Is.EqualTo("dataType"));
                Assert.That(Kind("Timestamp"), Is.EqualTo("dataType"));
                Assert.That(Kind("ProjectState"), Is.EqualTo("enumeration"));
                Assert.That(Kind("String"), Is.EqualTo("primitiveType"));

                foreach (var entry in entries.EnumerateObject())
                {
                    var file = entry.Value.GetProperty("file").GetString()!;

                    Assert.That(File.Exists(Path.Combine(pim.FullName, file)), Is.True, $"{entry.Name} -> {file}");
                }

                var pages = new DirectoryInfo(Path.Combine(pim.FullName, "elements"))
                    .EnumerateFiles("*.md")
                    .Select(page => Path.GetFileNameWithoutExtension(page.Name));

                Assert.That(
                    pages,
                    Is.EquivalentTo(entries.EnumerateObject().Select(entry => entry.Name)),
                    "every page can be found through the lookup, and nothing else can");
            });
        }

        [Test]
        public async Task The_graph_carries_the_interfaces_the_data_types_and_a_digest_of_its_inputs()
        {
            await this.generator.GenerateAsync(Tag);

            using var graph = JsonDocument.Parse(File.ReadAllText(this.layout.PimGraph(Tag).FullName));
            var root = graph.RootElement;

            Assert.Multiple(() =>
            {
                Assert.That(root.GetProperty("metamodel").GetString(), Is.EqualTo(PimFixture.ModelName));
                Assert.That(
                    root.GetProperty("modelVersionUri").GetString(),
                    Is.EqualTo("https://example.invalid/spec/FixtureApi/20260101"));
                Assert.That(root.GetProperty("sourceXmiSha256").GetString(), Does.Match("^[0-9a-f]{64}$"));

                Assert.That(
                    root.GetProperty("interfaces").EnumerateArray().Select(node => node.GetProperty("name").GetString()),
                    Is.EqualTo(new[] { "Data", "Query" }));
                Assert.That(
                    root.GetProperty("dataTypes").EnumerateArray().Select(node => node.GetProperty("name").GetString()),
                    Is.EqualTo(new[] { "Timestamp", "UUID" }));
                Assert.That(
                    root.GetProperty("classes").EnumerateArray().Select(node => node.GetProperty("name").GetString()),
                    Is.SupersetOf(new[] { "Commit", "Project", "Record" }));
            });
        }

        [Test]
        public async Task The_digest_follows_the_inputs()
        {
            // Provenance: knowledge generated from a different PIM model must not carry the same digest.
            await this.generator.GenerateAsync(Tag);
            var before = this.Digest();

            var pim = this.layout.PimModel(Tag).FullName;
            File.WriteAllText(pim, File.ReadAllText(pim).Replace("A Commit is a Record", "A Commit is the Record"));

            await this.generator.GenerateAsync(Tag);

            Assert.That(this.Digest(), Is.Not.EqualTo(before));
        }

        [Test]
        public async Task Two_runs_over_the_same_inputs_are_byte_for_byte_the_same()
        {
            // The same property KnowledgeRegenerationTests checks for the language releases: nothing
            // per-version is committed, so determinism is what there is to compare against.
            var first = new DirectoryInfo(Path.Combine(this.workspace.FullName, "first"));
            var second = new DirectoryInfo(Path.Combine(this.workspace.FullName, "second"));

            await this.GeneratorWritingTo(first).GenerateAsync(Tag);
            await this.GeneratorWritingTo(second).GenerateAsync(Tag);

            var firstFiles = RelativeFiles(first);
            var secondFiles = RelativeFiles(second);

            Assert.Multiple(() =>
            {
                Assert.That(firstFiles, Is.Not.Empty);
                Assert.That(secondFiles, Is.EqualTo(firstFiles));

                foreach (var file in firstFiles)
                {
                    Assert.That(
                        File.ReadAllBytes(Path.Combine(second.FullName, file)),
                        Is.EqualTo(File.ReadAllBytes(Path.Combine(first.FullName, file))),
                        file);
                }
            });
        }

        [Test]
        public async Task The_output_carries_no_machine_path_no_carriage_return_and_no_byte_order_mark()
        {
            await this.generator.GenerateAsync(Tag);

            Assert.Multiple(() =>
            {
                foreach (var file in this.layout.Pim(Tag).EnumerateFiles("*", SearchOption.AllDirectories))
                {
                    var bytes = File.ReadAllBytes(file.FullName);
                    var text = File.ReadAllText(file.FullName);

                    Assert.That(text, Does.Not.Contain(this.workspace.FullName), file.Name);
                    Assert.That(text, Does.Not.Contain(this.workspace.Name), file.Name);
                    Assert.That(bytes, Has.None.EqualTo((byte)0x0D), $"{file.Name} has CR bytes");
                    Assert.That(bytes.Take(3), Is.Not.EqualTo(new byte[] { 0xEF, 0xBB, 0xBF }), $"{file.Name} has a BOM");
                }
            });
        }

        [Test]
        public async Task The_pim_is_generated_into_the_folder_the_caller_asked_for()
        {
            // Everything this generator writes has to land under the output root, or
            // `hypha api generate --output` would silently write into the repository.
            var output = new DirectoryInfo(Path.Combine(this.workspace.FullName, "elsewhere"));

            var result = await this.GeneratorWritingTo(output).GenerateAsync(Tag);

            Assert.Multiple(() =>
            {
                Assert.That(result.Written, Is.Not.Empty);
                Assert.That(
                    result.Written,
                    Has.All.Matches<FileInfo>(file => file.FullName.StartsWith(
                        Path.Combine(output.FullName, "knowledge", "api", Tag, "pim") + Path.DirectorySeparatorChar,
                        StringComparison.Ordinal)),
                    "nothing may be written outside the requested output folder");
                Assert.That(
                    Directory.Exists(Path.Combine(this.workspace.FullName, "knowledge")), Is.False,
                    "the folder the inputs were read from gains no knowledge");
            });
        }

        [Test]
        public async Task Generating_changes_none_of_the_inputs()
        {
            var sources = new DirectoryInfo(Path.Combine(this.workspace.FullName, "sources"));

            Dictionary<string, byte[]> Snapshot() => sources
                .EnumerateFiles("*", SearchOption.AllDirectories)
                .ToDictionary(file => file.FullName, file => File.ReadAllBytes(file.FullName));

            var before = Snapshot();

            await this.generator.GenerateAsync(Tag);

            var after = Snapshot();

            Assert.Multiple(() =>
            {
                Assert.That(after.Keys, Is.EquivalentTo(before.Keys));

                foreach (var (path, bytes) in before)
                {
                    Assert.That(after[path], Is.EqualTo(bytes), path);
                }
            });
        }

        [Test]
        public async Task A_version_without_its_pim_model_is_skipped_with_a_reason()
        {
            this.layout.PimModel(Tag).Delete();

            var result = await this.generator.GenerateAsync(Tag);

            Assert.Multiple(() =>
            {
                Assert.That(result.Outcome, Is.EqualTo(GenerationOutcome.Skipped));
                Assert.That(result.Written, Is.Empty);
                Assert.That(result.Reason, Does.Contain($"no PIM model fetched for API version {Tag}"));
                Assert.That(result.Reason, Does.Contain($"hypha api fetch --tag {Tag}"));
                Assert.That(this.layout.Knowledge(Tag).Exists, Is.False, "a skipped run writes nothing");
            });
        }

        [Test]
        public async Task A_version_without_the_kerml_model_the_pim_refers_to_is_skipped_with_a_reason()
        {
            File.Delete(Path.Combine(this.layout.Xmi(Tag).FullName, ApiInputs.KermlXmi));

            var result = await this.generator.GenerateAsync(Tag);

            Assert.Multiple(() =>
            {
                Assert.That(result.Outcome, Is.EqualTo(GenerationOutcome.Skipped));
                Assert.That(result.Written, Is.Empty);
                Assert.That(result.Reason, Does.Contain($"hypha api fetch --tag {Tag}"));
                Assert.That(this.layout.Knowledge(Tag).Exists, Is.False, "a skipped run writes nothing");
            });
        }

        [Test]
        public async Task An_unfetched_version_is_skipped_with_a_reason()
        {
            var result = await this.generator.GenerateAsync("1999-01");

            Assert.Multiple(() =>
            {
                Assert.That(result.Outcome, Is.EqualTo(GenerationOutcome.Skipped));
                Assert.That(result.Reason, Does.Contain("1999-01"));
                Assert.That(result.Written, Is.Empty);
            });
        }

        [Test]
        public void A_cancelled_run_does_not_report_success()
        {
            using var source = new CancellationTokenSource();
            source.Cancel();

            Assert.That(
                () => this.generator.GenerateAsync(Tag, source.Token),
                Throws.InstanceOf<OperationCanceledException>());
        }

        [Test]
        public void A_blank_tag_and_missing_collaborators_are_refused()
        {
            Assert.Multiple(() =>
            {
                Assert.That(() => this.generator.GenerateAsync(" "), Throws.ArgumentException);
                Assert.Throws<ArgumentNullException>(
                    () => _ = new PimGenerator(null!, NullLogger<PimGenerator>.Instance));
                Assert.Throws<ArgumentNullException>(() => _ = new PimGenerator(this.layout, null!));
            });
        }

        [Test]
        public void The_pim_generator_is_registered_as_an_api_generator_and_never_as_a_language_one()
        {
            // It is an IKnowledgeGenerator too. Registered under that type, `hypha generate` would run
            // it for every language release.
            using var provider = new ServiceCollection()
                .AddHyphaKnowledge(options => options.RepositoryRoot = this.workspace)
                .AddHyphaMetamodelGen()
                .BuildServiceProvider();

            var api = provider.GetServices<IApiKnowledgeGenerator>().ToList();
            var language = provider.GetServices<IKnowledgeGenerator>().ToList();

            Assert.Multiple(() =>
            {
                Assert.That(api, Has.Count.EqualTo(1));
                Assert.That(api[0], Is.InstanceOf<PimGenerator>());

                Assert.That(language, Has.None.InstanceOf<IApiKnowledgeGenerator>());
                Assert.That(language.Select(candidate => candidate.Artifact), Does.Not.Contain("pim"));
                Assert.That(
                    language.Select(candidate => candidate.Artifact), Does.Contain("metamodel"),
                    "the registration beside it is still in place");
            });
        }

        [Test]
        public async Task The_registered_generator_reads_and_writes_where_the_options_point()
        {
            var output = new DirectoryInfo(Path.Combine(this.workspace.FullName, "composed"));

            using var provider = new ServiceCollection()
                .AddHyphaKnowledge(options =>
                {
                    options.RepositoryRoot = this.workspace;
                    options.OutputRoot = output;
                })
                .AddHyphaMetamodelGen()
                .BuildServiceProvider();

            var result = await provider.GetServices<IApiKnowledgeGenerator>()
                .Single(candidate => candidate.Artifact == "pim")
                .GenerateAsync(Tag);

            Assert.Multiple(() =>
            {
                Assert.That(result.Outcome, Is.EqualTo(GenerationOutcome.Generated));
                Assert.That(
                    File.Exists(Path.Combine(output.FullName, "knowledge", "api", Tag, "pim", "elements", "Data.md")),
                    Is.True);
            });
        }

        [Test]
        [Ignore("PimGenerator's path map for 'KerML Abstract Syntax.xml' is never consulted (uml4net applies "
                + "path maps to pathmap:// references only), so the KerML references do not resolve; see #146.")]
        public async Task A_reference_into_the_kerml_abstract_syntax_resolves_to_the_kerml_element()
        {
            // The fixture, like the published PIM, types a feature with the KerML Element and has the
            // KerML Element realize Data, both through an href into 'KerML Abstract Syntax.xml'.
            await this.generator.GenerateAsync(Tag);

            Assert.Multiple(() =>
            {
                Assert.That(this.Read("elements/Commit.md"), Does.Not.Contain("«untyped»"));
                Assert.That(this.Read("elements/Commit.md"), Does.Contain("\n### root\n\n`+` ").And.Contain("Element"));
                Assert.That(this.Read("elements/Data.md"), Does.Match(@"\nrealizedBy: \[Element, Project\]\n"));
            });
        }

        private PimGenerator GeneratorWritingTo(DirectoryInfo output) =>
            new(
                new ApiLayout(new Hypha.Knowledge.Layout.KnowledgeLayout(this.workspace, output)),
                NullLogger<PimGenerator>.Instance);

        private string Read(string relativePath) =>
            File.ReadAllText(Path.Combine(this.layout.Pim(Tag).FullName, relativePath)).Replace("\r\n", "\n");

        private string Digest()
        {
            using var graph = JsonDocument.Parse(File.ReadAllText(this.layout.PimGraph(Tag).FullName));

            return graph.RootElement.GetProperty("sourceXmiSha256").GetString()!;
        }

        private string Relative(FileSystemInfo path) =>
            Path.GetRelativePath(this.workspace.FullName, path.FullName).Replace('\\', '/');

        private static List<string> RelativeFiles(DirectoryInfo root) =>
            root.EnumerateFiles("*", SearchOption.AllDirectories)
                .Select(file => Path.GetRelativePath(root.FullName, file.FullName).Replace('\\', '/'))
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToList();
    }
}
