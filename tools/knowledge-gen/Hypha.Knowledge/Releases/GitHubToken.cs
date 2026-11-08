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
        /// <param name="read">
        /// Reads an environment variable; <c>null</c> uses <see cref="Environment.GetEnvironmentVariable(string)"/>.
        /// A test passes its own so it never depends on the ambient environment.
        /// </param>
        /// <returns>
        /// The first of the two that is set to something other than white space, <c>GITHUB_TOKEN</c> first,
        /// or <c>null</c>.
        /// </returns>
        public static string? FromEnvironment(Func<string, string?>? read = null)
        {
            read ??= Environment.GetEnvironmentVariable;

            return Coalesce(read("GITHUB_TOKEN")) ?? Coalesce(read("GH_TOKEN"));
        }

        /// <summary>Treats a blank value as an unset one.</summary>
        /// <param name="value">The environment variable's value.</param>
        /// <returns><paramref name="value"/>, or <c>null</c> when it is <c>null</c>, empty or white space.</returns>
        private static string? Coalesce(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
