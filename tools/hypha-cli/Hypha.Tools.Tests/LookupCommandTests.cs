// ------------------------------------------------------------------------------------------------
// <copyright file="LookupCommandTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Tests
{
    using System;
    using System.CommandLine;
    using System.Linq;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Lookup;
    using Hypha.Tools.Commands;

    using Moq;

    [TestFixture]
    public class LookupCommandTests
    {
        private static readonly LookupResult Found = new()
        {
            Release = "2026-09",
            Kind = KnowledgeKind.ModelLibrary,
            Status = LookupStatus.Found,
            Name = "ScalarValues::Real",
            Path = "/knowledge/2026-09/model-library/packages/ScalarValues.md",
            DeclarationLine = 11,
            StartLine = 11,
            EndLine = 12,
            TotalLines = 30,
            NextLine = 13,
            Content = "    datatype Real specializes NumericalValue;\n}",
            Matches = ["ScalarValues::Real"],
            TotalMatches = 1,
        };

        private RootCommand root = null!;

        private Mock<IKnowledgeLookup> lookup = null!;

        [SetUp]
        public void SetUp()
        {
            this.root = new RootCommand();
            this.root.Add(new LookupCommand());
            this.lookup = new Mock<IKnowledgeLookup>();
        }

        [Test]
        public async Task The_command_line_becomes_the_request()
        {
            this.lookup.Setup(mock => mock.Lookup(It.IsAny<LookupRequest>())).Returns(Found);

            await this.Invoke("lookup --kind model-library --name Real --tag 2026-09 --start-line 5 --max-lines 20 --json");

            this.lookup.Verify(mock => mock.Lookup(new LookupRequest(KnowledgeKind.ModelLibrary, "Real", "2026-09", 5, 20)));
        }

        [Test]
        public async Task The_defaults_leave_the_start_to_the_lookup_and_use_the_default_page()
        {
            this.lookup.Setup(mock => mock.Lookup(It.IsAny<LookupRequest>())).Returns(Found);

            await this.Invoke("lookup -k metamodel -n PartUsage --json");

            this.lookup.Verify(mock => mock.Lookup(
                new LookupRequest(KnowledgeKind.Metamodel, "PartUsage", null, null, LookupRequest.DefaultMaxLines)));
        }

        [Test]
        public async Task Json_output_is_one_line_holding_the_whole_contract()
        {
            this.lookup.Setup(mock => mock.Lookup(It.IsAny<LookupRequest>())).Returns(Found);
            using var console = new RecordedConsole();

            var result = await this.Invoke("lookup --kind model-library --name Real --json");

            var line = console.Output.TrimEnd('\r', '\n');
            using var document = JsonDocument.Parse(line);
            var json = document.RootElement;

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(0));
                Assert.That(line, Does.Not.Contain("\n"), "the JSON document has to be a single line");

                // Pinned: these names are what every host reads. Renaming one is a breaking change.
                Assert.That(json.GetProperty("release").GetString(), Is.EqualTo("2026-09"));
                Assert.That(json.GetProperty("kind").GetString(), Is.EqualTo("model-library"));
                Assert.That(json.GetProperty("status").GetString(), Is.EqualTo("found"));
                Assert.That(json.GetProperty("name").GetString(), Is.EqualTo("ScalarValues::Real"));
                Assert.That(json.GetProperty("path").GetString(), Is.EqualTo(Found.Path));
                Assert.That(json.GetProperty("declarationLine").GetInt32(), Is.EqualTo(11));
                Assert.That(json.GetProperty("startLine").GetInt32(), Is.EqualTo(11));
                Assert.That(json.GetProperty("endLine").GetInt32(), Is.EqualTo(12));
                Assert.That(json.GetProperty("totalLines").GetInt32(), Is.EqualTo(30));
                Assert.That(json.GetProperty("nextLine").GetInt32(), Is.EqualTo(13));
                Assert.That(json.GetProperty("content").GetString(), Is.EqualTo(Found.Content));
                Assert.That(json.GetProperty("matches")[0].GetString(), Is.EqualTo("ScalarValues::Real"));
                Assert.That(json.GetProperty("totalMatches").GetInt32(), Is.EqualTo(1));
            });
        }

        [Test]
        public async Task An_unresolved_name_is_still_a_successful_lookup_with_its_status_and_nulls()
        {
            this.lookup.Setup(mock => mock.Lookup(It.IsAny<LookupRequest>())).Returns(new LookupResult
            {
                Release = "2026-09",
                Kind = KnowledgeKind.ModelLibrary,
                Status = LookupStatus.Ambiguous,
                Matches = ["ISQBase::mass", "ISQMechanics::mass"],
                TotalMatches = 2,
            });
            using var console = new RecordedConsole();

            var result = await this.Invoke("lookup --kind model-library --name mass --json");

            using var document = JsonDocument.Parse(console.Output);
            var json = document.RootElement;

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(0));
                Assert.That(json.GetProperty("status").GetString(), Is.EqualTo("ambiguous"));
                Assert.That(json.GetProperty("content").ValueKind, Is.EqualTo(JsonValueKind.Null));
                Assert.That(json.GetProperty("nextLine").ValueKind, Is.EqualTo(JsonValueKind.Null));
                Assert.That(json.GetProperty("matches").GetArrayLength(), Is.EqualTo(2));
            });
        }

        [Test]
        public async Task A_lookup_that_cannot_run_is_a_json_error_and_exit_code_1()
        {
            this.lookup
                .Setup(mock => mock.Lookup(It.IsAny<LookupRequest>()))
                .Throws(new KnowledgeLookupException("Release 2025-01 is not installed."));
            using var console = new RecordedConsole();

            var result = await this.Invoke("lookup --kind metamodel --name PartUsage --tag 2025-01 --json");

            using var document = JsonDocument.Parse(console.Output);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(1));
                Assert.That(
                    document.RootElement.GetProperty("error").GetString(),
                    Is.EqualTo("Release 2025-01 is not installed."));
            });
        }

        [Test]
        public async Task Without_json_the_page_is_printed_with_where_it_came_from()
        {
            this.lookup.Setup(mock => mock.Lookup(It.IsAny<LookupRequest>())).Returns(Found);
            using var console = new RecordedConsole();

            var result = await this.Invoke("lookup --kind model-library --name Real");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(0));
                Assert.That(console.Output, Does.Contain("ScalarValues::Real"));
                Assert.That(console.Output, Does.Contain("datatype Real specializes NumericalValue;"));
                Assert.That(console.Output, Does.Contain("--start-line 13"));
            });
        }

        [Test]
        public async Task Without_json_an_ambiguous_name_lists_the_choices()
        {
            this.lookup.Setup(mock => mock.Lookup(It.IsAny<LookupRequest>())).Returns(new LookupResult
            {
                Release = "2026-09",
                Kind = KnowledgeKind.ModelLibrary,
                Status = LookupStatus.Ambiguous,
                Matches = ["ISQBase::mass", "ISQMechanics::mass"],
                TotalMatches = 2,
            });
            using var console = new RecordedConsole();

            await this.Invoke("lookup --kind model-library --name mass");

            Assert.That(console.Output, Does.Contain("ISQBase::mass").And.Contains("ISQMechanics::mass"));
        }

        [Test]
        public async Task A_request_the_lookup_refuses_is_a_json_error_and_exit_code_1()
        {
            this.lookup
                .Setup(mock => mock.Lookup(It.IsAny<LookupRequest>()))
                .Throws(new ArgumentException("The start line 500 is past the end."));
            using var console = new RecordedConsole();

            var result = await this.Invoke("lookup --kind metamodel --name PartUsage --start-line 500 --json");

            using var document = JsonDocument.Parse(console.Output);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(1));
                Assert.That(
                    document.RootElement.GetProperty("error").GetString(),
                    Is.EqualTo("The start line 500 is past the end."));
            });
        }

        [Test]
        public async Task Without_json_a_lookup_that_cannot_run_prints_the_reason_and_exits_1()
        {
            this.lookup
                .Setup(mock => mock.Lookup(It.IsAny<LookupRequest>()))
                .Throws(new KnowledgeLookupException("Release 2025-01 is not installed."));
            using var console = new RecordedConsole();

            var result = await this.Invoke("lookup --kind metamodel --name PartUsage --tag 2025-01");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(1));
                Assert.That(console.Output, Does.Contain("Release 2025-01 is not installed."));
            });
        }

        [TestCase(LookupStatus.Found, "found")]
        [TestCase(LookupStatus.Ambiguous, "ambiguous")]
        [TestCase(LookupStatus.Candidates, "candidates")]
        [TestCase(LookupStatus.NotFound, "not-found")]
        public async Task Every_status_has_its_pinned_json_name(LookupStatus status, string expected)
        {
            this.lookup.Setup(mock => mock.Lookup(It.IsAny<LookupRequest>())).Returns(Found with { Status = status });
            using var console = new RecordedConsole();

            await this.Invoke("lookup --kind model-library --name Real --json");

            using var document = JsonDocument.Parse(console.Output);
            Assert.That(document.RootElement.GetProperty("status").GetString(), Is.EqualTo(expected));
        }

        [TestCase(KnowledgeKind.Metamodel, "metamodel")]
        [TestCase(KnowledgeKind.ModelLibrary, "model-library")]
        public async Task Every_kind_has_its_pinned_json_name(KnowledgeKind kind, string expected)
        {
            this.lookup.Setup(mock => mock.Lookup(It.IsAny<LookupRequest>())).Returns(Found with { Kind = kind });
            using var console = new RecordedConsole();

            await this.Invoke($"lookup --kind {expected} --name Real --json");

            using var document = JsonDocument.Parse(console.Output);
            Assert.That(document.RootElement.GetProperty("kind").GetString(), Is.EqualTo(expected));
        }

        [Test]
        public async Task Without_json_a_name_nothing_matches_says_so()
        {
            this.lookup.Setup(mock => mock.Lookup(It.IsAny<LookupRequest>())).Returns(new LookupResult
            {
                Release = "2026-09",
                Kind = KnowledgeKind.Metamodel,
                Status = LookupStatus.NotFound,
            });
            using var console = new RecordedConsole();

            var result = await this.Invoke("lookup --kind metamodel --name Spaceship");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(0));
                Assert.That(console.Output, Does.Contain("Nothing in the 2026-09 index matches"));
            });
        }

        [Test]
        public async Task Without_json_candidates_beyond_the_cap_say_how_many_more()
        {
            this.lookup.Setup(mock => mock.Lookup(It.IsAny<LookupRequest>())).Returns(new LookupResult
            {
                Release = "2026-09",
                Kind = KnowledgeKind.Metamodel,
                Status = LookupStatus.Candidates,
                Matches = [.. Enumerable.Range(0, KnowledgeLookup.MaxMatches).Select(index => $"Widget{index:D2}")],
                TotalMatches = KnowledgeLookup.MaxMatches + 5,
            });
            using var console = new RecordedConsole();

            await this.Invoke("lookup --kind metamodel --name widget");

            Assert.Multiple(() =>
            {
                Assert.That(console.Output, Does.Contain("Did you mean"));
                Assert.That(console.Output, Does.Contain("Widget19"));
                Assert.That(console.Output, Does.Contain("... and 5 more."));
            });
        }

        [TestCase("lookup --name PartUsage")]
        [TestCase("lookup --kind metamodel")]
        [TestCase("lookup --kind spec --name PartUsage")]
        public void An_incomplete_or_unknown_request_does_not_parse(string commandLine)
        {
            Assert.That(this.root.Parse(commandLine).Errors, Is.Not.Empty);
        }

        private Task<int> Invoke(string commandLine) =>
            new LookupCommand.Handler(this.lookup.Object).InvokeAsync(this.root.Parse(commandLine), CancellationToken.None);
    }
}
