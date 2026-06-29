// ------------------------------------------------------------------------------------------------
// <copyright file="PrimitiveTypeFileGeneratorTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.MetamodelGen.Tests
{
    using System.IO;
    using System.Threading.Tasks;

    using Hypha.MetamodelGen.Generators;

    using NUnit.Framework;

    /// <summary>
    /// Tests for <see cref="PrimitiveTypeFileGenerator"/>. Running these regenerates the
    /// per-primitive-type files under <c>knowledge/metamodel/elements/</c>.
    /// </summary>
    [TestFixture]
    public class PrimitiveTypeFileGeneratorTests
    {
        private PrimitiveTypeFileGenerator generator = null!;

        [SetUp]
        public void SetUp()
        {
            this.generator = new PrimitiveTypeFileGenerator();
        }

        [Test]
        public async Task Generates_per_primitive_type_element_files()
        {
            var model = TestModel.LoadSysmlModel();
            if (model is null)
            {
                Assert.Ignore("No SysML *.uml model found under sources/xmi/.");
            }

            var outputDirectory = new DirectoryInfo(
                Path.Combine(TestModel.FindRepoRoot()!.FullName, "knowledge", "metamodel", "elements"));

            await this.generator.GenerateAsync(model!, outputDirectory);

            var stringType = Path.Combine(outputDirectory.FullName, "String.md");
            Assert.That(File.Exists(stringType), Is.True);

            var content = await File.ReadAllTextAsync(stringType);
            Assert.Multiple(() =>
            {
                Assert.That(content, Does.StartWith("---\nname: String").IgnoreCase.Or.StartWith("---\r\nname: String"));
                Assert.That(content, Does.Contain("kind: primitive"));
                Assert.That(content, Does.Contain("\n# String"));
            });
        }
    }
}
