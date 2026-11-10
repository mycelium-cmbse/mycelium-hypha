// ------------------------------------------------------------------------------------------------
// <copyright file="MetamodelReferenceTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Metamodel.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Hypha.Knowledge.Metamodel.Generators;

    using NUnit.Framework;

    using uml4net.Classification;
    using uml4net.Packages;
    using uml4net.StructuredClassifiers;
    using uml4net.xmi.Readers;

    // The facts the generator takes from uml4net so a reader never works them out itself: the owner of
    // every referenced feature, opposite ends, what redefines an inherited feature or operation in a
    // metaclass's context, and the inherited operations and constraints by reference.
    [TestFixture]
    public class MetamodelReferenceTests
    {
        private MetamodelDocument document = null!;

        private Dictionary<string, ClassNode> classes = null!;

        [OneTimeSetUp]
        public void Build()
        {
            var model = TestModel.Model;
            Assert.That(model, Is.Not.Null, "No SysML model found in the fixture.");

            this.document = MetamodelJsonGenerator.BuildDocument(model!, JsonSidecarTestSupport.ComputeSourceHash());
            this.classes = this.document.Classes.ToDictionary(@class => @class.Name, StringComparer.Ordinal);
        }

        [Test]
        public void Every_feature_reference_names_exactly_one_owner()
        {
            var references = this.document.Classes.SelectMany(@class =>
                    @class.OwnedAttributes.SelectMany(attribute => attribute.Redefines.Concat(attribute.Subsets).Append(attribute.Opposite))
                        .Concat(@class.InheritedAttributes.SelectMany(attribute =>
                            attribute.Redefines.Concat(attribute.Subsets).Append(attribute.Opposite).Append(attribute.RedefinedBy))))
                .OfType<FeatureReference>()
                .ToList();

            Assert.Multiple(() =>
            {
                Assert.That(references, Is.Not.Empty);
                Assert.That(references.Where(reference => (reference.Class is null) == (reference.Association is null)), Is.Empty);
            });
        }

        [Test]
        public void A_reference_to_a_metaclass_feature_names_a_feature_that_metaclass_owns()
        {
            var unresolved = this.document.Classes
                .SelectMany(@class => @class.OwnedAttributes.SelectMany(attribute => attribute.Redefines.Concat(attribute.Subsets)))
                .Where(reference => reference.Class is not null && this.classes.ContainsKey(reference.Class))
                .Where(reference => this.classes[reference.Class!].OwnedAttributes.All(attribute => attribute.Name != reference.Feature))
                .ToList();

            Assert.That(unresolved, Is.Empty);
        }

        [Test]
        public void A_redefinition_names_the_feature_it_redefines_with_its_owner()
        {
            Assert.Multiple(() =>
            {
                // A different name: Dependency::client redefines Relationship::source.
                Assert.That(
                    this.Attribute("Dependency", "client").Redefines,
                    Is.EqualTo(new[] { new FeatureReference("Relationship", null, "source") }));

                // The same name: before, a bare 'isReference' could not say which metaclass's it was.
                Assert.That(
                    this.Attribute("AttributeUsage", "isReference").Redefines,
                    Is.EqualTo(new[] { new FeatureReference("Usage", null, "isReference") }));
            });
        }

        [Test]
        public void A_subsetted_end_an_association_owns_is_named_by_its_association()
        {
            Assert.That(
                this.Attribute("AnnotatingElement", "annotation").Subsets,
                Is.EqualTo(new[] { new FeatureReference(null, "A_source_sourceRelationship", "sourceRelationship") }));
        }

        [Test]
        public void Opposite_ends_two_metaclasses_own_name_each_other()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.Attribute("Type", "ownedFeature").Opposite, Is.EqualTo(new FeatureReference("Feature", null, "owningType")));
                Assert.That(this.Attribute("Feature", "owningType").Opposite, Is.EqualTo(new FeatureReference("Type", null, "ownedFeature")));

                foreach (var @class in this.document.Classes)
                {
                    foreach (var attribute in @class.OwnedAttributes.Where(attribute => attribute.Opposite?.Class is not null))
                    {
                        var other = this.classes.TryGetValue(attribute.Opposite!.Class!, out var node)
                            ? node.OwnedAttributes.SingleOrDefault(candidate => candidate.Name == attribute.Opposite.Feature)
                            : null;

                        if (other is not null)
                        {
                            Assert.That(
                                other.Opposite,
                                Is.EqualTo(new FeatureReference(@class.Name, null, attribute.Name)),
                                $"{@class.Name}::{attribute.Name}");
                        }
                    }
                }
            });
        }

        [Test]
        public void An_attribute_without_an_association_has_no_opposite()
        {
            Assert.That(this.Attribute("Element", "isImpliedIncluded").Opposite, Is.Null);
        }

        [Test]
        public void An_inherited_feature_names_the_feature_that_redefines_it_in_the_metaclass_context()
        {
            var inherited = this.classes["Dependency"].InheritedAttributes.ToDictionary(attribute => attribute.Name, StringComparer.Ordinal);

            Assert.Multiple(() =>
            {
                Assert.That(inherited["source"].RedefinedBy, Is.EqualTo(new FeatureReference("Dependency", null, "client")));
                Assert.That(inherited["target"].RedefinedBy, Is.EqualTo(new FeatureReference("Dependency", null, "supplier")));
                Assert.That(inherited["isImplied"].RedefinedBy, Is.Null);
            });
        }

        [Test]
        public void The_smaller_facts_of_a_feature_come_from_the_model()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.Attribute("Element", "isImpliedIncluded").DefaultValue, Is.EqualTo("false"));
                Assert.That(this.Attribute("Expose", "visibility").DefaultValue, Is.EqualTo("protected"));
                Assert.That(this.Attribute("Type", "ownedFeature").DefaultValue, Is.Null);
                Assert.That(this.Attribute("Feature", "chainingFeature").IsUnique, Is.False);
                Assert.That(this.Attribute("Type", "ownedFeature").IsUnique, Is.True);
                Assert.That(this.Attribute("Namespace", "membership").IsDerivedUnion, Is.True);
                Assert.That(this.document.Classes.SelectMany(@class => @class.OwnedAttributes).Where(attribute => attribute.IsReadOnly), Is.Empty);
            });
        }

        [Test]
        public void An_inherited_operation_names_the_operation_that_redefines_it_in_the_metaclass_context()
        {
            var naming = this.classes["ConstraintUsage"].InheritedOperations
                .Where(operation => operation.Operation == "namingFeature")
                .ToList();

            Assert.Multiple(() =>
            {
                Assert.That(
                    naming,
                    Is.EqualTo(new[]
                    {
                        new InheritedOperationNode("Feature", "namingFeature", new OperationReference("Usage", "namingFeature")),
                        new InheritedOperationNode("Usage", "namingFeature", new OperationReference("ConstraintUsage", "namingFeature")),
                    }));
                Assert.That(
                    this.classes["ConstraintUsage"].OwnedOperations.Single(operation => operation.Name == "namingFeature").Redefines,
                    Is.EqualTo(new[] { new OperationReference("Usage", "namingFeature") }));
            });
        }

        [Test]
        public void Inherited_operations_and_constraints_are_every_one_an_ancestor_owns_once()
        {
            Assert.Multiple(() =>
            {
                foreach (var @class in this.document.Classes)
                {
                    var ancestors = @class.AllAncestors.Select(name => this.classes[name]).ToList();

                    Assert.That(
                        @class.InheritedOperations.Select(operation => $"{operation.Class}::{operation.Operation}"),
                        Is.EquivalentTo(ancestors.SelectMany(ancestor => ancestor.OwnedOperations.Select(operation => $"{ancestor.Name}::{operation.Name}"))),
                        @class.Name);
                    Assert.That(
                        @class.InheritedConstraints.Select(constraint => $"{constraint.Class}::{constraint.Constraint}"),
                        Is.EquivalentTo(ancestors.SelectMany(ancestor => ancestor.Constraints.Select(constraint => $"{ancestor.Name}::{constraint.Name}"))),
                        @class.Name);
                }
            });
        }

        [Test]
        public void Inherited_members_are_ordered_by_name_and_then_declaring_metaclass()
        {
            var usage = this.classes["ConstraintUsage"];

            Assert.Multiple(() =>
            {
                Assert.That(
                    usage.InheritedOperations,
                    Is.EqualTo(usage.InheritedOperations
                        .OrderBy(operation => operation.Operation, StringComparer.Ordinal)
                        .ThenBy(operation => operation.Class, StringComparer.Ordinal)));
                Assert.That(
                    usage.InheritedConstraints,
                    Is.EqualTo(usage.InheritedConstraints
                        .OrderBy(constraint => constraint.Constraint, StringComparer.Ordinal)
                        .ThenBy(constraint => constraint.Class, StringComparer.Ordinal)));
            });
        }

        [Test]
        public void A_constraint_names_the_metaclass_it_constrains()
        {
            var constrained = this.classes["AnnotatingElement"].Constraints
                .Single(rule => rule.Name == "deriveAnnotatingElementAnnotatedElement");

            // A rule the XMI writes without a constrainedElement has none.
            var unconstrained = this.classes["Annotation"].Constraints
                .Single(rule => rule.Name == "validateAnnotationAnnotatedElementOwnership");

            Assert.Multiple(() =>
            {
                Assert.That(constrained.ConstrainedElements, Is.EqualTo(new[] { new ElementReference("class", null, "AnnotatingElement") }));
                Assert.That(unconstrained.ConstrainedElements, Is.Empty);
            });
        }

        [Test]
        public void An_other_end_that_cannot_be_named_is_no_opposite()
        {
            // The PIM's associations have no name, as in the published one: an end two classes own is
            // still named by its class, but an end the unnamed association owns cannot be named at all.
            var model = PimFixture.Model;
            if (model is null)
            {
                Assert.Ignore("No PIM fixture found.");
            }

            var pim = MetamodelJsonGenerator.BuildDocument(model!, JsonSidecarTestSupport.ComputeSourceHash());
            AttributeNode Owned(string className, string name) =>
                pim.Classes.Single(@class => @class.Name == className).OwnedAttributes.Single(attribute => attribute.Name == name);

            var references = pim.Classes
                .SelectMany(@class => @class.OwnedAttributes.SelectMany(attribute => attribute.Redefines.Concat(attribute.Subsets).Append(attribute.Opposite))
                    .Concat(@class.InheritedAttributes.SelectMany(attribute =>
                        attribute.Redefines.Concat(attribute.Subsets).Append(attribute.Opposite).Append(attribute.RedefinedBy))))
                .OfType<FeatureReference>()
                .ToList();

            Assert.Multiple(() =>
            {
                Assert.That(Owned("Project", "commit").Opposite, Is.EqualTo(new FeatureReference("Commit", null, "owningProject")));
                Assert.That(Owned("Commit", "owningProject").Opposite, Is.EqualTo(new FeatureReference("Project", null, "commit")));
                Assert.That(Owned("Project", "headCommit").Opposite, Is.Null, "the other end has no name");
                Assert.That(Owned("Commit", "change").Opposite, Is.Null, "the other end is named, but its association is not");
                Assert.That(references, Is.Not.Empty);
                Assert.That(
                    references.Where(reference =>
                        string.IsNullOrEmpty(reference.Feature) || (reference.Class is null) == (reference.Association is null)),
                    Is.Empty);
            });
        }

        [Test]
        public void A_redefinition_of_a_property_no_named_classifier_owns_is_refused()
        {
            // Unlike an opposite, a redefinition has to say what it redefines, so it cannot be left out.
            var orphan = new Property { Name = "orphan", XmiId = "orphan" };
            var attribute = new Property { Name = "attribute", XmiId = "attribute" };
            attribute.RedefinedProperty.Add(orphan);

            var @class = new Class { Name = "Redefining", XmiId = "class" };
            @class.OwnedAttribute.Add(attribute);

            var package = new Package { Name = "Root", XmiId = "root" };
            package.PackagedElement.Add(@class);

            Assert.That(
                () => MetamodelJsonGenerator.BuildDocument(new XmiReaderResult { Packages = [package] }, "digest"),
                Throws.InvalidOperationException.With.Message.Contains("'orphan'"));
        }

        [Test]
        public void A_metaclass_owning_two_operations_of_one_name_is_refused()
        {
            // An inherited operation is written as a reference by name, which two of the same name would make ambiguous.
            var @class = new Class { Name = "Duplicated", XmiId = "class" };
            @class.OwnedOperation.Add(new Operation { Name = "check", XmiId = "first" });
            @class.OwnedOperation.Add(new Operation { Name = "check", XmiId = "second" });

            var package = new Package { Name = "Root", XmiId = "root" };
            package.PackagedElement.Add(@class);

            var model = new XmiReaderResult { Packages = [package] };

            Assert.That(
                () => MetamodelJsonGenerator.BuildDocument(model, "digest"),
                Throws.InvalidOperationException.With.Message.Contains("more than one operation named 'check'"));
        }

        private AttributeNode Attribute(string className, string name) =>
            this.classes[className].OwnedAttributes.Single(attribute => attribute.Name == name);
    }
}
