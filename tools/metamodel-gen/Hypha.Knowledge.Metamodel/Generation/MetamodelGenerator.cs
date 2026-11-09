// ------------------------------------------------------------------------------------------------
// <copyright file="MetamodelGenerator.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Metamodel.Generation
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Generation;
    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Metamodel.Generators;

    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Writes <c>knowledge/&lt;tag&gt;/metamodel/</c>: one page per element, the index, the JSON
    /// sidecars and the package diagrams.
    /// </summary>
    /// <remarks>
    /// Runs first. Everything downstream reads its index: the textual notation derives surface forms
    /// from the element names, and the cross-references key every entry on them.
    /// </remarks>
    public sealed class MetamodelGenerator : IKnowledgeGenerator
    {
        /// <summary>The title the generated index carries.</summary>
        private const string IndexTitle = "SysML v2";

        /// <summary>Where the per-element pages sit, under the metamodel directory.</summary>
        private const string ElementsFolder = "elements";

        /// <summary>Resolves where a release's XMI is read from and its metamodel knowledge is written to.</summary>
        private readonly IKnowledgeLayout layout;

        /// <summary>Reads, and caches, a release's combined KerML + SysML model.</summary>
        private readonly IMetamodelModelLoader loader;

        /// <summary>Reports the progress and the outcome of a run.</summary>
        private readonly ILogger<MetamodelGenerator> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MetamodelGenerator"/> class.
        /// </summary>
        /// <param name="layout">Resolves the release's XMI and metamodel output folders.</param>
        /// <param name="loader">Reads the release's metamodel.</param>
        /// <param name="logger">Reports the progress and the outcome of a run.</param>
        public MetamodelGenerator(
            IKnowledgeLayout layout, IMetamodelModelLoader loader, ILogger<MetamodelGenerator> logger)
        {
            this.layout = layout;
            this.loader = loader;
            this.logger = logger;
        }

        /// <summary>What this generator produces: <c>metamodel</c>. Used when reporting progress.</summary>
        public string Artifact => "metamodel";

        /// <summary>
        /// Where this generator sits in a full run: <c>10</c>, first, because every later artifact reads
        /// the metamodel index it writes.
        /// </summary>
        public int Order => 10;

        /// <summary>
        /// Generates <c>knowledge/&lt;tag&gt;/metamodel/</c> for <paramref name="tag"/>: the element
        /// pages, the index, the JSON sidecars and the package diagrams.
        /// </summary>
        /// <param name="tag">The release to generate the metamodel knowledge for.</param>
        /// <param name="cancellationToken">Observed once the files have been written.</param>
        /// <returns>
        /// Every file under the metamodel directory, or a <see cref="GenerationOutcome.Skipped"/> result
        /// when the release's metamodel XMI has not been fetched.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="OperationCanceledException">
        /// <paramref name="cancellationToken"/> was cancelled.
        /// </exception>
        public async Task<GenerationResult> GenerateAsync(
            string tag, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(tag);

            GenerationLog.Started(this.logger, this.Artifact, tag);

            if (this.loader.Load(tag) is not { } model)
            {
                return GenerationResult.Skipped($"no metamodel XMI fetched for {tag}")
                    .Report(this.logger, this.Artifact, tag);
            }

            var metamodel = this.layout.Metamodel(tag);
            var elements = new DirectoryInfo(Path.Combine(metamodel.FullName, ElementsFolder));

            await new MetaclassFileGenerator().GenerateAsync(model, elements);
            await new EnumerationFileGenerator().GenerateAsync(model, elements);
            await new PrimitiveTypeFileGenerator().GenerateAsync(model, elements);

            await new MetamodelIndexGenerator().GenerateAsync(model, metamodel, IndexTitle);
            await new PackageDiagramGenerator().GenerateAsync(model, metamodel);
            await MetamodelJsonGenerator.GenerateAsync(model, metamodel, this.SourceHash(tag));

            cancellationToken.ThrowIfCancellationRequested();

            metamodel.Refresh();

            return GenerationResult.Generated(
                [.. metamodel.EnumerateFiles("*", SearchOption.AllDirectories)])
                .Report(this.logger, this.Artifact, tag);
        }

        /// <summary>
        /// The deterministic digest of the XMI a release was generated from, recorded in the sidecar
        /// so a reader can tell which inputs produced it.
        /// </summary>
        /// <remarks>
        /// The primitive types are shared across releases, so they are read from the common
        /// <c>sources/</c> root rather than the tag folder.
        /// </remarks>
        /// <param name="tag">The release whose XMI is hashed.</param>
        /// <returns>The SHA-256 digest of the release's SysML, KerML and primitive-type XMI.</returns>
        public string SourceHash(string tag)
        {
            var xmi = this.layout.Xmi(tag);

            return MetamodelJsonGenerator.ComputeSourceXmiSha256(
            [
                Path.Combine(xmi.FullName, MetamodelModelLoader.RootDocument),
                Path.Combine(xmi.FullName, "KerML_only_xmi.uml"),
                this.layout.SharedPrimitiveTypes.FullName,
            ]);
        }
    }
}
