// ------------------------------------------------------------------------------------------------
// <copyright file="MetamodelSchemaTests.cs" company="Starion Group S.A.">
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
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.Json;

    using Hypha.Knowledge.Metamodel.Generators;

    using NUnit.Framework;

    // Checks the produced metamodel.json against the hand-authored knowledge/metamodel.schema.json:
    // the schema describes the shape, so a member added to the payload without a matching schema
    // entry (or the reverse) fails here.
    [TestFixture]
    public class MetamodelSchemaTests
    {
        private JsonDocument schema = null!;
        private JsonDocument produced = null!;

        [SetUp]
        public void SetUp()
        {
            var layout = Repository.Layout;
            if (layout is null)
            {
                Assert.Ignore("The tests are not running inside a checkout.");
            }

            var model = TestModel.Model;
            if (model is null)
            {
                Assert.Ignore("No SysML model found in the fixture.");
            }

            this.schema = JsonDocument.Parse(File.ReadAllText(layout!.MetamodelSchema.FullName, Encoding.UTF8));

            var document = MetamodelJsonGenerator.BuildDocument(model!, JsonSidecarTestSupport.ComputeSourceHash());

            this.produced = JsonDocument.Parse(MetamodelJsonGenerator.Serialize(document));
        }

        [TearDown]
        public void TearDown()
        {
            this.schema?.Dispose();
            this.produced?.Dispose();
        }

        private static IEnumerable<string> Keys(JsonElement element) =>
            element.EnumerateObject().Select(property => property.Name).Order(StringComparer.Ordinal);

        private static IEnumerable<string> Required(JsonElement element) =>
            element.GetProperty("required").EnumerateArray()
                .Select(item => item.GetString()!)
                .Order(StringComparer.Ordinal);

        private JsonElement Definition(string name) =>
            this.schema.RootElement.GetProperty("$defs").GetProperty(name);

        private JsonElement First(string arrayName) =>
            this.produced.RootElement.GetProperty(arrayName).EnumerateArray().First();

        [Test]
        public void The_document_has_exactly_the_required_top_level_keys() =>
            Assert.That(Keys(this.produced.RootElement), Is.EqualTo(Required(this.schema.RootElement)));

        [Test]
        public void The_counts_block_matches_the_schema() =>
            Assert.That(
                Keys(this.produced.RootElement.GetProperty("counts")),
                Is.EqualTo(Required(this.schema.RootElement.GetProperty("properties").GetProperty("counts"))));

        [Test]
        public void A_package_matches_the_schema() =>
            Assert.That(Keys(this.First("packages")), Is.EqualTo(Required(this.Definition("package"))));

        [Test]
        public void A_class_matches_the_schema() =>
            Assert.That(Keys(this.First("classes")), Is.EqualTo(Required(this.Definition("class"))));

        [Test]
        public void An_enumeration_matches_the_schema() =>
            Assert.That(Keys(this.First("enumerations")), Is.EqualTo(Required(this.Definition("enumeration"))));

        [Test]
        public void A_primitive_type_matches_the_schema() =>
            Assert.That(Keys(this.First("primitiveTypes")), Is.EqualTo(Required(this.Definition("type"))));

        [Test]
        public void An_owned_attribute_matches_the_schema()
        {
            var attribute = this.produced.RootElement.GetProperty("classes").EnumerateArray()
                .SelectMany(@class => @class.GetProperty("ownedAttributes").EnumerateArray())
                .First();

            Assert.That(Keys(attribute), Is.EqualTo(Required(this.Definition("attribute"))));
        }

        [Test]
        public void An_inherited_attribute_matches_the_schema()
        {
            var attribute = this.produced.RootElement.GetProperty("classes").EnumerateArray()
                .SelectMany(@class => @class.GetProperty("inheritedAttributes").EnumerateArray())
                .First();

            Assert.That(Keys(attribute), Is.EqualTo(Required(this.Definition("inheritedAttribute"))));
        }

        [Test]
        public void An_operation_and_its_parameters_match_the_schema()
        {
            var operation = this.produced.RootElement.GetProperty("classes").EnumerateArray()
                .SelectMany(@class => @class.GetProperty("ownedOperations").EnumerateArray())
                .First(candidate => candidate.GetProperty("parameters").GetArrayLength() > 0);

            Assert.Multiple(() =>
            {
                Assert.That(Keys(operation), Is.EqualTo(Required(this.Definition("operation"))));
                Assert.That(
                    Keys(operation.GetProperty("parameters").EnumerateArray().First()),
                    Is.EqualTo(Required(this.Definition("parameter"))));
            });
        }

        [Test]
        public void A_constraint_matches_the_schema()
        {
            var constraint = this.produced.RootElement.GetProperty("classes").EnumerateArray()
                .SelectMany(@class => @class.GetProperty("constraints").EnumerateArray())
                .First();

            Assert.That(Keys(constraint), Is.EqualTo(Required(this.Definition("constraint"))));
        }

        [Test]
        public void An_enumeration_literal_matches_the_schema()
        {
            var literal = this.produced.RootElement.GetProperty("enumerations").EnumerateArray()
                .SelectMany(enumeration => enumeration.GetProperty("literals").EnumerateArray())
                .First();

            Assert.That(Keys(literal), Is.EqualTo(Required(this.Definition("literal"))));
        }

        [Test]
        public void Every_element_of_every_collection_matches_its_definition()
        {
            Assert.Multiple(() =>
            {
                this.AssertAll("packages", "package");
                this.AssertAll("classes", "class");
                this.AssertAll("enumerations", "enumeration");
                this.AssertAll("primitiveTypes", "type");
            });
        }

        [Test]
        public void Every_reference_in_the_schema_resolves_to_a_definition()
        {
            var defined = this.schema.RootElement.GetProperty("$defs").EnumerateObject()
                .Select(definition => definition.Name)
                .ToHashSet(StringComparer.Ordinal);

            var referenced = References(this.schema.RootElement).ToHashSet(StringComparer.Ordinal);

            Assert.Multiple(() =>
            {
                Assert.That(referenced.Except(defined), Is.Empty, "the schema references a definition it does not declare");
                Assert.That(defined.Except(referenced), Is.Empty, "the schema declares a definition nothing references");
            });
        }

        [Test]
        public void An_interface_and_a_data_type_match_the_schema()
        {
            var model = PimFixture.Model;
            if (model is null)
            {
                Assert.Ignore("No PIM fixture found.");
            }

            var document = MetamodelJsonGenerator.BuildDocument(model!, JsonSidecarTestSupport.ComputeSourceHash());

            using var pim = JsonDocument.Parse(MetamodelJsonGenerator.Serialize(document));

            var declared = this.schema.RootElement.GetProperty("properties").EnumerateObject()
                .Select(property => property.Name);

            Assert.Multiple(() =>
            {
                Assert.That(
                    Keys(pim.RootElement.GetProperty("interfaces").EnumerateArray().First()),
                    Is.EqualTo(Required(this.Definition("interface"))));
                Assert.That(
                    Keys(pim.RootElement.GetProperty("dataTypes").EnumerateArray().First()),
                    Is.EqualTo(Required(this.Definition("type"))));
                Assert.That(
                    Keys(pim.RootElement),
                    Is.SupersetOf(Required(this.schema.RootElement)).And.SubsetOf(declared),
                    "a document member the schema does not declare");
            });
        }

        private static IEnumerable<string> References(JsonElement element) =>
            element.ValueKind switch
            {
                JsonValueKind.Object => element.EnumerateObject().SelectMany(property =>
                    property.NameEquals("$ref") && property.Value.ValueKind == JsonValueKind.String
                        ? [property.Value.GetString()!.Replace("#/$defs/", string.Empty, StringComparison.Ordinal)]
                        : References(property.Value)),
                JsonValueKind.Array => element.EnumerateArray().SelectMany(References),
                _ => [],
            };

        private void AssertAll(string arrayName, string definitionName)
        {
            var required = Required(this.Definition(definitionName)).ToList();

            foreach (var element in this.produced.RootElement.GetProperty(arrayName).EnumerateArray())
            {
                Assert.That(
                    Keys(element),
                    Is.EqualTo(required),
                    $"{arrayName}/{element.GetProperty("name").GetString()} does not match {definitionName}");
            }
        }
    }
}
