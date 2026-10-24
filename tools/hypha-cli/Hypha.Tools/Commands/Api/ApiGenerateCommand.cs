// ------------------------------------------------------------------------------------------------
// <copyright file="ApiGenerateCommand.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Commands.Api
{
    using System.CommandLine;
    using System.IO;

    using Hypha.Knowledge.Api;

    /// <summary>
    /// <c>hypha api generate</c> - writes the API knowledge base from the fetched inputs.
    /// </summary>
    /// <remarks>
    /// Runs the same loop as <see cref="GenerateCommand"/>, over the API generators and the installed
    /// API versions instead of the language ones - see <see cref="Scope"/>.
    /// </remarks>
    public sealed class ApiGenerateCommand : Command
    {
        /// <summary>The single artifact to generate; all of them when omitted.</summary>
        public static readonly Argument<string?> Artifact =
            new("artifact")
            {
                Description =
                    "The artifact to generate, e.g. pim or openapi. Every artifact, in dependency "
                    + "order, when omitted.",
                Arity = ArgumentArity.ZeroOrOne,
            };

        /// <summary>The API versions to generate for; every installed one when omitted.</summary>
        public static readonly Option<string[]> Tag =
            new("--tag", "-t")
            {
                Description =
                    "The API version to generate for; repeatable. Every installed API version when omitted.",
                AllowMultipleArgumentsPerToken = true,
            };

        /// <summary>Where to write, when not into the repository.</summary>
        public static readonly Option<DirectoryInfo?> Output =
            new("--output", "-o")
            {
                Description =
                    "Write the generated artifacts here instead of into the repository. Inputs are "
                    + "still read from the repository.",
            };

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiGenerateCommand"/> class.
        /// </summary>
        public ApiGenerateCommand()
            : base("generate", "Generate the API knowledge base from the fetched API inputs")
        {
            this.Arguments.Add(Artifact);
            this.Options.Add(Tag);
            this.Options.Add(Output);
        }

        /// <summary>What the shared generation loop runs over for the API.</summary>
        public static GenerateCommand.GenerationScope Scope(IApiLayout layout) => new(
            () => layout.InstalledTags,
            layout.VersionManifest.Directory!.FullName,
            "API version",
            "hypha api fetch --tag <version>",
            Artifact,
            Tag);
    }
}
