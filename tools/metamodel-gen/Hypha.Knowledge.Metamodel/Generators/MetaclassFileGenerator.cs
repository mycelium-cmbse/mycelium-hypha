// ------------------------------------------------------------------------------------------------
// <copyright file="MetaclassFileGenerator.cs" company="Starion Group S.A.">
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
    using System.Threading.Tasks;

    using uml4net.Classification;
    using uml4net.CommonStructure;
    using uml4net.Extensions;
    using uml4net.StructuredClassifiers;
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
        /// Gets the words the pages use for the model they describe. A metamodel unless said otherwise.
        /// </summary>
        public ModelVocabulary Vocabulary { get; init; } = ModelVocabulary.Metamodel;

        /// <summary>
        /// Renders the element markdown for a single metaclass. The <paramref name="subtypeIndex"/>
        /// (see <see cref="BuildSubtypeIndex"/>) supplies the direct subtypes, which cannot be
        /// derived from the metaclass alone. The <paramref name="linkableTypeNames"/> (see
        /// <see cref="ElementCatalog.LinkableTypeNames"/>) are the elements that have a generated
        /// element file, so feature types pointing at them can be rendered as links.
        /// </summary>
        /// <param name="class">The metaclass to render.</param>
        /// <param name="subtypeIndex">Each metaclass name mapped to its direct subtype names.</param>
        /// <param name="linkableTypeNames">The type names that have a page of their own to link to.</param>
        /// <returns>The page's Markdown.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="class"/>, <paramref name="subtypeIndex"/> or
        /// <paramref name="linkableTypeNames"/> is <c>null</c>.
        /// </exception>
        public string GenerateElement(
            IClass @class,
            IReadOnlyDictionary<string, IReadOnlyList<string>> subtypeIndex,
            IReadOnlySet<string> linkableTypeNames,
            ModelOwners owners)
        {
            ArgumentNullException.ThrowIfNull(@class);
            ArgumentNullException.ThrowIfNull(subtypeIndex);
            ArgumentNullException.ThrowIfNull(linkableTypeNames);
            ArgumentNullException.ThrowIfNull(owners);

            var payload = CreatePayload(@class, subtypeIndex, linkableTypeNames, owners);
            payload.Vocabulary = this.Vocabulary;

            return this.Templates[TemplateName](payload);
        }

        /// <summary>
        /// Renders one <c>&lt;MetaclassName&gt;.md</c> file per metaclass into
        /// <paramref name="outputDirectory"/>.
        /// </summary>
        /// <param name="model">The model whose metaclasses are written.</param>
        /// <param name="outputDirectory">The folder the pages are written to.</param>
        /// <returns>A task that completes when every page has been written.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="model"/> or <paramref name="outputDirectory"/> is <c>null</c>.
        /// </exception>
        public async Task GenerateAsync(XmiReaderResult model, DirectoryInfo outputDirectory)
        {
            ArgumentNullException.ThrowIfNull(model);
            ArgumentNullException.ThrowIfNull(outputDirectory);

            var metaclasses = QueryMetaclasses(model);
            var subtypeIndex = BuildSubtypeIndex(metaclasses);
            var linkableTypeNames = ElementCatalog.LinkableTypeNames(model);
            var owners = ModelOwners.Build(model);

            foreach (var @class in metaclasses)
            {
                var content = this.GenerateElement(@class, subtypeIndex, linkableTypeNames, owners);

                await WriteAsync(content, outputDirectory, $"{@class.Name}.md");
            }
        }

        /// <summary>
        /// Builds a map from each metaclass name to its direct subtype names (the inverse of the
        /// generalization relation), ordered.
        /// </summary>
        /// <param name="metaclasses">The metaclasses whose generalizations are inverted.</param>
        /// <returns>
        /// Each supertype name mapped to its distinct direct subtype names, ordered ordinally; a
        /// metaclass without subtypes has no entry.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="metaclasses"/> is <c>null</c>.</exception>
        public static IReadOnlyDictionary<string, IReadOnlyList<string>> BuildSubtypeIndex(
            IEnumerable<IClass> metaclasses)
        {
            ArgumentNullException.ThrowIfNull(metaclasses);

            var map = new Dictionary<string, List<string>>(StringComparer.Ordinal);

            foreach (var @class in metaclasses)
            {
                foreach (var superName in @class.SuperClass.Select(super => super.Name))
                {
                    if (string.IsNullOrEmpty(superName))
                    {
                        continue;
                    }

                    if (!map.TryGetValue(superName, out var subtypes))
                    {
                        subtypes = new List<string>();
                        map[superName] = subtypes;
                    }

                    subtypes.Add(@class.Name);
                }
            }

            return map.ToDictionary(
                entry => entry.Key,
                entry => (IReadOnlyList<string>)entry.Value
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(name => name, StringComparer.Ordinal)
                    .ToList(),
                StringComparer.Ordinal);
        }

        /// <summary>
        /// Returns all metaclasses (<see cref="IClass"/>) in the model, ordered by name.
        /// </summary>
        /// <param name="model">The model to read.</param>
        /// <returns>The metaclasses, ordered ordinally by name.</returns>
        public static IReadOnlyList<IClass> QueryMetaclasses(XmiReaderResult model) =>
            ElementCatalog.Metaclasses(model);

        /// <summary>
        /// Builds the deterministic payload for a metaclass from the model.
        /// </summary>
        /// <param name="class">The metaclass the payload describes.</param>
        /// <param name="subtypeIndex">Each metaclass name mapped to its direct subtype names.</param>
        /// <param name="linkableTypeNames">The type names that have a page of their own to link to.</param>
        /// <param name="owners">The owner of every attribute, association end and operation of the model.</param>
        /// <returns>The payload, every collection in it ordered by name.</returns>
        private static MetaclassPayload CreatePayload(
            IClass @class,
            IReadOnlyDictionary<string, IReadOnlyList<string>> subtypeIndex,
            IReadOnlySet<string> linkableTypeNames,
            ModelOwners owners)
        {
            var generalizations = @class.SuperClass
                .Select(super => super.Name)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToList();

            var specializations = subtypeIndex.TryGetValue(@class.Name, out var subtypes)
                ? subtypes.ToList()
                : new List<string>();

            var ownedNames = @class.OwnedAttribute
                .Select(property => property.Name)
                .ToHashSet(StringComparer.Ordinal);

            // The XMI reader does not populate the IProperty.Class back-reference, so attribute each
            // inherited feature to its declaring ancestor by walking the supertype classifiers.
            var inheritedFeatures = @class.QueryAllGeneralClassifiers()
                .OfType<IClass>()
                .OrderBy(ancestor => ancestor.Name, StringComparer.Ordinal)
                .SelectMany(ancestor => ancestor.OwnedAttribute
                    .Select(property => new InheritedFeature(
                        property.Name,
                        RenderTypeLink(property.Type?.Name ?? "«untyped»", linkableTypeNames),
                        property.QueryFormattedMultiplicity(),
                        ancestor.Name,
                        string.Join(", ", QueryModifierTokens(property)))))
                .Where(feature => !ownedNames.Contains(feature.Name))
                .GroupBy(feature => feature.Name, StringComparer.Ordinal)
                .Select(group => group.First())
                .OrderBy(feature => feature.Name, StringComparer.Ordinal)
                .ToList();

            var features = @class.OwnedAttribute
                .OrderBy(property => property.Name, StringComparer.Ordinal)
                .Select(property => new MetaclassFeature
                {
                    Name = property.Name,
                    Visibility = QueryVisibilitySigil(property.Visibility),
                    TypeMarkdown = RenderTypeLink(property.Type?.Name ?? "«untyped»", linkableTypeNames),
                    Multiplicity = property.QueryFormattedMultiplicity(),
                    Modifiers = string.Join(", ", QueryModifierTokens(property)),
                    Documentation = property.QueryDocumentationText(),
                    Redefines = property.RedefinedProperty.Select(redefined => RenderFeatureReference(redefined, @class, owners, linkableTypeNames)).ToList(),
                    Subsets = property.SubsettedProperty.Select(subsetted => RenderFeatureReference(subsetted, @class, owners, linkableTypeNames)).ToList(),
                })
                .ToList();

            var operations = @class.OwnedOperation
                .OrderBy(operation => operation.Name, StringComparer.Ordinal)
                .Select(operation => new MetaclassOperation(
                    operation.Name,
                    QueryOperationSignature(operation),
                    operation.IsQuery,
                    operation.QueryDocumentationText(),
                    operation.QueryBodyConditionText()))
                .ToList();

            var constraints = @class.OwnedRule
                .OrderBy(rule => rule.Name, StringComparer.Ordinal)
                .Select(rule => new MetaclassConstraint(
                    rule.Name,
                    rule.QueryDocumentationText(),
                    rule.QueryConstraintBody()))
                .ToList();

            return new MetaclassPayload
            {
                Name = @class.Name,
                Package = @class.Namespace?.Name ?? string.Empty,
                FullyQualifiedName = ElementCatalog.FullyQualifiedName(@class),
                IsAbstract = @class.IsAbstract,
                Visibility = ElementCatalog.VisibilityName(@class.Visibility),
                Documentation = @class.QueryDocumentationText(),
                Generalizations = generalizations,
                Specializations = specializations,
                Features = features,
                InheritedFeatures = inheritedFeatures,
                Operations = operations,
                Constraints = constraints,
            };
        }

        /// <summary>
        /// Returns the UML-style signature of an operation, e.g.
        /// <c>namesOf(element : Element [1..1]) : String [0..*]</c>. A parameter that is not
        /// <c>in</c> is prefixed with its direction; the return comes from the <c>return</c>
        /// parameter, and is left out when there is none.
        /// </summary>
        /// <param name="operation">The operation whose signature is rendered.</param>
        /// <returns>The signature.</returns>
        internal static string QueryOperationSignature(IOperation operation)
        {
            var parameters = operation.OwnedParameter
                .Where(parameter => parameter.Direction != ParameterDirectionKind.Return)
                .Select(parameter =>
                {
                    var direction = parameter.Direction == ParameterDirectionKind.In
                        ? string.Empty
                        : $"{parameter.Direction.ToString().ToLowerInvariant()} ";

                    return $"{direction}{parameter.Name} : {parameter.Type?.Name ?? "«untyped»"} {parameter.QueryFormattedMultiplicity()}";
                });

            var returnParameter = operation.OwnedParameter
                .FirstOrDefault(parameter => parameter.Direction == ParameterDirectionKind.Return);

            var returns = returnParameter is null
                ? string.Empty
                : $" : {returnParameter.Type?.Name ?? "«untyped»"} {returnParameter.QueryFormattedMultiplicity()}";

            return $"{operation.Name}({string.Join(", ", parameters)}){returns}";
        }

        /// <summary>
        /// Returns the modifier tokens (<c>derived</c>, <c>composite</c>, <c>ordered</c>) that apply
        /// to a property, in a stable order.
        /// </summary>
        /// <param name="property">The property whose modifiers are read.</param>
        /// <returns>The tokens that apply, possibly none.</returns>
        private static IEnumerable<string> QueryModifierTokens(IProperty property)
        {
            if (property.IsDerived)
            {
                yield return "derived";
            }

            if (property.IsComposite)
            {
                yield return "composite";
            }

            if (property.IsOrdered)
            {
                yield return "ordered";
            }
        }

        /// <summary>
        /// Renders a feature type as markdown: a link to its element file when the type resolves to a
        /// generated metaclass, otherwise the plain type name (e.g. a primitive, enumeration or the
        /// <c>«untyped»</c> placeholder).
        /// </summary>
        /// <param name="typeName">The name of the feature's type.</param>
        /// <param name="linkableTypeNames">The type names that have a page of their own to link to.</param>
        /// <returns>The type as Markdown.</returns>
        private static string RenderTypeLink(string typeName, IReadOnlySet<string> linkableTypeNames)
        {
            return linkableTypeNames.Contains(typeName) ? $"[{typeName}]({typeName}.md)" : typeName;
        }

        /// <summary>
        /// Renders a redefined/subsetted feature reference as a markdown link to the feature uml4net says
        /// it is: a same-file anchor when this metaclass owns it, the owner's element file when another
        /// metaclass with a page does, otherwise plain backtick text (an association end, for example).
        /// </summary>
        /// <param name="feature">The redefined or subsetted feature.</param>
        /// <param name="class">The metaclass whose page is rendered.</param>
        /// <param name="owners">The owner of every attribute, association end and operation of the model.</param>
        /// <param name="linkableTypeNames">The type names that have a page of their own to link to.</param>
        /// <returns>The reference as Markdown.</returns>
        private static string RenderFeatureReference(
            IProperty feature,
            IClass @class,
            ModelOwners owners,
            IReadOnlySet<string> linkableTypeNames)
        {
            var owner = owners.TryFeature(feature)?.Class;

            if (string.Equals(owner, @class.Name, StringComparison.Ordinal))
            {
                return $"[{feature.Name}](#{Slug(feature.Name)})";
            }

            if (owner is not null && linkableTypeNames.Contains(owner))
            {
                return $"[{feature.Name}]({owner}.md#{Slug(feature.Name)})";
            }

            return $"`{feature.Name}`";
        }

        /// <summary>
        /// Returns the GitHub-style heading anchor for a feature name (identifiers only need lowercasing).
        /// </summary>
        /// <param name="name">The feature name.</param>
        /// <returns>The anchor, without the leading <c>#</c>.</returns>
        private static string Slug(string name) => name.ToLowerInvariant();

        /// <summary>
        /// Returns the UML visibility sigil (<c>+</c> public, <c>-</c> private, <c>#</c> protected,
        /// <c>~</c> package); an unset visibility reads as public.
        /// </summary>
        /// <param name="visibility">The visibility the XMI states, or <c>null</c> when it is omitted.</param>
        /// <returns>The sigil.</returns>
        private static string QueryVisibilitySigil(VisibilityKind? visibility) => visibility switch
        {
            VisibilityKind.Private => "-",
            VisibilityKind.Protected => "#",
            VisibilityKind.Package => "~",
            _ => "+",
        };
    }
}
