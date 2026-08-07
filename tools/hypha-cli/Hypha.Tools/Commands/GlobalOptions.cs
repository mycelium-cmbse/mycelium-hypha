// ------------------------------------------------------------------------------------------------
// <copyright file="GlobalOptions.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Commands
{
    using System.CommandLine;
    using System.IO;

    using Serilog.Events;

    /// <summary>
    /// The options every verb accepts.
    /// </summary>
    /// <remarks>
    /// Declared once and marked recursive on the root command, so <c>hypha generate --token …</c> and
    /// <c>hypha fetch --token …</c> cannot drift apart. They are shared instances rather than names,
    /// so reading one back is checked by the compiler.
    /// </remarks>
    public static class GlobalOptions
    {
        /// <summary>Where <c>sources/</c> and <c>knowledge/</c> live.</summary>
        public static readonly Option<DirectoryInfo?> RepositoryRoot =
            new("--repository-root", "-r")
            {
                Description =
                    "The folder holding sources/ and knowledge/. Discovered by walking up from the "
                    + "working directory when omitted.",
                Recursive = true,
            };

        /// <summary>A GitHub token, for the calls that read the upstreams.</summary>
        public static readonly Option<string?> Token =
            new("--token")
            {
                Description =
                    "A GitHub token. Falls back to GITHUB_TOKEN / GH_TOKEN. Optional - every read is "
                    + "public - but the anonymous API allows only 60 requests an hour.",
                Recursive = true,
            };

        /// <summary>How much of the log reaches the console.</summary>
        public static readonly Option<LogEventLevel> LogLevel =
            new("--log-level")
            {
                Description = "Verbose, Debug, Information, Warning, Error or Fatal.",
                DefaultValueFactory = _ => LogEventLevel.Information,
                Recursive = true,
            };

        /// <summary>Suppresses the banner.</summary>
        public static readonly Option<bool> NoLogo =
            new("--no-logo")
            {
                Description = "Suppress the banner.",
                DefaultValueFactory = _ => false,
                Recursive = true,
            };
    }
}
