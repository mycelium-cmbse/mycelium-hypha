// ------------------------------------------------------------------------------------------------
// <copyright file="RootCommandTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Tests
{
    using System.CommandLine;
    using System.IO;
    using System.Linq;

    using Hypha.Tools.Commands;
    using Hypha.Tools.Commands.Api;

    /// <summary>
    /// Suite of tests for the command line the tool exposes.
    /// </summary>
    [TestFixture]
    public class RootCommandTests
    {
        private RootCommand root = null!;

        [SetUp]
        public void SetUp() => this.root = Program.BuildRootCommand();

        [Test]
        public void Every_verb_is_reachable()
        {
            var verbs = this.root.Subcommands.Select(command => command.Name);

            Assert.That(
                verbs,
                Is.EquivalentTo(new[]
                {
                    "discover", "fetch", "generate", "list", "move-window", "use", "remove", "check", "api",
                }));
        }

        [Test]
        public void Every_api_verb_is_reachable()
        {
            // The API is versioned on its own axis, so it gets the version verbs over again - under
            // `api`, where they cannot be mistaken for the language releases' ones.
            Assert.That(
                this.Api().Subcommands.Select(command => command.Name),
                Is.EquivalentTo(new[] { "discover", "fetch", "generate", "list", "use", "remove" }));
        }

        [Test]
        public void Every_verb_has_an_action_bound_to_it()
        {
            // A verb with no action parses and then does nothing, which looks like success. `api` is
            // the one exception: it only groups its own verbs, and those are held to the same rule.
            var verbs = this.root.Subcommands
                .Where(command => command.Subcommands.Count == 0)
                .Concat(this.Api().Subcommands)
                .ToList();

            Assert.Multiple(() =>
            {
                Assert.That(verbs, Has.Count.EqualTo(14));
                Assert.That(verbs.Select(command => command.Action), Has.All.Not.Null);
            });
        }

        [Test]
        public void Api_on_its_own_is_not_a_complete_command()
        {
            Assert.That(this.root.Parse("api").Errors, Is.Not.Empty);
        }

        [TestCase("discover")]
        [TestCase("fetch --tag 2026-05")]
        [TestCase("generate")]
        [TestCase("list")]
        [TestCase("move-window --tag 2026-06")]
        [TestCase("use --tag 2026-05")]
        [TestCase("remove --tag 2026-05")]
        [TestCase("check")]
        [TestCase("api discover")]
        [TestCase("api fetch --tag 2026-04")]
        [TestCase("api generate")]
        [TestCase("api list")]
        [TestCase("api use --tag 2026-04")]
        [TestCase("api remove --tag 2026-04")]
        public void The_global_options_reach_every_verb(string commandLine)
        {
            var parsed = this.root.Parse(
                $"{commandLine} --repository-root . --token secret --log-level Debug --no-logo");

            Assert.Multiple(() =>
            {
                Assert.That(parsed.Errors, Is.Empty);
                Assert.That(parsed.GetValue(GlobalOptions.Token), Is.EqualTo("secret"));
                Assert.That(parsed.GetValue(GlobalOptions.NoLogo), Is.True);
                Assert.That(parsed.GetValue(GlobalOptions.RepositoryRoot), Is.Not.Null);
            });
        }

        [Test]
        public void Generate_takes_the_artifact_as_an_optional_argument()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.root.Parse("generate").Errors, Is.Empty);
                Assert.That(
                    this.root.Parse("generate metamodel").GetValue(GenerateCommand.Artifact),
                    Is.EqualTo("metamodel"));
            });
        }

        [Test]
        public void Generate_accepts_several_releases_and_an_output_folder()
        {
            var parsed = this.root.Parse("generate --tag 2026-05 --tag 2026-04 --output out");

            Assert.Multiple(() =>
            {
                Assert.That(parsed.Errors, Is.Empty);
                Assert.That(parsed.GetValue(GenerateCommand.Tag), Is.EqualTo(new[] { "2026-05", "2026-04" }));
                Assert.That(parsed.GetValue(GenerateCommand.Output), Is.InstanceOf<DirectoryInfo>());
            });
        }

        [Test]
        public void Api_generate_takes_the_artifact_as_an_optional_argument()
        {
            var parsed = this.root.Parse("api generate pim");

            Assert.Multiple(() =>
            {
                Assert.That(this.root.Parse("api generate").Errors, Is.Empty);
                Assert.That(parsed.Errors, Is.Empty);
                Assert.That(parsed.GetValue(ApiGenerateCommand.Artifact), Is.EqualTo("pim"));
            });
        }

        [Test]
        public void Api_generate_accepts_several_versions_and_an_output_folder()
        {
            var parsed = this.root.Parse("api generate --tag 2026-04 --tag 2025-09 --output out");

            Assert.Multiple(() =>
            {
                Assert.That(parsed.Errors, Is.Empty);
                Assert.That(parsed.GetValue(ApiGenerateCommand.Tag), Is.EqualTo(new[] { "2026-04", "2025-09" }));
                Assert.That(parsed.GetValue(ApiGenerateCommand.Output), Is.InstanceOf<DirectoryInfo>());
            });
        }

        [Test]
        public void Api_fetch_reads_its_flags()
        {
            var parsed = this.root.Parse("api fetch -t 2026-04 --no-specs --force --no-default");
            var defaults = this.root.Parse("api fetch --tag 2026-04");

            Assert.Multiple(() =>
            {
                Assert.That(parsed.Errors, Is.Empty);
                Assert.That(parsed.GetValue(ApiFetchCommand.Tag), Is.EqualTo("2026-04"));
                Assert.That(parsed.GetValue(ApiFetchCommand.NoSpecs), Is.True);
                Assert.That(parsed.GetValue(ApiFetchCommand.Force), Is.True);
                Assert.That(parsed.GetValue(ApiFetchCommand.NoDefault), Is.True);

                Assert.That(defaults.GetValue(ApiFetchCommand.NoSpecs), Is.False);
                Assert.That(defaults.GetValue(ApiFetchCommand.Force), Is.False);
                Assert.That(defaults.GetValue(ApiFetchCommand.NoDefault), Is.False);
            });
        }

        [Test]
        public void Api_discover_lists_ten_versions_unless_told_otherwise()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.root.Parse("api discover").GetValue(ApiDiscoverCommand.Limit), Is.EqualTo(10));
                Assert.That(
                    this.root.Parse("api discover --limit 0").GetValue(ApiDiscoverCommand.Limit), Is.EqualTo(0));
            });
        }

        [TestCase("api fetch")]
        [TestCase("api use")]
        [TestCase("api remove")]
        public void An_api_verb_that_acts_on_one_version_requires_its_tag(string commandLine)
        {
            Assert.That(this.root.Parse(commandLine).Errors, Is.Not.Empty);
        }

        [Test]
        public void An_unknown_verb_is_rejected()
        {
            Assert.That(this.root.Parse("regenerate").Errors, Is.Not.Empty);
        }

        [Test]
        public void An_unknown_api_verb_is_rejected()
        {
            // move-window and check are verbs of the language releases only.
            Assert.Multiple(() =>
            {
                Assert.That(this.root.Parse("api regenerate").Errors, Is.Not.Empty);
                Assert.That(this.root.Parse("api move-window --tag 2026-04").Errors, Is.Not.Empty);
                Assert.That(this.root.Parse("api check").Errors, Is.Not.Empty);
            });
        }

        private Command Api() => this.root.Subcommands.Single(command => command.Name == "api");
    }
}
