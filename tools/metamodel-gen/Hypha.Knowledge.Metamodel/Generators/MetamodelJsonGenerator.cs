// ------------------------------------------------------------------------------------------------
// <copyright file="MetamodelJsonGenerator.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Metamodel.Generators
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.Encodings.Web;
    using System.Text.Json;
    using System.Text.Json.Serialization.Metadata;
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
        public const string SchemaVersion = "2.0.0";

        /// <summary>
        /// The fixed serializer options: indented, camel-cased property names and relaxed escaping, so
        /// the documentation text stays readable.
        /// </summary>
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            TypeInfoResolver = new MetamodelJsonContext(),
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
            var owners = ModelOwners.Build(model);

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
                    @class, linkable, subtypeIndex, ancestors[@class.Name], descendants[@class.Name], owners))
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
                        .Select(operation => BuildOperation(operation, owners))
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
        /// <exception cref="NotSupportedException">
        /// <typeparamref name="T"/> is not one of the documents <see cref="MetamodelJsonContext"/>
        /// declares.
        /// </exception>
        public static string Serialize<T>(T value) =>
            JsonSerializer.Serialize(value, (JsonTypeInfo<T>)SerializerOptions.GetTypeInfo(typeof(T)))
                .Replace("\r\n", "\n") + "\n";

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
        /// <param name="owners">The owner of every attribute, association end and operation of the model.</param>
        /// <returns>The node.</returns>
        private static ClassNode BuildClass(
            IClass @class,
            IReadOnlySet<string> linkable,
            IReadOnlyDictionary<string, IReadOnlyList<string>> subtypeIndex,
            IReadOnlyList<string> allAncestors,
            SortedSet<string> allDescendants,
            ModelOwners owners)
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

            // An inherited operation or constraint is written as a reference by name, so a name must be
            // unique among what its metaclass owns.
            RequireUniqueNames(@class, "operation", @class.OwnedOperation.Select(operation => operation.Name));
            RequireUniqueNames(@class, "constraint", @class.OwnedRule.Select(rule => rule.Name));

            var ownedNames = @class.OwnedAttribute.Select(property => property.Name).ToHashSet(StringComparer.Ordinal);

            var ownedAttributes = @class.OwnedAttribute
                .OrderBy(property => property.Name, StringComparer.Ordinal)
                .Select(property => BuildAttribute(property, linkable, owners))
                .ToList();

            // The supertypes in name order, so a member two of them declare is attributed to the same one
            // on every run (mirrors the markdown generator).
            var supertypes = @class.QueryAllGeneralClassifiers()
                .OfType<IClass>()
                .Where(ancestor => !ReferenceEquals(ancestor, @class))
                .OrderBy(ancestor => ancestor.Name, StringComparer.Ordinal)
                .ToList();

            var inheritedAttributes = supertypes
                .SelectMany(ancestor => ancestor.OwnedAttribute.Select(property => (ancestor, property)))
                .Where(entry => !ownedNames.Contains(entry.property.Name))
                .GroupBy(entry => entry.property.Name, StringComparer.Ordinal)
                .Select(group => group.First())
                .OrderBy(entry => entry.property.Name, StringComparer.Ordinal)
                .Select(entry => BuildInheritedAttribute(entry.property, entry.ancestor.Name, @class, linkable, owners))
                .ToList();

            var ownedOperations = @class.OwnedOperation
                .OrderBy(operation => operation.Name, StringComparer.Ordinal)
                .Select(operation => BuildOperation(operation, owners))
                .ToList();

            var inheritedOperations = supertypes
                .SelectMany(ancestor => ancestor.OwnedOperation.Select(operation => (ancestor, operation)))
                .DistinctBy(entry => entry.operation)
                .OrderBy(entry => entry.operation.Name, StringComparer.Ordinal)
                .ThenBy(entry => entry.ancestor.Name, StringComparer.Ordinal)
                .Select(entry => new InheritedOperationNode(
                    entry.ancestor.Name,
                    entry.operation.Name,
                    entry.operation.TryQueryRedefinedByOperation(@class, out var redefinedBy)
                        ? owners.Operation(redefinedBy)
                        : null))
                .ToList();

            var constraints = @class.OwnedRule
                .OrderBy(rule => rule.Name, StringComparer.Ordinal)
                .Select(rule => new ConstraintNode(
                    rule.Name,
                    rule.QueryDocumentationText(),
                    rule.QueryConstraintBody(),
                    rule.ConstrainedElement.Select(owners.Element).ToList()))
                .ToList();

            var inheritedConstraints = supertypes
                .SelectMany(ancestor => ancestor.OwnedRule.Select(rule => (ancestor, rule)))
                .DistinctBy(entry => entry.rule)
                .OrderBy(entry => entry.rule.Name, StringComparer.Ordinal)
                .ThenBy(entry => entry.ancestor.Name, StringComparer.Ordinal)
                .Select(entry => new InheritedConstraintNode(entry.ancestor.Name, entry.rule.Name))
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
                inheritedOperations,
                constraints,
                inheritedConstraints);
        }

        /// <summary>Builds the node for one owned attribute.</summary>
        /// <param name="property">The attribute the node describes.</param>
        /// <param name="linkable">The type names that have an element of their own in the document.</param>
        /// <param name="owners">The owner of every attribute, association end and operation of the model.</param>
        /// <returns>The node.</returns>
        private static AttributeNode BuildAttribute(IProperty property, IReadOnlySet<string> linkable, ModelOwners owners)
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
                property.IsDerivedUnion,
                property.IsComposite,
                property.IsOrdered,
                property.IsUnique,
                property.IsReadOnly,
                DefaultValue(property),
                property.RedefinedProperty.Select(owners.Feature).ToList(),
                property.SubsettedProperty.Select(owners.Feature).ToList(),
                Opposite(property, owners),
                property.QueryDocumentationText());
        }

        /// <summary>Builds the node for one inherited attribute.</summary>
        /// <param name="property">The attribute the node describes.</param>
        /// <param name="inheritedFrom">The name of the supertype that declares it.</param>
        /// <param name="context">The metaclass that inherits it, in whose context a redefinition is looked for.</param>
        /// <param name="linkable">The type names that have an element of their own in the document.</param>
        /// <param name="owners">The owner of every attribute, association end and operation of the model.</param>
        /// <returns>The node.</returns>
        private static InheritedAttributeNode BuildInheritedAttribute(
            IProperty property, string inheritedFrom, IClass context, IReadOnlySet<string> linkable, ModelOwners owners)
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
                property.IsDerivedUnion,
                property.IsComposite,
                property.IsOrdered,
                property.IsUnique,
                property.IsReadOnly,
                DefaultValue(property),
                property.RedefinedProperty.Select(owners.Feature).ToList(),
                property.SubsettedProperty.Select(owners.Feature).ToList(),
                Opposite(property, owners),
                property.QueryDocumentationText(),
                inheritedFrom,
                property.TryQueryRedefinedByProperty(context, out var redefinedBy) ? owners.Feature(redefinedBy) : null);
        }

        /// <summary>The default value of an attribute as the model writes it.</summary>
        /// <param name="property">The attribute.</param>
        /// <returns>The default value, or <c>null</c> when the attribute has none.</returns>
        private static string? DefaultValue(IProperty property) =>
            property.QueryHasDefaultValue() ? property.QueryDefaultValueAsString() : null;

        /// <summary>The other end of the binary association an attribute is an end of.</summary>
        /// <param name="property">The attribute.</param>
        /// <param name="owners">The owner of every attribute, association end and operation of the model.</param>
        /// <returns>
        /// The reference to the other end; <c>null</c> when the attribute is not an end of a binary
        /// association, or when the other end cannot be named because it, or the association that owns
        /// it, has no name (as in the Systems Modeling API PIM).
        /// </returns>
        private static FeatureReference? Opposite(IProperty property, ModelOwners owners) =>
            property.Opposite is { } opposite ? owners.TryFeature(opposite) : null;

        /// <summary>
        /// Builds the node for one operation, its return read from the <c>return</c> parameter.
        /// </summary>
        /// <param name="operation">The operation the node describes.</param>
        /// <param name="owners">The owner of every attribute, association end and operation of the model.</param>
        /// <returns>The node; without a <c>return</c> parameter its return multiplicity is <c>0..1</c>.</returns>
        private static OperationNode BuildOperation(IOperation operation, ModelOwners owners)
        {
            var (returnType, returnLower, returnUpper, parameters) = Signature(operation);

            return new OperationNode(
                operation.Name,
                returnType,
                returnLower,
                returnUpper,
                operation.IsQuery,
                parameters,
                operation.RedefinedOperation.Select(owners.Operation).ToList(),
                operation.QueryDocumentationText(),
                operation.QueryBodyConditionText());
        }

        /// <summary>Refuses a metaclass that owns two members of one kind with the same name.</summary>
        /// <param name="class">The metaclass.</param>
        /// <param name="kind">The kind of member, for the message.</param>
        /// <param name="names">The names of the members.</param>
        /// <exception cref="InvalidOperationException">Two of the members share a name.</exception>
        private static void RequireUniqueNames(IClass @class, string kind, IEnumerable<string> names)
        {
            var duplicate = names
                .GroupBy(name => name, StringComparer.Ordinal)
                .FirstOrDefault(group => group.Count() > 1);

            if (duplicate is not null)
            {
                throw new InvalidOperationException(
                    $"The metaclass '{@class.Name}' owns more than one {kind} named '{duplicate.Key}', so an inherited "
                    + $"{kind} cannot be written as a reference by name.");
            }
        }

        /// <summary>The return and the non-return parameters of an operation.</summary>
        /// <param name="operation">The operation.</param>
        /// <returns>
        /// The return type and multiplicity, read from the <c>return</c> parameter (<c>0..1</c> without
        /// one), and the other parameters in declaration order.
        /// </returns>
        private static (string? Type, int Lower, int Upper, List<OperationParameterNode> Parameters) Signature(
            IOperation operation)
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

            return (
                returnParameter?.Type?.Name,
                returnParameter?.Lower ?? 0,
                returnParameter is null ? 1 : ModelQueryExtensions.QueryUpperBound(returnParameter.Upper),
                parameters);
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
