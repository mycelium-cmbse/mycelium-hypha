// ------------------------------------------------------------------------------------------------
// <copyright file="SpecGenerator.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Generation
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Hosting;
    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Toolchain;

    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Writes <c>knowledge/&lt;tag&gt;/spec/{kerml,sysml2}/</c> - the full verbatim clause text of the
    /// OMG specifications - by running <c>tools/spec-extract</c> (Python/pdfplumber) through <c>uv</c>.
    /// </summary>
    /// <remarks>
    /// Unlike every other generator, the real work happens outside this process entirely: <c>uv</c>
    /// resolves or fetches a matching Python itself and installs <c>tools/spec-extract</c>'s
    /// dependencies into a managed venv on demand, so this needs neither a pre-existing Python nor a
    /// provisioned <c>.venv</c> - the one thing that used to make spec-citation quoting maintainer-only.
    /// <c>tools/spec-extract</c> is already present on disk for an installed plugin (it ships as a
    /// tag-pinned full checkout); only the runtime to execute it was ever missing.
    /// <para>
    /// Runs before <see cref="CrossReferenceGenerator"/> (<see cref="Order"/> 40) so a single
    /// <c>hypha generate</c> pass gives the cross-references the freshly generated spec catalog to read,
    /// per <see cref="IKnowledgeLayout.SpecificationCatalog"/>'s own contract.
    /// </para>
    /// </remarks>
    public sealed class SpecGenerator : IKnowledgeGenerator
    {
        private const string KermlPdf = "1-Kernel_Modeling_Language.pdf";
        private const string SysmlPdf = "2a-OMG_Systems_Modeling_Language.pdf";

        private readonly IKnowledgeLayout layout;
        private readonly IUvProvisioner uv;
        private readonly IProcessRunner processes;
        private readonly ILogger<SpecGenerator> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpecGenerator"/> class.
        /// </summary>
        public SpecGenerator(
            IKnowledgeLayout layout, IUvProvisioner uv, IProcessRunner processes, ILogger<SpecGenerator> logger)
        {
            this.layout = layout;
            this.uv = uv;
            this.processes = processes;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public string Artifact => "spec";

        /// <inheritdoc/>
        public int Order => 37;

        /// <inheritdoc/>
        public async Task<GenerationResult> GenerateAsync(
            string tag, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(tag);

            GenerationLog.Started(this.logger, this.Artifact, tag);

            var specs = this.layout.Specifications(tag);
            if (!File.Exists(Path.Combine(specs.FullName, KermlPdf))
                || !File.Exists(Path.Combine(specs.FullName, SysmlPdf)))
            {
                return GenerationResult.Skipped(
                        $"OMG spec PDFs not present for {tag} under {specs.FullName} (git-ignored); "
                        + $"run 'hypha fetch --tag {tag}' (not --no-specs) to obtain them")
                    .Report(this.logger, this.Artifact, tag);
            }

            var executable = await this.uv.EnsureAsync(cancellationToken);
            if (executable is null)
            {
                return GenerationResult.Skipped(
                        "could not provision the uv Python toolchain (offline, an unverifiable "
                        + "download, or an unsupported platform); spec-citation will still name the "
                        + "governing clause via cross-references.json, just not quote it verbatim")
                    .Report(this.logger, this.Artifact, tag);
            }

            var specExtract = new DirectoryInfo(Path.Combine(this.layout.Root.FullName, "tools", "spec-extract"));

            var exitCode = await this.processes.RunAsync(
                executable.FullName,
                $"run --project \"{specExtract.FullName}\" python -m spec_extract "
                + $"--repo-root \"{this.layout.Root.FullName}\" --tag {tag} "
                + $"--out-root \"{this.layout.OutputRoot.FullName}\"",
                this.layout.Root,
                cancellationToken);

            if (exitCode != 0)
            {
                return GenerationResult.Skipped($"spec extraction exited with code {exitCode} for {tag}")
                    .Report(this.logger, this.Artifact, tag);
            }

            var specRoot = new DirectoryInfo(Path.Combine(this.layout.Knowledge(tag).FullName, "spec"));
            var written = specRoot.Exists
                ? specRoot.GetFiles("*", SearchOption.AllDirectories)
                : [];

            if (written.Length == 0)
            {
                return GenerationResult.Skipped($"spec extraction reported success but wrote nothing for {tag}")
                    .Report(this.logger, this.Artifact, tag);
            }

            return GenerationResult.Generated(written).Report(this.logger, this.Artifact, tag);
        }
    }
}
