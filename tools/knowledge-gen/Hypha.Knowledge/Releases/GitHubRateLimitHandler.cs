// ------------------------------------------------------------------------------------------------
// <copyright file="GitHubRateLimitHandler.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Releases
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Records GitHub's rate-limit headers on every response into <see cref="GitHubRateLimit"/>, and turns a
    /// request refused because the limit is used up into a <see cref="GitHubRateLimitExceededException"/> that says so.
    /// </summary>
    /// <remarks>
    /// It sits outside the resilience handler, so it sees the response that is finally returned, after any
    /// retry. GitHub refuses with 403 or 429 and <c>x-ratelimit-remaining: 0</c>; any other refusal is left to
    /// the caller as it was.
    /// </remarks>
    public sealed class GitHubRateLimitHandler : DelegatingHandler
    {
        /// <summary>Where the headers are recorded.</summary>
        private readonly GitHubRateLimit rateLimit;

        /// <summary>Initializes a new instance of the <see cref="GitHubRateLimitHandler"/> class.</summary>
        /// <param name="rateLimit">Where the headers are recorded.</param>
        /// <exception cref="ArgumentNullException"><paramref name="rateLimit"/> is <c>null</c>.</exception>
        public GitHubRateLimitHandler(GitHubRateLimit rateLimit)
        {
            ArgumentNullException.ThrowIfNull(rateLimit);

            this.rateLimit = rateLimit;
        }

        /// <summary>Sends the request, then records the response's rate-limit headers.</summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">Cancels the request.</param>
        /// <returns>The response.</returns>
        /// <exception cref="GitHubRateLimitExceededException">GitHub refused the request because the rate limit is used up.</exception>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            var remaining = Header(response, GitHubRateLimit.RemainingHeader);
            this.rateLimit.Record(Header(response, GitHubRateLimit.LimitHeader), remaining, Header(response, GitHubRateLimit.ResetHeader));

            if (response.StatusCode is (HttpStatusCode.Forbidden or HttpStatusCode.TooManyRequests) && remaining == "0")
            {
                var status = response.StatusCode;
                response.Dispose();

                throw new GitHubRateLimitExceededException(this.rateLimit.Exhausted(), status);
            }

            return response;
        }

        /// <summary>The first value of a response header, or <c>null</c>.</summary>
        /// <param name="response">The response.</param>
        /// <param name="name">The header.</param>
        /// <returns>The value, or <c>null</c> when absent.</returns>
        private static string? Header(HttpResponseMessage response, string name) =>
            response.Headers.TryGetValues(name, out var values) ? values.FirstOrDefault() : null;
    }
}
