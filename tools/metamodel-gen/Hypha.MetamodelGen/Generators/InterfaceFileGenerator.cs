// ------------------------------------------------------------------------------------------------
// <copyright file="InterfaceFileGenerator.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.MetamodelGen.Generators
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using uml4net.Extensions;
    using uml4net.SimpleClassifiers;
    using uml4net.xmi.Readers;

    /// <summary>
    /// Writes one page per interface: what it is, and which classifiers realize it.
    /// </summary>
    /// <remarks>
    /// Neither KerML nor SysML declares an interface, so this writes nothing for them. The Systems
    /// Modeling API PIM does: <c>Data</c>, which the versioned payloads realize.
    /// </remarks>
    public class InterfaceFileGenerator : HandleBarsGenerator
    {
        private const string TemplateName = "interface";

        /// <summary>
        /// Initializes a new instance of the <see cref="InterfaceFileGenerator"/> class.
        /// </summary>
        public InterfaceFileGenerator()
            : base(TemplateName)
        {
        }

        /// <summary>Renders the page for one interface.</summary>
        public string GenerateElement(
            IInterface @interface, IReadOnlyList<string> realizedBy, IReadOnlySet<string> linkableTypeNames)
        {
            ArgumentNullException.ThrowIfNull(@interface);
            ArgumentNullException.ThrowIfNull(realizedBy);
            ArgumentNullException.ThrowIfNull(linkableTypeNames);

            var payload = new InterfacePayload
            {
                Name = @interface.Name,
                Package = @interface.Namespace?.Name ?? string.Empty,
                FullyQualifiedName = ElementCatalog.FullyQualifiedName(@interface),
                Visibility = ElementCatalog.VisibilityName(@interface.Visibility),
                Documentation = @interface.QueryDocumentationText(),
                RealizedBy = realizedBy,
                RealizedByMarkdown = realizedBy
                    .Select(name => linkableTypeNames.Contains(name) ? $"[{name}]({name}.md)" : name)
                    .ToList(),
                Operations = @interface.OwnedOperation
                    .OrderBy(operation => operation.Name, StringComparer.Ordinal)
                    .Select(operation => new MetaclassOperation(
                        operation.Name,
                        MetaclassFileGenerator.QueryOperationSignature(operation),
                        operation.IsQuery,
                        operation.QueryDocumentationText(),
                        operation.QueryBodyConditionText()))
                    .ToList(),
            };

            return this.Templates[TemplateName](payload);
        }

        /// <summary>Writes one <c>{Name}.md</c> per interface into <paramref name="outputDirectory"/>.</summary>
        public async Task GenerateAsync(XmiReaderResult model, DirectoryInfo outputDirectory)
        {
            ArgumentNullException.ThrowIfNull(model);
            ArgumentNullException.ThrowIfNull(outputDirectory);

            var linkableTypeNames = ElementCatalog.LinkableTypeNames(model);

            foreach (var @interface in ElementCatalog.Interfaces(model))
            {
                var content = this.GenerateElement(
                    @interface, ElementCatalog.RealizedBy(model, @interface), linkableTypeNames);

                await WriteAsync(content, outputDirectory, $"{@interface.Name}.md");
            }
        }
    }
}
