// ------------------------------------------------------------------------------------------------
// <copyright file="PackageDiagramGeneratorTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.MetamodelGen.Tests
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Hypha.MetamodelGen.Generators;

    using NUnit.Framework;

    /// <summary>
    /// Tests for <see cref="PackageDiagramGenerator"/>. The rendering rules are exercised on
    /// hand-built payloads (no model needed); the extraction rules are exercised against the real
    /// SysML metamodel. Running the last test regenerates <c>knowledge/metamodel/diagrams/</c>.
    /// </summary>
    [TestFixture]
    public class PackageDiagramGeneratorTests
    {
        private PackageDiagramGenerator generator = null!;

        [SetUp]
        public void SetUp()
        {
            this.generator = new PackageDiagramGenerator();
        }

        [Test]
        public void Renders_a_class_without_a_body_when_it_is_concrete_and_has_no_attributes()
        {
            var content = this.generator.GenerateDiagram(Payload(
                new DiagramClass { Name = "Engine", IsAbstract = false, Attributes = [] }));

            Assert.That(content, Does.Contain("    class Engine\n").Or.Contain("    class Engine\r\n"));
            Assert.That(content, Does.Not.Contain("class Engine {"));
        }

        [Test]
        public void Marks_an_abstract_class_with_the_mermaid_annotation()
        {
            var content = this.generator.GenerateDiagram(Payload(
                new DiagramClass { Name = "Feature", IsAbstract = true, Attributes = [] }));

            Assert.That(content, Does.Contain("class Feature {"));
            Assert.That(content, Does.Contain("<<abstract>>"));
        }

        [Test]
        public void Renders_primitive_typed_features_inside_the_class_box()
        {
            var content = this.generator.GenerateDiagram(Payload(
                new DiagramClass
                {
                    Name = "Comment",
                    IsAbstract = false,
                    Attributes =
                    [
                        new DiagramAttribute { Name = "body", Type = "String", Multiplicity = "[1]" },
                    ],
                }));

            Assert.That(content, Does.Contain("+String body [1]"));
        }

        [Test]
        public void Renders_generalization_composition_and_association_edges()
        {
            var payload = new PackageDiagramPayload
            {
                Package = "Elements",
                Classes = [new DiagramClass { Name = "Element", IsAbstract = true, Attributes = [] }],
                BoundaryClasses = ["Relationship"],
                Relationships =
                [
                    new DiagramRelationship
                    {
                        Left = "Element", Arrow = DiagramRelationship.Generalization, Right = "Comment",
                    },
                    new DiagramRelationship
                    {
                        Left = "Element",
                        Arrow = DiagramRelationship.Composition,
                        Right = "Relationship",
                        Multiplicity = "0..*",
                        Label = "ownedRelationship",
                    },
                    new DiagramRelationship
                    {
                        Left = "Element",
                        Arrow = DiagramRelationship.Association,
                        Right = "Relationship",
                        Multiplicity = "1",
                        Label = "owner",
                    },
                ],
            };

            var content = this.generator.GenerateDiagram(payload);

            Assert.Multiple(() =>
            {
                Assert.That(content, Does.Contain("Element <|-- Comment"));
                Assert.That(content, Does.Contain("Element *-- \"0..*\" Relationship : ownedRelationship"));
                Assert.That(content, Does.Contain("Element --> \"1\" Relationship : owner"));
                Assert.That(content, Does.Contain("class Relationship"), "boundary nodes must be declared");
            });
        }

        [Test]
        public void Opens_a_mermaid_block_and_links_the_element_files()
        {
            var content = this.generator.GenerateDiagram(Payload(
                new DiagramClass { Name = "Engine", IsAbstract = false, Attributes = [] }));

            Assert.Multiple(() =>
            {
                Assert.That(content, Does.StartWith("# Parts — class diagram"));
                Assert.That(content, Does.Contain("```mermaid\nclassDiagram").IgnoreCase.Or
                    .Contain("```mermaid\r\nclassDiagram").IgnoreCase);
                Assert.That(content, Does.Contain("- [Engine](../elements/Engine.md)"));
            });
        }

        [Test]
        public void Excludes_derived_features_from_the_real_model()
        {
            var model = TestModel.Model;
            if (model is null)
            {
                Assert.Ignore("No SysML *.uml model found under sources/xmi/.");
            }

            var payloads = PackageDiagramGenerator.CreatePayloads(model!);
            var types = TypesPackage(payloads);

            // In the Types package, Type.feature and Type.ownedGeneralization are derived, while
            // Specialization.general is not. Only the non-derived one may appear as an edge.
            Assert.Multiple(() =>
            {
                Assert.That(
                    types.Relationships.Any(relationship => relationship.Label == "feature"),
                    Is.False,
                    "derived features must not be drawn");
                Assert.That(
                    types.Relationships.Any(relationship => relationship.Label == "ownedGeneralization"),
                    Is.False,
                    "derived features must not be drawn, whatever their type");
                Assert.That(
                    types.Relationships.Any(relationship =>
                        relationship.Left == "Specialization"
                        && relationship.Label == "general"
                        && relationship.Right == "Type"
                        && relationship.Arrow == DiagramRelationship.Association),
                    Is.True,
                    "non-derived metaclass-typed features must be drawn as edges");
            });
        }

        [Test]
        public void Draws_metaclass_typed_features_as_edges_and_primitives_as_attributes()
        {
            var model = TestModel.Model;
            if (model is null)
            {
                Assert.Ignore("No SysML *.uml model found under sources/xmi/.");
            }

            var payloads = PackageDiagramGenerator.CreatePayloads(model!);
            var types = TypesPackage(payloads);
            var type = types.Classes.Single(@class => @class.Name == "Type");

            Assert.Multiple(() =>
            {
                Assert.That(
                    type.Attributes.Any(attribute => attribute.Name == "isAbstract" && attribute.Type == "Boolean"),
                    Is.True,
                    "primitive-typed features belong in the class box");
                Assert.That(
                    types.Relationships.Any(relationship => relationship.Right == "Boolean"),
                    Is.False,
                    "primitives must never become nodes");
            });
        }

        [Test]
        public void Pulls_out_of_package_supertypes_in_as_boundary_nodes()
        {
            var model = TestModel.Model;
            if (model is null)
            {
                Assert.Ignore("No SysML *.uml model found under sources/xmi/.");
            }

            var payloads = PackageDiagramGenerator.CreatePayloads(model!);
            var annotations = payloads.Single(payload => payload.Package == "Annotations");

            Assert.Multiple(() =>
            {
                // AnnotatingElement generalizes Element, which lives in the Elements package.
                Assert.That(annotations.BoundaryClasses, Does.Contain("Element"));
                Assert.That(
                    annotations.Classes.Select(@class => @class.Name),
                    Does.Not.Contain("Element"),
                    "a boundary node must not be listed as one of the package's own classes");
                Assert.That(
                    annotations.Relationships.Any(relationship =>
                        relationship.Left == "Element"
                        && relationship.Arrow == DiagramRelationship.Generalization
                        && relationship.Right == "AnnotatingElement"),
                    Is.True);
            });
        }

        [Test]
        public void Produces_a_payload_for_every_package_that_owns_metaclasses()
        {
            var model = TestModel.Model;
            if (model is null)
            {
                Assert.Ignore("No SysML *.uml model found under sources/xmi/.");
            }

            var payloads = PackageDiagramGenerator.CreatePayloads(model!);

            Assert.Multiple(() =>
            {
                Assert.That(payloads, Is.Not.Empty);
                Assert.That(
                    payloads.All(payload => payload.Classes.Count > 0),
                    Is.True,
                    "packages without metaclasses must be skipped, not emitted empty");
                Assert.That(
                    payloads.Select(payload => payload.Package),
                    Is.Ordered.Using<string>(System.StringComparer.Ordinal),
                    "payloads must be ordered deterministically");
            });
        }

        [Test]
        public void Merges_packages_that_share_a_name()
        {
            var model = TestModel.Model;
            if (model is null)
            {
                Assert.Ignore("No SysML *.uml model found under sources/xmi/.");
            }

            var payloads = PackageDiagramGenerator.CreatePayloads(model!);

            Assert.That(
                payloads.Select(payload => payload.Package),
                Is.Unique,
                "one diagram file per package name - a duplicate would silently overwrite the other");

            // KerML::Kernel::Metadata and SysML::Systems::Metadata are distinct packages with the same
            // name; the merged diagram must carry the metaclasses of both.
            var metadata = payloads.Single(payload => payload.Package == "Metadata");
            var names = metadata.Classes.Select(@class => @class.Name).ToList();

            Assert.That(names, Does.Contain("Metaclass").And.Contain("MetadataDefinition"));
        }

        [Test]
        public async Task Generates_deterministic_diagrams_for_the_knowledge_base()
        {
            var model = TestModel.Model;
            if (model is null)
            {
                Assert.Ignore("No SysML *.uml model found under sources/xmi/.");
            }

            var payloads = PackageDiagramGenerator.CreatePayloads(model!);
            var first = this.generator.GenerateDiagram(payloads[0]);

            Assert.That(this.generator.GenerateDiagram(payloads[0]), Is.EqualTo(first));

            // Produce the committed knowledge base files, for every installed release.
            foreach (var tag in TestModel.Tags)
            {
                var tagModel = TestModel.ModelFor(tag);
                Assert.That(tagModel, Is.Not.Null, $"No model found for release {tag}.");

                var outputDirectory = KnowledgeVersions.MetamodelDirectory(tag);
                await this.generator.GenerateAsync(tagModel!, outputDirectory);

                var diagramDirectory = Path.Combine(
                    outputDirectory.FullName, PackageDiagramGenerator.DiagramDirectoryName);
                var expected = PackageDiagramGenerator.CreatePayloads(tagModel!)[0].Package;

                Assert.That(File.Exists(Path.Combine(diagramDirectory, $"{expected}.md")), Is.True);
            }
        }

        private static PackageDiagramPayload TypesPackage(IReadOnlyList<PackageDiagramPayload> payloads) =>
            payloads.Single(payload => payload.Package == "Types");

        private static PackageDiagramPayload Payload(params DiagramClass[] classes) =>
            new()
            {
                Package = "Parts",
                Classes = classes,
                BoundaryClasses = [],
                Relationships = [],
            };
    }
}
