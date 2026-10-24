// ------------------------------------------------------------------------------------------------
// <copyright file="DataTypeFileGenerator.cs" company="Starion Group S.A.">
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
    /// Writes one page per data type that is neither a primitive type nor an enumeration.
    /// </summary>
    /// <remarks>
    /// Neither KerML nor SysML declares one, so this writes nothing for them. The Systems Modeling API
    /// PIM does: <c>UUID</c>, <c>IRI</c> and <c>ISO8601DateTime</c>.
    /// </remarks>
    public class DataTypeFileGenerator : HandleBarsGenerator
    {
        private const string TemplateName = "datatype";

        /// <summary>
        /// Initializes a new instance of the <see cref="DataTypeFileGenerator"/> class.
        /// </summary>
        public DataTypeFileGenerator()
            : base(TemplateName)
        {
        }

        /// <summary>Renders the page for one data type.</summary>
        public string GenerateElement(IDataType dataType)
        {
            ArgumentNullException.ThrowIfNull(dataType);

            return this.Templates[TemplateName](new PrimitiveTypePayload(
                dataType.Name,
                dataType.Namespace?.Name ?? string.Empty,
                ElementCatalog.FullyQualifiedName(dataType),
                ElementCatalog.VisibilityName(dataType.Visibility),
                dataType.QueryDocumentationText()));
        }

        /// <summary>Writes one <c>{Name}.md</c> per data type into <paramref name="outputDirectory"/>.</summary>
        public async Task GenerateAsync(XmiReaderResult model, DirectoryInfo outputDirectory)
        {
            ArgumentNullException.ThrowIfNull(model);
            ArgumentNullException.ThrowIfNull(outputDirectory);

            foreach (var dataType in ElementCatalog.DataTypes(model))
            {
                await WriteAsync(this.GenerateElement(dataType), outputDirectory, $"{dataType.Name}.md");
            }
        }
    }
}
