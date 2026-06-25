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
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using uml4net.Classification;
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
        /// Renders the element markdown for a single metaclass. The <paramref name="subtypeIndex"/>
        /// (see <see cref="BuildSubtypeIndex"/>) supplies the direct subtypes, which cannot be
        /// derived from the metaclass alone.
        /// </summary>
        public string GenerateElement(IClass @class, IReadOnlyDictionary<string, IReadOnlyList<string>> subtypeIndex)
        {
            ArgumentNullException.ThrowIfNull(@class);
            ArgumentNullException.ThrowIfNull(subtypeIndex);

            var payload = CreatePayload(@class, subtypeIndex);

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

            foreach (var @class in metaclasses)
            {
                var content = this.GenerateElement(@class, subtypeIndex);

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
                foreach (var super in @class.SuperClass)
                {
                    if (string.IsNullOrEmpty(super.Name))
                    {
                        continue;
                    }

                    if (!map.TryGetValue(super.Name, out var subtypes))
                    {
                        subtypes = new List<string>();
                        map[super.Name] = subtypes;
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
        private static MetaclassPayload CreatePayload(
            IClass @class,
            IReadOnlyDictionary<string, IReadOnlyList<string>> subtypeIndex)
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

            var features = @class.OwnedAttribute
                .OrderBy(property => property.Name, StringComparer.Ordinal)
                .Select(property => new MetaclassFeature(
                    property.Name,
                    property.Type?.Name ?? "«untyped»",
                    property.QueryFormattedMultiplicity(),
                    QueryOwnedModifiers(property),
                    property.QueryDocumentationText(),
                    property.RedefinedProperty.Select(redefined => redefined.Name).ToList(),
                    property.SubsettedProperty.Select(subsetted => subsetted.Name).ToList()))
                .ToList();

            // The XMI reader does not populate the IProperty.Class back-reference, so attribute each
            // inherited feature to its declaring ancestor by walking the supertype classifiers.
            var inheritedFeatures = @class.QueryAllGeneralClassifiers()
                .OfType<IClass>()
                .OrderBy(ancestor => ancestor.Name, StringComparer.Ordinal)
                .SelectMany(ancestor => ancestor.OwnedAttribute
                    .Select(property => new InheritedFeature(
                        property.Name,
                        property.Type?.Name ?? "«untyped»",
                        property.QueryFormattedMultiplicity(),
                        ancestor.Name,
                        string.Join(", ", QueryModifierTokens(property)))))
                .Where(feature => !ownedNames.Contains(feature.Name))
                .GroupBy(feature => feature.Name, StringComparer.Ordinal)
                .Select(group => group.First())
                .OrderBy(feature => feature.Name, StringComparer.Ordinal)
                .ToList();

            var constraints = @class.OwnedRule
                .OrderBy(rule => rule.Name, StringComparer.Ordinal)
                .Select(rule => new MetaclassConstraint(
                    rule.Name,
                    rule.QueryDocumentationText(),
                    QueryConstraintBody(rule)))
                .ToList();

            return new MetaclassPayload(
                @class.Name,
                @class.Namespace?.Name ?? string.Empty,
                @class.IsAbstract,
                @class.QueryDocumentationText(),
                generalizations,
                specializations,
                features,
                inheritedFeatures,
                constraints);
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
        /// Renders the owned-feature modifier annotation (e.g. <c>{derived, ordered}</c>), or an
        /// empty string when the property has no modifiers.
        /// </summary>
        private static string QueryOwnedModifiers(IProperty property)
        {
            var tokens = QueryModifierTokens(property).ToList();

            return tokens.Count == 0 ? string.Empty : $"{{{string.Join(", ", tokens)}}}";
        }

        /// <summary>
        /// Extracts the textual body of a constraint specification. The specification is held in a
        /// container; the contained opaque expression carries the (OCL) body.
        /// </summary>
        private static string QueryConstraintBody(IConstraint constraint)
        {
            if (constraint.Specification is IEnumerable specifications)
            {
                var expression = specifications.OfType<IOpaqueExpression>().FirstOrDefault();

                if (expression is not null)
                {
                    return string.Join("\n", expression.Body).Replace("\r\n", "\n").Trim();
                }
            }

            return string.Empty;
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
