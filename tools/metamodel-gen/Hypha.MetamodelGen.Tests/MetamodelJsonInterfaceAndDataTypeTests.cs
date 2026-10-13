// ------------------------------------------------------------------------------------------------
// <copyright file="MetamodelJsonInterfaceAndDataTypeTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.MetamodelGen.Tests
{
    using System.Linq;
    using System.Text.Json;

    using Hypha.MetamodelGen.Generators;

    using NUnit.Framework;

    /// <summary>
    /// Tests for what <see cref="MetamodelJsonGenerator"/> writes about interfaces and data types:
    /// present, in both sidecars, for a model that declares them - and absent, property and all, for
    /// one that does not.
    /// </summary>
    [TestFixture]
    public class MetamodelJsonInterfaceAndDataTypeTests
    {
        private const string Hash = "0000000000000000000000000000000000000000000000000000000000000000";

        [Test]
        public void The_graph_carries_the_interfaces_with_their_realizers_and_operations()
        {
            var document = PimDocument();

            var data = document.Interfaces!.Single(@interface => @interface.Name == "Data");
            var identity = data.OwnedOperations.Single();

            Assert.Multiple(() =>
            {
                Assert.That(
                    document.Interfaces!.Select(@interface => @interface.Name), Is.EqualTo(new[] { "Data", "Query" }));

                Assert.That(data.XmiId, Is.EqualTo("_fixture_Data"));
                Assert.That(data.Package, Is.EqualTo(PimFixture.PackageName));
                Assert.That(data.QualifiedName, Is.EqualTo("FixtureApi::ProjectDataVersioning::Data"));
                Assert.That(data.Visibility, Is.EqualTo("public"));
                Assert.That(data.Documentation, Is.EqualTo("Data is whatever a Commit can carry a version of."));
                Assert.That(data.Summary, Is.EqualTo("Data is whatever a Commit can carry a version of."));
                Assert.That(data.RealizedBy, Does.Contain("Project"));

                Assert.That(identity.Name, Is.EqualTo("identity"));
                Assert.That(identity.ReturnType, Is.EqualTo("UUID"));
                Assert.That(identity.IsQuery, Is.True);
                Assert.That(identity.Parameters, Is.Empty);
                Assert.That(
                    identity.Documentation,
                    Is.EqualTo("The identifier that stays the same across the versions of the Data."));

                var query = document.Interfaces!.Single(@interface => @interface.Name == "Query");

                Assert.That(query.RealizedBy, Is.Empty);
                Assert.That(query.OwnedOperations, Is.Empty);
                Assert.That(query.Documentation, Is.Empty);
            });
        }

        [Test]
        public void The_graph_carries_the_data_types_apart_from_the_primitive_types_and_enumerations()
        {
            var document = PimDocument();

            var uuid = document.DataTypes!.Single(dataType => dataType.Name == "UUID");

            Assert.Multiple(() =>
            {
                Assert.That(
                    document.DataTypes!.Select(dataType => dataType.Name), Is.EqualTo(new[] { "Timestamp", "UUID" }));

                Assert.That(uuid.XmiId, Is.EqualTo("_fixture_UUID"));
                Assert.That(uuid.Package, Is.EqualTo(PimFixture.PackageName));
                Assert.That(uuid.QualifiedName, Is.EqualTo("FixtureApi::ProjectDataVersioning::UUID"));
                Assert.That(uuid.Visibility, Is.EqualTo("public"));
                Assert.That(
                    uuid.Documentation,
                    Is.EqualTo("A universally unique identifier, in its canonical textual form."));

                Assert.That(
                    document.PrimitiveTypes.Select(primitiveType => primitiveType.Name),
                    Has.None.EqualTo("UUID").And.None.EqualTo("Timestamp"));
                Assert.That(
                    document.Enumerations.Select(enumeration => enumeration.Name), Does.Contain("ProjectState"));
                Assert.That(
                    document.DataTypes!.Select(dataType => dataType.Name), Does.Not.Contain("ProjectState"));
            });
        }

        [Test]
        public void The_serialized_graph_names_the_two_collections_in_camel_case()
        {
            var json = MetamodelJsonGenerator.Serialize(PimDocument());

            using var parsed = JsonDocument.Parse(json);
            var root = parsed.RootElement;

            Assert.Multiple(() =>
            {
                Assert.That(root.GetProperty("interfaces").GetArrayLength(), Is.EqualTo(2));
                Assert.That(root.GetProperty("dataTypes").GetArrayLength(), Is.EqualTo(2));

                var data = root.GetProperty("interfaces")[0];

                Assert.That(data.GetProperty("name").GetString(), Is.EqualTo("Data"));
                Assert.That(
                    data.GetProperty("realizedBy").EnumerateArray().Select(name => name.GetString()),
                    Does.Contain("Project"));
                Assert.That(
                    data.GetProperty("ownedOperations")[0].GetProperty("returnType").GetString(), Is.EqualTo("UUID"));

                Assert.That(json, Does.Not.Contain("\r"), "LF endings, independent of the host");
                Assert.That(json, Does.EndWith("}\n"));
            });
        }

        [Test]
        public void The_serialized_graph_is_deterministic()
        {
            Assert.That(
                MetamodelJsonGenerator.Serialize(PimDocument()),
                Is.EqualTo(MetamodelJsonGenerator.Serialize(PimDocument())));
        }

        [Test]
        public void The_lookup_resolves_an_interface_and_a_data_type_to_its_page()
        {
            var index = MetamodelJsonGenerator.BuildIndex(PimDocument());

            Assert.Multiple(() =>
            {
                Assert.That(
                    index.Entries["Data"],
                    Is.EqualTo(new LookupEntry(
                        "interface",
                        PimFixture.PackageName,
                        "FixtureApi::ProjectDataVersioning::Data",
                        false,
                        "Data is whatever a Commit can carry a version of.",
                        index.Entries["Data"].Extends,
                        "elements/Data.md")));
                Assert.That(index.Entries["Data"].Extends, Is.Empty);

                Assert.That(index.Entries["Query"].Kind, Is.EqualTo("interface"));

                Assert.That(index.Entries["UUID"].Kind, Is.EqualTo("dataType"));
                Assert.That(index.Entries["UUID"].File, Is.EqualTo("elements/UUID.md"));
                Assert.That(index.Entries["UUID"].IsAbstract, Is.False);
                Assert.That(index.Entries["UUID"].Extends, Is.Empty);
                Assert.That(index.Entries["Timestamp"].Kind, Is.EqualTo("dataType"));
                Assert.That(index.Entries["Timestamp"].Summary, Is.Empty);

                // The kinds that were already there keep their own.
                Assert.That(index.Entries["Record"].Kind, Is.EqualTo("class"));
                Assert.That(index.Entries["ProjectState"].Kind, Is.EqualTo("enumeration"));
                Assert.That(index.Entries["String"].Kind, Is.EqualTo("primitiveType"));
            });
        }

        [Test]
        public void The_serialized_lookup_spells_the_two_kinds_as_the_skills_read_them()
        {
            var json = MetamodelJsonGenerator.Serialize(MetamodelJsonGenerator.BuildIndex(PimDocument()));

            using var parsed = JsonDocument.Parse(json);
            var entries = parsed.RootElement.GetProperty("entries");

            Assert.Multiple(() =>
            {
                Assert.That(entries.GetProperty("Data").GetProperty("kind").GetString(), Is.EqualTo("interface"));
                Assert.That(entries.GetProperty("UUID").GetProperty("kind").GetString(), Is.EqualTo("dataType"));
                Assert.That(
                    entries.GetProperty("Data").GetProperty("file").GetString(), Is.EqualTo("elements/Data.md"));
            });
        }

        [Test]
        public void A_metamodel_document_has_neither_collection()
        {
            // Null rather than empty: the property is then left out, so a KerML or SysML document
            // keeps the exact shape it had - which is what the golden files compare.
            var model = TestModel.Model;
            if (model is null)
            {
                Assert.Ignore("No SysML model found in the fixture.");
            }

            var document = MetamodelJsonGenerator.BuildDocument(model!, Hash);
            var json = MetamodelJsonGenerator.Serialize(document);

            using var parsed = JsonDocument.Parse(json);

            Assert.Multiple(() =>
            {
                Assert.That(document.Interfaces, Is.Null);
                Assert.That(document.DataTypes, Is.Null);
                Assert.That(parsed.RootElement.TryGetProperty("interfaces", out _), Is.False);
                Assert.That(parsed.RootElement.TryGetProperty("dataTypes", out _), Is.False);
                Assert.That(json, Does.Not.Contain("\"interfaces\""));
                Assert.That(json, Does.Not.Contain("\"dataTypes\""));
            });
        }

        [Test]
        public void A_metamodel_lookup_has_no_entry_of_either_kind()
        {
            var model = TestModel.Model;
            if (model is null)
            {
                Assert.Ignore("No SysML model found in the fixture.");
            }

            var index = MetamodelJsonGenerator.BuildIndex(MetamodelJsonGenerator.BuildDocument(model!, Hash));

            Assert.That(
                index.Entries.Values.Select(entry => entry.Kind).Distinct(),
                Is.EquivalentTo(new[] { "class", "enumeration", "primitiveType" }));
        }

        private static MetamodelDocument PimDocument()
        {
            var model = PimFixture.Model;
            if (model is null)
            {
                Assert.Ignore("No PIM fixture found.");
            }

            return MetamodelJsonGenerator.BuildDocument(model!, Hash);
        }
    }
}
