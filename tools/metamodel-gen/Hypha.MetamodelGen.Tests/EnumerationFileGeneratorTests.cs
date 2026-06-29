// ------------------------------------------------------------------------------------------------
// <copyright file="EnumerationFileGeneratorTests.cs" company="Starion Group S.A.">
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
    /// Tests for <see cref="EnumerationFileGenerator"/>. Running these regenerates the per-enumeration
    /// files under <c>knowledge/metamodel/elements/</c>.
    /// </summary>
    [TestFixture]
    public class EnumerationFileGeneratorTests
    {
        private EnumerationFileGenerator generator = null!;

        [SetUp]
        public void SetUp()
        {
            this.generator = new EnumerationFileGenerator();
        }

        [Test]
        public async Task Generates_per_enumeration_element_files()
        {
            var model = TestModel.LoadSysmlModel();
            if (model is null)
            {
                Assert.Ignore("No SysML *.uml model found under sources/xmi/.");
            }

            var outputDirectory = new DirectoryInfo(
                Path.Combine(TestModel.FindRepoRoot()!.FullName, "knowledge", "metamodel", "elements"));

            await this.generator.GenerateAsync(model!, outputDirectory);

            var featureDirectionKind = Path.Combine(outputDirectory.FullName, "FeatureDirectionKind.md");
            Assert.That(File.Exists(featureDirectionKind), Is.True);

            var content = await File.ReadAllTextAsync(featureDirectionKind);
            Assert.Multiple(() =>
            {
                Assert.That(content, Does.StartWith("---\nname: FeatureDirectionKind").IgnoreCase.Or.StartWith("---\r\nname: FeatureDirectionKind"));
                Assert.That(content, Does.Contain("kind: enumeration"));
                Assert.That(content, Does.Contain("\n## Literals"));
            });
        }
    }
}
