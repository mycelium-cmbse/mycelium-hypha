// ------------------------------------------------------------------------------------------------
// <copyright file="CacheLayoutTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Hook.Tests
{
    using System;
    using System.IO;

    /// <summary>
    /// Suite of tests for <see cref="CacheLayout"/>.
    /// </summary>
    [TestFixture]
    public class CacheLayoutTests
    {
        [Test]
        public void The_root_is_resolved_from_local_application_data()
        {
            var root = CacheLayout.ResolveRoot(folder =>
                folder == Environment.SpecialFolder.LocalApplicationData
                    ? Path.Combine("C:", "Users", "sam", "AppData", "Local")
                    : throw new InvalidOperationException("unexpected folder requested"));

            Assert.That(root.FullName, Is.EqualTo(
                Path.Combine("C:", "Users", "sam", "AppData", "Local", "mycelium-hypha")));
        }

        [Test]
        public void The_same_path_always_produces_the_same_install_key()
        {
            var first = CacheLayout.ComputeInstallKey(Path.Combine(Path.GetTempPath(), "plugin-root"));
            var second = CacheLayout.ComputeInstallKey(Path.Combine(Path.GetTempPath(), "plugin-root"));

            Assert.That(first, Is.EqualTo(second));
        }

        [Test]
        public void Different_paths_produce_different_install_keys()
        {
            var a = CacheLayout.ComputeInstallKey(Path.Combine(Path.GetTempPath(), "plugin-a"));
            var b = CacheLayout.ComputeInstallKey(Path.Combine(Path.GetTempPath(), "plugin-b"));

            Assert.That(a, Is.Not.EqualTo(b));
        }

        [Test]
        public void A_trailing_directory_separator_does_not_change_the_install_key()
        {
            var root = Path.Combine(Path.GetTempPath(), "plugin-root");

            var withoutSlash = CacheLayout.ComputeInstallKey(root);
            var withSlash = CacheLayout.ComputeInstallKey(root + Path.DirectorySeparatorChar);

            Assert.That(withoutSlash, Is.EqualTo(withSlash));
        }

        [Test]
        public void Casing_does_not_change_the_install_key()
        {
            var lower = CacheLayout.ComputeInstallKey(Path.Combine(Path.GetTempPath(), "plugin-root"));
            var upper = CacheLayout.ComputeInstallKey(Path.Combine(Path.GetTempPath(), "PLUGIN-ROOT"));

            Assert.That(lower, Is.EqualTo(upper));
        }

        [Test]
        public void Bin_directory_and_executable_are_scoped_by_version_and_rid()
        {
            var cache = new CacheLayout(new DirectoryInfo(Path.Combine(Path.GetTempPath(), "cache")), "abc123");

            var executable = cache.CliExecutable("1.2.0", "linux-x64");

            Assert.That(
                executable.FullName,
                Is.EqualTo(Path.Combine(Path.GetTempPath(), "cache", "bin", "1.2.0", "linux-x64", "hypha")));
        }

        [Test]
        public void State_files_are_scoped_by_install_key()
        {
            var cache = new CacheLayout(new DirectoryInfo(Path.Combine(Path.GetTempPath(), "cache")), "abc123");

            Assert.Multiple(() =>
            {
                Assert.That(
                    cache.StatusFile.FullName,
                    Is.EqualTo(Path.Combine(Path.GetTempPath(), "cache", "state", "abc123", "sync-status.json")));
                Assert.That(
                    cache.SyncLogFile.FullName,
                    Is.EqualTo(Path.Combine(Path.GetTempPath(), "cache", "state", "abc123", "sync.log")));
            });
        }
    }
}
