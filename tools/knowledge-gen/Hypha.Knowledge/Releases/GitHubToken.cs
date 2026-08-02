// ------------------------------------------------------------------------------------------------
// <copyright file="GitHubToken.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Releases
{
    using System;

    /// <summary>
    /// Resolves an optional GitHub token from the environment.
    /// </summary>
    /// <remarks>
    /// Every repository read here is public, so a token is never required. It matters anyway: the
    /// anonymous API allows only 60 requests an hour, which discovering tags across two repositories
    /// and resolving a couple of them can exhaust. A token raises that to 5000.
    /// </remarks>
    public static class GitHubToken
    {
        /// <summary>The <c>GITHUB_TOKEN</c> or <c>GH_TOKEN</c> value, or <c>null</c> when neither is set.</summary>
        public static string? FromEnvironment(Func<string, string?>? read = null)
        {
            read ??= Environment.GetEnvironmentVariable;

            return Coalesce(read("GITHUB_TOKEN")) ?? Coalesce(read("GH_TOKEN"));
        }

        private static string? Coalesce(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
