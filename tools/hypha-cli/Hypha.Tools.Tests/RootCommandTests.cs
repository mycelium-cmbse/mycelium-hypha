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
                    "discover", "fetch", "generate", "list", "move-window", "use", "remove", "check",
                }));
        }

        [Test]
        public void Every_verb_has_an_action_bound_to_it()
        {
            // A verb with no action parses and then does nothing, which looks like success.
            Assert.That(this.root.Subcommands.Select(command => command.Action), Has.All.Not.Null);
        }

        [TestCase("discover")]
        [TestCase("fetch --tag 2026-05")]
        [TestCase("generate")]
        [TestCase("list")]
        [TestCase("move-window --tag 2026-06")]
        [TestCase("use --tag 2026-05")]
        [TestCase("remove --tag 2026-05")]
        [TestCase("check")]
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
        public void An_unknown_verb_is_rejected()
        {
            Assert.That(this.root.Parse("regenerate").Errors, Is.Not.Empty);
        }
    }
}
