// ------------------------------------------------------------------------------------------------
// <copyright file="PrimitiveTypeFileGenerator.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.MetamodelGen.Generators
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    using uml4net.SimpleClassifiers;
    using uml4net.xmi.Readers;

    /// <summary>
    /// Generates one markdown file per primitive type (name, package, fully qualified name,
    /// visibility and documentation). Output is deterministic.
    /// </summary>
    public class PrimitiveTypeFileGenerator : HandleBarsGenerator
    {
        /// <summary>
        /// The name of the primitive-type element template (<c>{name}.hbs</c>).
        /// </summary>
        private const string TemplateName = "primitive";

        /// <summary>
        /// Initializes a new instance of the <see cref="PrimitiveTypeFileGenerator"/> class.
        /// </summary>
        public PrimitiveTypeFileGenerator()
            : base(TemplateName)
        {
        }

        /// <summary>
        /// Renders the element markdown for a single primitive type.
        /// </summary>
        public string GenerateElement(IPrimitiveType primitiveType)
        {
            ArgumentNullException.ThrowIfNull(primitiveType);

            return this.Templates[TemplateName](CreatePayload(primitiveType));
        }

        /// <summary>
        /// Renders one <c>&lt;PrimitiveTypeName&gt;.md</c> file per primitive type into
        /// <paramref name="outputDirectory"/>.
        /// </summary>
        public async Task GenerateAsync(XmiReaderResult model, DirectoryInfo outputDirectory)
        {
            ArgumentNullException.ThrowIfNull(model);
            ArgumentNullException.ThrowIfNull(outputDirectory);

            foreach (var primitiveType in ElementCatalog.PrimitiveTypes(model))
            {
                var content = this.GenerateElement(primitiveType);

                await WriteAsync(content, outputDirectory, $"{primitiveType.Name}.md");
            }
        }

        /// <summary>
        /// Builds the deterministic payload for a primitive type from the model.
        /// </summary>
        private static PrimitiveTypePayload CreatePayload(IPrimitiveType primitiveType)
        {
            return new PrimitiveTypePayload(
                primitiveType.Name,
                primitiveType.Namespace?.Name ?? string.Empty,
                ElementCatalog.FullyQualifiedName(primitiveType),
                ElementCatalog.VisibilityName(primitiveType.Visibility),
                primitiveType.QueryDocumentationText());
        }
    }
}
