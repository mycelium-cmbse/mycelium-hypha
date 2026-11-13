// ------------------------------------------------------------------------------------------------
// <copyright file="MigrateCommandTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Tests
{
    using System.Collections.Generic;
    using System.CommandLine;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Migration;
    using Hypha.Tools.Commands;

    using Moq;

    [TestFixture]
    public class MigrateCommandTests
    {
        private static readonly DirectoryInfo Home = new(Path.Combine(Path.GetTempPath(), "hypha-home"));

        private static readonly LegacyInstall[] Clients =
        [
            new(new DirectoryInfo(Path.Combine(Path.GetTempPath(), "plugin")), "Claude plugin 2.1.2"),
            new(new DirectoryInfo(Path.Combine(Path.GetTempPath(), "vscode")), "VS Code"),
        ];

        private RootCommand root = null!;
        private Mock<IKnowledgeMigration> migration = null!;

        [SetUp]
        public void SetUp()
        {
            this.root = new RootCommand();
            this.root.Add(new MigrateCommand());
            this.migration = new Mock<IKnowledgeMigration>();
        }

        [Test]
        public async Task It_says_what_it_copied_from_which_client()
        {
            this.migration
                .Setup(m => m.MigrateAsync(It.IsAny<IReadOnlyList<DirectoryInfo>?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MigrationResult(
                    Home,
                    Clients,
                    [new MigratedVersion(MigratedVersion.Release, "2026-05", "Claude plugin 2.1.2"), new MigratedVersion(MigratedVersion.Api, "2026-04", "VS Code")]));

            using var console = new RecordedConsole(width: 1000);
            var result = await this.Invoke("migrate");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Zero);
                Assert.That(console.Output, Does.Contain("Copied release 2026-05 from Claude plugin 2.1.2"));
                Assert.That(console.Output, Does.Contain("Copied API version 2026-04 from VS Code"));
                Assert.That(console.Output, Does.Contain(Home.FullName));
            });
        }

        [Test]
        public async Task With_nothing_to_copy_it_says_so()
        {
            this.migration
                .Setup(m => m.MigrateAsync(It.IsAny<IReadOnlyList<DirectoryInfo>?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MigrationResult(Home, Clients, []));

            using var console = new RecordedConsole(width: 1000);
            var result = await this.Invoke("migrate");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Zero);
                Assert.That(console.Output, Does.Contain("Nothing to copy").And.Contain("those 2 folders"));
            });
        }

        [Test]
        public async Task With_no_client_install_found_it_says_so_and_points_at_from()
        {
            this.migration
                .Setup(m => m.MigrateAsync(It.IsAny<IReadOnlyList<DirectoryInfo>?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MigrationResult(Home, [], []));

            using var console = new RecordedConsole(width: 1000);
            var result = await this.Invoke("migrate");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Zero);
                Assert.That(console.Output, Does.Contain("No Claude plugin or VS Code extension install was found").And.Contain("--from"));
                Assert.That(console.Output, Does.Not.Contain("Nothing to copy"));
            });
        }

        [Test]
        public async Task A_from_folder_without_knowledge_is_a_usage_error_naming_it()
        {
            this.migration
                .Setup(m => m.MigrateAsync(It.IsAny<IReadOnlyList<DirectoryInfo>?>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new System.ArgumentException("'C:\\typo' holds no knowledge/versions.json, so it has nothing to copy"));

            using var console = new RecordedConsole(width: 1000);
            var result = await this.Invoke("migrate --from typo");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(2));
                Assert.That(console.Output, Does.Contain("holds no knowledge/versions.json"));
            });
        }

        [Test]
        public async Task The_folders_named_with_from_are_the_ones_copied_from()
        {
            IReadOnlyList<DirectoryInfo>? asked = null;
            this.migration
                .Setup(m => m.MigrateAsync(It.IsAny<IReadOnlyList<DirectoryInfo>?>(), It.IsAny<CancellationToken>()))
                .Callback<IReadOnlyList<DirectoryInfo>?, CancellationToken>((from, _) => asked = from)
                .ReturnsAsync(new MigrationResult(Home, [], []));

            var first = Path.Combine(Path.GetTempPath(), "one");
            var second = Path.Combine(Path.GetTempPath(), "two");
            using var console = new RecordedConsole(width: 1000);
            await this.Invoke($"migrate --from \"{first}\" --from \"{second}\"");

            Assert.That(asked!.Select(folder => folder.FullName), Is.EqualTo(new[] { first, second }));
        }

        [Test]
        public async Task Without_from_the_clients_folders_are_looked_for()
        {
            IReadOnlyList<DirectoryInfo>? asked = [new DirectoryInfo("unset")];
            this.migration
                .Setup(m => m.MigrateAsync(It.IsAny<IReadOnlyList<DirectoryInfo>?>(), It.IsAny<CancellationToken>()))
                .Callback<IReadOnlyList<DirectoryInfo>?, CancellationToken>((from, _) => asked = from)
                .ReturnsAsync(new MigrationResult(Home, [], []));

            using var console = new RecordedConsole(width: 1000);
            await this.Invoke("migrate");

            Assert.That(asked, Is.Null.Or.Empty);
        }

        private Task<int> Invoke(string commandLine) =>
            new MigrateCommand.Handler(this.migration.Object).InvokeAsync(this.root.Parse(commandLine), CancellationToken.None);
    }
}
