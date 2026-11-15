// ------------------------------------------------------------------------------------------------
// <copyright file="LeftOutReleasesTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Tests
{
    using System.Linq;

    using Hypha.Knowledge.Migration;

    using NUnit.Framework;

    public class LeftOutReleasesTests
    {
        private TemporaryKnowledge knowledge = null!;

        [SetUp]
        public void SetUp()
        {
            this.knowledge = new TemporaryKnowledge();
        }

        [TearDown]
        public void TearDown()
        {
            this.knowledge.Dispose();
        }

        [Test]
        public void The_status_names_a_release_the_migration_left_out_and_why()
        {
            LeftOutFile.Add(this.knowledge.Root, [new LeftOutVersion(MigratedVersion.Release, "2026-02", "VS Code extension", "its keywords.json is in schema 1.0.0, not 2.0.0")]);

            var inventory = this.knowledge.Query.Releases();

            Assert.Multiple(() =>
            {
                Assert.That(inventory.Releases, Is.Empty);
                Assert.That(inventory.LeftOut.Select(left => left.Tag), Is.EqualTo(new[] { "2026-02" }));
                Assert.That(inventory.LeftOut[0].Reason, Does.Contain("schema 1.0.0"));
            });
        }

        [Test]
        public void A_release_left_out_and_since_installed_is_no_longer_named()
        {
            LeftOutFile.Add(this.knowledge.Root, [new LeftOutVersion(MigratedVersion.Release, "2026-02", "VS Code extension", "older format")]);
            this.knowledge.Install("2026-02");

            Assert.That(this.knowledge.Query.Releases().LeftOut, Is.Empty);
        }

        [Test]
        public void An_API_version_left_out_is_named_until_that_API_version_is_installed()
        {
            // A release of the same tag is no API version: installing it does not answer for one.
            LeftOutFile.Add(this.knowledge.Root, [new LeftOutVersion(MigratedVersion.Api, "2026-02", "Claude plugin", "older format")]);
            this.knowledge.Install("2026-02");

            Assert.That(
                this.knowledge.Query.Releases().LeftOut.Select(left => (left.Kind, left.Tag)),
                Is.EqualTo(new[] { (MigratedVersion.Api, "2026-02") }));
        }
    }
}
