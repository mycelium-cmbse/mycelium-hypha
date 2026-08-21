// ------------------------------------------------------------------------------------------------
// <copyright file="ChecksumTableTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Hook.Tests
{
    /// <summary>
    /// Suite of tests for <see cref="ChecksumTable"/>.
    /// </summary>
    [TestFixture]
    public class ChecksumTableTests
    {
        // Shaped exactly like release.yml's "Record the checksums" step actually writes into
        // RELEASE_NOTES.md, which becomes the GitHub release body.
        private const string ReleaseBody = """
            `hypha` 1.2.0

            Install as a dotnet tool:

            ```sh
            dotnet tool install --global Hypha.Tools --version 1.2.0
            ```

            Or download the self-contained build for your platform below - it bundles the .NET
            runtime, so nothing needs installing.

            | asset | SHA256 |
            | --- | --- |
            | `hypha-1.2.0-win-x64.zip` | 3f2a91b7c4d0e5f61829304a5b6c7d8e9f0a1b2c3d4e5f60718293a4b5c6d7e0 |
            | `hypha-1.2.0-linux-x64.zip` | a1b2c3d4e5f60718293a4b5c6d7e83f2a91b7c4d0e5f61829304a5b6c7d8e9f0 |
            | `hypha-1.2.0-osx-x64.zip` | 0e5f61829304a5b6c7d8e9f3f2a91b7c4da1b2c3d4e5f60718293a4b5c6d7e80 |
            | `hypha-1.2.0-osx-arm64.zip` | c7d8e9f0a1b2c3d4e5f61829304a5b6c7d83f2a91b7c4d0e5f60718293a4b5c0 |
            """;

        [Test]
        public void Finds_the_checksum_for_the_matching_asset_name()
        {
            var sha = ChecksumTable.Find(ReleaseBody, "hypha-1.2.0-win-x64.zip");

            Assert.That(sha, Is.EqualTo("3f2a91b7c4d0e5f61829304a5b6c7d8e9f0a1b2c3d4e5f60718293a4b5c6d7e0"));
        }

        [Test]
        public void Finds_a_different_row_for_a_different_asset()
        {
            var sha = ChecksumTable.Find(ReleaseBody, "hypha-1.2.0-osx-arm64.zip");

            Assert.That(sha, Is.EqualTo("c7d8e9f0a1b2c3d4e5f61829304a5b6c7d83f2a91b7c4d0e5f60718293a4b5c0"));
        }

        [Test]
        public void An_asset_that_is_not_in_the_table_is_not_found()
        {
            Assert.That(ChecksumTable.Find(ReleaseBody, "hypha-1.2.0-linux-arm64.zip"), Is.Null);
        }

        [TestCase(null)]
        [TestCase("")]
        public void An_empty_or_missing_body_is_not_found(string? body)
        {
            Assert.That(ChecksumTable.Find(body, "hypha-1.2.0-win-x64.zip"), Is.Null);
        }
    }
}
