// ------------------------------------------------------------------------------------------------
// <copyright file="InterfaceFileGeneratorTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.MetamodelGen.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Hypha.MetamodelGen.Generators;

    using NUnit.Framework;

    using uml4net.SimpleClassifiers;

    /// <summary>
    /// Tests for <see cref="InterfaceFileGenerator"/>, exercised against the PIM-shaped fixture (see
    /// <see cref="PimFixture"/>) and written to a scratch directory (see <see cref="TestOutput"/>).
    /// </summary>
    [TestFixture]
    public class InterfaceFileGeneratorTests
    {
        private InterfaceFileGenerator generator = null!;

        [SetUp]
        public void SetUp()
        {
            this.generator = new InterfaceFileGenerator();
        }

        [Test]
        public void Renders_the_interface_with_its_realizers_and_operations()
        {
            var data = Interface("Data");

            var content = this.generator
                .GenerateElement(data, ["Project"], new HashSet<string> { "Project", "UUID" })
                .Replace("\r\n", "\n");

            Assert.That(
                content.TrimEnd('\n'),
                Is.EqualTo(
                    """
                    ---
                    name: Data
                    package: ProjectDataVersioning
                    fully qualified name: FixtureApi::ProjectDataVersioning::Data
                    visibility: public
                    kind: interface
                    realizedBy: [Project]
                    ---

                    # Data

                    `ProjectDataVersioning` package · interface

                    Data is whatever a Commit can carry a version of.

                    ## Realized by

                    - [Project](Project.md)

                    ## Operations

                    ### identity

                    `identity() : UUID [1..1]` · *query*

                    The identifier that stays the same across the versions of the Data.
                    """.Replace("\r\n", "\n")));
        }

        [Test]
        public void A_realizer_without_a_page_is_named_but_not_linked()
        {
            // A realizing classifier may live in another model, which has no page here to link to.
            var content = this.generator
                .GenerateElement(Interface("Data"), ["Element", "Project"], new HashSet<string> { "Project" })
                .Replace("\r\n", "\n");

            Assert.Multiple(() =>
            {
                Assert.That(content, Does.Contain("\nrealizedBy: [Element, Project]\n"));
                Assert.That(content, Does.Contain("\n- Element\n"));
                Assert.That(content, Does.Not.Contain("(Element.md)"));
                Assert.That(content, Does.Contain("\n- [Project](Project.md)\n"));
            });
        }

        [Test]
        public void An_interface_with_no_realizers_operations_or_documentation_gets_only_its_heading()
        {
            var content = this.generator
                .GenerateElement(Interface("Query"), [], new HashSet<string>())
                .Replace("\r\n", "\n");

            Assert.Multiple(() =>
            {
                Assert.That(content, Does.Contain("\nkind: interface\n"));
                Assert.That(content, Does.Contain("\nrealizedBy: []\n"));
                Assert.That(content, Does.Contain("\n# Query\n"));
                Assert.That(content, Does.Not.Contain("## Realized by"));
                Assert.That(content, Does.Not.Contain("## Operations"));
            });
        }

        [Test]
        public void Rendering_is_deterministic()
        {
            var data = Interface("Data");
            var linkable = ElementCatalog.LinkableTypeNames(PimFixture.Model!);
            var realizedBy = ElementCatalog.RealizedBy(PimFixture.Model!, data);

            Assert.That(
                this.generator.GenerateElement(data, realizedBy, linkable),
                Is.EqualTo(new InterfaceFileGenerator().GenerateElement(data, realizedBy, linkable)));
        }

        [Test]
        public async Task Generates_one_file_per_interface_and_nothing_else()
        {
            var model = PimFixture.Model;
            if (model is null)
            {
                Assert.Ignore("No PIM fixture found.");
            }

            var outputDirectory = Scratch("interfaces");

            await this.generator.GenerateAsync(model!, outputDirectory);

            var content = (await File.ReadAllTextAsync(Path.Combine(outputDirectory.FullName, "Data.md")))
                .Replace("\r\n", "\n");

            Assert.Multiple(() =>
            {
                Assert.That(
                    outputDirectory.EnumerateFiles().Select(file => file.Name).OrderBy(name => name, StringComparer.Ordinal),
                    Is.EqualTo(new[] { "Data.md", "Query.md" }));
                Assert.That(content, Does.StartWith("---\nname: Data\n"));
                Assert.That(content, Does.Contain("\nkind: interface\n"));
                Assert.That(
                    content, Does.Contain("\n- [Project](Project.md)\n"),
                    "the realizers are read from the model's realizations");
            });
        }

        [Test]
        public async Task Generates_nothing_for_a_metamodel()
        {
            // Neither KerML nor SysML declares an interface, so their knowledge gains no file.
            var model = TestModel.Model;
            if (model is null)
            {
                Assert.Ignore("No SysML model found in the fixture.");
            }

            var outputDirectory = Scratch("interfaces-of-a-metamodel");

            await this.generator.GenerateAsync(model!, outputDirectory);

            outputDirectory.Refresh();

            Assert.That(outputDirectory.Exists && outputDirectory.EnumerateFiles().Any(), Is.False);
        }

        [Test]
        public void Missing_arguments_are_refused()
        {
            var data = Interface("Data");
            var linkable = new HashSet<string>();

            Assert.Multiple(() =>
            {
                Assert.Throws<ArgumentNullException>(() => this.generator.GenerateElement(null!, [], linkable));
                Assert.Throws<ArgumentNullException>(() => this.generator.GenerateElement(data, null!, linkable));
                Assert.Throws<ArgumentNullException>(() => this.generator.GenerateElement(data, [], null!));
                Assert.That(
                    () => this.generator.GenerateAsync(null!, Scratch("unused")), Throws.ArgumentNullException);
                Assert.That(
                    () => this.generator.GenerateAsync(PimFixture.Model!, null!), Throws.ArgumentNullException);
            });
        }

        private static IInterface Interface(string name)
        {
            var model = PimFixture.Model;
            if (model is null)
            {
                Assert.Ignore("No PIM fixture found.");
            }

            return ElementCatalog.Interfaces(model!).Single(@interface => @interface.Name == name);
        }

        /// <summary>An empty scratch directory of this fixture's own.</summary>
        private static DirectoryInfo Scratch(string name)
        {
            var directory = TestOutput.Directory(PimFixture.Tag, nameof(InterfaceFileGeneratorTests), name);

            directory.Delete(recursive: true);
            directory.Refresh();

            return directory;
        }
    }
}
