// ------------------------------------------------------------------------------------------------
// <copyright file="SpecExtractProjectTests.cs" company="Starion Group S.A.">
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
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Toolchain;

    [TestFixture]
    public class SpecExtractProjectTests
    {
        private DirectoryInfo workspace = null!;
        private DirectoryInfo home = null!;
        private DirectoryInfo toolchain = null!;
        private SpecExtractProject project = null!;

        [SetUp]
        public void SetUp()
        {
            this.workspace = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"hypha-spec-project-{Guid.NewGuid():N}"));
            this.home = this.workspace.CreateSubdirectory("home");
            this.toolchain = new DirectoryInfo(Path.Combine(this.home.FullName, "toolchain"));
            this.project = new SpecExtractProject(new KnowledgeLayout(this.home), this.toolchain);
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

        private string Unpacked(params string[] parts) =>
            Path.Combine([this.toolchain.FullName, SpecExtractProject.FolderName, .. parts]);

        [Test]
        public async Task A_knowledge_folder_without_tools_gets_the_project_unpacked_beside_the_toolchain()
        {
            var folder = await this.project.EnsureAsync();

            Assert.Multiple(() =>
            {
                Assert.That(folder.FullName, Is.EqualTo(this.Unpacked()));
                Assert.That(File.ReadAllText(this.Unpacked("pyproject.toml")), Does.Contain("name = \"spec-extract\""));
                Assert.That(File.Exists(this.Unpacked("uv.lock")), Is.True);
                Assert.That(File.Exists(this.Unpacked("README.md")), Is.True, "pyproject.toml names it as the readme");
                Assert.That(File.Exists(this.Unpacked("src", "spec_extract", "__main__.py")), Is.True);
                Assert.That(File.Exists(this.Unpacked("src", "spec_extract", "pipeline.py")), Is.True);
                Assert.That(Directory.Exists(Path.Combine(this.home.FullName, "tools")), Is.False, "nothing is written into the knowledge folder itself");
            });
        }

        [Test]
        public async Task The_unpacked_project_is_the_one_the_repository_has()
        {
            await this.project.EnsureAsync();

            var source = new DirectoryInfo(Path.Combine(Repository.Layout!.Root.FullName, "tools", "spec-extract"));
            var expected = new[] { "pyproject.toml", "uv.lock", "README.md" }
                .Concat(new DirectoryInfo(Path.Combine(source.FullName, "src"))
                    .EnumerateFiles("*.py", SearchOption.AllDirectories)
                    .Select(file => Path.GetRelativePath(source.FullName, file.FullName)))
                .Select(relative => relative.Replace('\\', '/'))
                .Order(StringComparer.Ordinal);

            var unpacked = new DirectoryInfo(this.Unpacked())
                .EnumerateFiles("*", SearchOption.AllDirectories)
                .Select(file => Path.GetRelativePath(this.Unpacked(), file.FullName).Replace('\\', '/'))
                .Order(StringComparer.Ordinal);

            Assert.Multiple(() =>
            {
                Assert.That(unpacked, Is.EqualTo(expected), "every source and the three project files, and no tests");
                Assert.That(
                    File.ReadAllText(this.Unpacked("src", "spec_extract", "pipeline.py")),
                    Is.EqualTo(File.ReadAllText(Path.Combine(source.FullName, "src", "spec_extract", "pipeline.py"))));
            });
        }

        [Test]
        public async Task A_source_checkout_keeps_using_its_own_project()
        {
            var own = Directory.CreateDirectory(Path.Combine(this.home.FullName, "tools", "spec-extract"));
            File.WriteAllText(Path.Combine(own.FullName, "pyproject.toml"), "[project]");

            var folder = await this.project.EnsureAsync();

            Assert.Multiple(() =>
            {
                Assert.That(folder.FullName, Is.EqualTo(own.FullName));
                Assert.That(Directory.Exists(this.Unpacked()), Is.False);
                Assert.That(File.ReadAllText(Path.Combine(own.FullName, "pyproject.toml")), Is.EqualTo("[project]"), "a checkout's project is never overwritten");
            });
        }

        [Test]
        public async Task A_tools_folder_without_the_project_file_is_not_taken_for_a_checkout()
        {
            Directory.CreateDirectory(Path.Combine(this.home.FullName, "tools", "spec-extract"));

            var folder = await this.project.EnsureAsync();

            Assert.That(folder.FullName, Is.EqualTo(this.Unpacked()));
        }

        [Test]
        public async Task What_an_older_hypha_unpacked_is_brought_in_step_and_its_environment_kept()
        {
            await this.project.EnsureAsync();
            File.WriteAllText(this.Unpacked("pyproject.toml"), "[project]\nname = \"older\"");
            File.WriteAllText(this.Unpacked("src", "spec_extract", "removed.py"), "# a module this hypha no longer has");
            Directory.CreateDirectory(this.Unpacked(".venv"));
            File.WriteAllText(this.Unpacked(".venv", "marker"), "kept");

            await this.project.EnsureAsync();

            Assert.Multiple(() =>
            {
                Assert.That(File.ReadAllText(this.Unpacked("pyproject.toml")), Does.Contain("name = \"spec-extract\""));
                Assert.That(File.Exists(this.Unpacked("src", "spec_extract", "removed.py")), Is.False);
                Assert.That(File.ReadAllText(this.Unpacked(".venv", "marker")), Is.EqualTo("kept"));
            });
        }

        [Test]
        public async Task A_project_already_in_step_is_not_written_again()
        {
            await this.project.EnsureAsync();
            var file = new FileInfo(this.Unpacked("uv.lock"));
            var written = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            File.SetLastWriteTimeUtc(file.FullName, written);

            await this.project.EnsureAsync();

            file.Refresh();
            Assert.That(file.LastWriteTimeUtc, Is.EqualTo(written));
        }

        [Test]
        public void A_cancelled_unpack_stops()
        {
            using var cancelled = new CancellationTokenSource();
            cancelled.Cancel();

            Assert.That(() => this.project.EnsureAsync(cancelled.Token), Throws.InstanceOf<OperationCanceledException>());
        }
    }
}
