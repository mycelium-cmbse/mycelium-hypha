// ------------------------------------------------------------------------------------------------
// <copyright file="UpstreamToolsTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Mcp.Tests
{
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Releases;
    using Hypha.Mcp.Tools;

    using ModelContextProtocol;

    using Moq;

    [TestFixture]
    public class UpstreamToolsTests
    {
        private Mock<IReleaseCheck> check = null!;

        [SetUp]
        public void SetUp()
        {
            this.check = new Mock<IReleaseCheck>(MockBehavior.Strict);
            this.check.Setup(c => c.CheckReleasesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UpstreamComparison(["2026-05"], "2026-05", ["2026-09", "2026-05"], "2026-09"));
        }

        [Test]
        public async Task It_answers_both_axes_with_the_newer_release_named()
        {
            this.check.Setup(c => c.CheckReleasesAndApiAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UpstreamCheck(
                    new UpstreamComparison(["2026-05"], "2026-05", ["2026-09", "2026-05"], "2026-09"),
                    new UpstreamComparison(["2026-04"], "2026-04", ["2026-04"], null)));

            using var answer = JsonDocument.Parse(await new UpstreamTools(this.check.Object).CheckReleases());
            var root = answer.RootElement;

            Assert.Multiple(() =>
            {
                Assert.That(root.GetProperty("releases").GetProperty("newer").GetString(), Is.EqualTo("2026-09"));
                Assert.That(root.GetProperty("releases").GetProperty("installed")[0].GetString(), Is.EqualTo("2026-05"));
                Assert.That(root.GetProperty("releases").GetProperty("offered").GetArrayLength(), Is.EqualTo(2));
                Assert.That(root.GetProperty("releases").GetProperty("default").GetString(), Is.EqualTo("2026-05"));
                Assert.That(root.GetProperty("api").GetProperty("newer").ValueKind, Is.EqualTo(JsonValueKind.Null));
            });
        }

        [Test]
        public async Task Without_the_API_it_asks_GitHub_only_about_the_releases()
        {
            using var answer = JsonDocument.Parse(await new UpstreamTools(this.check.Object).CheckReleases(api: false));

            Assert.That(answer.RootElement.GetProperty("api").ValueKind, Is.EqualTo(JsonValueKind.Null));
            this.check.Verify(c => c.CheckReleasesAndApiAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public void GitHub_out_of_reach_is_a_tool_error_naming_the_token()
        {
            this.check.Setup(c => c.CheckReleasesAndApiAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("Response status code does not indicate success: 403 (rate limit exceeded)."));

            Assert.That(
                () => new UpstreamTools(this.check.Object).CheckReleases(),
                Throws.TypeOf<McpException>().With.Message.Contains("403").And.Message.Contains("GITHUB_TOKEN"));
        }
    }
}
