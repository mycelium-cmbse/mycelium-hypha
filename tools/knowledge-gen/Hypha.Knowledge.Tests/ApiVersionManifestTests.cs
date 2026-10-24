// ------------------------------------------------------------------------------------------------
// <copyright file="ApiVersionManifestTests.cs" company="Starion Group S.A.">
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

    using Hypha.Knowledge.Api;
    using Hypha.Knowledge.Releases;

    /// <summary>
    /// Tests for the installed-API-versions manifest: assembly, ordering, and the file it produces.
    /// </summary>
    [TestFixture]
    public class ApiVersionManifestTests
    {
        private static IReadOnlyList<InstalledApiVersion> TwoVersions() =>
        [
            Version("2025-09", "1111aaaa", "2222bbbb", "3333cccc"),
            Version("2026-04", "4444dddd", "5555eeee", "6666ffff"),
        ];

        [Test]
        public void Lists_versions_newest_first_regardless_of_input_order()
        {
            var manifest = ApiVersionManifest.Build("2026-04", TwoVersions());

            Assert.That(manifest.GetTags(), Is.EqualTo(new[] { "2026-04", "2025-09" }));
        }

        [Test]
        public void A_point_release_sorts_after_its_base_tag()
        {
            var manifest = ApiVersionManifest.Build(
                "2025-09",
                [
                    Version("2025-09", "a", "b", "c"),
                    Version("2026-04", "a", "b", "c"),
                    Version("2025-09.1", "a", "b", "c"),
                ]);

            Assert.That(manifest.GetTags(), Is.EqualTo(new[] { "2026-04", "2025-09.1", "2025-09" }));
        }

        [Test]
        public void Records_all_three_upstream_commits_and_the_pim_document_per_tag()
        {
            var newest = ApiVersionManifest.Build("2026-04", TwoVersions()).Versions[0];

            Assert.Multiple(() =>
            {
                Assert.That(newest.Services.Commit, Is.EqualTo("4444dddd"));
                Assert.That(newest.Release.Commit, Is.EqualTo("5555eeee"));
                Assert.That(newest.Pilot.Commit, Is.EqualTo("6666ffff"));
                Assert.That(newest.Services.Repo, Is.EqualTo(ApiUpstream.ServicesRepository));
                Assert.That(newest.Release.Repo, Is.EqualTo(Upstream.ReleaseRepository));
                Assert.That(newest.Pilot.Repo, Is.EqualTo(Upstream.PilotRepository));
                Assert.That(newest.Pim, Is.EqualTo(ApiUpstream.PimDocument));
            });
        }

        [Test]
        public void Carries_the_default_and_schema_version()
        {
            var manifest = ApiVersionManifest.Build("2025-09", TwoVersions());

            Assert.Multiple(() =>
            {
                Assert.That(manifest.Default, Is.EqualTo("2025-09"));
                Assert.That(manifest.SchemaVersion, Is.EqualTo(ApiVersionManifest.CurrentSchemaVersion));
            });
        }

        [Test]
        public void Rejects_a_default_that_is_not_installed()
        {
            // A default pointing at an absent version would send every skill to a missing directory.
            var exception = Assert.Throws<ArgumentException>(
                () => ApiVersionManifest.Build("2026-05", TwoVersions()));

            Assert.That(exception!.Message, Does.Contain("not among the installed API versions"));
        }

        [Test]
        public void Rejects_a_manifest_with_nothing_installed()
        {
            Assert.Throws<ArgumentException>(() => ApiVersionManifest.Build("2026-04", []));
        }

        [Test]
        public void Rejects_a_blank_default()
        {
            Assert.Multiple(() =>
            {
                Assert.Throws<ArgumentException>(() => ApiVersionManifest.Build(" ", TwoVersions()));
                Assert.Throws<ArgumentNullException>(() => ApiVersionManifest.Build("2026-04", null!));
            });
        }

        [Test]
        public void Render_is_deterministic_and_newline_terminated()
        {
            var first = ApiVersionManifestFile.Render(ApiVersionManifest.Build("2026-04", TwoVersions()));

            var reversed = new List<InstalledApiVersion>(TwoVersions());
            reversed.Reverse();
            var second = ApiVersionManifestFile.Render(ApiVersionManifest.Build("2026-04", reversed));

            Assert.Multiple(() =>
            {
                Assert.That(second, Is.EqualTo(first));
                Assert.That(first, Does.EndWith("}\n"));
                Assert.That(first, Does.Not.Contain("\r\n"), "LF endings, independent of the host");
            });
        }

        [Test]
        public void Render_uses_the_documented_property_names()
        {
            var rendered = ApiVersionManifestFile.Render(ApiVersionManifest.Build("2026-04", TwoVersions()));

            Assert.Multiple(() =>
            {
                Assert.That(rendered, Does.Contain("\"schemaVersion\": \"1.0.0\""));
                Assert.That(rendered, Does.Contain("\"default\": \"2026-04\""));
                Assert.That(rendered, Does.Contain("\"versions\": ["));
                Assert.That(rendered, Does.Contain("\"tag\": \"2026-04\""));
                Assert.That(rendered, Does.Contain("\"services\": {"));
                Assert.That(rendered, Does.Contain("\"release\": {"));
                Assert.That(rendered, Does.Contain("\"pilot\": {"));
                Assert.That(rendered, Does.Contain("\"pim\": \"ptc/25-02-29\""), "the slash is not escaped");
                Assert.That(
                    rendered,
                    Does.Contain("\"repo\": \"Systems-Modeling/SysML-v2-API-Services\""),
                    "the repository is recorded as owner/name, unescaped");
            });
        }

        [Test]
        public void Round_trips_through_disk()
        {
            var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"{Guid.NewGuid():N}.json");

            try
            {
                var written = ApiVersionManifestFile.Write(
                    ApiVersionManifest.Build("2025-09", TwoVersions()), path);

                var read = ApiVersionManifestFile.ReadIfPresent(path);

                Assert.Multiple(() =>
                {
                    Assert.That(written.FullName, Is.EqualTo(new FileInfo(path).FullName));
                    Assert.That(read, Is.Not.Null);
                    Assert.That(read!.GetTags(), Is.EqualTo(new[] { "2026-04", "2025-09" }));
                    Assert.That(read.Default, Is.EqualTo("2025-09"));
                    Assert.That(read.Versions[0], Is.EqualTo(Version("2026-04", "4444dddd", "5555eeee", "6666ffff")));
                    Assert.That(ApiVersionManifestFile.Render(read), Is.EqualTo(File.ReadAllText(path)));
                });
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
                ApiVersionManifestFile.Write(ApiVersionManifest.Build("2026-04", TwoVersions()), path);

                var bytes = File.ReadAllBytes(path);

                Assert.Multiple(() =>
                {
                    Assert.That(bytes[0], Is.EqualTo((byte)'{'), "no UTF-8 BOM");
                    Assert.That(bytes, Has.None.EqualTo((byte)0x0D), "no CR bytes");
                    Assert.That(bytes[^1], Is.EqualTo((byte)0x0A), "a trailing newline");
                });
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Test]
        public void Write_creates_the_folder_the_manifest_lives_in()
        {
            // knowledge/api/ does not exist before the first API version is installed.
            var folder = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"{Guid.NewGuid():N}");
            var path = Path.Combine(folder, "knowledge", "api", ApiVersionManifest.FileName);

            try
            {
                ApiVersionManifestFile.Write(ApiVersionManifest.Build("2026-04", TwoVersions()), path);

                Assert.That(File.Exists(path), Is.True);
            }
            finally
            {
                Directory.Delete(folder, recursive: true);
            }
        }

        [Test]
        public void Read_if_present_returns_null_for_a_missing_file()
        {
            var missing = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"{Guid.NewGuid():N}.json");

            Assert.That(ApiVersionManifestFile.ReadIfPresent(missing), Is.Null);
        }

        [Test]
        public void A_file_holding_json_null_is_not_a_readable_manifest()
        {
            var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"{Guid.NewGuid():N}.json");

            try
            {
                File.WriteAllText(path, "null");

                var exception = Assert.Throws<InvalidOperationException>(
                    () => ApiVersionManifestFile.ReadIfPresent(path));

                Assert.That(exception!.Message, Does.Contain("not a readable API version manifest"));
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Test]
        public void Blank_paths_and_a_missing_manifest_are_refused()
        {
            Assert.Multiple(() =>
            {
                Assert.Throws<ArgumentNullException>(() => ApiVersionManifestFile.Render(null!));
                Assert.Throws<ArgumentException>(() => ApiVersionManifestFile.ReadIfPresent(" "));
                Assert.Throws<ArgumentException>(
                    () => ApiVersionManifestFile.Write(ApiVersionManifest.Build("2026-04", TwoVersions()), " "));
            });
        }

        private static InstalledApiVersion Version(string tag, string services, string release, string pilot) =>
            new(
                tag,
                new UpstreamReference(ApiUpstream.ServicesRepository, services),
                new UpstreamReference(Upstream.ReleaseRepository, release),
                new UpstreamReference(Upstream.PilotRepository, pilot),
                ApiUpstream.PimDocument);
    }
}
