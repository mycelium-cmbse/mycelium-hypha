// ------------------------------------------------------------------------------------------------
// <copyright file="PimGenerator.cs" company="Starion Group S.A.">
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
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Api;
    using Hypha.Knowledge.Generation;
    using Hypha.MetamodelGen.Generators;

    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Writes <c>knowledge/api/&lt;tag&gt;/pim/</c>: the Systems Modeling API Platform Independent Model,
    /// in the same shape as the KerML / SysML metamodel knowledge.
    /// </summary>
    /// <remarks>
    /// The PIM is a UML model like the metamodels, so it goes through the same generators: one page
    /// per class with its full effective feature set, the index, the JSON sidecars and the package
    /// diagrams. Two things differ. Its classes are not metaclasses, so the pages say <i>class</i>;
    /// and it declares an interface (<c>Data</c>) and data types (<c>UUID</c>, <c>IRI</c>,
    /// <c>ISO8601DateTime</c>), which the metamodels do not, so those get pages of their own.
    /// </remarks>
    public sealed class PimGenerator : IApiKnowledgeGenerator
    {
        /// <summary>
        /// How the PIM model refers to the UML primitive types.
        /// </summary>
        /// <remarks>
        /// Not a location we choose: it is the literal reference the OMG XMI contains, and the key we
        /// have to answer to for resolution to succeed.
        /// </remarks>
        [SuppressMessage(
            "Major Code Smell",
            "S1075:URIs should not be hardcoded",
            Justification = "Fixed identifier appearing in the upstream XMI, not a configurable path.")]
        private const string PrimitiveTypesReference = "http://www.omg.org/spec/UML/20161101/PrimitiveTypes.xmi";

        /// <summary>The title the generated index carries.</summary>
        private const string IndexTitle = "Systems Modeling API PIM";

        private readonly IApiLayout layout;
        private readonly ILogger<PimGenerator> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PimGenerator"/> class.
        /// </summary>
        public PimGenerator(IApiLayout layout, ILogger<PimGenerator> logger)
        {
            ArgumentNullException.ThrowIfNull(layout);
            ArgumentNullException.ThrowIfNull(logger);

            this.layout = layout;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public string Artifact => "pim";

        /// <inheritdoc/>
        public int Order => 10;

        /// <inheritdoc/>
        public async Task<GenerationResult> GenerateAsync(string tag, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(tag);

            GenerationLog.Started(this.logger, this.Artifact, tag);

            var xmi = this.layout.Xmi(tag);
            var pim = this.layout.PimModel(tag);
            var kerml = new FileInfo(Path.Combine(xmi.FullName, ApiInputs.KermlXmi));

            if (!pim.Exists || !kerml.Exists)
            {
                return GenerationResult.Skipped(
                        $"no PIM model fetched for API version {tag} under {xmi.FullName}; "
                        + $"run 'hypha api fetch --tag {tag}'")
                    .Report(this.logger, this.Artifact, tag);
            }

            var primitives = this.layout.SharedPrimitiveTypes;

            var model = XmiModelReader.Read(
                pim.FullName,
                new Dictionary<string, string> { [PrimitiveTypesReference] = primitives.FullName },
                xmi.FullName);

            // The KerML model is read so the PIM's references into it resolve - ExternalRelationship's
            // elementEnd is a KerML Element - but it is not part of the PIM: its metaclasses belong to
            // the language metamodel knowledge, not here. Dropping its packages keeps the names the PIM
            // refers to while writing pages for the PIM's own types only.
            model.Packages.RemoveAll(package =>
                string.Equals(package.DocumentName, kerml.Name, StringComparison.OrdinalIgnoreCase));

            var output = this.layout.Pim(tag);
            var elements = new DirectoryInfo(Path.Combine(output.FullName, "elements"));
            var vocabulary = ModelVocabulary.Model;

            await new MetaclassFileGenerator { Vocabulary = vocabulary }.GenerateAsync(model, elements);
            await new InterfaceFileGenerator().GenerateAsync(model, elements);
            await new DataTypeFileGenerator().GenerateAsync(model, elements);
            await new EnumerationFileGenerator().GenerateAsync(model, elements);
            await new PrimitiveTypeFileGenerator().GenerateAsync(model, elements);
            await new MetamodelIndexGenerator { Vocabulary = vocabulary }.GenerateAsync(model, output, IndexTitle);
            await new PackageDiagramGenerator { Vocabulary = vocabulary }.GenerateAsync(model, output);

            await MetamodelJsonGenerator.GenerateAsync(
                model,
                output,
                MetamodelJsonGenerator.ComputeSourceXmiSha256([pim.FullName, kerml.FullName, primitives.FullName]));

            cancellationToken.ThrowIfCancellationRequested();

            output.Refresh();

            return GenerationResult.Generated([.. output.EnumerateFiles("*", SearchOption.AllDirectories)])
                .Report(this.logger, this.Artifact, tag);
        }
    }
}
