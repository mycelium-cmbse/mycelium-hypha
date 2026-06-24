// ------------------------------------------------------------------------------------------------
// <copyright file="MetamodelIndexGenerator.cs" company="Starion Group S.A.">
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
    using uml4net.Packages;
    using uml4net.StructuredClassifiers;
    using uml4net.xmi.Readers;

    /// <summary>
    /// Generates the metamodel <c>index.md</c> manifest: every metaclass, grouped by its owning
    /// package and linked to its element file under <c>elements/</c>. Output is deterministic.
    /// </summary>
    public class MetamodelIndexGenerator : HandleBarsGenerator
    {
        /// <summary>
        /// The name of the index template (<c>{name}.hbs</c>).
        /// </summary>
        private const string TemplateName = "metamodel-index";

        /// <inheritdoc/>
        protected override void RegisterTemplates()
        {
            this.RegisterTemplate(TemplateName);
        }

        /// <summary>
        /// Renders the index markdown for the model in <paramref name="xmiReaderResult"/>.
        /// </summary>
        /// <param name="xmiReaderResult">The deserialized metamodel.</param>
        /// <param name="title">The human-readable metamodel title (e.g. "SysML v2").</param>
        /// <returns>The generated index markdown.</returns>
        public string GenerateIndex(XmiReaderResult xmiReaderResult, string title)
        {
            ArgumentNullException.ThrowIfNull(xmiReaderResult);
            ArgumentException.ThrowIfNullOrWhiteSpace(title);

            var payload = CreatePayload(xmiReaderResult, title);
            var template = this.Templates[TemplateName];

            return template(payload);
        }

        /// <summary>
        /// Renders the index and writes it to <c>index.md</c> in <paramref name="outputDirectory"/>.
        /// </summary>
        public async Task GenerateAsync(XmiReaderResult xmiReaderResult, DirectoryInfo outputDirectory, string title)
        {
            ArgumentNullException.ThrowIfNull(outputDirectory);

            var content = this.GenerateIndex(xmiReaderResult, title);

            await WriteAsync(content, outputDirectory, "index.md");
        }

        /// <summary>
        /// Builds the deterministic, ordered index payload from the model.
        /// </summary>
        private static MetamodelIndexPayload CreatePayload(XmiReaderResult xmiReaderResult, string title)
        {
            var packages = xmiReaderResult.Packages
                .SelectMany(ExpandPackages)
                .DistinctBy(package => package.XmiId);

            var groups = packages
                .Select(package => new PackageGroup(
                    package.Name,
                    package.PackagedElement
                        .OfType<IClass>()
                        .OrderBy(@class => @class.Name, StringComparer.Ordinal)
                        .Select(@class => new MetaclassEntry(@class.Name))
                        .ToList()))
                .Where(group => group.Metaclasses.Count > 0)
                .OrderBy(group => group.Name, StringComparer.Ordinal)
                .ToList();

            var metaclassCount = groups.Sum(group => group.Metaclasses.Count);

            return new MetamodelIndexPayload(title, metaclassCount, groups.Count, groups);
        }

        /// <summary>
        /// Yields a package together with all of its (recursively) nested packages.
        /// </summary>
        private static IEnumerable<IPackage> ExpandPackages(IPackage package)
        {
            yield return package;

            foreach (var nested in package.QueryPackages())
            {
                yield return nested;
            }
        }
    }
}
