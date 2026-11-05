// ------------------------------------------------------------------------------------------------
// <copyright file="ApiUseCommand.cs" company="Starion Group S.A.">
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
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Api;

    using Spectre.Console;

    /// <summary>
    /// <c>hypha api use</c> - switches which installed API version the plugin answers from by default.
    /// </summary>
    public sealed class ApiUseCommand : Command
    {
        /// <summary>The installed API version to answer from.</summary>
        public static readonly Option<string> Tag =
            new("--tag", "-t")
            {
                Description = "The installed API version to answer from by default.",
                Required = true,
            };

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiUseCommand"/> class.
        /// </summary>
        public ApiUseCommand()
            : base("use", "Switch which installed API version the plugin answers from by default")
        {
            this.Options.Add(Tag);
        }

        /// <summary>Runs the command.</summary>
        public sealed class Handler
        {
            private readonly IApiLayout layout;

            /// <summary>
            /// Initializes a new instance of the <see cref="Handler"/> class.
            /// </summary>
            public Handler(IApiLayout layout)
            {
                ArgumentNullException.ThrowIfNull(layout);

                this.layout = layout;
            }

            /// <summary>Makes the requested API version the default.</summary>
            public Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
            {
                ArgumentNullException.ThrowIfNull(parseResult);
                cancellationToken.ThrowIfCancellationRequested();

                var tag = parseResult.GetValue(Tag)!;
                var path = this.layout.VersionManifest.FullName;
                var manifest = ApiVersionManifestFile.ReadIfPresent(path);

                if (manifest is null || !manifest.GetTags().Contains(tag, StringComparer.Ordinal))
                {
                    var installed = manifest?.GetTags() ?? [];

                    AnsiConsole.MarkupLine($"[red]API version '{Markup.Escape(tag)}' is not installed.[/]");
                    AnsiConsole.MarkupLine(
                        installed.Count > 0
                            ? $"[grey]Installed: {Markup.Escape(string.Join(", ", installed))}. "
                              + "Run 'hypha api fetch --tag <version>' first.[/]"
                            : "[grey]Nothing is installed. Run 'hypha api fetch --tag <version>' first.[/]");

                    return Task.FromResult(2);
                }

                if (string.Equals(manifest.Default, tag, StringComparison.Ordinal))
                {
                    AnsiConsole.MarkupLine($"[bold]{Markup.Escape(tag)}[/] is already the default API version.");
                    return Task.FromResult(0);
                }

                ApiVersionManifestFile.Write(ApiVersionManifest.Build(tag, manifest.Versions), path);

                AnsiConsole.MarkupLine(
                    $"[bold]{Markup.Escape(tag)}[/] is now the default API version "
                    + $"[grey](was {Markup.Escape(manifest.Default)})[/].");

                return Task.FromResult(0);
            }
        }
    }
}
