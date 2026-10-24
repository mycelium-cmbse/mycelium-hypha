// ------------------------------------------------------------------------------------------------
// <copyright file="ElementCatalogTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.MetamodelGen.Tests
{
    using System;
    using System.Linq;

    using Hypha.MetamodelGen.Generators;

    using NUnit.Framework;

    /// <summary>
    /// Tests for the parts of <see cref="ElementCatalog"/> that only an ordinary model exercises:
    /// its interfaces, its data types that are neither primitive nor an enumeration, and the
    /// classifiers that realize an interface.
    /// </summary>
    /// <remarks>
    /// The PIM-shaped fixture (see <see cref="PimFixture"/>) declares all three; the KerML / SysML
    /// fixture declares none, which is the other half of the contract - nothing new may appear for
    /// the metamodels.
    /// </remarks>
    [TestFixture]
    public class ElementCatalogTests
    {
        [Test]
        public void Lists_the_interfaces_ordered_by_name()
        {
            var model = PimFixture.Model;
            if (model is null)
            {
                Assert.Ignore("No PIM fixture found.");
            }

            Assert.That(
                ElementCatalog.Interfaces(model!).Select(@interface => @interface.Name),
                Is.EqualTo(new[] { "Data", "Query" }));
        }

        [Test]
        public void Lists_the_data_types_that_are_neither_primitive_nor_an_enumeration()
        {
            var model = PimFixture.Model;
            if (model is null)
            {
                Assert.Ignore("No PIM fixture found.");
            }

            var dataTypes = ElementCatalog.DataTypes(model!).Select(dataType => dataType.Name).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(dataTypes, Is.EqualTo(new[] { "Timestamp", "UUID" }));
                Assert.That(dataTypes, Does.Not.Contain("ProjectState"), "an enumeration is a data type in UML");
                Assert.That(dataTypes, Does.Not.Contain("String"), "and so is a primitive type");

                // Both still have a page of their own kind.
                Assert.That(
                    ElementCatalog.Enumerations(model!).Select(enumeration => enumeration.Name),
                    Does.Contain("ProjectState"));
                Assert.That(
                    ElementCatalog.PrimitiveTypes(model!).Select(primitiveType => primitiveType.Name),
                    Does.Contain("String"));
            });
        }

        [Test]
        public void An_interface_is_realized_by_the_clients_of_the_realizations_that_supply_it()
        {
            var model = PimFixture.Model;
            if (model is null)
            {
                Assert.Ignore("No PIM fixture found.");
            }

            var data = ElementCatalog.Interfaces(model!).Single(@interface => @interface.Name == "Data");
            var realizedBy = ElementCatalog.RealizedBy(model!, data);

            Assert.Multiple(() =>
            {
                Assert.That(realizedBy, Does.Contain("Project"));
                Assert.That(realizedBy, Does.Not.Contain("Commit"), "Commit has a Data-typed feature, no realization");
                Assert.That(realizedBy, Is.Unique);
                Assert.That(realizedBy, Is.Ordered.Using<string>(StringComparer.Ordinal));
                Assert.That(realizedBy, Has.None.Empty, "a client that could not be named is left out");
            });
        }

        [Test]
        public void An_interface_nothing_realizes_is_realized_by_nothing()
        {
            var model = PimFixture.Model;
            if (model is null)
            {
                Assert.Ignore("No PIM fixture found.");
            }

            var query = ElementCatalog.Interfaces(model!).Single(@interface => @interface.Name == "Query");

            Assert.That(ElementCatalog.RealizedBy(model!, query), Is.Empty);
        }

        [Test]
        public void Interfaces_and_data_types_can_be_linked_to()
        {
            // A feature typed by Data or UUID links to its page, as one typed by a class does.
            var model = PimFixture.Model;
            if (model is null)
            {
                Assert.Ignore("No PIM fixture found.");
            }

            Assert.That(
                ElementCatalog.LinkableTypeNames(model!),
                Is.SupersetOf(new[]
                {
                    "Data", "Query", "UUID", "Timestamp", "Record", "Project", "Commit", "ProjectState", "String",
                }));
        }

        [Test]
        public void The_metamodels_declare_no_interface_and_no_such_data_type()
        {
            var model = TestModel.Model;
            if (model is null)
            {
                Assert.Ignore("No SysML model found in the fixture.");
            }

            Assert.Multiple(() =>
            {
                Assert.That(ElementCatalog.Interfaces(model!), Is.Empty);
                Assert.That(ElementCatalog.DataTypes(model!), Is.Empty);
            });
        }

        [Test]
        public void Missing_arguments_are_refused()
        {
            var model = PimFixture.Model;
            if (model is null)
            {
                Assert.Ignore("No PIM fixture found.");
            }

            var data = ElementCatalog.Interfaces(model!).First();

            Assert.Multiple(() =>
            {
                Assert.Throws<ArgumentNullException>(() => ElementCatalog.Interfaces(null!));
                Assert.Throws<ArgumentNullException>(() => ElementCatalog.DataTypes(null!));
                Assert.Throws<ArgumentNullException>(() => ElementCatalog.RealizedBy(model!, null!));
                Assert.Throws<ArgumentNullException>(() => ElementCatalog.RealizedBy(null!, data));
            });
        }
    }
}
