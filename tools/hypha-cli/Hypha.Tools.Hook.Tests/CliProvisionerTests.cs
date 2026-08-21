// ------------------------------------------------------------------------------------------------
// <copyright file="CliProvisionerTests.cs" company="Starion Group S.A.">
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
    /// Suite of tests for the pure "is it already cached" half of <see cref="CliProvisioner"/>.
    /// </summary>
    /// <remarks>
    /// Downloading and extracting a real archive needs a real network and real GitHub release, so it
    /// is left to manual verification rather than a unit test - see <c>tools/hypha-cli/README.md</c>.
    /// </remarks>
    [TestFixture]
    public class CliProvisionerTests
    {
        private DirectoryInfo workspace = null!;
        private CacheLayout cache = null!;

        [SetUp]
        public void SetUp()
        {
            this.workspace = Directory.CreateDirectory(
                Path.Combine(Path.GetTempPath(), $"hypha-hook-cache-{Guid.NewGuid():N}"));

            this.cache = new CacheLayout(this.workspace, "abc123");
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

        [Test]
        public void Nothing_cached_yet_is_reported_as_not_cached()
        {
            Assert.That(CliProvisioner.IsCached(this.cache, "1.2.0", "linux-x64"), Is.False);
        }

        [Test]
        public void An_executable_with_no_ok_marker_is_not_trusted()
        {
            // The marker is only ever written after a checksum verifies - an executable without one
            // could be a previous run's failed, partial extraction.
            var executable = this.cache.CliExecutable("1.2.0", "linux-x64");
            executable.Directory!.Create();
            File.WriteAllText(executable.FullName, "not really a binary");

            Assert.That(CliProvisioner.IsCached(this.cache, "1.2.0", "linux-x64"), Is.False);
        }

        [Test]
        public void An_ok_marker_with_no_executable_is_not_trusted()
        {
            var marker = this.cache.OkMarker("1.2.0", "linux-x64");
            marker.Directory!.Create();
            File.WriteAllText(marker.FullName, "deadbeef");

            Assert.That(CliProvisioner.IsCached(this.cache, "1.2.0", "linux-x64"), Is.False);
        }

        [Test]
        public void Both_the_executable_and_its_marker_present_is_cached()
        {
            var executable = this.cache.CliExecutable("1.2.0", "linux-x64");
            executable.Directory!.Create();
            File.WriteAllText(executable.FullName, "pretend binary");
            File.WriteAllText(this.cache.OkMarker("1.2.0", "linux-x64").FullName, "deadbeef");

            Assert.That(CliProvisioner.IsCached(this.cache, "1.2.0", "linux-x64"), Is.True);
        }

        [Test]
        public void A_different_version_or_rid_is_cached_independently()
        {
            var executable = this.cache.CliExecutable("1.2.0", "linux-x64");
            executable.Directory!.Create();
            File.WriteAllText(executable.FullName, "pretend binary");
            File.WriteAllText(this.cache.OkMarker("1.2.0", "linux-x64").FullName, "deadbeef");

            Assert.Multiple(() =>
            {
                Assert.That(CliProvisioner.IsCached(this.cache, "1.3.0", "linux-x64"), Is.False);
                Assert.That(CliProvisioner.IsCached(this.cache, "1.2.0", "win-x64"), Is.False);
            });
        }
    }
}
