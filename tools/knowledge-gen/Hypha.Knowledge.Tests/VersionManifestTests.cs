// ------------------------------------------------------------------------------------------------
// <copyright file="VersionManifestTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    using Hypha.Knowledge.Releases;

    /// <summary>
    /// Tests for the installed-versions manifest: assembly, ordering, and the file it produces.
    /// </summary>
    [TestFixture]
    public class VersionManifestTests
    {
        private static IReadOnlyList<InstalledVersion> TwoVersions() =>
        [
            Version("2026-04", "9baca590", "20897e31"),
            Version("2026-05", "de1070ae", "fa709f28"),
        ];

        [Test]
        public void Lists_versions_newest_first_regardless_of_input_order()
        {
            var manifest = VersionManifest.Build("2026-05", TwoVersions());

            Assert.That(manifest.GetTags(), Is.EqualTo(new[] { "2026-05", "2026-04" }));
        }

        [Test]
        public void Records_both_upstream_commits_per_tag()
        {
            var newest = VersionManifest.Build("2026-05", TwoVersions()).Versions[0];

            Assert.Multiple(() =>
            {
                Assert.That(newest.Release.Commit, Is.EqualTo("de1070ae"));
                Assert.That(newest.Pilot.Commit, Is.EqualTo("fa709f28"));
                Assert.That(newest.Release.Repo, Does.Contain("Release"));
                Assert.That(newest.Pilot.Repo, Does.Contain("Pilot"));
            });
        }

        [Test]
        public void Carries_the_default_and_schema_version()
        {
            var manifest = VersionManifest.Build("2026-04", TwoVersions());

            Assert.Multiple(() =>
            {
                Assert.That(manifest.Default, Is.EqualTo("2026-04"));
                Assert.That(manifest.SchemaVersion, Is.EqualTo(VersionManifest.CurrentSchemaVersion));
            });
        }

        [Test]
        public void Rejects_a_default_that_is_not_installed()
        {
            // A default pointing at an absent release would send every skill to a missing directory.
            var exception = Assert.Throws<ArgumentException>(
                () => VersionManifest.Build("2026-03", TwoVersions()));

            Assert.That(exception!.Message, Does.Contain("not among the installed versions"));
        }

        [Test]
        public void Carries_no_model_uri()
        {
            // The tag is the identifier; the URI inside the XMI tracks neither release nor content.
            var rendered = VersionManifestFile.Render(VersionManifest.Build("2026-05", TwoVersions()));

            Assert.Multiple(() =>
            {
                Assert.That(rendered, Does.Not.Contain("20250201"));
                Assert.That(rendered, Does.Not.Contain("modelVersionUri"));
            });
        }

        [Test]
        public void Render_is_deterministic_and_newline_terminated()
        {
            var first = VersionManifestFile.Render(VersionManifest.Build("2026-05", TwoVersions()));
            var reversed = new List<InstalledVersion>(TwoVersions());
            reversed.Reverse();
            var second = VersionManifestFile.Render(VersionManifest.Build("2026-05", reversed));

            Assert.Multiple(() =>
            {
                Assert.That(second, Is.EqualTo(first));
                Assert.That(first, Does.EndWith("}\n"));
                Assert.That(first, Does.Not.Contain("\r\n"), "LF endings, independent of the host");
            });
        }

        [Test]
        public void Round_trips_through_disk()
        {
            var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"{Guid.NewGuid():N}.json");

            try
            {
                VersionManifestFile.Write(VersionManifest.Build("2026-05", TwoVersions()), path);

                Assert.That(VersionManifestFile.Read(path).GetTags(), Is.EqualTo(new[] { "2026-05", "2026-04" }));
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Test]
        public void Writes_utf8_without_a_byte_order_mark()
        {
            var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"{Guid.NewGuid():N}.json");

            try
            {
                VersionManifestFile.Write(VersionManifest.Build("2026-05", TwoVersions()), path);
                var bytes = File.ReadAllBytes(path);

                Assert.Multiple(() =>
                {
                    Assert.That(bytes[0], Is.Not.EqualTo(0xEF), "no UTF-8 BOM");
                    Assert.That(bytes, Has.None.EqualTo((byte)0x0D), "no CR bytes");
                });
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Test]
        public void Read_if_present_returns_null_for_a_missing_file()
        {
            var missing = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"{Guid.NewGuid():N}.json");

            Assert.That(VersionManifestFile.ReadIfPresent(missing), Is.Null);
        }

        [Test]
        public void Reads_the_manifest_this_repository_actually_ships()
        {
            var committed = RepositoryLayout.VersionManifestPath();
            if (committed is null)
            {
                Assert.Ignore("no committed knowledge/versions.json found");
            }

            var manifest = VersionManifestFile.Read(committed!);

            Assert.Multiple(() =>
            {
                Assert.That(manifest.GetTags(), Is.Not.Empty);
                Assert.That(manifest.GetTags(), Does.Contain(manifest.Default));
                Assert.That(manifest.Versions[0].Release.Commit, Is.Not.Empty);
            });
        }

        [Test]
        public void Rewrites_the_committed_manifest_unchanged()
        {
            // Proves the .NET writer is compatible with the file the Python side produced, without
            // needing the network to re-resolve commits.
            var committed = RepositoryLayout.VersionManifestPath();
            if (committed is null)
            {
                Assert.Ignore("no committed knowledge/versions.json found");
            }

            var onDisk = File.ReadAllText(committed!).Replace("\r\n", "\n");
            var rewritten = VersionManifestFile.Render(VersionManifestFile.Read(committed!));

            Assert.That(rewritten, Is.EqualTo(onDisk));
        }

        private static InstalledVersion Version(string tag, string release, string pilot) =>
            new(
                tag,
                new UpstreamReference(Upstream.ReleaseRepository, release),
                new UpstreamReference(Upstream.PilotRepository, pilot));

        private static class RepositoryLayout
        {
            public static string? VersionManifestPath()
            {
                for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
                {
                    var candidate = Path.Combine(dir.FullName, "knowledge", VersionManifest.FileName);
                    if (File.Exists(candidate))
                    {
                        return candidate;
                    }
                }

                return null;
            }
        }
    }
}
