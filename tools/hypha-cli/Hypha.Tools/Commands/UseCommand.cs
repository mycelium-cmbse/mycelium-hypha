// ------------------------------------------------------------------------------------------------
// <copyright file="UseCommand.cs" company="Starion Group S.A.">
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
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Releases;

    using Spectre.Console;

    /// <summary>
    /// Switches which installed release every skill answers from by default.
    /// </summary>
    public sealed class UseCommand : Command
    {
        /// <summary>The release to make the default.</summary>
        public static readonly Option<string> Tag =
            new("--tag", "-t")
            {
                Description = "The installed release to answer from by default.",
                Required = true,
            };

        /// <summary>
        /// Initializes a new instance of the <see cref="UseCommand"/> class.
        /// </summary>
        public UseCommand()
            : base("use", "Switch which installed release the plugin answers from by default")
        {
            this.Options.Add(Tag);
        }

        /// <summary>Runs the <see cref="UseCommand"/>.</summary>
        public sealed class Handler
        {
            private readonly IKnowledgeLayout layout;

            /// <summary>
            /// Initializes a new instance of the <see cref="Handler"/> class.
            /// </summary>
            public Handler(IKnowledgeLayout layout)
            {
                ArgumentNullException.ThrowIfNull(layout);

                this.layout = layout;
            }

            /// <summary>Switches the default release.</summary>
            /// <returns>0 when switched; 2 when the tag is not installed.</returns>
            public Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
            {
                ArgumentNullException.ThrowIfNull(parseResult);

                cancellationToken.ThrowIfCancellationRequested();

                var tag = parseResult.GetValue(Tag)!;
                var path = this.layout.VersionManifest.FullName;
                var manifest = VersionManifestFile.ReadIfPresent(path);

                if (manifest is null || !manifest.GetTags().Contains(tag, StringComparer.Ordinal))
                {
                    var installed = manifest?.GetTags() ?? [];

                    AnsiConsole.MarkupLine($"[red]'{Markup.Escape(tag)}' is not installed.[/]");
                    AnsiConsole.MarkupLine(
                        installed.Count > 0
                            ? $"[grey]Installed: {Markup.Escape(string.Join(", ", installed))}. "
                              + "Run 'hypha fetch --tag <release>' first.[/]"
                            : "[grey]Nothing is installed. Run 'hypha fetch --tag <release>' first.[/]");

                    return Task.FromResult(2);
                }

                if (string.Equals(manifest.Default, tag, StringComparison.Ordinal))
                {
                    AnsiConsole.MarkupLine($"[bold]{Markup.Escape(tag)}[/] is already the default.");

                    return Task.FromResult(0);
                }

                VersionManifestFile.Write(VersionManifest.Build(tag, manifest.Versions), path);

                AnsiConsole.MarkupLine(
                    $"[bold]{Markup.Escape(tag)}[/] is now the default release "
                    + $"[grey](was {Markup.Escape(manifest.Default)})[/].");

                return Task.FromResult(0);
            }
        }
    }
}
