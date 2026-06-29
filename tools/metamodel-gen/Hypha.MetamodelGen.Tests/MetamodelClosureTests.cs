// ------------------------------------------------------------------------------------------------
// <copyright file="MetamodelClosureTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.MetamodelGen.Tests
{
    using System.Collections.Generic;
    using System.Linq;

    using Hypha.MetamodelGen.Generators;

    using NUnit.Framework;

    /// <summary>
    /// Tests the precomputed inheritance closures and inherited-attribute attribution in the rich graph.
    /// </summary>
    [TestFixture]
    public class MetamodelClosureTests
    {
        private static MetamodelDocument document = null!;
        private static Dictionary<string, ClassNode> classesByName = null!;

        [OneTimeSetUp]
        public void Build()
        {
            var model = TestModel.Model;
            if (model is null)
            {
                Assert.Ignore("No SysML model found under sources/xmi/.");
            }

            document = MetamodelJsonGenerator.BuildDocument(model!, "test-hash");
            classesByName = document.Classes.ToDictionary(@class => @class.Name);
        }

        [Test]
        public void Root_class_Element_has_no_supertypes()
        {
            var element = classesByName["Element"];
            Assert.Multiple(() =>
            {
                Assert.That(element.Extends, Is.Empty);
                Assert.That(element.AllAncestors, Is.Empty);
            });
        }

        [Test]
        public void At_least_one_class_has_multiple_inheritance()
        {
            Assert.That(document.Classes.Any(@class => @class.Extends.Count > 1), Is.True);
        }

        [Test]
        public void At_least_one_class_is_abstract()
        {
            Assert.That(document.Classes.Any(@class => @class.IsAbstract), Is.True);
        }

        [Test]
        public void A_deep_class_inherits_from_three_or_more_ancestors()
        {
            var partUsage = classesByName["PartUsage"];
            var distinctOwners = partUsage.InheritedAttributes.Select(a => a.InheritedFrom).Distinct().Count();
            Assert.That(distinctOwners, Is.GreaterThanOrEqualTo(3));
        }

        [Test]
        public void Nested_packages_yield_qualified_names()
        {
            Assert.That(document.Classes.Any(@class => @class.QualifiedName.Contains("::")), Is.True);
        }

        [Test]
        public void Ancestors_and_descendants_are_mutual_inverses()
        {
            foreach (var @class in document.Classes)
            {
                foreach (var ancestor in @class.AllAncestors)
                {
                    Assert.That(
                        classesByName[ancestor].AllDescendants, Does.Contain(@class.Name),
                        $"{ancestor} is an ancestor of {@class.Name} but does not list it as a descendant");
                }
            }
        }

        [Test]
        public void Owned_attribute_names_are_not_repeated_as_inherited()
        {
            foreach (var @class in document.Classes)
            {
                var owned = @class.OwnedAttributes.Select(a => a.Name).ToHashSet();
                Assert.That(
                    @class.InheritedAttributes.Any(a => owned.Contains(a.Name)), Is.False,
                    $"{@class.Name} lists an owned attribute name among its inherited attributes");
            }
        }
    }
}
