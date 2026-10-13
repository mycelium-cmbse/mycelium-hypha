// ------------------------------------------------------------------------------------------------
// <copyright file="ModelVocabularyTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.MetamodelGen.Tests
{
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Hypha.MetamodelGen.Generators;

    using NUnit.Framework;

    /// <summary>
    /// Tests for <see cref="ModelVocabulary"/> and for the three generators that speak in it.
    /// </summary>
    /// <remarks>
    /// Two things are pinned down. A generator that is not told otherwise still says <i>metaclass</i>
    /// and <i>metamodel</i>, word for word as before - the KerML / SysML golden files depend on it.
    /// And one told it describes an ordinary model says <i>class</i> and <i>model</i> everywhere, with
    /// nothing else on the page changing.
    /// </remarks>
    [TestFixture]
    public class ModelVocabularyTests
    {
        [Test]
        public void A_metamodel_has_metaclasses_and_an_ordinary_model_has_classes()
        {
            Assert.Multiple(() =>
            {
                Assert.That(
                    ModelVocabulary.Metamodel,
                    Is.EqualTo(new ModelVocabulary("metamodel", "metaclass", "metaclasses")));
                Assert.That(ModelVocabulary.Model, Is.EqualTo(new ModelVocabulary("model", "class", "classes")));
            });
        }

        [Test]
        public void Every_generator_and_payload_describes_a_metamodel_unless_told_otherwise()
        {
            Assert.Multiple(() =>
            {
                Assert.That(new MetaclassFileGenerator().Vocabulary, Is.SameAs(ModelVocabulary.Metamodel));
                Assert.That(new MetamodelIndexGenerator().Vocabulary, Is.SameAs(ModelVocabulary.Metamodel));
                Assert.That(new PackageDiagramGenerator().Vocabulary, Is.SameAs(ModelVocabulary.Metamodel));
                Assert.That(DiagramPayload().Vocabulary, Is.SameAs(ModelVocabulary.Metamodel));
            });
        }

        [Test]
        public void A_class_page_says_metaclass_by_default()
        {
            var model = PimFixture.Model;
            if (model is null)
            {
                Assert.Ignore("No PIM fixture found.");
            }

            var generator = new MetaclassFileGenerator();

            Assert.Multiple(() =>
            {
                Assert.That(
                    ClassPage(generator, "Record"),
                    Does.Contain("\n`ProjectDataVersioning` package · abstract metaclass\n"));
                Assert.That(
                    ClassPage(generator, "Project"),
                    Does.Contain("\n`ProjectDataVersioning` package · concrete metaclass\n"));
            });
        }

        [Test]
        public void A_class_page_of_an_ordinary_model_says_class_and_changes_nothing_else()
        {
            var model = PimFixture.Model;
            if (model is null)
            {
                Assert.Ignore("No PIM fixture found.");
            }

            var asMetamodel = new MetaclassFileGenerator();
            var asModel = new MetaclassFileGenerator { Vocabulary = ModelVocabulary.Model };

            Assert.Multiple(() =>
            {
                foreach (var name in new[] { "Record", "Project", "Commit" })
                {
                    var page = ClassPage(asModel, name);

                    Assert.That(page, Does.Not.Contain("metaclass"), name);
                    Assert.That(
                        page,
                        Is.EqualTo(ClassPage(asMetamodel, name)
                            .Replace(" · abstract metaclass\n", " · abstract class\n")
                            .Replace(" · concrete metaclass\n", " · concrete class\n")),
                        $"only the noun on {name}'s page may differ");
                }

                Assert.That(ClassPage(asModel, "Record"), Does.Contain(" package · abstract class\n"));
                Assert.That(ClassPage(asModel, "Project"), Does.Contain(" package · concrete class\n"));
            });
        }

        [Test]
        public void The_index_says_metamodel_and_metaclasses_by_default()
        {
            var model = PimFixture.Model;
            if (model is null)
            {
                Assert.Ignore("No PIM fixture found.");
            }

            var content = new MetamodelIndexGenerator().GenerateIndex(model!, "Fixture").Replace("\r\n", "\n");

            Assert.Multiple(() =>
            {
                Assert.That(content, Does.StartWith("# Fixture metamodel index\n"));
                Assert.That(content, Does.Contain(" metaclasses across "));
            });
        }

        [Test]
        public void The_index_of_an_ordinary_model_says_model_and_classes_and_changes_nothing_else()
        {
            var model = PimFixture.Model;
            if (model is null)
            {
                Assert.Ignore("No PIM fixture found.");
            }

            var asMetamodel = new MetamodelIndexGenerator().GenerateIndex(model!, "Fixture").Replace("\r\n", "\n");
            var asModel = new MetamodelIndexGenerator { Vocabulary = ModelVocabulary.Model }
                .GenerateIndex(model!, "Fixture")
                .Replace("\r\n", "\n");

            Assert.Multiple(() =>
            {
                Assert.That(asModel, Does.StartWith("# Fixture model index\n"));
                Assert.That(asModel, Does.Contain(" classes across "));
                Assert.That(asModel, Does.Not.Contain("metaclass"));
                Assert.That(asModel, Does.Not.Contain("metamodel index"));
                Assert.That(
                    asModel,
                    Is.EqualTo(asMetamodel
                        .Replace("# Fixture metamodel index\n", "# Fixture model index\n")
                        .Replace(" metaclasses across ", " classes across ")),
                    "only the nouns may differ");
            });
        }

        [Test]
        public void The_index_lists_interfaces_and_data_types_between_the_packages_and_the_enumerations()
        {
            var model = PimFixture.Model;
            if (model is null)
            {
                Assert.Ignore("No PIM fixture found.");
            }

            var content = new MetamodelIndexGenerator { Vocabulary = ModelVocabulary.Model }
                .GenerateIndex(model!, "Fixture")
                .Replace("\r\n", "\n");

            Assert.Multiple(() =>
            {
                Assert.That(
                    content,
                    Does.Contain(
                        "\n## Interfaces\n\n"
                        + "- [Data](elements/Data.md) — Data is whatever a Commit can carry a version of.\n"
                        + "- [Query](elements/Query.md)\n"));
                Assert.That(
                    content,
                    Does.Contain(
                        "\n## Data types\n\n"
                        + "- [Timestamp](elements/Timestamp.md)\n"
                        + "- [UUID](elements/UUID.md) — A universally unique identifier, in its canonical textual form.\n"));

                Assert.That(
                    new[]
                    {
                        content.IndexOf("\n## ProjectDataVersioning\n", System.StringComparison.Ordinal),
                        content.IndexOf("\n## Interfaces\n", System.StringComparison.Ordinal),
                        content.IndexOf("\n## Data types\n", System.StringComparison.Ordinal),
                        content.IndexOf("\n## Enumeration types\n", System.StringComparison.Ordinal),
                        content.IndexOf("\n## Primitive types\n", System.StringComparison.Ordinal),
                    },
                    Is.Ordered.Ascending.And.All.GreaterThan(0));
            });
        }

        [Test]
        public void The_index_of_a_metamodel_gains_no_interface_or_data_type_section()
        {
            var model = TestModel.Model;
            if (model is null)
            {
                Assert.Ignore("No SysML model found in the fixture.");
            }

            var content = new MetamodelIndexGenerator().GenerateIndex(model!, "SysML v2").Replace("\r\n", "\n");

            // SysML has a package called Interfaces, so that heading is there - once, as a package.
            var headings = content.Split('\n').Where(line => line == "## Interfaces").ToList();

            Assert.Multiple(() =>
            {
                Assert.That(headings, Has.Count.EqualTo(1));
                Assert.That(content, Does.Contain("\n## Interfaces\n\n[Class diagram](diagrams/Interfaces.md)\n"));
                Assert.That(content, Does.Not.Contain("## Data types"));
            });
        }

        [Test]
        public void A_diagram_says_metaclasses_by_default()
        {
            var content = new PackageDiagramGenerator().GenerateDiagram(DiagramPayload()).Replace("\r\n", "\n");

            Assert.Multiple(() =>
            {
                Assert.That(content, Does.Contain("\n1 metaclasses in this package, 0 boundary nodes from other packages.\n"));
                Assert.That(content, Does.Contain("box; metaclass-typed features are shown as edges"));
            });
        }

        [Test]
        public void A_diagram_of_an_ordinary_model_says_classes_and_changes_nothing_else()
        {
            var generator = new PackageDiagramGenerator();

            var asMetamodel = generator.GenerateDiagram(DiagramPayload()).Replace("\r\n", "\n");

            var payload = DiagramPayload();
            payload.Vocabulary = ModelVocabulary.Model;
            var asModel = generator.GenerateDiagram(payload).Replace("\r\n", "\n");

            Assert.Multiple(() =>
            {
                Assert.That(asModel, Does.Contain("\n1 classes in this package, 0 boundary nodes from other packages.\n"));
                Assert.That(asModel, Does.Contain("box; class-typed features are shown as edges"));
                Assert.That(asModel, Does.Not.Contain("metaclass"));
                Assert.That(
                    asModel,
                    Is.EqualTo(asMetamodel
                        .Replace(" metaclasses in this package", " classes in this package")
                        .Replace("; metaclass-typed features", "; class-typed features")),
                    "only the nouns may differ");
            });
        }

        [Test]
        public async Task The_diagram_generator_passes_its_vocabulary_on_to_every_diagram_it_writes()
        {
            // The words live on the payload the template reads, so the generator has to hand them over.
            var model = PimFixture.Model;
            if (model is null)
            {
                Assert.Ignore("No PIM fixture found.");
            }

            var asMetamodel = TestOutput.Directory(PimFixture.Tag, nameof(ModelVocabularyTests), "metamodel");
            var asModel = TestOutput.Directory(PimFixture.Tag, nameof(ModelVocabularyTests), "model");

            await new PackageDiagramGenerator().GenerateAsync(model!, asMetamodel);
            await new PackageDiagramGenerator { Vocabulary = ModelVocabulary.Model }.GenerateAsync(model!, asModel);

            var diagram = Path.Combine(PackageDiagramGenerator.DiagramDirectoryName, $"{PimFixture.PackageName}.md");

            Assert.Multiple(() =>
            {
                Assert.That(
                    File.ReadAllText(Path.Combine(asMetamodel.FullName, diagram)),
                    Does.Contain(" metaclasses in this package").And.Contain("metaclass-typed"));
                Assert.That(
                    File.ReadAllText(Path.Combine(asModel.FullName, diagram)),
                    Does.Contain(" classes in this package").And.Contain("class-typed").And.Not.Contain("metaclass"));
            });
        }

        private static string ClassPage(MetaclassFileGenerator generator, string name)
        {
            var model = PimFixture.Model!;
            var classes = MetaclassFileGenerator.QueryMetaclasses(model);

            return generator
                .GenerateElement(
                    classes.Single(@class => @class.Name == name),
                    MetaclassFileGenerator.BuildSubtypeIndex(classes),
                    ElementCatalog.LinkableTypeNames(model))
                .Replace("\r\n", "\n");
        }

        private static PackageDiagramPayload DiagramPayload() =>
            new()
            {
                Package = "Parts",
                Classes = [new DiagramClass { Name = "Engine", IsAbstract = false, Attributes = [] }],
                BoundaryClasses = [],
                Relationships = [],
            };
    }
}
