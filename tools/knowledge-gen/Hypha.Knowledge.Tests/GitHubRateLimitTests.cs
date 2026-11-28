// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Hypha.Knowledge.Tests
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Releases;

    [TestFixture]
    public class GitHubRateLimitTests
    {
        // 2026-11-15 14:05:00 UTC.
        private const string Reset = "1794751500";

        [Test]
        public void Before_any_response_there_is_no_notice()
        {
            Assert.That(new GitHubRateLimit(authenticated: false).Notice(), Is.Null);
        }

        [Test]
        public void With_more_than_the_low_water_mark_left_there_is_no_notice()
        {
            var limit = new GitHubRateLimit(authenticated: false);
            limit.Record("60", (GitHubRateLimit.LowWater + 1).ToString(System.Globalization.CultureInfo.InvariantCulture), Reset);

            Assert.That(limit.Notice(), Is.Null);
        }

        [Test]
        public void Without_a_token_and_few_requests_left_the_notice_says_how_many_until_when_and_how_to_raise_it()
        {
            var limit = new GitHubRateLimit(authenticated: false);
            limit.Record("60", "12", Reset);

            Assert.That(
                limit.Notice(),
                Does.Contain("Only 12 of GitHub's 60").And.Contain("until 14:05 UTC").And.Contain("GITHUB_TOKEN"));
        }

        [Test]
        public void The_notice_names_no_cost_per_check_or_install_since_it_grows_with_the_upstream_tags()
        {
            var limit = new GitHubRateLimit(authenticated: false);
            limit.Record("60", "12", Reset);

            Assert.That(limit.Notice(), Does.Not.Match(@"\b(three|five|six)\b"));
        }

        [Test]
        public void At_the_low_water_mark_there_is_a_notice()
        {
            var limit = new GitHubRateLimit(authenticated: false);
            limit.Record("60", GitHubRateLimit.LowWater.ToString(System.Globalization.CultureInfo.InvariantCulture), Reset);

            Assert.That(limit.Notice(), Does.Contain($"Only {GitHubRateLimit.LowWater} of"));
        }

        [Test]
        public void With_a_token_there_is_no_notice_however_few_are_left()
        {
            var limit = new GitHubRateLimit(authenticated: true);
            limit.Record("5000", "3", Reset);

            Assert.That(limit.Notice(), Is.Null);
        }

        [Test]
        public void A_response_without_the_headers_changes_nothing()
        {
            var limit = new GitHubRateLimit(authenticated: false);
            limit.Record("60", "12", Reset);
            limit.Record(null, null, null);

            Assert.That(limit.Remaining, Is.EqualTo(12));
        }

        [Test]
        public async Task The_handler_records_each_response_and_passes_it_on()
        {
            var limit = new GitHubRateLimit(authenticated: false);
            using var client = new HttpClient(new GitHubRateLimitHandler(limit) { InnerHandler = new Respond(HttpStatusCode.OK, "41") });

            using var response = await client.GetAsync(new Uri("https://api.github.com/repos/owner/repo/tags"));

            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(limit.Remaining, Is.EqualTo(41));
            });
        }

        [TestCase(HttpStatusCode.Forbidden)]
        [TestCase(HttpStatusCode.TooManyRequests)]
        public async Task A_request_refused_for_the_limit_says_so_with_when_it_resets_and_how_to_raise_it(HttpStatusCode status)
        {
            var limit = new GitHubRateLimit(authenticated: false);
            using var client = new HttpClient(new GitHubRateLimitHandler(limit) { InnerHandler = new Respond(status, "0") });

            var refused = await Assert.ThrowsAsync<GitHubRateLimitExceededException>(() => client.GetAsync(new Uri("https://api.github.com/repos/owner/repo/tags")));

            Assert.Multiple(() =>
            {
                Assert.That(refused!.StatusCode, Is.EqualTo(status));
                Assert.That(refused.Message, Does.Contain("is used up").And.Contain("until 14:05 UTC").And.Contain("GITHUB_TOKEN"));
            });
        }

        [Test]
        public async Task A_forbidden_response_with_requests_left_is_not_taken_for_the_limit()
        {
            var limit = new GitHubRateLimit(authenticated: false);
            using var client = new HttpClient(new GitHubRateLimitHandler(limit) { InnerHandler = new Respond(HttpStatusCode.Forbidden, "30") });

            using var response = await client.GetAsync(new Uri("https://api.github.com/repos/owner/repo/tags"));

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        }

        [Test]
        public void The_handler_needs_somewhere_to_record()
        {
            Assert.That(() => new GitHubRateLimitHandler(null!), Throws.ArgumentNullException);
        }

        private sealed class Respond : HttpMessageHandler
        {
            private readonly HttpStatusCode status;

            private readonly string remaining;

            public Respond(HttpStatusCode status, string remaining)
            {
                this.status = status;
                this.remaining = remaining;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var response = new HttpResponseMessage(this.status) { Content = new StringContent("[]") };
                response.Headers.Add(GitHubRateLimit.LimitHeader, "60");
                response.Headers.Add(GitHubRateLimit.RemainingHeader, this.remaining);
                response.Headers.Add(GitHubRateLimit.ResetHeader, Reset);

                return Task.FromResult(response);
            }
        }
    }
}
