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
        /// Renders the element markdown for a single metaclass. The <paramref name="subtypeIndex"/>
        /// (see <see cref="BuildSubtypeIndex"/>) supplies the direct subtypes, which cannot be
        /// derived from the metaclass alone. The <paramref name="linkableTypeNames"/> (see
        /// <see cref="QueryMetaclassNames"/>) are the metaclasses that have a generated element file,
        /// so feature types pointing at them can be rendered as links.
        /// </summary>
        public string GenerateElement(
            IClass @class,
            IReadOnlyDictionary<string, IReadOnlyList<string>> subtypeIndex,
            IReadOnlySet<string> linkableTypeNames)
        {
            ArgumentNullException.ThrowIfNull(@class);
            ArgumentNullException.ThrowIfNull(subtypeIndex);
            ArgumentNullException.ThrowIfNull(linkableTypeNames);

            var payload = CreatePayload(@class, subtypeIndex, linkableTypeNames);

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

            var metaclasses = QueryMetaclasses(model);
            var subtypeIndex = BuildSubtypeIndex(metaclasses);
            var linkableTypeNames = ElementCatalog.LinkableTypeNames(model);

            foreach (var @class in metaclasses)
            {
                var content = this.GenerateElement(@class, subtypeIndex, linkableTypeNames);

                await WriteAsync(content, outputDirectory, $"{@class.Name}.md");
            }
        }

        /// <summary>
        /// Builds a map from each metaclass name to its direct subtype names (the inverse of the
        /// generalization relation), ordered.
        /// </summary>
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
        public static IReadOnlyList<IClass> QueryMetaclasses(XmiReaderResult model) =>
            ElementCatalog.Metaclasses(model);

        /// <summary>
        /// Builds the deterministic payload for a metaclass from the model.
        /// </summary>
        private static MetaclassPayload CreatePayload(
            IClass @class,
            IReadOnlyDictionary<string, IReadOnlyList<string>> subtypeIndex,
            IReadOnlySet<string> linkableTypeNames)
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

            // Resolve a redefined/subsetted feature name to the supertype that declares it, so the
            // reference can link to that owner's element file (#anchor when it is owned locally).
            var inheritedOwners = inheritedFeatures.ToDictionary(
                feature => feature.Name, feature => feature.Owner, StringComparer.Ordinal);

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
                    Redefines = property.RedefinedProperty.Select(redefined => RenderFeatureReference(redefined.Name, ownedNames, inheritedOwners)).ToList(),
                    Subsets = property.SubsettedProperty.Select(subsetted => RenderFeatureReference(subsetted.Name, ownedNames, inheritedOwners)).ToList(),
                })
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
                Constraints = constraints,
            };
        }

        /// <summary>
        /// Returns the modifier tokens (<c>derived</c>, <c>composite</c>, <c>ordered</c>) that apply
        /// to a property, in a stable order.
        /// </summary>
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
        private static string RenderTypeLink(string typeName, IReadOnlySet<string> linkableTypeNames)
        {
            return linkableTypeNames.Contains(typeName) ? $"[{typeName}]({typeName}.md)" : typeName;
        }

        /// <summary>
        /// Renders a redefined/subsetted feature reference as a markdown link: a same-file anchor when
        /// the feature is owned locally, the declaring supertype's element file when it is inherited,
        /// otherwise plain backtick text when the target is outside this metaclass' feature set.
        /// </summary>
        private static string RenderFeatureReference(
            string featureName,
            HashSet<string> ownedNames,
            Dictionary<string, string> inheritedOwners)
        {
            if (ownedNames.Contains(featureName))
            {
                return $"[{featureName}](#{Slug(featureName)})";
            }

            if (inheritedOwners.TryGetValue(featureName, out var owner))
            {
                return $"[{featureName}]({owner}.md#{Slug(featureName)})";
            }

            return $"`{featureName}`";
        }

        /// <summary>
        /// Returns the GitHub-style heading anchor for a feature name (identifiers only need lowercasing).
        /// </summary>
        private static string Slug(string name) => name.ToLowerInvariant();

        /// <summary>
        /// Returns the UML visibility sigil (<c>+</c> public, <c>-</c> private, <c>#</c> protected,
        /// <c>~</c> package).
        /// </summary>
        private static string QueryVisibilitySigil(VisibilityKind visibility) => visibility switch
        {
            VisibilityKind.Private => "-",
            VisibilityKind.Protected => "#",
            VisibilityKind.Package => "~",
            _ => "+",
        };
    }
}
