// ------------------------------------------------------------------------------------------------
// <copyright file="ProvisioningJsonContextTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Launcher.Tests
{
    using System.Text.Json;

    using Hypha.Launcher.Provisioning;

    [TestFixture]
    public class ProvisioningJsonContextTests
    {
        [Test]
        public void GitHubRelease_reads_the_body_and_assets()
        {
            var release = JsonSerializer.Deserialize(
                """
                {"body": "checksum table", "assets": [
                    {"name": "hypha-1.2.0-win-x64.zip", "browser_download_url": "https://example.test/a.zip"}
                ]}
                """,
                ProvisioningJsonContext.Default.GitHubRelease);

            Assert.Multiple(() =>
            {
                Assert.That(release!.Body, Is.EqualTo("checksum table"));
                Assert.That(release.Assets, Has.Count.EqualTo(1));
                Assert.That(release.Assets![0].Name, Is.EqualTo("hypha-1.2.0-win-x64.zip"));
                Assert.That(release.Assets[0].BrowserDownloadUrl, Is.EqualTo("https://example.test/a.zip"));
            });
        }
    }
}
