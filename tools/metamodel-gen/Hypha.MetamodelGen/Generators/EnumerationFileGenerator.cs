// ------------------------------------------------------------------------------------------------
// <copyright file="EnumerationFileGenerator.cs" company="Starion Group S.A.">
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
    using System.Linq;
    using System.Threading.Tasks;

    using uml4net.SimpleClassifiers;
    using uml4net.xmi.Readers;

    /// <summary>
    /// Generates one markdown file per enumeration (name, package, fully qualified name, visibility,
    /// documentation and literals). Output is deterministic.
    /// </summary>
    public class EnumerationFileGenerator : HandleBarsGenerator
    {
        /// <summary>
        /// The name of the enumeration element template (<c>{name}.hbs</c>).
        /// </summary>
        private const string TemplateName = "enumeration";

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumerationFileGenerator"/> class.
        /// </summary>
        public EnumerationFileGenerator()
            : base(TemplateName)
        {
        }

        /// <summary>
        /// Renders the element markdown for a single enumeration.
        /// </summary>
        public string GenerateElement(IEnumeration enumeration)
        {
            ArgumentNullException.ThrowIfNull(enumeration);

            return this.Templates[TemplateName](CreatePayload(enumeration));
        }

        /// <summary>
        /// Renders one <c>&lt;EnumerationName&gt;.md</c> file per enumeration into
        /// <paramref name="outputDirectory"/>.
        /// </summary>
        public async Task GenerateAsync(XmiReaderResult model, DirectoryInfo outputDirectory)
        {
            ArgumentNullException.ThrowIfNull(model);
            ArgumentNullException.ThrowIfNull(outputDirectory);

            foreach (var enumeration in ElementCatalog.Enumerations(model))
            {
                var content = this.GenerateElement(enumeration);

                await WriteAsync(content, outputDirectory, $"{enumeration.Name}.md");
            }
        }

        /// <summary>
        /// Builds the deterministic payload for an enumeration from the model.
        /// </summary>
        private static EnumerationPayload CreatePayload(IEnumeration enumeration)
        {
            var literals = enumeration.OwnedLiteral
                .Select(literal => new EnumerationLiteralEntry(literal.Name, literal.QueryDocumentationText()))
                .ToList();

            return new EnumerationPayload(
                enumeration.Name,
                enumeration.Namespace?.Name ?? string.Empty,
                ElementCatalog.FullyQualifiedName(enumeration),
                ElementCatalog.VisibilityName(enumeration.Visibility),
                enumeration.QueryDocumentationText(),
                literals);
        }
    }
}
