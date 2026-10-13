// ------------------------------------------------------------------------------------------------
// <copyright file="ApiSpecGenerator.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Generation;
    using Hypha.Knowledge.Hosting;
    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Toolchain;

    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Writes <c>knowledge/api/&lt;tag&gt;/spec/</c> - the full verbatim clause text of the Systems
    /// Modeling API and Services specification - by running <c>tools/spec-extract</c> through
    /// <c>uv</c>, exactly as <see cref="SpecGenerator"/> does for the KerML and SysML specifications.
    /// </summary>
    /// <remarks>
    /// The text is OMG-copyrighted: it is generated locally from a PDF the user fetched, lands in a
    /// git-ignored folder, and is never committed. Runs before the API cross-references, which read the
    /// clause catalog and the two PIM-to-PSM mapping tables from it.
    /// </remarks>
    public sealed class ApiSpecGenerator : IApiKnowledgeGenerator
    {
        private readonly IApiLayout layout;
        private readonly IKnowledgeLayout repository;
        private readonly IUvProvisioner uv;
        private readonly IProcessRunner processes;
        private readonly ILogger<ApiSpecGenerator> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiSpecGenerator"/> class.
        /// </summary>
        public ApiSpecGenerator(
            IApiLayout layout,
            IKnowledgeLayout repository,
            IUvProvisioner uv,
            IProcessRunner processes,
            ILogger<ApiSpecGenerator> logger)
        {
            ArgumentNullException.ThrowIfNull(layout);
            ArgumentNullException.ThrowIfNull(repository);
            ArgumentNullException.ThrowIfNull(uv);
            ArgumentNullException.ThrowIfNull(processes);
            ArgumentNullException.ThrowIfNull(logger);

            this.layout = layout;
            this.repository = repository;
            this.uv = uv;
            this.processes = processes;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public string Artifact => "spec";

        /// <inheritdoc/>
        public int Order => 30;

        /// <inheritdoc/>
        public async Task<GenerationResult> GenerateAsync(string tag, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(tag);

            GenerationLog.Started(this.logger, this.Artifact, tag);

            var specs = this.layout.Specifications(tag);

            if (!File.Exists(Path.Combine(specs.FullName, ApiInputs.Specification)))
            {
                return GenerationResult.Skipped(
                        $"OMG API specification PDF not present for API version {tag} under {specs.FullName} "
                        + $"(git-ignored); run 'hypha api fetch --tag {tag}' (not --no-specs) to obtain it")
                    .Report(this.logger, this.Artifact, tag);
            }

            var executable = await this.uv.EnsureAsync(cancellationToken);

            if (executable is null)
            {
                return GenerationResult.Skipped(
                        "could not provision the uv Python toolchain (offline, an unverifiable download, or "
                        + "an unsupported platform); the API cross-references are still generated, without "
                        + "clause references")
                    .Report(this.logger, this.Artifact, tag);
            }

            var specExtract = new DirectoryInfo(Path.Combine(this.repository.Root.FullName, "tools", "spec-extract"));

            var exitCode = await this.processes.RunAsync(
                executable.FullName,
                $"run --project \"{specExtract.FullName}\" python -m spec_extract "
                + $"--repo-root \"{this.repository.Root.FullName}\" --api-version {tag} "
                + $"--out-root \"{this.repository.OutputRoot.FullName}\"",
                this.repository.Root,
                cancellationToken);

            if (exitCode != 0)
            {
                return GenerationResult.Skipped($"spec extraction exited with code {exitCode} for API version {tag}")
                    .Report(this.logger, this.Artifact, tag);
            }

            var specRoot = this.layout.SpecificationText(tag);
            specRoot.Refresh();

            var written = specRoot.Exists ? specRoot.GetFiles("*", SearchOption.AllDirectories) : [];

            if (written.Length == 0)
            {
                return GenerationResult.Skipped(
                        $"spec extraction reported success but wrote nothing for API version {tag}")
                    .Report(this.logger, this.Artifact, tag);
            }

            return GenerationResult.Generated(written).Report(this.logger, this.Artifact, tag);
        }
    }
}
