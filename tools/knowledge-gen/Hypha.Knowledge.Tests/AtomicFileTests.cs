// ------------------------------------------------------------------------------------------------
// <copyright file="AtomicFileTests.cs" company="Starion Group S.A.">
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

    [TestFixture]
    public class AtomicFileTests
    {
        private DirectoryInfo workspace = null!;

        private string FilePath => Path.Combine(this.workspace.FullName, "nested", "versions.json");

        [SetUp]
        public void SetUp() =>
            this.workspace = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"hypha-atomic-{Guid.NewGuid():N}"));

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
        public async Task The_file_is_written_as_utf8_without_a_bom_creating_its_folder_and_leaving_nothing_beside_it()
        {
            AtomicFile.WriteAllText(this.FilePath, "{\"default\":\"2026-05\"}\n");
            await AtomicFile.WriteAllTextAsync(Path.Combine(this.workspace.FullName, "nested", "other.md"), "Ünïcode\n");

            Assert.Multiple(() =>
            {
                Assert.That(File.ReadAllBytes(this.FilePath).Take(3), Is.Not.EqualTo(new byte[] { 0xEF, 0xBB, 0xBF }));
                Assert.That(File.ReadAllText(this.FilePath), Is.EqualTo("{\"default\":\"2026-05\"}\n"));
                Assert.That(
                    Directory.GetFiles(Path.GetDirectoryName(this.FilePath)!).Select(Path.GetFileName),
                    Is.EquivalentTo(new[] { "versions.json", "other.md" }));
            });
        }

        [Test]
        public async Task An_existing_file_is_replaced()
        {
            AtomicFile.WriteAllText(this.FilePath, "old, and longer than the new content");
            await AtomicFile.WriteAllTextAsync(this.FilePath, "new");

            Assert.That(File.ReadAllText(this.FilePath), Is.EqualTo("new"));
        }

        [Test]
        public void A_cancelled_write_leaves_the_old_file_and_no_temporary_file()
        {
            AtomicFile.WriteAllText(this.FilePath, "old");
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();

            Assert.That(
                () => AtomicFile.WriteAllTextAsync(this.FilePath, "new", cancellation.Token),
                Throws.InstanceOf<OperationCanceledException>());

            Assert.Multiple(() =>
            {
                Assert.That(File.ReadAllText(this.FilePath), Is.EqualTo("old"));
                Assert.That(Directory.GetFiles(Path.GetDirectoryName(this.FilePath)!), Has.Length.EqualTo(1));
            });
        }

        [Test]
        public async Task A_reader_holding_the_old_file_delays_the_write_rather_than_failing_it()
        {
            AtomicFile.WriteAllText(this.FilePath, "old");

            Task writing;
            using (new FileStream(this.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                writing = AtomicFile.WriteAllTextAsync(this.FilePath, "new");
                await Task.Delay(100);
            }

            await writing;

            Assert.That(File.ReadAllText(this.FilePath), Is.EqualTo("new"));
        }

        [Test]
        public async Task A_reader_in_parallel_sees_the_old_content_or_the_new_never_part_of_one()
        {
            var first = new string('a', 64 * 1024);
            var second = new string('b', 32 * 1024);
            AtomicFile.WriteAllText(this.FilePath, first);

            var writer = Task.Run(async () =>
            {
                for (var i = 0; i < 100; i++)
                {
                    await AtomicFile.WriteAllTextAsync(this.FilePath, i % 2 == 0 ? second : first);
                }
            });

            var reads = 0;
            while (!writer.IsCompleted)
            {
                string content;
                try
                {
                    // The way the knowledge readers read a file.
                    content = await File.ReadAllTextAsync(this.FilePath);
                }
                catch (Exception exception) when (exception is UnauthorizedAccessException
                    || (exception is IOException && exception is not FileNotFoundException))
                {
                    // Windows can refuse to open a file in the instant it is being replaced; the reader
                    // tries again. What matters is that the file is never missing and never half written.
                    continue;
                }

                Assert.That(content == first || content == second, Is.True, $"read {content.Length} characters");
                reads++;

                // A reader opens the file to read it and then lets go, as every knowledge reader does.
                // Windows cannot rename over a file while it is open, so one that hardly ever let go
                // would make the write give up (loudly, the old file intact) rather than wait forever.
                await Task.Delay(10);
            }

            await writer;

            Assert.That(reads, Is.GreaterThan(0));
        }

        [Test]
        public void The_path_and_the_content_are_required()
        {
            Assert.Multiple(() =>
            {
                Assert.That(() => AtomicFile.WriteAllText(" ", "x"), Throws.ArgumentException);
                Assert.That(() => AtomicFile.WriteAllText(this.FilePath, null!), Throws.ArgumentNullException);
                Assert.That(() => AtomicFile.WriteAllTextAsync(null!, "x"), Throws.InstanceOf<ArgumentException>());
            });
        }
    }
}
