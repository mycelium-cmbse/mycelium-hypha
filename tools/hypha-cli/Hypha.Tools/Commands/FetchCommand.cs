// ------------------------------------------------------------------------------------------------
// <copyright file="FetchCommand.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Commands
{
    using System;
    using System.CommandLine;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Releases;

    using Spectre.Console;

    /// <summary>
    /// Downloads one release's inputs into <c>sources/</c> and records it in the version manifest.
    /// </summary>
    public sealed class FetchCommand : Command
    {
        /// <summary>The release to fetch.</summary>
        public static readonly Option<string> Tag =
            new("--tag", "-t")
            {
                Description = "The release tag to fetch, e.g. 2026-05.",
                Required = true,
            };

        /// <summary>Whether the OMG specification PDFs come too.</summary>
        public static readonly Option<bool> IncludeSpecifications =
            new("--include-specs")
            {
                Description =
                    "Also download the OMG specification PDFs. They are copyright OMG, land in a "
                    + "git-ignored folder and must never be committed.",
                DefaultValueFactory = _ => false,
            };

        /// <summary>Whether files already on disk are downloaded again.</summary>
        public static readonly Option<bool> Force =
            new("--force")
            {
                Description = "Download every file again, rather than resuming and keeping what is there.",
                DefaultValueFactory = _ => false,
            };

        /// <summary>Whether the release becomes the default one.</summary>
        public static readonly Option<bool> NoDefault =
            new("--no-default")
            {
                Description = "Leave the existing default release alone rather than switching to this one.",
                DefaultValueFactory = _ => false,
            };

        /// <summary>
        /// Initializes a new instance of the <see cref="FetchCommand"/> class.
        /// </summary>
        public FetchCommand()
            : base("fetch", "Download a release's inputs into sources/ and record it")
        {
            this.Options.Add(Tag);
            this.Options.Add(IncludeSpecifications);
            this.Options.Add(Force);
            this.Options.Add(NoDefault);
        }

        /// <summary>Runs the <see cref="FetchCommand"/>.</summary>
        public sealed class Handler
        {
            private readonly IReleaseInstaller installer;

            /// <summary>
            /// Initializes a new instance of the <see cref="Handler"/> class.
            /// </summary>
            public Handler(IReleaseInstaller installer)
            {
                ArgumentNullException.ThrowIfNull(installer);

                this.installer = installer;
            }

            /// <summary>Fetches the release.</summary>
            /// <returns>0 when the release was installed, 2 when the tag was rejected.</returns>
            public async Task<int> InvokeAsync(
                ParseResult parseResult, CancellationToken cancellationToken = default)
            {
                ArgumentNullException.ThrowIfNull(parseResult);

                var request = new ReleaseInstallRequest
                {
                    Tag = parseResult.GetValue(Tag)!,
                    IncludeSpecifications = parseResult.GetValue(IncludeSpecifications),
                    SkipExisting = !parseResult.GetValue(Force),
                    MakeDefault = !parseResult.GetValue(NoDefault),
                };

                ReleaseInstallation installation;

                try
                {
                    installation = await this.installer.InstallAsync(request, cancellationToken);
                }
                catch (ArgumentException exception)
                {
                    // A tag that is not release-shaped is a usage error, not a failure to report as a
                    // crash: say what was wrong and let 'hypha discover' show what is on offer.
                    AnsiConsole.MarkupLine($"[red]{Markup.Escape(exception.Message)}[/]");
                    AnsiConsole.MarkupLine("[grey]Run 'hypha discover' to list the offerable releases.[/]");

                    return 2;
                }

                AnsiConsole.MarkupLine(
                    $"Fetched [bold]{Markup.Escape(installation.Tag)}[/]: "
                    + $"{installation.Metamodel.Count} metamodel, {installation.Textual.Count} textual"
                    + (installation.Specifications.Count > 0
                        ? $", {installation.Specifications.Count} specification"
                        : string.Empty)
                    + " files.");

                AnsiConsole.MarkupLine(
                    $"[grey]Recorded in {Markup.Escape(installation.Manifest.FullName)}.[/]");

                return 0;
            }
        }
    }
}
