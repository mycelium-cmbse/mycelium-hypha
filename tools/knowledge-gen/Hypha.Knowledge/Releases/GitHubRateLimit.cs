// ------------------------------------------------------------------------------------------------
// <copyright file="GitHubRateLimit.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Releases
{
    using System;
    using System.Globalization;
    using System.Threading;

    /// <summary>
    /// What GitHub last said about the API rate limit, as its <c>x-ratelimit-*</c> response headers give it,
    /// so that a user without a token hears that the limit is running out before it has.
    /// </summary>
    /// <remarks>
    /// Without a token GitHub allows 60 API requests an hour from one address; with one, 5,000. An install
    /// of the newest release makes about five, a check three, an API version install about six, so a
    /// session that checks and installs a few times reaches the anonymous limit. Downloads of the files
    /// themselves come from <c>raw.githubusercontent.com</c> and do not count.
    /// </remarks>
    public sealed class GitHubRateLimit
    {
        /// <summary>At or below this many requests left, <see cref="Notice"/> warns a user without a token.</summary>
        public const int LowWater = 20;

        /// <summary>The header naming how many requests the hour allows.</summary>
        public const string LimitHeader = "x-ratelimit-limit";

        /// <summary>The header naming how many of them are left.</summary>
        public const string RemainingHeader = "x-ratelimit-remaining";

        /// <summary>The header naming when the count starts again, in seconds since the Unix epoch.</summary>
        public const string ResetHeader = "x-ratelimit-reset";

        /// <summary>How to raise the limit.</summary>
        private const string TokenAdvice =
            "Set GITHUB_TOKEN (or GH_TOKEN) to a GitHub token, which needs no scopes, where hypha is started to raise the limit to 5,000.";

        /// <summary>Guards the three values, which a response sets together.</summary>
        private readonly Lock gate = new();

        /// <summary>The last limit GitHub stated, or <c>null</c> before any API response.</summary>
        private int? limit;

        /// <summary>The last number of requests left GitHub stated.</summary>
        private int? remaining;

        /// <summary>When the count starts again, as GitHub last stated it.</summary>
        private DateTimeOffset? reset;

        /// <summary>Initializes a new instance of the <see cref="GitHubRateLimit"/> class.</summary>
        /// <param name="authenticated">Whether requests carry a token, so that the anonymous limit does not apply.</param>
        public GitHubRateLimit(bool authenticated)
        {
            this.Authenticated = authenticated;
        }

        /// <summary>Gets a value indicating whether requests carry a token.</summary>
        public bool Authenticated { get; }

        /// <summary>Gets how many requests GitHub last said were left, or <c>null</c> before any API response.</summary>
        public int? Remaining
        {
            get
            {
                lock (this.gate)
                {
                    return this.remaining;
                }
            }
        }

        /// <summary>Records the rate-limit headers of one API response; a response without them changes nothing.</summary>
        /// <param name="limitValue">The <see cref="LimitHeader"/> value, or <c>null</c>.</param>
        /// <param name="remainingValue">The <see cref="RemainingHeader"/> value, or <c>null</c>.</param>
        /// <param name="resetValue">The <see cref="ResetHeader"/> value, or <c>null</c>.</param>
        public void Record(string? limitValue, string? remainingValue, string? resetValue)
        {
            if (!int.TryParse(remainingValue, NumberStyles.None, CultureInfo.InvariantCulture, out var left))
            {
                return;
            }

            lock (this.gate)
            {
                this.remaining = left;
                this.limit = int.TryParse(limitValue, NumberStyles.None, CultureInfo.InvariantCulture, out var hour) ? hour : this.limit;
                this.reset = long.TryParse(resetValue, NumberStyles.None, CultureInfo.InvariantCulture, out var seconds)
                    ? DateTimeOffset.FromUnixTimeSeconds(seconds)
                    : this.reset;
            }
        }

        /// <summary>
        /// A warning for a user without a token once few requests are left, or <c>null</c>: with a token, before
        /// any API response, or with more than <see cref="LowWater"/> left.
        /// </summary>
        /// <returns>The warning, naming what is left, when the count starts again and how to raise the limit.</returns>
        public string? Notice()
        {
            lock (this.gate)
            {
                if (this.Authenticated || this.remaining is not { } left || left > LowWater)
                {
                    return null;
                }

                return $"Only {left} of GitHub's {this.limit ?? 60} API requests an hour without a token are left"
                       + $"{Until(this.reset)}. Each check costs about three, each install about five. {TokenAdvice}";
            }
        }

        /// <summary>The message for a request GitHub refused because the limit is used up.</summary>
        /// <returns>The message, naming when the count starts again and how to raise the limit.</returns>
        public string Exhausted()
        {
            lock (this.gate)
            {
                return this.Authenticated
                    ? $"GitHub's API rate limit for this token is used up{Until(this.reset)}."
                    : $"GitHub's limit of {this.limit ?? 60} API requests an hour without a token is used up{Until(this.reset)}. {TokenAdvice}";
            }
        }

        /// <summary>When the count starts again, as a phrase.</summary>
        /// <param name="at">The moment, or <c>null</c> when unknown.</param>
        /// <returns>Such as <c> until 14:05 UTC</c>, or nothing.</returns>
        private static string Until(DateTimeOffset? at) =>
            at is { } moment ? $" until {moment.UtcDateTime.ToString("HH:mm", CultureInfo.InvariantCulture)} UTC" : string.Empty;
    }
}
