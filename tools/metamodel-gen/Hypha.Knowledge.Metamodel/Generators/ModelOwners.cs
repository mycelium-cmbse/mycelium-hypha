// ------------------------------------------------------------------------------------------------
// <copyright file="ModelOwners.cs" company="Starion Group S.A.">
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
    using System.Linq;

    using uml4net.Classification;
    using uml4net.CommonStructure;
    using uml4net.SimpleClassifiers;
    using uml4net.StructuredClassifiers;
    using uml4net.xmi.Readers;

    /// <summary>
    /// The owner of every attribute, association end and operation of a model, so a reference to one
    /// (a redefinition, a subsetting, an opposite end, a constrained element) names the element that
    /// declares it, not only its name.
    /// </summary>
    /// <remarks>
    /// The XMI reader does not populate the back-references <c>IProperty.Class</c> and
    /// <c>IOperation.Class</c>, so the owners are collected the other way round, from what each
    /// classifier and association owns, keyed by XMI id.
    /// <para>
    /// Only named elements with a named owner are recorded: a reference to anything else could not say
    /// what it refers to. <see cref="TryFeature"/> returns <c>null</c> for such an element, and
    /// <see cref="Feature"/> refuses it.
    /// </para>
    /// </remarks>
    public sealed class ModelOwners
    {
        /// <summary>The owner of each attribute and association end, by XMI id.</summary>
        private readonly Dictionary<string, FeatureReference> features;

        /// <summary>The owner of each operation, by XMI id.</summary>
        private readonly Dictionary<string, OperationReference> operations;

        /// <summary>Initializes a new instance of the <see cref="ModelOwners"/> class.</summary>
        /// <param name="features">The owner of each attribute and association end, by XMI id.</param>
        /// <param name="operations">The owner of each operation, by XMI id.</param>
        private ModelOwners(Dictionary<string, FeatureReference> features, Dictionary<string, OperationReference> operations)
        {
            this.features = features;
            this.operations = operations;
        }

        /// <summary>Collects the owners of every attribute, association end and operation of a model.</summary>
        /// <param name="model">The model.</param>
        /// <returns>The owners.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="model"/> is <c>null</c>.</exception>
        public static ModelOwners Build(XmiReaderResult model)
        {
            ArgumentNullException.ThrowIfNull(model);

            var features = new Dictionary<string, FeatureReference>(StringComparer.Ordinal);
            var operations = new Dictionary<string, OperationReference>(StringComparer.Ordinal);

            var classifiers = model.Packages
                .SelectMany(ElementCatalog.ExpandPackages)
                .SelectMany(package => package.PackagedElement.OfType<IClassifier>());

            // A reference names a feature and its owner, so an element without a name, or one whose owner
            // has none (the unnamed associations of the Systems Modeling API PIM), cannot be referenced and
            // is left out.
            foreach (var classifier in classifiers.Where(classifier => !string.IsNullOrEmpty(classifier.Name)))
            {
                if (classifier is IAssociation association)
                {
                    foreach (var end in association.OwnedEnd.Where(end => !string.IsNullOrEmpty(end.Name)))
                    {
                        Add(features, end.XmiId, new FeatureReference(null, association.Name, end.Name));
                    }
                }

                foreach (var attribute in OwnedAttributes(classifier).Where(attribute => !string.IsNullOrEmpty(attribute.Name)))
                {
                    Add(features, attribute.XmiId, new FeatureReference(classifier.Name, null, attribute.Name));
                }

                foreach (var operation in OwnedOperations(classifier).Where(operation => !string.IsNullOrEmpty(operation.Name)))
                {
                    Add(operations, operation.XmiId, new OperationReference(classifier.Name, operation.Name));
                }
            }

            return new ModelOwners(features, operations);
        }

        /// <summary>The reference to an attribute or association end, naming its owner.</summary>
        /// <param name="property">The attribute or end.</param>
        /// <returns>The reference.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="property"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">
        /// No named classifier or association of the model owns the property, or it has no name, so a
        /// reference to it would be wrong rather than incomplete.
        /// </exception>
        public FeatureReference Feature(IProperty property)
        {
            ArgumentNullException.ThrowIfNull(property);

            return this.TryFeature(property)
                   ?? throw new InvalidOperationException(
                       $"The property '{property.Name}' ({property.XmiId}) has no name or is owned by no named classifier or association of the model.");
        }

        /// <summary>The reference to an attribute or association end, or <c>null</c> when the model does not own it.</summary>
        /// <param name="property">The attribute or end.</param>
        /// <returns>The reference, or <c>null</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="property"/> is <c>null</c>.</exception>
        public FeatureReference? TryFeature(IProperty property)
        {
            ArgumentNullException.ThrowIfNull(property);

            return property.XmiId is not null && this.features.TryGetValue(property.XmiId, out var reference)
                ? reference
                : null;
        }

        /// <summary>The reference to an operation, naming its owner.</summary>
        /// <param name="operation">The operation.</param>
        /// <returns>The reference.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="operation"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">No classifier of the model owns the operation.</exception>
        public OperationReference Operation(IOperation operation)
        {
            ArgumentNullException.ThrowIfNull(operation);

            return operation.XmiId is not null && this.operations.TryGetValue(operation.XmiId, out var reference)
                ? reference
                : throw new InvalidOperationException(
                    $"The operation '{operation.Name}' ({operation.XmiId}) is owned by no classifier of the model.");
        }

        /// <summary>The reference to any model element, by its UML metaclass, owner and name.</summary>
        /// <param name="element">The element.</param>
        /// <returns>
        /// The reference; an attribute or operation names its owner, any other element has none.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="element"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">The element is an attribute or operation the model does not own.</exception>
        public ElementReference Element(IElement element)
        {
            ArgumentNullException.ThrowIfNull(element);

            return element switch
            {
                IProperty property when this.Feature(property) is var feature =>
                    new ElementReference("property", feature.Class ?? feature.Association, feature.Feature),
                IOperation operation when this.Operation(operation) is var reference =>
                    new ElementReference("operation", reference.Class, reference.Operation),
                _ => new ElementReference(KindOf(element), null, (element as INamedElement)?.Name ?? string.Empty),
            };
        }

        /// <summary>The UML metaclass of an element, camel-cased, such as <c>class</c> or <c>dataType</c>.</summary>
        /// <param name="element">The element.</param>
        /// <returns>The metaclass name with its first letter in lower case.</returns>
        private static string KindOf(IElement element)
        {
            var name = element.GetType().Name;

            return string.Concat(char.ToLowerInvariant(name[0]).ToString(), name[1..]);
        }

        /// <summary>The attributes a classifier owns, for the classifiers that own any.</summary>
        /// <param name="classifier">The classifier.</param>
        /// <returns>The owned attributes; none for a classifier kind that owns no attributes.</returns>
        private static IEnumerable<IProperty> OwnedAttributes(IClassifier classifier) => classifier switch
        {
            IClass @class => @class.OwnedAttribute,
            IDataType dataType => dataType.OwnedAttribute,
            IInterface @interface => @interface.OwnedAttribute,
            _ => [],
        };

        /// <summary>The operations a classifier owns, for the classifiers that own any.</summary>
        /// <param name="classifier">The classifier.</param>
        /// <returns>The owned operations; none for a classifier kind that owns no operations.</returns>
        private static IEnumerable<IOperation> OwnedOperations(IClassifier classifier) => classifier switch
        {
            IClass @class => @class.OwnedOperation,
            IDataType dataType => dataType.OwnedOperation,
            IInterface @interface => @interface.OwnedOperation,
            _ => [],
        };

        /// <summary>Records an owner, keeping the first one seen for an XMI id.</summary>
        /// <typeparam name="T">The reference type.</typeparam>
        /// <param name="owners">The owners recorded so far.</param>
        /// <param name="xmiId">The owned element's XMI id, or <c>null</c> when it has none.</param>
        /// <param name="reference">The reference to record.</param>
        private static void Add<T>(Dictionary<string, T> owners, string? xmiId, T reference)
        {
            if (!string.IsNullOrEmpty(xmiId))
            {
                owners.TryAdd(xmiId, reference);
            }
        }
    }
}
