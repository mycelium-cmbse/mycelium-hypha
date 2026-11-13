// ------------------------------------------------------------------------------------------------
// <copyright file="LegacyInstallLocatorTests.cs" company="Starion Group S.A.">
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
    using System.Linq;

    using Hypha.Knowledge.Migration;

    [TestFixture]
    public class LegacyInstallLocatorTests
    {
        private DirectoryInfo workspace = null!;
        private string profile = null!;
        private string roaming = null!;
        private Dictionary<string, string?> environment = null!;

        [SetUp]
        public void SetUp()
        {
            this.workspace = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"hypha-legacy-{Guid.NewGuid():N}"));
            this.profile = Path.Combine(this.workspace.FullName, "profile");
            this.roaming = Path.Combine(this.workspace.FullName, "roaming");
            this.environment = [];
        }

        [TearDown]
        public void TearDown() => this.workspace.Delete(recursive: true);

        [Test]
        public void Every_version_of_the_Claude_plugin_that_installed_something_is_found()
        {
            Installed(this.profile, ".claude", "plugins", "cache", "mycelium", "hypha", "2.1.2");
            Installed(this.profile, ".claude", "plugins", "cache", "other-marketplace", "hypha", "2.0.0");
            Directory.CreateDirectory(Path.Combine(this.profile, ".claude", "plugins", "cache", "mycelium", "hypha", "1.9.0"));
            Installed(this.profile, ".claude", "plugins", "cache", "mycelium", "uml4net", "1.0.0");

            var found = this.Locator(windows: true).Locate();

            Assert.Multiple(() =>
            {
                Assert.That(found.Select(install => install.Origin), Is.EquivalentTo(new[] { "Claude plugin 2.1.2", "Claude plugin 2.0.0" }));
                Assert.That(
                    found.Select(install => install.Root.FullName),
                    Has.Some.EqualTo(Path.Combine(this.profile, ".claude", "plugins", "cache", "mycelium", "hypha", "2.1.2")));
            });
        }

        [Test]
        public void CLAUDE_CONFIG_DIR_moves_the_plugin_cache()
        {
            var config = Path.Combine(this.workspace.FullName, "claude-config");
            Installed(config, "plugins", "cache", "mycelium", "hypha", "2.1.2");
            Installed(this.profile, ".claude", "plugins", "cache", "mycelium", "hypha", "1.0.0");
            this.environment["CLAUDE_CONFIG_DIR"] = config;

            var found = this.Locator(windows: true).Locate();

            Assert.That(found.Select(install => install.Origin), Is.EqualTo(new[] { "Claude plugin 2.1.2" }));
        }

        [Test]
        public void On_Windows_the_extension_storage_of_each_VS_Code_build_is_found_in_the_roaming_application_data()
        {
            Installed(this.roaming, "Code", "User", "globalStorage", LegacyInstallLocator.ExtensionId);
            Installed(this.roaming, "Code - Insiders", "User", "globalStorage", LegacyInstallLocator.ExtensionId);
            Directory.CreateDirectory(Path.Combine(this.roaming, "VSCodium", "User", "globalStorage", LegacyInstallLocator.ExtensionId));

            var found = this.Locator(windows: true).Locate();

            Assert.That(found.Select(install => install.Origin), Is.EquivalentTo(new[] { "VS Code", "Code - Insiders" }));
        }

        [Test]
        public void On_macOS_it_is_in_the_application_support_folder()
        {
            Installed(this.profile, "Library", "Application Support", "Code", "User", "globalStorage", LegacyInstallLocator.ExtensionId);

            Assert.That(this.Locator(macOS: true).Locate().Select(install => install.Origin), Is.EqualTo(new[] { "VS Code" }));
        }

        [Test]
        public void On_Linux_it_is_in_XDG_CONFIG_HOME_or_else_dot_config()
        {
            var xdg = Path.Combine(this.workspace.FullName, "xdg");
            Installed(xdg, "VSCodium", "User", "globalStorage", LegacyInstallLocator.ExtensionId);
            Installed(this.profile, ".config", "Code", "User", "globalStorage", LegacyInstallLocator.ExtensionId);

            var withoutXdg = this.Locator().Locate().Select(install => install.Origin).ToList();
            this.environment["XDG_CONFIG_HOME"] = xdg;
            var withXdg = this.Locator().Locate().Select(install => install.Origin).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(withoutXdg, Is.EqualTo(new[] { "VS Code" }));
                Assert.That(withXdg, Is.EqualTo(new[] { "VSCodium" }));
            });
        }

        [Test]
        public void Nothing_installed_finds_nothing()
        {
            Assert.That(this.Locator(windows: true).Locate(), Is.Empty);
        }

        private static void Installed(string root, params string[] parts)
        {
            var knowledge = Directory.CreateDirectory(Path.Combine([root, .. parts, "knowledge"]));
            File.WriteAllText(Path.Combine(knowledge.FullName, "versions.json"), "{}");
        }

        private LegacyInstallLocator Locator(bool windows = false, bool macOS = false) =>
            new(
                name => this.environment.GetValueOrDefault(name),
                folder => folder switch
                {
                    Environment.SpecialFolder.UserProfile => this.profile,
                    Environment.SpecialFolder.ApplicationData => this.roaming,
                    _ => string.Empty,
                },
                windows,
                macOS);
    }
}
