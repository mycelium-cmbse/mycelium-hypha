// ------------------------------------------------------------------------------------------------
// <copyright file="ReleaseDiscoveryTests.cs" company="Starion Group S.A.">
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
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Releases;

    /// <summary>
    /// Tests for the networked part of discovery, against a stubbed transport - no live calls.
    /// </summary>
    [TestFixture]
    public class ReleaseDiscoveryTests
    {
        [Test]
        public async Task Follows_pagination_until_a_page_is_empty()
        {
            var handler = new StubHandler(page => page == 1
                ? """[{"name":"2026-05"},{"name":"2026-04"}]"""
                : "[]");

            using var client = new HttpClient(handler);
            var tags = await new ReleaseDiscovery(client, "token").FetchTagsAsync("owner/repo");

            Assert.Multiple(() =>
            {
                Assert.That(tags, Is.EqualTo(new[] { "2026-05", "2026-04" }));
                Assert.That(handler.Requests, Has.Count.EqualTo(2), "should stop at the first empty page");
            });
        }

        [Test]
        public async Task Sends_the_token_as_a_bearer_credential()
        {
            var handler = new StubHandler(_ => "[]");

            using var client = new HttpClient(handler);
            await new ReleaseDiscovery(client, "secret").FetchTagsAsync("owner/repo");

            Assert.That(handler.Authorization, Is.EqualTo("Bearer secret"));
        }

        [Test]
        public async Task Is_anonymous_when_no_token_is_available()
        {
            var handler = new StubHandler(_ => "[]");

            // Environment injected, so the result does not depend on whatever the machine exports.
            using var client = new HttpClient(handler);
            await new ReleaseDiscovery(client, null, _ => null).FetchTagsAsync("owner/repo");

            Assert.That(handler.Authorization, Is.Null);
        }

        [Test]
        public async Task Falls_back_to_the_environment_when_the_token_is_blank()
        {
            var handler = new StubHandler(_ => "[]");

            using var client = new HttpClient(handler);
            await new ReleaseDiscovery(client, "   ", name => name == "GH_TOKEN" ? "from-env" : null)
                .FetchTagsAsync("owner/repo");

            Assert.That(handler.Authorization, Is.EqualTo("Bearer from-env"));
        }

        [Test]
        public async Task Available_intersects_both_upstreams()
        {
            var handler = new StubHandler((page, url) =>
            {
                if (page != 1)
                {
                    return "[]";
                }

                return url.Contains("Pilot", StringComparison.Ordinal)
                    ? """[{"name":"2026-05"},{"name":"2026-05-pre"}]"""
                    : """[{"name":"2026-05"},{"name":"2023-07.1"}]""";
            });

            using var client = new HttpClient(handler);
            var versions = await new ReleaseDiscovery(client, "token").AvailableAsync();

            // 2023-07.1 is Release-only and 2026-05-pre is not a release: only 2026-05 survives.
            Assert.That(versions, Is.EqualTo(new[] { "2026-05" }));
        }

        [Test]
        public async Task Reads_from_the_default_host_when_none_is_given()
        {
            var handler = new StubHandler(_ => "[]");

            using var client = new HttpClient(handler);
            await new ReleaseDiscovery(client, "token").FetchTagsAsync("owner/repo");

            Assert.That(handler.Requests[0], Does.StartWith(Upstream.DefaultApiBaseAddress.ToString()));
        }

        [Test]
        public async Task Reads_from_an_overridden_host()
        {
            // The host is configurable so an enterprise instance - or a stub - can be pointed at.
            var handler = new StubHandler(_ => "[]");

            using var client = new HttpClient(handler);
            await new ReleaseDiscovery(client, "token", null, new Uri("https://github.example.com/api/"))
                .FetchTagsAsync("owner/repo");

            Assert.That(handler.Requests[0], Is.EqualTo(
                "https://github.example.com/api/repos/owner/repo/tags?per_page=100&page=1"));
        }

        [Test]
        public void Rejects_a_missing_repository()
        {
            using var client = new HttpClient(new StubHandler(_ => "[]"));
            var discovery = new ReleaseDiscovery(client, "token");

            Assert.ThrowsAsync<ArgumentException>(() => discovery.FetchTagsAsync("  "));
        }

        private sealed class StubHandler : HttpMessageHandler
        {
            private readonly Func<int, string, string> respond;

            public StubHandler(Func<int, string> respond)
                : this((page, _) => respond(page))
            {
            }

            public StubHandler(Func<int, string, string> respond)
            {
                this.respond = respond;
            }

            public List<string> Requests { get; } = [];

            public string? Authorization { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var url = request.RequestUri!.ToString();
                this.Requests.Add(url);
                this.Authorization = request.Headers.Authorization?.ToString();

                var page = int.Parse(url.Split("page=").Last());

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(this.respond(page, url)),
                });
            }
        }
    }
}
