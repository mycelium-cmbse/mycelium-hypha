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
    /// Writes <c>knowledge/&lt;tag&gt;/spec/{kerml,sysml2}/</c>, the full verbatim clause text of the
    /// OMG specifications, by running <c>tools/spec-extract</c> (Python/pdfplumber) through <c>uv</c>.
    /// </summary>
    /// <remarks>
    /// Unlike every other generator, the real work happens outside this process entirely: <c>uv</c>
    /// resolves or fetches a matching Python itself and installs <c>tools/spec-extract</c>'s
    /// dependencies into a managed venv on demand, so this needs neither a pre-existing Python nor a
    /// provisioned <c>.venv</c> - the one thing that used to make spec-citation quoting maintainer-only.
    /// The <c>spec-extract</c> project itself comes from <see cref="ISpecExtractProject"/>: a source
    /// checkout's own <c>tools/spec-extract</c>, or the copy hypha carries, unpacked next to <c>uv</c>.
    /// <para>
    /// Runs before <see cref="CrossReferenceGenerator"/> (<see cref="CrossReferenceGenerator.Order"/> 40) so a single
    /// <c>hypha generate</c> pass gives the cross-references the freshly generated spec catalog to read,
    /// per <see cref="IKnowledgeLayout.SpecificationCatalog"/>'s own contract.
    /// </para>
    /// </remarks>
    public sealed class SpecGenerator : IKnowledgeGenerator
    {
        /// <summary>The file name of the KerML specification PDF under <c>sources/&lt;tag&gt;/specs/</c>.</summary>
        private const string KermlPdf = "1-Kernel_Modeling_Language.pdf";

        /// <summary>The file name of the SysML v2 specification PDF under <c>sources/&lt;tag&gt;/specs/</c>.</summary>
        private const string SysmlPdf = "2a-OMG_Systems_Modeling_Language.pdf";

        /// <summary>Where the PDFs are read from and the clause text is written.</summary>
        private readonly IKnowledgeLayout layout;

        /// <summary>Finds the <c>spec-extract</c> project, unpacking it when the knowledge folder has none.</summary>
        private readonly ISpecExtractProject project;

        /// <summary>Provides the cached <c>uv</c> executable and the environment it runs with.</summary>
        private readonly IUvProvisioner uv;

        /// <summary>Runs <c>uv</c> as a child process.</summary>
        private readonly IProcessRunner processes;

        /// <summary>The log, which receives the generation's progress and the reason for a skip.</summary>
        private readonly ILogger<SpecGenerator> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpecGenerator"/> class.
        /// </summary>
        /// <param name="layout">Where the PDFs are read from and the clause text is written.</param>
        /// <param name="project">Finds the <c>spec-extract</c> project, unpacking it when the knowledge folder has none.</param>
        /// <param name="uv">Provides the cached <c>uv</c> executable and the environment it runs with.</param>
        /// <param name="processes">Runs <c>uv</c> as a child process.</param>
        /// <param name="logger">The log, which receives the generation's progress and the reason for a skip.</param>
        public SpecGenerator(
            IKnowledgeLayout layout, ISpecExtractProject project, IUvProvisioner uv, IProcessRunner processes, ILogger<SpecGenerator> logger)
        {
            this.layout = layout;
            this.project = project;
            this.uv = uv;
            this.processes = processes;
            this.logger = logger;
        }

        /// <summary>Gets the artifact name, <c>spec</c>, used when reporting progress.</summary>
        public string Artifact => "spec";

        /// <summary>
        /// Gets this generator's place in a full run, <c>37</c>: after the model library and before the
        /// cross-references, which read the spec catalogs it writes.
        /// </summary>
        public int Order => 37;

        /// <summary>
        /// Writes <c>knowledge/&lt;tag&gt;/spec/{kerml,sysml2}/</c> by running <c>tools/spec-extract</c>
        /// through a provisioned <c>uv</c> against the release's specification PDFs.
        /// </summary>
        /// <param name="tag">The release to generate for.</param>
        /// <param name="cancellationToken">Cancels provisioning <c>uv</c> and the extraction.</param>
        /// <returns>
        /// Every file under the release's <c>spec/</c> folder, or a <see cref="GenerationOutcome.Skipped"/>
        /// result when either PDF is absent, <c>uv</c> cannot be provisioned, the extraction exits with a
        /// non-zero code, or it writes nothing. None of these throws: the specification text is optional.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is <c>null</c>, empty or white space.</exception>
        public async Task<GenerationResult> GenerateAsync(string tag, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(tag);

            GenerationLog.Started(this.logger, this.Artifact, tag);

            var specs = this.layout.Specifications(tag);
            if (!File.Exists(Path.Combine(specs.FullName, KermlPdf)) || !File.Exists(Path.Combine(specs.FullName, SysmlPdf)))
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

            var specExtract = await this.project.EnsureAsync(cancellationToken);

            var exitCode = await this.processes.RunAsync(
                executable.FullName,
                $"run --project \"{specExtract.FullName}\" python -m spec_extract "
                + $"--repo-root \"{this.layout.Root.FullName}\" --tag {tag} "
                + $"--out-root \"{this.layout.OutputRoot.FullName}\"",
                this.layout.Root,
                this.uv.Environment,
                cancellationToken);

            if (exitCode != 0)
            {
                return GenerationResult.Skipped($"spec extraction exited with code {exitCode} for {tag}")
                    .Report(this.logger, this.Artifact, tag);
            }

            var specRoot = new DirectoryInfo(Path.Combine(this.layout.Knowledge(tag).FullName, "spec"));
            var written = specRoot.Exists ? specRoot.GetFiles("*", SearchOption.AllDirectories) : [];

            if (written.Length == 0)
            {
                return GenerationResult.Skipped($"spec extraction reported success but wrote nothing for {tag}")
                    .Report(this.logger, this.Artifact, tag);
            }

            return GenerationResult.Generated(written).Report(this.logger, this.Artifact, tag);
        }
    }
}
