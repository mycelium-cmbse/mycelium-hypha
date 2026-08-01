// ------------------------------------------------------------------------------------------------
// <copyright file="PackageDiagramGenerator.cs" company="Starion Group S.A.">
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
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using uml4net.Classification;
    using uml4net.StructuredClassifiers;
    using uml4net.xmi.Readers;

    /// <summary>
    /// Generates one Mermaid class diagram per package under <c>diagrams/</c>, read straight from the
    /// uml4net object graph (the XMI is the source of truth). Output is deterministic: every
    /// collection is ordered with <see cref="StringComparer.Ordinal"/> and nothing carries a
    /// timestamp or machine path.
    /// </summary>
    /// <remarks>
    /// Two filters keep the diagrams legible. Derived features are omitted entirely — they are
    /// computed views over other features rather than structure, and they outnumber the structural
    /// ones roughly three to one. Of what remains, only metaclass-typed features become edges;
    /// primitive- and enumeration-typed features are drawn inside the class box, so that
    /// <c>String</c> and <c>Boolean</c> do not appear as hub nodes in every package.
    /// </remarks>
    public class PackageDiagramGenerator : HandleBarsGenerator
    {
        /// <summary>The name of the diagram template (<c>{name}.hbs</c>).</summary>
        private const string TemplateName = "package-diagram";

        /// <summary>The directory, relative to the metamodel knowledge root, the diagrams are written to.</summary>
        public const string DiagramDirectoryName = "diagrams";

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageDiagramGenerator"/> class.
        /// </summary>
        public PackageDiagramGenerator()
            : base(TemplateName)
        {
        }

        /// <summary>
        /// Builds the deterministic payloads, one per package that owns at least one metaclass,
        /// ordered by package name.
        /// </summary>
        public static IReadOnlyList<PackageDiagramPayload> CreatePayloads(XmiReaderResult model)
        {
            ArgumentNullException.ThrowIfNull(model);

            var metaclassNames = ElementCatalog.Metaclasses(model)
                .Select(@class => @class.Name)
                .ToHashSet(StringComparer.Ordinal);

            var boxTypeNames = ElementCatalog.Enumerations(model).Select(element => element.Name)
                .Concat(ElementCatalog.PrimitiveTypes(model).Select(element => element.Name))
                .ToHashSet(StringComparer.Ordinal);

            // Package *names* are not unique across the two metamodels - KerML::Kernel::Metadata and
            // SysML::Systems::Metadata both exist - and the index groups by name, so merge same-named
            // packages into one diagram rather than letting the second overwrite the first's file.
            return model.Packages
                .SelectMany(ElementCatalog.ExpandPackages)
                .DistinctBy(package => package.XmiId)
                .Where(package => !string.IsNullOrEmpty(package.Name))
                .GroupBy(package => package.Name, StringComparer.Ordinal)
                .Select(group => CreatePayload(
                    group.Key,
                    group.SelectMany(package => package.PackagedElement.OfType<IClass>()),
                    metaclassNames,
                    boxTypeNames))
                .Where(payload => payload.Classes.Count > 0)
                .OrderBy(payload => payload.Package, StringComparer.Ordinal)
                .ToList();
        }

        /// <summary>
        /// Builds the payload for one package name. <paramref name="metaclassNames"/> decides which
        /// feature types become edges; <paramref name="boxTypeNames"/> which become box attributes.
        /// </summary>
        public static PackageDiagramPayload CreatePayload(
            string packageName,
            IEnumerable<IClass> packagedClasses,
            IReadOnlySet<string> metaclassNames,
            IReadOnlySet<string> boxTypeNames)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(packageName);
            ArgumentNullException.ThrowIfNull(packagedClasses);
            ArgumentNullException.ThrowIfNull(metaclassNames);
            ArgumentNullException.ThrowIfNull(boxTypeNames);

            var classes = packagedClasses
                .DistinctBy(@class => @class.XmiId)
                .OrderBy(@class => @class.Name, StringComparer.Ordinal)
                .ToList();

            var owned = classes.Select(@class => @class.Name).ToHashSet(StringComparer.Ordinal);
            var relationships = new List<DiagramRelationship>();
            var boundary = new SortedSet<string>(StringComparer.Ordinal);

            foreach (var @class in classes)
            {
                foreach (var super in SupertypeNames(@class))
                {
                    Reach(super);
                    relationships.Add(new DiagramRelationship
                    {
                        Left = super,
                        Arrow = DiagramRelationship.Generalization,
                        Right = @class.Name,
                    });
                }

                foreach (var property in StructuralFeatures(@class))
                {
                    var typeName = property.Type?.Name;

                    if (typeName is null || !metaclassNames.Contains(typeName))
                    {
                        continue;
                    }

                    Reach(typeName);
                    relationships.Add(new DiagramRelationship
                    {
                        Left = @class.Name,
                        Arrow = property.IsComposite
                            ? DiagramRelationship.Composition
                            : DiagramRelationship.Association,
                        Right = typeName,
                        Multiplicity = FormatMultiplicity(property, brackets: false),
                        Label = property.Name,
                    });
                }
            }

            return new PackageDiagramPayload
            {
                Package = packageName,
                Classes = classes
                    .Select(@class => new DiagramClass
                    {
                        Name = @class.Name,
                        IsAbstract = @class.IsAbstract,
                        Attributes = BoxAttributes(@class, boxTypeNames),
                    })
                    .ToList(),
                BoundaryClasses = boundary.ToList(),
                Relationships = relationships
                    .OrderBy(relationship => relationship.Left, StringComparer.Ordinal)
                    .ThenBy(relationship => relationship.Arrow, StringComparer.Ordinal)
                    .ThenBy(relationship => relationship.Right, StringComparer.Ordinal)
                    .ThenBy(relationship => relationship.Label, StringComparer.Ordinal)
                    .ToList(),
            };

            void Reach(string name)
            {
                if (!owned.Contains(name))
                {
                    boundary.Add(name);
                }
            }
        }

        /// <summary>Renders the markdown (front matter, Mermaid block and element links) for a package.</summary>
        public string GenerateDiagram(PackageDiagramPayload payload)
        {
            ArgumentNullException.ThrowIfNull(payload);

            return this.Templates[TemplateName](payload);
        }

        /// <summary>
        /// Renders every package diagram and writes it to <c>{outputDirectory}/diagrams/{Package}.md</c>.
        /// </summary>
        public async Task GenerateAsync(XmiReaderResult model, DirectoryInfo outputDirectory)
        {
            ArgumentNullException.ThrowIfNull(outputDirectory);

            var diagramDirectory = new DirectoryInfo(
                Path.Combine(outputDirectory.FullName, DiagramDirectoryName));

            foreach (var payload in CreatePayloads(model))
            {
                await WriteAsync(this.GenerateDiagram(payload), diagramDirectory, $"{payload.Package}.md");
            }
        }

        /// <summary>
        /// The structural features of a metaclass: owned attributes that are not derived, ordered by
        /// name. Derived features are computed views, not structure, and are left to the element files.
        /// </summary>
        internal static IEnumerable<IProperty> StructuralFeatures(IClass @class) =>
            @class.OwnedAttribute
                .Where(property => !property.IsDerived)
                .OrderBy(property => property.Name, StringComparer.Ordinal);

        /// <summary>
        /// Formats a property's multiplicity, either bare (<c>0..*</c>, for a Mermaid edge label) or
        /// bracketed (<c>[0..*]</c>, for a class-box attribute).
        /// </summary>
        internal static string FormatMultiplicity(IProperty property, bool brackets)
        {
            var lower = property.Lower;
            var upper = ModelQueryExtensions.QueryUpperBound(property.Upper);

            var text = upper == lower
                ? lower.ToString(CultureInfo.InvariantCulture)
                : string.Concat(
                    lower.ToString(CultureInfo.InvariantCulture),
                    "..",
                    upper == -1 ? "*" : upper.ToString(CultureInfo.InvariantCulture));

            return brackets ? $"[{text}]" : text;
        }

        /// <summary>
        /// The non-derived features whose type is a primitive type or an enumeration, rendered inside
        /// the class box rather than as edges.
        /// </summary>
        private static List<DiagramAttribute> BoxAttributes(IClass @class, IReadOnlySet<string> boxTypeNames) =>
            StructuralFeatures(@class)
                .Where(property => property.Type?.Name is not null && boxTypeNames.Contains(property.Type.Name))
                .Select(property => new DiagramAttribute
                {
                    Name = property.Name,
                    Type = property.Type!.Name,
                    Multiplicity = FormatMultiplicity(property, brackets: true),
                })
                .ToList();

        /// <summary>The distinct, ordered names of a metaclass's direct supertypes.</summary>
        private static IEnumerable<string> SupertypeNames(IClass @class) =>
            @class.SuperClass
                .Select(super => super.Name)
                .Where(name => !string.IsNullOrEmpty(name))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(name => name, StringComparer.Ordinal);
    }
}
