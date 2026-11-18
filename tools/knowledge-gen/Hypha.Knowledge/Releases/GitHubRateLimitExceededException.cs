// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Hypha.Knowledge.Releases
{
    using System.Net;
    using System.Net.Http;

    /// <summary>
    /// GitHub refused a request because the API rate limit is used up. Its message already says until
    /// when and how to raise the limit, so a caller shows it as it is, not as an upstream that could not
    /// be reached.
    /// </summary>
    public sealed class GitHubRateLimitExceededException : HttpRequestException
    {
        /// <summary>Initializes a new instance of the <see cref="GitHubRateLimitExceededException"/> class.</summary>
        /// <param name="message">What is used up, until when, and how to raise the limit.</param>
        /// <param name="statusCode">The status GitHub refused the request with, 403 or 429.</param>
        public GitHubRateLimitExceededException(string message, HttpStatusCode statusCode)
            : base(message, null, statusCode)
        {
        }
    }
}
