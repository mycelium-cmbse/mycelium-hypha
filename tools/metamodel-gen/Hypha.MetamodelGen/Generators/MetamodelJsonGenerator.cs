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

        /// <summary>
        /// The fixed serializer options: indented, camel-cased property names and relaxed escaping, so
        /// the documentation text stays readable.
        /// </summary>
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
        /// <param name="model">The model the document describes.</param>
        /// <param name="sourceXmiSha256">The provenance digest of the input XMI.</param>
        /// <returns>The document, every collection in it ordered ordinally.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="model"/> or <paramref name="sourceXmiSha256"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidOperationException">The model has no named root package.</exception>
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

            var interfaces = ElementCatalog.Interfaces(model)
                .Select(@interface => new InterfaceNode(
                    @interface.Name,
                    @interface.XmiId ?? string.Empty,
                    @interface.Namespace?.Name ?? string.Empty,
                    ElementCatalog.FullyQualifiedName(@interface),
                    ElementCatalog.VisibilityName(@interface.Visibility),
                    @interface.QueryDocumentationText(),
                    @interface.QuerySummary(),
                    ElementCatalog.RealizedBy(model, @interface),
                    @interface.OwnedOperation
                        .OrderBy(operation => operation.Name, StringComparer.Ordinal)
                        .Select(BuildOperation)
                        .ToList()))
                .ToList();

            var dataTypes = ElementCatalog.DataTypes(model)
                .Select(dataType => new PrimitiveTypeNode(
                    dataType.Name,
                    dataType.XmiId ?? string.Empty,
                    dataType.Namespace?.Name ?? string.Empty,
                    ElementCatalog.FullyQualifiedName(dataType),
                    ElementCatalog.VisibilityName(dataType.Visibility),
                    dataType.QueryDocumentationText(),
                    dataType.QuerySummary()))
                .ToList();

            return new MetamodelDocument(
                SchemaVersion,
                root.Name ?? string.Empty,
                root.URI ?? string.Empty,
                sourceXmiSha256,
                counts,
                packages,
                classes,
                enumerations,
                primitiveTypes)
            {
                Interfaces = interfaces.Count > 0 ? interfaces : null,
                DataTypes = dataTypes.Count > 0 ? dataTypes : null,
            };
        }

        /// <summary>Builds the flat name → record lookup from a rich-graph document.</summary>
        /// <param name="document">The rich-graph document to index.</param>
        /// <returns>The lookup, its entries ordered ordinally by name.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="document"/> is <c>null</c>.</exception>
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

            foreach (var @interface in document.Interfaces ?? [])
            {
                entries[@interface.Name] = new LookupEntry(
                    "interface", @interface.Package, @interface.QualifiedName, false, @interface.Summary,
                    Array.Empty<string>(), $"elements/{@interface.Name}.md");
            }

            foreach (var dataType in document.DataTypes ?? [])
            {
                entries[dataType.Name] = new LookupEntry(
                    "dataType", dataType.Package, dataType.QualifiedName, false, dataType.Summary,
                    Array.Empty<string>(), $"elements/{dataType.Name}.md");
            }

            return new MetamodelLookup(
                SchemaVersion, document.Metamodel, document.ModelVersionUri, entries);
        }

        /// <summary>
        /// Writes <c>metamodel.json</c> and <c>index.json</c> into <paramref name="outputDirectory"/>.
        /// </summary>
        /// <param name="model">The model the files describe.</param>
        /// <param name="outputDirectory">The folder the files are written to.</param>
        /// <param name="sourceXmiSha256">The provenance digest of the input XMI.</param>
        /// <returns>A task that completes when both files have been written.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="model"/>, <paramref name="outputDirectory"/> or
        /// <paramref name="sourceXmiSha256"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidOperationException">The model has no named root package.</exception>
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
        /// <typeparam name="T">The type of the payload.</typeparam>
        /// <param name="value">The payload to serialize.</param>
        /// <returns>The JSON text.</returns>
        public static string Serialize<T>(T value) =>
            JsonSerializer.Serialize(value, SerializerOptions).Replace("\r\n", "\n") + "\n";

        /// <summary>
        /// Computes the provenance digest over the input XMI files: SHA-256 of each file's
        /// newline-normalized UTF-8 content (so the digest is independent of checkout line endings),
        /// combined in filename order into a single stable hex string.
        /// </summary>
        /// <param name="filePaths">The XMI files the model was read from.</param>
        /// <returns>The digest, as lowercase hexadecimal.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="filePaths"/> is <c>null</c>.</exception>
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

        /// <summary>
        /// Builds the node for one metaclass: its supertypes, closures, owned and inherited attributes,
        /// operations and constraints.
        /// </summary>
        /// <param name="class">The metaclass the node describes.</param>
        /// <param name="linkable">The type names that have an element of their own in the document.</param>
        /// <param name="subtypeIndex">Each metaclass name mapped to its direct subtype names.</param>
        /// <param name="allAncestors">The metaclass' transitive supertypes, ordered.</param>
        /// <param name="allDescendants">The metaclass' transitive subtypes.</param>
        /// <returns>The node.</returns>
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

        /// <summary>Builds the node for one owned attribute.</summary>
        /// <param name="property">The attribute the node describes.</param>
        /// <param name="linkable">The type names that have an element of their own in the document.</param>
        /// <returns>The node.</returns>
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

        /// <summary>Builds the node for one inherited attribute.</summary>
        /// <param name="property">The attribute the node describes.</param>
        /// <param name="inheritedFrom">The name of the supertype that declares it.</param>
        /// <param name="linkable">The type names that have an element of their own in the document.</param>
        /// <returns>The node.</returns>
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

        /// <summary>
        /// Builds the node for one operation, its return read from the <c>return</c> parameter.
        /// </summary>
        /// <param name="operation">The operation the node describes.</param>
        /// <returns>The node; without a <c>return</c> parameter its return multiplicity is <c>0..1</c>.</returns>
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
                parameters,
                operation.QueryDocumentationText(),
                operation.QueryBodyConditionText());
        }

        /// <summary>Builds the node for one enumeration, its literals in declaration order.</summary>
        /// <param name="enumeration">The enumeration the node describes.</param>
        /// <returns>The node.</returns>
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

        /// <summary>Builds the node for one primitive type.</summary>
        /// <param name="primitiveType">The primitive type the node describes.</param>
        /// <returns>The node.</returns>
        private static PrimitiveTypeNode BuildPrimitiveType(IPrimitiveType primitiveType) =>
            new(
                primitiveType.Name,
                primitiveType.XmiId ?? string.Empty,
                primitiveType.Namespace?.Name ?? string.Empty,
                ElementCatalog.FullyQualifiedName(primitiveType),
                ElementCatalog.VisibilityName(primitiveType.Visibility),
                primitiveType.QueryDocumentationText(),
                primitiveType.QuerySummary());

        /// <summary>Builds a node for every named package of the model, nested ones included.</summary>
        /// <param name="model">The model whose packages are described.</param>
        /// <returns>The nodes, ordered ordinally by qualified name.</returns>
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
