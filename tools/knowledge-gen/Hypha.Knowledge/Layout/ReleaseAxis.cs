// ------------------------------------------------------------------------------------------------
// <copyright file="ReleaseAxis.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Layout
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Hypha.Knowledge.Lookup;

    /// <summary>
    /// One version axis, the language releases or the Systems Modeling API versions, and the user-facing
    /// messages for choosing a version on it.
    /// </summary>
    /// <remarks>
    /// Every reader that answers from "the requested version, or the default one" resolves it here, so
    /// the messages, which name the CLI verbs to run, are written once.
    /// </remarks>
    internal sealed class ReleaseAxis
    {
        /// <summary>The noun for one version, as a sentence names it mid-sentence.</summary>
        private readonly string noun;

        /// <summary>The CLI command the verbs of this axis hang off, such as <c>hypha</c>.</summary>
        private readonly string command;

        /// <summary>The placeholder for a version in a command line, such as <c>&lt;release&gt;</c>.</summary>
        private readonly string placeholder;

        /// <summary>Initializes a new instance of the <see cref="ReleaseAxis"/> class.</summary>
        /// <param name="noun">The noun for one version, as a sentence names it mid-sentence.</param>
        /// <param name="command">The CLI command the verbs of this axis hang off.</param>
        /// <param name="placeholder">The placeholder for a version in a command line.</param>
        private ReleaseAxis(string noun, string command, string placeholder)
        {
            this.noun = noun;
            this.command = command;
            this.placeholder = placeholder;
        }

        /// <summary>Gets the language releases: <c>hypha fetch</c>, <c>hypha use</c>, <c>hypha generate</c>.</summary>
        public static ReleaseAxis Language { get; } = new("release", "hypha", "<release>");

        /// <summary>Gets the Systems Modeling API versions: <c>hypha api fetch</c> and so on.</summary>
        public static ReleaseAxis Api { get; } = new("API version", "hypha api", "<version>");

        /// <summary>The requested version, or the default one, checked to be installed.</summary>
        /// <param name="installed">The installed versions.</param>
        /// <param name="defaultVersion">The default version, or <c>null</c> when none is selected.</param>
        /// <param name="requested">The requested version, or <c>null</c> for the default.</param>
        /// <returns>The version to answer from.</returns>
        /// <exception cref="KnowledgeLookupException">
        /// Nothing is installed, no default is selected, or the requested version is not installed.
        /// </exception>
        public string Resolve(IReadOnlyList<string> installed, string? defaultVersion, string? requested)
        {
            ArgumentNullException.ThrowIfNull(installed);

            if (requested is null)
            {
                return defaultVersion
                       ?? throw new KnowledgeLookupException(
                           installed.Count == 0
                               ? $"No {this.noun} is installed. Run '{this.command} fetch --tag {this.placeholder}' "
                                 + $"and '{this.command} generate'."
                               : $"No default {this.noun} is selected. Run '{this.command} use --tag {this.placeholder}'.");
            }

            if (!installed.Contains(requested, StringComparer.Ordinal))
            {
                throw new KnowledgeLookupException(
                    $"{char.ToUpperInvariant(this.noun[0])}{this.noun[1..]} {requested} is not installed. Installed: "
                    + (installed.Count == 0 ? "none" : string.Join(", ", installed)) + ".");
            }

            return requested;
        }

        /// <summary>The message for an artifact a version has not generated.</summary>
        /// <param name="artifact">The artifact, as a user would name it.</param>
        /// <param name="version">The version.</param>
        /// <returns>The message, naming the command that generates it.</returns>
        public string NotGenerated(string artifact, string version) =>
            $"The {artifact} of {this.noun} {version} has not been generated. Run '{this.GenerateCommand(version)}'.";

        /// <summary>The command line that generates a version's knowledge.</summary>
        /// <param name="version">The version.</param>
        /// <returns>The command line, such as <c>hypha generate --tag 2026-05</c>.</returns>
        public string GenerateCommand(string version) => $"{this.command} generate --tag {version}";
    }
}
