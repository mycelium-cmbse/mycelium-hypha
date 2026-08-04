// ------------------------------------------------------------------------------------------------
// <copyright file="CommitResolverTests.cs" company="Starion Group S.A.">
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
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Releases;

    /// <summary>
    /// Tests for resolving a tag to a commit, against a stubbed transport - no live calls.
    /// </summary>
    [TestFixture]
    public class CommitResolverTests
    {
        [Test]
        public async Task Resolves_a_tag_to_its_commit()
        {
            using var client = new HttpClient(new StubHandler());

            var sha = await new CommitResolver(client).ResolveAsync("owner/repo", "2026-05");

            Assert.That(sha, Is.EqualTo("de1070ae8e79c21532b8004fc663d47b35d0e9fa"));
        }

        [Test]
        public async Task Resolves_both_upstreams_for_one_tag()
        {
            var handler = new StubHandler();
            using var client = new HttpClient(handler);

            var version = await new CommitResolver(client).ResolveVersionAsync("2026-05");

            Assert.Multiple(() =>
            {
                Assert.That(version.Tag, Is.EqualTo("2026-05"));
                Assert.That(version.Release.Repo, Is.EqualTo(Upstream.ReleaseRepository));
                Assert.That(version.Pilot.Repo, Is.EqualTo(Upstream.PilotRepository));
                Assert.That(handler.Requests, Has.Count.EqualTo(2), "one call per upstream");
            });
        }

        [Test]
        public async Task Reads_from_an_overridden_host()
        {
            var handler = new StubHandler();
            using var client = new HttpClient(handler);

            await new CommitResolver(client, new Uri("https://github.example.com/api/"))
                .ResolveAsync("owner/repo", "2026-05");

            Assert.That(handler.Requests[0], Is.EqualTo(
                "https://github.example.com/api/repos/owner/repo/commits/2026-05"));
        }

        [Test]
        public void Rejects_a_missing_repository_or_tag()
        {
            using var client = new HttpClient(new StubHandler());
            var resolver = new CommitResolver(client);

            Assert.Multiple(() =>
            {
                Assert.ThrowsAsync<ArgumentException>(() => resolver.ResolveAsync(" ", "2026-05"));
                Assert.ThrowsAsync<ArgumentException>(() => resolver.ResolveAsync("owner/repo", " "));
            });
        }

        private sealed class StubHandler : HttpMessageHandler
        {
            public List<string> Requests { get; } = [];

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                this.Requests.Add(request.RequestUri!.ToString());

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("""{"sha":"de1070ae8e79c21532b8004fc663d47b35d0e9fa"}"""),
                });
            }
        }
    }
}
