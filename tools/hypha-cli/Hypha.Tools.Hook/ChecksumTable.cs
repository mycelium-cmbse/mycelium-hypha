// ------------------------------------------------------------------------------------------------
// <copyright file="ChecksumTable.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Hook
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Reads the per-asset SHA256 table out of a GitHub release's notes.
    /// </summary>
    /// <remarks>
    /// The table is exactly what <c>.github/workflows/release.yml</c>'s "Record the checksums" step
    /// writes into <c>RELEASE_NOTES.md</c>, which becomes the release body: rows shaped
    /// <c>| `hypha-1.2.0-win-x64.zip` | &lt;64-hex-sha256&gt; |</c>. A line-oriented regex is enough -
    /// no Markdown table parser is needed for output this ever moved by one script.
    /// </remarks>
    public static partial class ChecksumTable
    {
        [GeneratedRegex(@"`(?<name>[^`]+\.zip)`\s*\|\s*(?<sha>[0-9a-fA-F]{64})")]
        private static partial Regex RowPattern();

        /// <summary>The SHA256 recorded for <paramref name="assetName"/>, or <c>null</c> if absent.</summary>
        public static string? Find(string? releaseBody, string assetName)
        {
            if (string.IsNullOrEmpty(releaseBody))
            {
                return null;
            }

            return RowPattern()
                .Matches(releaseBody)
                .Where(match => string.Equals(match.Groups["name"].Value, assetName, StringComparison.Ordinal))
                .Select(match => match.Groups["sha"].Value.ToLowerInvariant())
                .FirstOrDefault();
        }
    }
}
