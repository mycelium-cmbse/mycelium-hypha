// ------------------------------------------------------------------------------------------------
// <copyright file="LaunchTests.cs" company="Starion Group S.A.">
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

    using Hypha.Knowledge.Layout;
    using Hypha.Launcher.Provisioning;

    [TestFixture]
    public class LaunchTests
    {
        private DirectoryInfo workspace = null!;
        private Dictionary<string, string?> environment = null!;
        private string localData = null!;

        [SetUp]
        public void SetUp()
        {
            this.workspace = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"hypha-launch-{Guid.NewGuid():N}"));
            this.localData = Path.Combine(this.workspace.FullName, "local");
            this.environment = [];
        }

        [TearDown]
        public void TearDown() => this.workspace.Delete(recursive: true);

        [TestCase("bin")]
        [TestCase("bin", "linux-x64")]
        public void The_manifest_is_found_in_bin_above_the_launcher_on_every_platform(params string[] launcherFolder)
        {
            var bin = Directory.CreateDirectory(Path.Combine(this.workspace.FullName, "plugin", "bin"));
            File.WriteAllText(Path.Combine(bin.FullName, LaunchManifest.FileName), """{"hyphaVersion": "1.6.0"}""");
            var launcher = Directory.CreateDirectory(Path.Combine([this.workspace.FullName, "plugin", .. launcherFolder]));

            var manifest = Launch.FindManifest(launcher);

            Assert.Multiple(() =>
            {
                Assert.That(manifest?.FullName, Is.EqualTo(Path.Combine(bin.FullName, LaunchManifest.FileName)));
                Assert.That(Launch.ReadVersion(manifest!), Is.EqualTo("1.6.0"));
            });
        }

        [TestCase("{}")]
        [TestCase("""{"hyphaVersion": "  "}""")]
        [TestCase("not json")]
        public void A_manifest_that_names_no_version_names_none(string content)
        {
            var manifest = new FileInfo(Path.Combine(this.workspace.FullName, LaunchManifest.FileName));
            File.WriteAllText(manifest.FullName, content);

            Assert.That(Launch.ReadVersion(manifest), Is.Null);
        }

        [Test]
        public async Task HYPHA_BIN_is_started_as_it_is_without_asking_the_network()
        {
            var hypha = new FileInfo(Path.Combine(this.workspace.FullName, "hypha.exe"));
            File.WriteAllText(hypha.FullName, string.Empty);
            this.environment[Launch.BinVariable] = $"  {hypha.FullName} ";

            var target = await this.Launcher(Unreachable()).FindAsync(this.workspace, "win-x64");

            Assert.Multiple(() =>
            {
                Assert.That(target.Executable?.FullName, Is.EqualTo(hypha.FullName));
                Assert.That(target.Failure, Is.Null);
            });
        }

        [TestCase("hypha.exe")]
        [TestCase("tools/hypha")]
        public async Task A_relative_HYPHA_BIN_is_refused_rather_than_resolved_against_the_working_directory(string relative)
        {
            this.environment[Launch.BinVariable] = relative;

            var target = await this.Launcher(Unreachable()).FindAsync(this.workspace, "win-x64");

            Assert.Multiple(() =>
            {
                Assert.That(target.Executable, Is.Null);
                Assert.That(target.Failure, Does.Contain("HYPHA_BIN must be an absolute path").And.Contain(relative));
            });
        }

        [TestCase(@"\tools\hypha.exe")]
        [TestCase("C:hypha.exe")]
        [Platform("Win")]
        public async Task A_HYPHA_BIN_rooted_but_not_fully_qualified_is_refused_on_Windows(string rooted)
        {
            this.environment[Launch.BinVariable] = rooted;

            var target = await this.Launcher(Unreachable()).FindAsync(this.workspace, "win-x64");

            Assert.Multiple(() =>
            {
                Assert.That(target.Executable, Is.Null);
                Assert.That(target.Failure, Does.Contain("HYPHA_BIN must be an absolute path").And.Contain(rooted));
            });
        }

        [Test]
        public async Task A_HYPHA_BIN_that_does_not_exist_says_so_rather_than_downloading()
        {
            this.environment[Launch.BinVariable] = Path.Combine(this.workspace.FullName, "missing");

            var target = await this.Launcher(Unreachable()).FindAsync(this.workspace, "win-x64");

            Assert.Multiple(() =>
            {
                Assert.That(target.Executable, Is.Null);
                Assert.That(target.Failure, Does.Contain("HYPHA_BIN").And.Contain("does not exist"));
            });
        }

        [Test]
        public async Task A_package_without_its_manifest_is_reported_as_damaged()
        {
            var target = await this.Launcher(Unreachable()).FindAsync(this.workspace, "win-x64");

            Assert.That(target.Failure, Does.Contain(LaunchManifest.FileName).And.Contain("HYPHA_BIN"));
        }

        [Test]
        public async Task The_version_is_provisioned_into_HYPHA_HOME_bin()
        {
            var bin = this.Package("1.6.0");
            var home = Path.Combine(this.workspace.FullName, "home");
            this.environment[HomeFolder.EnvironmentVariable] = home;
            var cached = new CacheLayout(new DirectoryInfo(home), "unused").CliExecutable("1.6.0", "win-x64");
            Directory.CreateDirectory(cached.DirectoryName!);
            File.WriteAllText(cached.FullName, string.Empty);
            File.WriteAllText(Path.Combine(cached.DirectoryName!, ".ok"), "sha");

            var target = await this.Launcher(Unreachable()).FindAsync(bin, "win-x64");

            Assert.Multiple(() =>
            {
                Assert.That(target.Executable?.FullName, Is.EqualTo(cached.FullName));
                Assert.That(target.Version, Is.EqualTo("1.6.0"));
                Assert.That(cached.FullName, Does.StartWith(Path.Combine(home, "bin", "1.6.0")));
            });
        }

        [Test]
        public async Task A_download_that_fails_says_why_and_how_to_fix_it()
        {
            var bin = this.Package("1.6.0");
            this.environment[HomeFolder.EnvironmentVariable] = Path.Combine(this.workspace.FullName, "home");

            var target = await this.Launcher(Unreachable()).FindAsync(bin, "win-x64");

            Assert.Multiple(() =>
            {
                Assert.That(target.Executable, Is.Null);
                Assert.That(target.Version, Is.EqualTo("1.6.0"));
                Assert.That(target.Failure, Does.Contain("hypha 1.6.0 for win-x64 could not be downloaded").And.Contain("no route"));
                Assert.That(target.Failure, Does.Contain("HTTPS_PROXY").And.Contain("GITHUB_TOKEN").And.Contain("HYPHA_BIN"));
            });
        }

        [Test]
        public async Task A_relative_HYPHA_HOME_is_reported_rather_than_used()
        {
            var bin = this.Package("1.6.0");
            this.environment[HomeFolder.EnvironmentVariable] = "relative/home";

            var target = await this.Launcher(Unreachable()).FindAsync(bin, "win-x64");

            Assert.That(target.Failure, Does.Contain("HYPHA_HOME must be an absolute path"));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        public void HYPHA_HOME_resolves_to_the_folder_the_knowledge_library_resolves(string? configured)
        {
            Func<string, string?> variable = name => name == HomeFolder.EnvironmentVariable ? configured : null;
            Func<Environment.SpecialFolder, string> folder = _ => this.localData;

            Assert.That(HomeFolder.Resolve(variable, folder).FullName, Is.EqualTo(HyphaHome.Resolve(variable, folder).FullName));
        }

        [Test]
        public void An_absolute_HYPHA_HOME_resolves_to_the_folder_the_knowledge_library_resolves()
        {
            var configured = $" {Path.Combine(this.workspace.FullName, "elsewhere")} ";
            Func<string, string?> variable = name => name == HomeFolder.EnvironmentVariable ? configured : null;
            Func<Environment.SpecialFolder, string> folder = _ => this.localData;

            Assert.That(HomeFolder.Resolve(variable, folder).FullName, Is.EqualTo(HyphaHome.Resolve(variable, folder).FullName));
        }

        [TestCase("relative")]
        [TestCase("./home")]
        public void HYPHA_HOME_refuses_what_the_knowledge_library_refuses(string configured)
        {
            Func<string, string?> variable = name => name == HomeFolder.EnvironmentVariable ? configured : null;
            Func<Environment.SpecialFolder, string> folder = _ => this.localData;

            Assert.Multiple(() =>
            {
                Assert.That(() => HomeFolder.Resolve(variable, folder), Throws.InvalidOperationException);
                Assert.That(() => HyphaHome.Resolve(variable, folder), Throws.InvalidOperationException);
            });
        }

        [Test]
        public void Locating_a_cached_hypha_asks_nothing_of_the_network()
        {
            var bin = this.Package("1.6.0");
            var home = Path.Combine(this.workspace.FullName, "home");
            this.environment[HomeFolder.EnvironmentVariable] = home;
            var cached = new CacheLayout(new DirectoryInfo(home), "unused").CliExecutable("1.6.0", "win-x64");
            Directory.CreateDirectory(cached.DirectoryName!);
            File.WriteAllText(cached.FullName, string.Empty);
            File.WriteAllText(Path.Combine(cached.DirectoryName!, ".ok"), "sha");

            var located = this.Launcher(Unreachable()).Locate(bin, "win-x64");

            Assert.Multiple(() =>
            {
                Assert.That(located.Executable?.FullName, Is.EqualTo(cached.FullName));
                Assert.That(located.NeedsDownload, Is.False);
            });
        }

        [Test]
        public void Locating_a_hypha_that_is_not_cached_yet_says_what_to_download_without_downloading_it()
        {
            var bin = this.Package("1.6.0");
            this.environment[HomeFolder.EnvironmentVariable] = Path.Combine(this.workspace.FullName, "home");

            var located = this.Launcher(Unreachable()).Locate(bin, "win-x64");

            Assert.Multiple(() =>
            {
                Assert.That(located.NeedsDownload, Is.True);
                Assert.That(located.Version, Is.EqualTo("1.6.0"));
                Assert.That(located.Executable, Is.Null);
                Assert.That(located.Failure, Is.Null);
            });
        }

        [Test]
        public async Task A_download_that_fails_in_any_way_ends_in_a_reason_rather_than_an_exception()
        {
            var bin = this.Package("1.6.0");
            this.environment[HomeFolder.EnvironmentVariable] = Path.Combine(this.workspace.FullName, "home");
            var launch = this.Launcher(new CliProvisioner(new HttpClient(new Throwing(new NotSupportedException("an unexpected failure")))
            {
                BaseAddress = new Uri("https://api.github.com/"),
            }));

            var target = await launch.DownloadAsync(launch.Locate(bin, "win-x64"), "win-x64");

            Assert.Multiple(() =>
            {
                Assert.That(target.Executable, Is.Null);
                Assert.That(target.Failure, Does.Contain("an unexpected failure").And.Contain("HYPHA_BIN"));
            });
        }

        [Test]
        public void Downloading_what_needs_no_download_is_refused()
        {
            var launch = this.Launcher(Unreachable());

            Assert.That(() => launch.DownloadAsync(LaunchTarget.Missing("no"), "win-x64"), Throws.ArgumentException);
        }

        private static CliProvisioner Unreachable() =>
            new(new HttpClient(new Refusing()) { BaseAddress = new Uri("https://api.github.com/") });

        private DirectoryInfo Package(string version)
        {
            var bin = Directory.CreateDirectory(Path.Combine(this.workspace.FullName, "plugin", "bin"));
            File.WriteAllText(Path.Combine(bin.FullName, LaunchManifest.FileName), $$"""{"hyphaVersion": "{{version}}"}""");

            return bin;
        }

        private Launch Launcher(CliProvisioner provisioner) =>
            new(name => this.environment.GetValueOrDefault(name), _ => this.localData, provisioner);

        private sealed class Throwing(Exception exception) : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
                throw exception;
        }

        private sealed class Refusing : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
                throw new HttpRequestException("no route to api.github.com", null, HttpStatusCode.ServiceUnavailable);
        }
    }
}
