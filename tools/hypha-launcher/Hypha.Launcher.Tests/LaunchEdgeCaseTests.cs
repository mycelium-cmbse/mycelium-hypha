// ------------------------------------------------------------------------------------------------
// <copyright file="LaunchEdgeCaseTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Launcher.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Launcher.Provisioning;

    [TestFixture]
    public class LaunchEdgeCaseTests
    {
        private const string Version = "1.6.0";
        private const string Rid = "win-x64";

        private DirectoryInfo workspace = null!;
        private Dictionary<string, string?> environment = null!;
        private string localData = null!;

        [SetUp]
        public void SetUp()
        {
            this.workspace = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"hypha-launch-edge-{Guid.NewGuid():N}"));
            this.localData = Path.Combine(this.workspace.FullName, "local");
            this.environment = [];
        }

        [TearDown]
        public void TearDown() => this.workspace.Delete(recursive: true);

        [Test]
        public void Without_HYPHA_HOME_or_a_per_user_application_data_folder_the_reason_says_to_set_HYPHA_HOME()
        {
            this.localData = string.Empty;

            var target = this.Launcher(Unreachable()).Locate(this.Package(), Rid);

            Assert.Multiple(() =>
            {
                Assert.That(target.Executable, Is.Null);
                Assert.That(target.NeedsDownload, Is.False, "nothing is downloaded without a folder to put it in");
                Assert.That(target.Version, Is.EqualTo(Version));
                Assert.That(target.Failure, Does.Contain("no per-user application data folder").And.Contain("HYPHA_HOME"));
            });
        }

        [TestCase("")]
        [TestCase("   ")]
        public void A_blank_HYPHA_BIN_is_as_if_it_were_not_set(string blank)
        {
            this.environment[Launch.BinVariable] = blank;

            var target = this.Launcher(Unreachable()).Locate(this.Package(), Rid);

            Assert.Multiple(() =>
            {
                Assert.That(target.NeedsDownload, Is.True, "the package's version is downloaded");
                Assert.That(target.Version, Is.EqualTo(Version));
                Assert.That(target.Failure, Is.Null);
            });
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void The_manifest_is_found_up_to_two_folders_above_the_launcher(int depth)
        {
            var bin = this.Package();

            Assert.That(Launch.FindManifest(Below(bin, depth))?.FullName, Is.EqualTo(Path.Combine(bin.FullName, LaunchManifest.FileName)));
        }

        [Test]
        public void A_manifest_three_folders_above_the_launcher_is_not_looked_for()
        {
            var bin = this.Package();

            Assert.That(Launch.FindManifest(Below(bin, 3)), Is.Null);
        }

        [Test]
        public async Task A_failed_download_is_logged_in_the_state_folder_of_this_package_under_HYPHA_HOME()
        {
            var home = Path.Combine(this.workspace.FullName, "home");
            this.environment[HomeFolder.EnvironmentVariable] = home;
            var bin = this.Package();

            await this.Launcher(Unreachable()).FindAsync(bin, Rid);
            await this.Launcher(Unreachable()).FindAsync(bin, Rid);

            var log = new CacheLayout(new DirectoryInfo(home), CacheLayout.ComputeInstallKey(bin.FullName)).DownloadErrorLog;

            Assert.Multiple(() =>
            {
                Assert.That(log.FullName, Does.StartWith(Path.Combine(home, "state")));
                Assert.That(log.Exists, Is.True);
                Assert.That(File.ReadAllLines(log.FullName), Has.Length.EqualTo(2), "each failure is appended");
                Assert.That(File.ReadAllText(log.FullName), Does.Contain("HttpRequestException").And.Contain("no route"));
                Assert.That(Directory.Exists(this.localData), Is.False, "nothing is written outside HYPHA_HOME");
            });
        }

        [Test]
        public async Task A_log_that_cannot_be_written_does_not_hide_why_the_download_failed()
        {
            var home = Path.Combine(this.workspace.FullName, "home");
            this.environment[HomeFolder.EnvironmentVariable] = home;
            Directory.CreateDirectory(home);
            File.WriteAllText(Path.Combine(home, "state"), "a file where the state folder belongs");

            var target = await this.Launcher(Unreachable()).FindAsync(this.Package(), Rid);

            Assert.Multiple(() =>
            {
                Assert.That(target.Executable, Is.Null);
                Assert.That(target.Failure, Does.Contain("could not be downloaded").And.Contain("no route"));
            });
        }

        private static DirectoryInfo Below(DirectoryInfo folder, int depth)
        {
            var below = folder;
            for (var level = 0; level < depth; level++)
            {
                below = below.CreateSubdirectory($"level-{level}");
            }

            return below;
        }

        private static CliProvisioner Unreachable() =>
            new(new HttpClient(new Refusing()) { BaseAddress = new Uri("https://api.github.com/") });

        private DirectoryInfo Package()
        {
            var bin = Directory.CreateDirectory(Path.Combine(this.workspace.FullName, "plugin", "bin"));
            File.WriteAllText(Path.Combine(bin.FullName, LaunchManifest.FileName), $$"""{"hyphaVersion": "{{Version}}"}""");

            return bin;
        }

        private Launch Launcher(CliProvisioner provisioner) =>
            new(name => this.environment.GetValueOrDefault(name), _ => this.localData, provisioner);

        private sealed class Refusing : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
                throw new HttpRequestException("no route", null, HttpStatusCode.ServiceUnavailable);
        }
    }
}
