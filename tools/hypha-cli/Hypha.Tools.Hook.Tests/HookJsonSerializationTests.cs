// ------------------------------------------------------------------------------------------------
// <copyright file="HookJsonSerializationTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Hook.Tests
{
    using System.Text.Json;

    /// <summary>
    /// Suite of tests for the small JSON-shaped types read/written through
    /// <see cref="HookJsonContext"/> - the source-generated context NativeAOT needs.
    /// </summary>
    [TestFixture]
    public class HookJsonSerializationTests
    {
        [Test]
        public void PluginManifest_reads_the_pinned_version_field()
        {
            var manifest = JsonSerializer.Deserialize(
                """{"name": "hypha", "hyphaCliVersion": "1.2.0"}""", HookJsonContext.Default.PluginManifest);

            Assert.That(manifest!.HyphaCliVersion, Is.EqualTo("1.2.0"));
        }

        [Test]
        public void PluginManifest_tolerates_a_missing_version_field()
        {
            var manifest = JsonSerializer.Deserialize(
                """{"name": "hypha"}""", HookJsonContext.Default.PluginManifest);

            Assert.That(manifest!.HyphaCliVersion, Is.Null);
        }

        [Test]
        public void GitHubRelease_reads_the_body_and_assets()
        {
            var release = JsonSerializer.Deserialize(
                """
                {"body": "checksum table", "assets": [
                    {"name": "hypha-1.2.0-win-x64.zip", "browser_download_url": "https://example.test/a.zip"}
                ]}
                """,
                HookJsonContext.Default.GitHubRelease);

            Assert.Multiple(() =>
            {
                Assert.That(release!.Body, Is.EqualTo("checksum table"));
                Assert.That(release.Assets, Has.Count.EqualTo(1));
                Assert.That(release.Assets![0].Name, Is.EqualTo("hypha-1.2.0-win-x64.zip"));
                Assert.That(release.Assets[0].BrowserDownloadUrl, Is.EqualTo("https://example.test/a.zip"));
            });
        }

        [Test]
        public void HookOutput_SessionStart_builds_the_shape_a_hook_prints()
        {
            var output = HookOutput.SessionStart("Hypha: downloading 2026-06 sources - 50%.");

            Assert.Multiple(() =>
            {
                Assert.That(output.HookSpecificOutput.HookEventName, Is.EqualTo("SessionStart"));
                Assert.That(
                    output.HookSpecificOutput.AdditionalContext,
                    Is.EqualTo("Hypha: downloading 2026-06 sources - 50%."));
            });
        }

        [Test]
        public void HookOutput_serializes_to_the_shape_Claude_Code_expects()
        {
            var json = JsonSerializer.Serialize(
                HookOutput.SessionStart("hello"), HookJsonContext.Default.HookOutput);

            using var document = JsonDocument.Parse(json);
            var specific = document.RootElement.GetProperty("hookSpecificOutput");

            Assert.Multiple(() =>
            {
                Assert.That(specific.GetProperty("hookEventName").GetString(), Is.EqualTo("SessionStart"));
                Assert.That(specific.GetProperty("additionalContext").GetString(), Is.EqualTo("hello"));
            });
        }
    }
}
