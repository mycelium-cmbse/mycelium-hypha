// ------------------------------------------------------------------------------------------------
// <copyright file="HyphaAddress.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Query
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    /// A stable address of one generated page: <c>hypha://{tag}/{kind}/{name}</c>, such as
    /// <c>hypha://2026-05/metamodel/PartUsage</c> or <c>hypha://2026-05/kerml/7.4.2</c>.
    /// </summary>
    /// <remarks>
    /// The address names what the page is about, not where it is stored, so it stays valid when the
    /// file layout changes; <see cref="IKnowledgeQuery.Resolve"/> turns it into a page through the
    /// release's indexes. The name is percent-encoded, so a qualified name's <c>::</c> is written
    /// <c>%3A%3A</c>; parsing accepts it either way.
    /// </remarks>
    /// <param name="Tag">The release.</param>
    /// <param name="Kind">One of <see cref="Kinds"/>.</param>
    /// <param name="Name">
    /// The element name, the library declaration's qualified name, the clause number, or the example's page
    /// name (its file name without <c>.md</c>).
    /// </param>
    public sealed record HyphaAddress(string Tag, string Kind, string Name)
    {
        /// <summary>The address scheme.</summary>
        public const string Scheme = "hypha";

        /// <summary>A metaclass, enumeration or primitive type, by name.</summary>
        public const string Metamodel = "metamodel";

        /// <summary>A standard-library declaration, by qualified name.</summary>
        public const string ModelLibrary = "model-library";

        /// <summary>A clause of the KerML specification, by number.</summary>
        public const string Kerml = "kerml";

        /// <summary>A clause of the SysML specification, by number.</summary>
        public const string Sysml = "sysml2";

        /// <summary>The kind of a worked example the release ships, named by its page.</summary>
        public const string Examples = "examples";

        /// <summary>Gets every kind an address may name.</summary>
        public static IReadOnlyList<string> Kinds { get; } = [Metamodel, ModelLibrary, Kerml, Sysml, Examples];

        /// <summary>Reads an address.</summary>
        /// <param name="text">The address, such as <c>hypha://2026-05/metamodel/PartUsage</c>.</param>
        /// <returns>The address.</returns>
        /// <exception cref="FormatException">
        /// The text is not a <c>hypha://</c> address with a tag, one of <see cref="Kinds"/> and a name.
        /// </exception>
        public static HyphaAddress Parse(string text) =>
            TryParse(text, out var address)
                ? address
                : throw new FormatException(
                    $"'{text}' is not a hypha address; expected hypha://<tag>/<kind>/<name>, "
                    + $"with a kind of {string.Join(", ", Kinds)}.");

        /// <summary>Reads an address, without throwing when the text is not one.</summary>
        /// <param name="text">The address.</param>
        /// <param name="address">The address, when the text is one; otherwise <c>null</c>.</param>
        /// <returns>Whether the text is an address.</returns>
        public static bool TryParse(string? text, [NotNullWhen(true)] out HyphaAddress? address)
        {
            address = null;
            var prefix = Scheme + "://";

            if (text is null || !text.StartsWith(prefix, StringComparison.Ordinal))
            {
                return false;
            }

            // The name is the rest, so it may hold '/' once decoded; the tag and kind are one segment each.
            var parts = text[prefix.Length..].Split('/', 3);

            if (parts.Length != 3 || parts.Any(string.IsNullOrWhiteSpace) || !Kinds.Contains(parts[1], StringComparer.Ordinal))
            {
                return false;
            }

            // An incomplete escape such as '%A' is kept as written, which then matches no name.
            address = new HyphaAddress(parts[0], parts[1], System.Uri.UnescapeDataString(parts[2]));

            return true;
        }

        /// <summary>Writes the address.</summary>
        /// <returns>The address, with the name percent-encoded.</returns>
        public override string ToString() => $"{Scheme}://{this.Tag}/{this.Kind}/{System.Uri.EscapeDataString(this.Name)}";

        /// <summary>
        /// Gets the address as text, as <see cref="ToString"/> writes it, so a JSON answer carries the one
        /// value a client can open as an MCP resource or pass to <c>resolve</c>.
        /// </summary>
        public string Uri => this.ToString();
    }
}
