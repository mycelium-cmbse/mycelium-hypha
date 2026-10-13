// ------------------------------------------------------------------------------------------------
// <copyright file="ApiFetchCommand.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Commands.Api
{
    using System;
    using System.CommandLine;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Api;

    using Spectre.Console;

    /// <summary>
    /// <c>hypha api fetch</c> - downloads one API version's inputs into <c>sources/api/</c> and
    /// records it.
    /// </summary>
    public sealed class ApiFetchCommand : Command
    {
        /// <summary>The API version to fetch.</summary>
        public static readonly Option<string> Tag =
            new("--tag", "-t")
            {
                Description = "The API version to fetch, e.g. 2026-04.",
                Required = true,
            };

        /// <summary>Leaves the copyrighted specification PDF out.</summary>
        public static readonly Option<bool> NoSpecs =
            new("--no-specs")
            {
                Description =
                    "Skip the OMG specification PDF. Downloaded by default - it is copyright OMG, lands "
                    + "in a git-ignored folder and must never be committed.",
                DefaultValueFactory = _ => false,
            };

        /// <summary>Downloads everything again.</summary>
        public static readonly Option<bool> Force =
            new("--force")
            {
                Description = "Download every file again, rather than resuming and keeping what is there.",
                DefaultValueFactory = _ => false,
            };

        /// <summary>Leaves the default API version alone.</summary>
        public static readonly Option<bool> NoDefault =
            new("--no-default")
            {
                Description = "Leave the existing default API version alone rather than switching to this one.",
                DefaultValueFactory = _ => false,
            };

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiFetchCommand"/> class.
        /// </summary>
        public ApiFetchCommand()
            : base("fetch", "Download an API version's inputs into sources/api/ and record it")
        {
            this.Options.Add(Tag);
            this.Options.Add(NoSpecs);
            this.Options.Add(Force);
            this.Options.Add(NoDefault);
        }

        /// <summary>Runs the command.</summary>
        public sealed class Handler
        {
            private readonly IApiInstaller installer;

            /// <summary>
            /// Initializes a new instance of the <see cref="Handler"/> class.
            /// </summary>
            public Handler(IApiInstaller installer)
            {
                ArgumentNullException.ThrowIfNull(installer);

                this.installer = installer;
            }

            /// <summary>Fetches the requested API version.</summary>
            public async Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
            {
                ArgumentNullException.ThrowIfNull(parseResult);

                var request = new ApiInstallRequest
                {
                    Tag = parseResult.GetValue(Tag)!,
                    IncludeSpecification = !parseResult.GetValue(NoSpecs),
                    SkipExisting = !parseResult.GetValue(Force),
                    MakeDefault = !parseResult.GetValue(NoDefault),
                };

                ApiInstallation installation;

                try
                {
                    installation = await this.installer.InstallAsync(request, cancellationToken);
                }
                catch (ArgumentException exception)
                {
                    // A tag that is not version-shaped is a usage error, not a crash to report.
                    AnsiConsole.MarkupLine($"[red]{Markup.Escape(exception.Message)}[/]");
                    AnsiConsole.MarkupLine("[grey]Run 'hypha api discover' to list the offerable API versions.[/]");
                    return 2;
                }

                AnsiConsole.MarkupLine(
                    $"Fetched API version [bold]{Markup.Escape(installation.Tag)}[/]: "
                    + $"{installation.Files.Count} files.");
                AnsiConsole.MarkupLine(
                    $"[grey]Recorded in {Markup.Escape(installation.Manifest.FullName)}.[/]");

                return 0;
            }
        }
    }
}
