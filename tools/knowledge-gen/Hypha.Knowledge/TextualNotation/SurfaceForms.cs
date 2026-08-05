// ------------------------------------------------------------------------------------------------
// <copyright file="SurfaceForms.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.TextualNotation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Derives how a metaclass is written in the textual notation.
    /// </summary>
    /// <remarks>
    /// Follows the convention the metamodel itself uses: <c>&lt;Words&gt;Definition</c> is written
    /// <c>&lt;words&gt; def</c> and <c>&lt;Words&gt;Usage</c> is written <c>&lt;words&gt;</c>. Deriving
    /// it beats curating it - a curated table would go stale at the next release, and this cannot
    /// invent an element because only the names handed in are considered.
    /// </remarks>
    public sealed partial class SurfaceForms : ISurfaceForms
    {
        /// <summary>The suffix a metaclass ends in, and what replaces it in the notation.</summary>
        private static readonly (string Suffix, string Tail)[] Conventions =
            [("Definition", " def"), ("Usage", "")];

        [GeneratedRegex(@"[A-Z][a-z0-9]*")]
        private static partial Regex CamelWord();

        /// <inheritdoc/>
        public string? Of(string metaclass)
        {
            ArgumentNullException.ThrowIfNull(metaclass);

            foreach (var (suffix, tail) in Conventions)
            {
                if (!metaclass.EndsWith(suffix, StringComparison.Ordinal)
                    || string.Equals(metaclass, suffix, StringComparison.Ordinal))
                {
                    continue;
                }

                // "Definition" and "Usage" on their own are the abstract metaclasses, not a notation.
                var words = CamelWord()
                    .Matches(metaclass[..^suffix.Length])
                    .Select(match => match.Value.ToLowerInvariant())
                    .ToList();

                return words.Count == 0 ? null : string.Join(" ", words) + tail;
            }

            return null;
        }

        /// <inheritdoc/>
        public SurfaceFormIndex Index(IEnumerable<string> metaclasses)
        {
            ArgumentNullException.ThrowIfNull(metaclasses);

            var forms = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (var metaclass in metaclasses)
            {
                var form = this.Of(metaclass);

                if (!string.IsNullOrEmpty(form))
                {
                    forms[metaclass] = form;
                }
            }

            return new SurfaceFormIndex(forms);
        }
    }
}
