// ------------------------------------------------------------------------------------------------
// <copyright file="HyphaHomeTests.cs" company="Starion Group S.A.">
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

    using Hypha.Knowledge.Layout;

    [TestFixture]
    public class HyphaHomeTests
    {
        private static readonly string Data = Path.Combine(Path.GetTempPath(), "hypha-data");

        [Test]
        public void HYPHA_HOME_names_the_folder_as_a_full_path()
        {
            var configured = Path.Combine(Path.GetTempPath(), "somewhere", "hypha-home");

            var home = HyphaHome.Resolve(Variable(configured), _ => Data);

            Assert.That(home.FullName, Is.EqualTo(Path.GetFullPath(configured)));
        }

        [Test]
        [TestCase("relative-home")]
        [TestCase(" relative-home ")]
        [TestCase("./home")]
        [TestCase("~/hypha")]
        [TestCase(@"\hypha")]
        [TestCase("C:hypha")]
        public void A_relative_HYPHA_HOME_is_refused_so_every_client_shares_one_folder(string configured)
        {
            Assert.That(
                () => HyphaHome.Resolve(Variable(configured), _ => Data),
                Throws.InvalidOperationException.With.Message.Contains("HYPHA_HOME must be an absolute path")
                    .And.Message.Contains(configured.Trim()));
        }

        [Test]
        public void An_invalid_HYPHA_HOME_says_it_is_not_a_valid_folder()
        {
            var configured = Path.Combine(Path.GetTempPath(), "hy\0pha");

            Assert.That(
                () => HyphaHome.Resolve(Variable(configured), _ => Data),
                Throws.InvalidOperationException.With.Message.Contains("HYPHA_HOME is not a valid folder"));
        }

        [Test]
        public void An_absolute_HYPHA_HOME_is_trimmed()
        {
            var configured = Path.Combine(Path.GetTempPath(), "hypha-home");

            Assert.That(HyphaHome.Resolve(Variable($"  {configured}  "), _ => Data).FullName, Is.EqualTo(Path.GetFullPath(configured)));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void Without_HYPHA_HOME_it_is_hypha_in_the_per_user_local_data_folder(string? configured)
        {
            Environment.SpecialFolder? asked = null;

            var home = HyphaHome.Resolve(Variable(configured), folder =>
            {
                asked = folder;
                return Data;
            });

            Assert.Multiple(() =>
            {
                Assert.That(asked, Is.EqualTo(Environment.SpecialFolder.LocalApplicationData));
                Assert.That(home.FullName, Is.EqualTo(Path.Combine(Data, "hypha")));
            });
        }

        [Test]
        public void Without_HYPHA_HOME_or_a_local_data_folder_it_says_to_set_HYPHA_HOME()
        {
            Assert.That(
                () => HyphaHome.Resolve(Variable(null), _ => string.Empty),
                Throws.InvalidOperationException.With.Message.Contains("HYPHA_HOME"));
        }

        [Test]
        public void Only_HYPHA_HOME_is_read()
        {
            string? read = null;

            HyphaHome.Resolve(name =>
            {
                read = name;
                return null;
            }, _ => Data);

            Assert.That(read, Is.EqualTo("HYPHA_HOME"));
        }

        [Test]
        public void Configured_names_the_folder_only_when_HYPHA_HOME_is_set()
        {
            var configured = Path.Combine(Path.GetTempPath(), "hypha-home");

            Assert.Multiple(() =>
            {
                Assert.That(HyphaHome.Configured(Variable(configured))?.FullName, Is.EqualTo(Path.GetFullPath(configured)));
                Assert.That(HyphaHome.Configured(Variable(null)), Is.Null);
                Assert.That(HyphaHome.Configured(Variable("  ")), Is.Null);
            });
        }

        [Test]
        public void The_toolchain_is_kept_in_toolchain_under_the_home_folder()
        {
            var home = new DirectoryInfo(Path.Combine(Data, "hypha"));

            Assert.Multiple(() =>
            {
                Assert.That(HyphaHome.Toolchain(home).FullName, Is.EqualTo(Path.Combine(home.FullName, "toolchain")));
                Assert.That(() => HyphaHome.Toolchain(null!), Throws.ArgumentNullException);
            });
        }

        private static Func<string, string?> Variable(string? value) =>
            name => name == HyphaHome.EnvironmentVariable ? value : null;
    }
}
