// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Hypha.Knowledge.Tests
{
    using System.IO;

    using Hypha.Knowledge.Migration;

    using NUnit.Framework;

    [TestFixture]
    public class LeftOutFileTests
    {
        private DirectoryInfo home = null!;

        [SetUp]
        public void SetUp()
        {
            this.home = Directory.CreateTempSubdirectory("hypha-left-out-");
        }

        [TearDown]
        public void TearDown()
        {
            this.home.Delete(recursive: true);
        }

        [Test]
        public void Without_a_record_nothing_was_left_out()
        {
            Assert.That(LeftOutFile.Read(this.home), Is.Empty);
        }

        [Test]
        public void A_record_that_cannot_be_read_counts_as_nothing_left_out()
        {
            File.WriteAllText(LeftOutFile.In(this.home).FullName, "{ not json");

            Assert.That(LeftOutFile.Read(this.home), Is.Empty);
        }

        [Test]
        public void Nothing_left_out_writes_no_record()
        {
            LeftOutFile.Add(this.home, []);

            Assert.That(LeftOutFile.In(this.home).Exists, Is.False);
        }

        [Test]
        public void A_later_migration_replaces_the_entry_of_the_same_kind_and_tag_and_keeps_the_others()
        {
            LeftOutFile.Add(this.home, [
                new LeftOutVersion(MigratedVersion.Release, "2026-02", "Claude plugin", "old"),
                new LeftOutVersion(MigratedVersion.Api, "2026-02", "Claude plugin", "old"),
            ]);

            LeftOutFile.Add(this.home, [new LeftOutVersion(MigratedVersion.Release, "2026-02", "VS Code", "new")]);

            Assert.That(
                LeftOutFile.Read(this.home),
                Is.EqualTo(new[]
                {
                    new LeftOutVersion(MigratedVersion.Api, "2026-02", "Claude plugin", "old"),
                    new LeftOutVersion(MigratedVersion.Release, "2026-02", "VS Code", "new"),
                }));
        }

        [Test]
        public void One_migration_naming_the_same_kind_and_tag_twice_is_recorded_once_keeping_the_first()
        {
            LeftOutFile.Add(this.home, [
                new LeftOutVersion(MigratedVersion.Release, "2026-02", "VS Code", "first"),
                new LeftOutVersion(MigratedVersion.Release, "2026-02", "Claude plugin 2.2.0", "second"),
            ]);

            Assert.That(
                LeftOutFile.Read(this.home),
                Is.EqualTo(new[] { new LeftOutVersion(MigratedVersion.Release, "2026-02", "VS Code", "first") }));
        }

        [Test]
        public void The_record_is_written_with_line_feeds_and_camel_case_names()
        {
            LeftOutFile.Add(this.home, [new LeftOutVersion(MigratedVersion.Release, "2026-02", "Claude plugin", "old")]);

            var text = File.ReadAllText(LeftOutFile.In(this.home).FullName);

            Assert.Multiple(() =>
            {
                Assert.That(text, Does.Not.Contain("\r"));
                Assert.That(text, Does.Contain("\"versions\"").And.Contain("\"reason\""));
            });
        }
    }
}
