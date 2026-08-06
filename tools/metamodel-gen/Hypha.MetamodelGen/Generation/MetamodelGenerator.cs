// ------------------------------------------------------------------------------------------------
// <copyright file="MetamodelGenerator.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.MetamodelGen.Generation
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Generation;
    using Hypha.Knowledge.Layout;
    using Hypha.MetamodelGen.Generators;

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

        private readonly IKnowledgeLayout layout;
        private readonly IMetamodelModelLoader loader;
        private readonly ILogger<MetamodelGenerator> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MetamodelGenerator"/> class.
        /// </summary>
        public MetamodelGenerator(
            IKnowledgeLayout layout, IMetamodelModelLoader loader, ILogger<MetamodelGenerator> logger)
        {
            this.layout = layout;
            this.loader = loader;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public string Artifact => "metamodel";

        /// <inheritdoc/>
        public int Order => 10;

        /// <inheritdoc/>
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
