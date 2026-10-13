// ------------------------------------------------------------------------------------------------
// <copyright file="DataTypeFileGeneratorTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.MetamodelGen.Tests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Hypha.MetamodelGen.Generators;

    using NUnit.Framework;

    using uml4net.SimpleClassifiers;

    /// <summary>
    /// Tests for <see cref="DataTypeFileGenerator"/>, exercised against the PIM-shaped fixture (see
    /// <see cref="PimFixture"/>) and written to a scratch directory (see <see cref="TestOutput"/>).
    /// </summary>
    [TestFixture]
    public class DataTypeFileGeneratorTests
    {
        private DataTypeFileGenerator generator = null!;

        [SetUp]
        public void SetUp()
        {
            this.generator = new DataTypeFileGenerator();
        }

        [Test]
        public void Renders_the_data_type_with_its_documentation()
        {
            var content = this.generator.GenerateElement(DataType("UUID")).Replace("\r\n", "\n");

            Assert.That(
                content.TrimEnd('\n'),
                Is.EqualTo(
                    """
                    ---
                    name: UUID
                    package: ProjectDataVersioning
                    fully qualified name: FixtureApi::ProjectDataVersioning::UUID
                    visibility: public
                    kind: datatype
                    ---

                    # UUID

                    `ProjectDataVersioning` package · data type

                    A universally unique identifier, in its canonical textual form.
                    """.Replace("\r\n", "\n")));
        }

        [Test]
        public void An_undocumented_data_type_gets_only_its_heading()
        {
            var content = this.generator.GenerateElement(DataType("Timestamp")).Replace("\r\n", "\n");

            Assert.That(
                content.TrimEnd('\n'),
                Is.EqualTo(
                    """
                    ---
                    name: Timestamp
                    package: ProjectDataVersioning
                    fully qualified name: FixtureApi::ProjectDataVersioning::Timestamp
                    visibility: public
                    kind: datatype
                    ---

                    # Timestamp

                    `ProjectDataVersioning` package · data type
                    """.Replace("\r\n", "\n")));
        }

        [Test]
        public void A_data_type_page_does_not_pass_for_a_primitive_type()
        {
            // The two share a payload, not a template: a skill filtering on `kind` must tell them apart.
            var content = this.generator.GenerateElement(DataType("UUID"));

            Assert.Multiple(() =>
            {
                Assert.That(content, Does.Contain("kind: datatype"));
                Assert.That(content, Does.Not.Contain("kind: primitive"));
                Assert.That(content, Does.Not.Contain("primitive type"));
            });
        }

        [Test]
        public async Task Generates_one_file_per_data_type_and_none_for_primitives_or_enumerations()
        {
            var model = PimFixture.Model;
            if (model is null)
            {
                Assert.Ignore("No PIM fixture found.");
            }

            var outputDirectory = Scratch("data-types");

            await this.generator.GenerateAsync(model!, outputDirectory);

            Assert.That(
                outputDirectory.EnumerateFiles().Select(file => file.Name).OrderBy(name => name, StringComparer.Ordinal),
                Is.EqualTo(new[] { "Timestamp.md", "UUID.md" }),
                "ProjectState is an enumeration and String a primitive type: each has a generator of its own");
        }

        [Test]
        public async Task Generates_nothing_for_a_metamodel()
        {
            // Neither KerML nor SysML declares such a data type, so their knowledge gains no file.
            var model = TestModel.Model;
            if (model is null)
            {
                Assert.Ignore("No SysML model found in the fixture.");
            }

            var outputDirectory = Scratch("data-types-of-a-metamodel");

            await this.generator.GenerateAsync(model!, outputDirectory);

            outputDirectory.Refresh();

            Assert.That(outputDirectory.Exists && outputDirectory.EnumerateFiles().Any(), Is.False);
        }

        [Test]
        public void Missing_arguments_are_refused()
        {
            Assert.Multiple(() =>
            {
                Assert.Throws<ArgumentNullException>(() => this.generator.GenerateElement(null!));
                Assert.That(
                    () => this.generator.GenerateAsync(null!, Scratch("unused")), Throws.ArgumentNullException);
                Assert.That(
                    () => this.generator.GenerateAsync(PimFixture.Model!, null!), Throws.ArgumentNullException);
            });
        }

        private static IDataType DataType(string name)
        {
            var model = PimFixture.Model;
            if (model is null)
            {
                Assert.Ignore("No PIM fixture found.");
            }

            return ElementCatalog.DataTypes(model!).Single(dataType => dataType.Name == name);
        }

        /// <summary>An empty scratch directory of this fixture's own.</summary>
        private static DirectoryInfo Scratch(string name)
        {
            var directory = TestOutput.Directory(PimFixture.Tag, nameof(DataTypeFileGeneratorTests), name);

            directory.Delete(recursive: true);
            directory.Refresh();

            return directory;
        }
    }
}
