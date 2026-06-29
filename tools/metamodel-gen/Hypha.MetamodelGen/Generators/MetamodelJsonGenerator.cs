// ------------------------------------------------------------------------------------------------
// <copyright file="MetamodelJsonGenerator.cs" company="Starion Group S.A.">
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
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.Encodings.Web;
    using System.Text.Json;
    using System.Threading.Tasks;

    using uml4net.Classification;
    using uml4net.Extensions;
    using uml4net.SimpleClassifiers;
    using uml4net.StructuredClassifiers;
    using uml4net.xmi.Readers;

    /// <summary>
    /// Emits the structured JSON form of the metamodel (<c>metamodel.json</c> rich graph +
    /// <c>index.json</c> flat lookup) from the same uml4net object graph that produces the markdown.
    /// Output is deterministic (sorted, fixed serializer options, LF newlines, no timestamps).
    /// </summary>
    public class MetamodelJsonGenerator : Generator
    {
        /// <summary>The version of the JSON shape produced by this generator.</summary>
        public const string SchemaVersion = "1.0.0";

        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        /// <summary>
        /// Builds the deterministic rich-graph document for the model. <paramref name="sourceXmiSha256"/>
        /// is the provenance digest of the input XMI (see <see cref="ComputeSourceXmiSha256"/>).
        /// </summary>
        public static MetamodelDocument BuildDocument(XmiReaderResult model, string sourceXmiSha256)
        {
            ArgumentNullException.ThrowIfNull(model);
            ArgumentNullException.ThrowIfNull(sourceXmiSha256);

            var root = model.Packages.FirstOrDefault(package => package.Name == "SysML")
                ?? model.Packages.FirstOrDefault(package => !string.IsNullOrEmpty(package.Name))
                ?? throw new InvalidOperationException("The model has no named root package.");
            var metaclasses = ElementCatalog.Metaclasses(model);
            var linkable = ElementCatalog.LinkableTypeNames(model);
            var subtypeIndex = MetaclassFileGenerator.BuildSubtypeIndex(metaclasses);

            // Transitive ancestors per class, then inverted into transitive descendants (second pass).
            var ancestors = metaclasses.ToDictionary(
                @class => @class.Name,
                @class => @class.QueryAllGeneralClassifiers()
                    .OfType<IClass>()
                    .Select(ancestor => ancestor.Name)
                    .Where(name => !string.IsNullOrEmpty(name) && !string.Equals(name, @class.Name, StringComparison.Ordinal))
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(name => name, StringComparer.Ordinal)
                    .ToList(),
                StringComparer.Ordinal);

            var descendants = metaclasses.ToDictionary(
                @class => @class.Name,
                _ => new SortedSet<string>(StringComparer.Ordinal),
                StringComparer.Ordinal);

            foreach (var className in metaclasses.Select(@class => @class.Name))
            {
                foreach (var ancestor in ancestors[className])
                {
                    if (descendants.TryGetValue(ancestor, out var set))
                    {
                        set.Add(className);
                    }
                }
            }

            var classes = metaclasses
                .Select(@class => BuildClass(
                    @class, linkable, subtypeIndex, ancestors[@class.Name], descendants[@class.Name]))
                .ToList();

            var enumerations = ElementCatalog.Enumerations(model).Select(BuildEnumeration).ToList();
            var primitiveTypes = ElementCatalog.PrimitiveTypes(model).Select(BuildPrimitiveType).ToList();
            var packages = BuildPackages(model);

            var counts = new MetamodelCounts(
                classes.Count, enumerations.Count, primitiveTypes.Count, packages.Count);

            return new MetamodelDocument(
                SchemaVersion,
                root.Name ?? string.Empty,
                root.URI ?? string.Empty,
                sourceXmiSha256,
                counts,
                packages,
                classes,
                enumerations,
                primitiveTypes);
        }

        /// <summary>Builds the flat name → record lookup from a rich-graph document.</summary>
        public static MetamodelLookup BuildIndex(MetamodelDocument document)
        {
            ArgumentNullException.ThrowIfNull(document);

            var entries = new SortedDictionary<string, LookupEntry>(StringComparer.Ordinal);

            foreach (var @class in document.Classes)
            {
                entries[@class.Name] = new LookupEntry(
                    "class", @class.Package, @class.QualifiedName, @class.IsAbstract, @class.Summary,
                    @class.Extends, $"elements/{@class.Name}.md");
            }

            foreach (var enumeration in document.Enumerations)
            {
                entries[enumeration.Name] = new LookupEntry(
                    "enumeration", enumeration.Package, enumeration.QualifiedName, false, enumeration.Summary,
                    Array.Empty<string>(), $"elements/{enumeration.Name}.md");
            }

            foreach (var primitiveType in document.PrimitiveTypes)
            {
                entries[primitiveType.Name] = new LookupEntry(
                    "primitiveType", primitiveType.Package, primitiveType.QualifiedName, false,
                    primitiveType.Summary, Array.Empty<string>(), $"elements/{primitiveType.Name}.md");
            }

            return new MetamodelLookup(
                SchemaVersion, document.Metamodel, document.ModelVersionUri, entries);
        }

        /// <summary>
        /// Writes <c>metamodel.json</c> and <c>index.json</c> into <paramref name="outputDirectory"/>.
        /// </summary>
        public static async Task GenerateAsync(
            XmiReaderResult model, DirectoryInfo outputDirectory, string sourceXmiSha256)
        {
            ArgumentNullException.ThrowIfNull(model);
            ArgumentNullException.ThrowIfNull(outputDirectory);

            var document = BuildDocument(model, sourceXmiSha256);
            var index = BuildIndex(document);

            await WriteAsync(Serialize(document), outputDirectory, "metamodel.json");
            await WriteAsync(Serialize(index), outputDirectory, "index.json");
        }

        /// <summary>
        /// Serializes a payload deterministically: fixed options, LF newlines (independent of the host
        /// platform), and a trailing newline.
        /// </summary>
        public static string Serialize<T>(T value) =>
            JsonSerializer.Serialize(value, SerializerOptions).Replace("\r\n", "\n") + "\n";

        /// <summary>
        /// Computes the provenance digest over the input XMI files: SHA-256 of each file's
        /// newline-normalized UTF-8 content (so the digest is independent of checkout line endings),
        /// combined in filename order into a single stable hex string.
        /// </summary>
        public static string ComputeSourceXmiSha256(IEnumerable<string> filePaths)
        {
            ArgumentNullException.ThrowIfNull(filePaths);

            var builder = new StringBuilder();

            foreach (var path in filePaths.OrderBy(Path.GetFileName, StringComparer.Ordinal))
            {
                var normalized = File.ReadAllText(path).Replace("\r\n", "\n");
                var hex = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(normalized)))
                    .ToLowerInvariant();
                builder.Append(Path.GetFileName(path)).Append(':').Append(hex).Append('\n');
            }

            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(builder.ToString())))
                .ToLowerInvariant();
        }

        private static ClassNode BuildClass(
            IClass @class,
            IReadOnlySet<string> linkable,
            IReadOnlyDictionary<string, IReadOnlyList<string>> subtypeIndex,
            IReadOnlyList<string> allAncestors,
            SortedSet<string> allDescendants)
        {
            var extends = @class.SuperClass
                .Select(super => super.Name)
                .Where(name => !string.IsNullOrEmpty(name))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToList();

            var directSubclasses = subtypeIndex.TryGetValue(@class.Name, out var subtypes)
                ? subtypes.ToList()
                : new List<string>();

            var ownedNames = @class.OwnedAttribute.Select(property => property.Name).ToHashSet(StringComparer.Ordinal);

            var ownedAttributes = @class.OwnedAttribute
                .OrderBy(property => property.Name, StringComparer.Ordinal)
                .Select(property => BuildAttribute(property, linkable))
                .ToList();

            // The XMI reader does not populate IProperty.Class, so attribute each inherited feature to
            // its declaring ancestor by walking the supertype classifiers (mirrors the markdown generator).
            var inheritedAttributes = @class.QueryAllGeneralClassifiers()
                .OfType<IClass>()
                .OrderBy(ancestor => ancestor.Name, StringComparer.Ordinal)
                .SelectMany(ancestor => ancestor.OwnedAttribute.Select(property => (ancestor, property)))
                .Where(entry => !ownedNames.Contains(entry.property.Name))
                .GroupBy(entry => entry.property.Name, StringComparer.Ordinal)
                .Select(group => group.First())
                .OrderBy(entry => entry.property.Name, StringComparer.Ordinal)
                .Select(entry => BuildInheritedAttribute(entry.property, entry.ancestor.Name, linkable))
                .ToList();

            var ownedOperations = @class.OwnedOperation
                .OrderBy(operation => operation.Name, StringComparer.Ordinal)
                .Select(BuildOperation)
                .ToList();

            var constraints = @class.OwnedRule
                .OrderBy(rule => rule.Name, StringComparer.Ordinal)
                .Select(rule => new ConstraintNode(
                    rule.Name, rule.QueryDocumentationText(), rule.QueryConstraintBody()))
                .ToList();

            return new ClassNode(
                @class.Name,
                @class.XmiId ?? string.Empty,
                @class.Namespace?.Name ?? string.Empty,
                ElementCatalog.FullyQualifiedName(@class),
                @class.IsAbstract,
                ElementCatalog.VisibilityName(@class.Visibility),
                @class.QueryDocumentationText(),
                @class.QuerySummary(),
                extends,
                allAncestors,
                directSubclasses,
                allDescendants.ToList(),
                ownedAttributes,
                inheritedAttributes,
                ownedOperations,
                constraints);
        }

        private static AttributeNode BuildAttribute(IProperty property, IReadOnlySet<string> linkable)
        {
            var typeName = property.Type?.Name;

            return new AttributeNode(
                property.Name,
                typeName,
                typeName is not null && linkable.Contains(typeName),
                ElementCatalog.VisibilityName(property.Visibility),
                property.Lower,
                ModelQueryExtensions.QueryUpperBound(property.Upper),
                property.IsDerived,
                property.IsComposite,
                property.IsOrdered,
                property.RedefinedProperty.Select(redefined => redefined.Name).ToList(),
                property.SubsettedProperty.Select(subsetted => subsetted.Name).ToList(),
                property.QueryDocumentationText());
        }

        private static InheritedAttributeNode BuildInheritedAttribute(
            IProperty property, string inheritedFrom, IReadOnlySet<string> linkable)
        {
            var typeName = property.Type?.Name;

            return new InheritedAttributeNode(
                property.Name,
                typeName,
                typeName is not null && linkable.Contains(typeName),
                ElementCatalog.VisibilityName(property.Visibility),
                property.Lower,
                ModelQueryExtensions.QueryUpperBound(property.Upper),
                property.IsDerived,
                property.IsComposite,
                property.IsOrdered,
                property.RedefinedProperty.Select(redefined => redefined.Name).ToList(),
                property.SubsettedProperty.Select(subsetted => subsetted.Name).ToList(),
                property.QueryDocumentationText(),
                inheritedFrom);
        }

        private static OperationNode BuildOperation(IOperation operation)
        {
            // The operation's return type/multiplicity are derived (and unimplemented in uml4net); the
            // stored values live on the direction=return parameter.
            var returnParameter = operation.OwnedParameter
                .FirstOrDefault(parameter => parameter.Direction == ParameterDirectionKind.Return);

            var parameters = operation.OwnedParameter
                .Where(parameter => parameter.Direction != ParameterDirectionKind.Return)
                .Select(parameter => new OperationParameterNode(
                    parameter.Name,
                    parameter.Type?.Name,
                    parameter.Direction.ToString().ToLowerInvariant(),
                    parameter.Lower,
                    ModelQueryExtensions.QueryUpperBound(parameter.Upper)))
                .ToList();

            return new OperationNode(
                operation.Name,
                returnParameter?.Type?.Name,
                returnParameter?.Lower ?? 0,
                returnParameter is null ? 1 : ModelQueryExtensions.QueryUpperBound(returnParameter.Upper),
                operation.IsQuery,
                parameters);
        }

        private static EnumerationNode BuildEnumeration(IEnumeration enumeration)
        {
            var literals = enumeration.OwnedLiteral
                .Select(literal => new EnumerationLiteralNode(literal.Name, literal.QueryDocumentationText()))
                .ToList();

            return new EnumerationNode(
                enumeration.Name,
                enumeration.XmiId ?? string.Empty,
                enumeration.Namespace?.Name ?? string.Empty,
                ElementCatalog.FullyQualifiedName(enumeration),
                ElementCatalog.VisibilityName(enumeration.Visibility),
                enumeration.QueryDocumentationText(),
                enumeration.QuerySummary(),
                literals);
        }

        private static PrimitiveTypeNode BuildPrimitiveType(IPrimitiveType primitiveType) =>
            new(
                primitiveType.Name,
                primitiveType.XmiId ?? string.Empty,
                primitiveType.Namespace?.Name ?? string.Empty,
                ElementCatalog.FullyQualifiedName(primitiveType),
                ElementCatalog.VisibilityName(primitiveType.Visibility),
                primitiveType.QueryDocumentationText(),
                primitiveType.QuerySummary());

        private static List<PackageNode> BuildPackages(XmiReaderResult model) =>
            model.Packages
                .SelectMany(ElementCatalog.ExpandPackages)
                .DistinctBy(package => package.XmiId)
                .Where(package => !string.IsNullOrEmpty(package.Name))
                .OrderBy(package => ElementCatalog.FullyQualifiedName(package), StringComparer.Ordinal)
                .Select(package => new PackageNode(
                    package.Name,
                    ElementCatalog.FullyQualifiedName(package),
                    package.Namespace?.Name,
                    package.PackagedElement
                        .OfType<IClass>()
                        .Select(@class => @class.Name)
                        .OrderBy(name => name, StringComparer.Ordinal)
                        .ToList()))
                .ToList();
    }
}
