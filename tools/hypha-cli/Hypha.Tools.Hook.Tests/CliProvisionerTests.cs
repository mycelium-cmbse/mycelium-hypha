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
    /// Suite of tests for the "is it already cached" half of <see cref="CliProvisioner"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="CliProvisionerEnsureAsyncTests"/> covers the download/verify/extract half, against a
    /// stubbed transport; downloading a real archive from a real GitHub release is left to manual
    /// verification - see <c>tools/hypha-cli/README.md</c>.
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

        [TestFixture]
        public class CreateClientTests
        {
            private string? previousGitHubToken;
            private string? previousGhToken;

            [SetUp]
            public void SetUp()
            {
                this.previousGitHubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
                this.previousGhToken = Environment.GetEnvironmentVariable("GH_TOKEN");
                Environment.SetEnvironmentVariable("GITHUB_TOKEN", null);
                Environment.SetEnvironmentVariable("GH_TOKEN", null);
            }

            [TearDown]
            public void TearDown()
            {
                Environment.SetEnvironmentVariable("GITHUB_TOKEN", this.previousGitHubToken);
                Environment.SetEnvironmentVariable("GH_TOKEN", this.previousGhToken);
            }

            [Test]
            public void Points_at_the_GitHub_API_with_the_headers_GitHub_requires()
            {
                using var client = CliProvisioner.CreateClient();

                Assert.Multiple(() =>
                {
                    Assert.That(client.BaseAddress, Is.EqualTo(new Uri("https://api.github.com/")));
                    Assert.That(client.DefaultRequestHeaders.UserAgent.ToString(), Does.Contain("mycelium-hypha"));
                    Assert.That(client.DefaultRequestHeaders.Accept.ToString(), Does.Contain("application/vnd.github+json"));
                });
            }

            [Test]
            public void No_authorization_header_when_neither_token_is_set()
            {
                using var client = CliProvisioner.CreateClient();

                Assert.That(client.DefaultRequestHeaders.Authorization, Is.Null);
            }

            [Test]
            public void GITHUB_TOKEN_becomes_the_bearer_token()
            {
                Environment.SetEnvironmentVariable("GITHUB_TOKEN", "secret-1");

                using var client = CliProvisioner.CreateClient();

                Assert.That(client.DefaultRequestHeaders.Authorization!.Parameter, Is.EqualTo("secret-1"));
            }

            [Test]
            public void GH_TOKEN_is_used_when_GITHUB_TOKEN_is_not_set()
            {
                Environment.SetEnvironmentVariable("GH_TOKEN", "secret-2");

                using var client = CliProvisioner.CreateClient();

                Assert.That(client.DefaultRequestHeaders.Authorization!.Parameter, Is.EqualTo("secret-2"));
            }

            [Test]
            public void GITHUB_TOKEN_wins_when_both_are_set()
            {
                Environment.SetEnvironmentVariable("GITHUB_TOKEN", "secret-1");
                Environment.SetEnvironmentVariable("GH_TOKEN", "secret-2");

                using var client = CliProvisioner.CreateClient();

                Assert.That(client.DefaultRequestHeaders.Authorization!.Parameter, Is.EqualTo("secret-1"));
            }

            [Test]
            public void A_blank_GITHUB_TOKEN_falls_back_to_GH_TOKEN()
            {
                Environment.SetEnvironmentVariable("GITHUB_TOKEN", "   ");
                Environment.SetEnvironmentVariable("GH_TOKEN", "secret-2");

                using var client = CliProvisioner.CreateClient();

                Assert.That(client.DefaultRequestHeaders.Authorization!.Parameter, Is.EqualTo("secret-2"));
            }
        }
    }
}
