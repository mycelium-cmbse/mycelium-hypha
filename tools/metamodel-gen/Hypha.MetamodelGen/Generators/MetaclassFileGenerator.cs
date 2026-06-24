// ------------------------------------------------------------------------------------------------
// <copyright file="MetaclassFileGenerator.cs" company="Starion Group S.A.">
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

    using uml4net.CommonStructure;
    using uml4net.Extensions;
    using uml4net.Packages;
    using uml4net.StructuredClassifiers;
    using uml4net.Values;
    using uml4net.xmi.Readers;

    /// <summary>
    /// Generates one markdown file per metaclass (name, package, abstractness, documentation, owned
    /// features, generalizations, redefinitions/subsettings and constraints). Output is deterministic.
    /// </summary>
    public class MetaclassFileGenerator : HandleBarsGenerator
    {
        /// <summary>
        /// The name of the metaclass element template (<c>{name}.hbs</c>).
        /// </summary>
        private const string TemplateName = "metaclass";

        /// <summary>
        /// Initializes a new instance of the <see cref="MetaclassFileGenerator"/> class.
        /// </summary>
        public MetaclassFileGenerator()
            : base(TemplateName)
        {
        }

        /// <summary>
        /// Renders the element markdown for a single metaclass.
        /// </summary>
        public string GenerateElement(IClass @class)
        {
            ArgumentNullException.ThrowIfNull(@class);

            var payload = CreatePayload(@class);

            return this.Templates[TemplateName](payload);
        }

        /// <summary>
        /// Renders one <c>&lt;MetaclassName&gt;.md</c> file per metaclass into
        /// <paramref name="outputDirectory"/>.
        /// </summary>
        public async Task GenerateAsync(XmiReaderResult model, DirectoryInfo outputDirectory)
        {
            ArgumentNullException.ThrowIfNull(model);
            ArgumentNullException.ThrowIfNull(outputDirectory);

            foreach (var @class in QueryMetaclasses(model))
            {
                var content = this.GenerateElement(@class);

                await WriteAsync(content, outputDirectory, $"{@class.Name}.md");
            }
        }

        /// <summary>
        /// Returns all metaclasses (<see cref="IClass"/>) in the model, ordered by name.
        /// </summary>
        public static IReadOnlyList<IClass> QueryMetaclasses(XmiReaderResult model)
        {
            ArgumentNullException.ThrowIfNull(model);

            return model.Packages
                .SelectMany(ExpandPackages)
                .DistinctBy(package => package.XmiId)
                .SelectMany(package => package.PackagedElement.OfType<IClass>())
                .DistinctBy(@class => @class.XmiId)
                .OrderBy(@class => @class.Name, StringComparer.Ordinal)
                .ToList();
        }

        /// <summary>
        /// Builds the deterministic payload for a metaclass from the model.
        /// </summary>
        private static MetaclassPayload CreatePayload(IClass @class)
        {
            var generalizations = @class.SuperClass
                .Select(super => super.Name)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToList();

            var features = @class.OwnedAttribute
                .OrderBy(property => property.Name, StringComparer.Ordinal)
                .Select(property => new MetaclassFeature(
                    property.Name,
                    property.Type?.Name ?? "«untyped»",
                    property.QueryFormattedMultiplicity(),
                    property.QueryDocumentationText(),
                    property.RedefinedProperty.Select(redefined => redefined.Name).ToList(),
                    property.SubsettedProperty.Select(subsetted => subsetted.Name).ToList()))
                .ToList();

            var constraints = @class.OwnedRule
                .OrderBy(rule => rule.Name, StringComparer.Ordinal)
                .Select(rule => new MetaclassConstraint(rule.Name, QueryConstraintBody(rule)))
                .ToList();

            return new MetaclassPayload(
                @class.Name,
                @class.Namespace?.Name ?? string.Empty,
                @class.IsAbstract,
                @class.QueryDocumentationText(),
                generalizations,
                features,
                constraints);
        }

        /// <summary>
        /// Extracts the textual body of a constraint specification, when it is an opaque expression.
        /// </summary>
        private static string QueryConstraintBody(IConstraint constraint)
        {
            return constraint.Specification is IOpaqueExpression expression
                ? string.Join(" ", expression.Body)
                : string.Empty;
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
