// ------------------------------------------------------------------------------------------------
// <copyright file="MetaclassFileGeneratorTests.cs" company="Starion Group S.A.">
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
    /// Tests for <see cref="MetaclassFileGenerator"/>. Running these regenerates the per-metaclass
    /// files under <c>knowledge/sysml2/elements/</c>.
    /// </summary>
    [TestFixture]
    public class MetaclassFileGeneratorTests
    {
        private MetaclassFileGenerator generator = null!;

        [SetUp]
        public void SetUp()
        {
            this.generator = new MetaclassFileGenerator();
        }

        [Test]
        public async Task Generates_per_metaclass_element_files()
        {
            var model = TestModel.LoadSysmlModel();
            if (model is null)
            {
                Assert.Ignore("No SysML *.uml model found under sources/xmi/.");
            }

            var outputDirectory = new DirectoryInfo(
                Path.Combine(TestModel.FindRepoRoot()!.FullName, "knowledge", "sysml2", "elements"));

            await this.generator.GenerateAsync(model!, outputDirectory);

            var partUsage = Path.Combine(outputDirectory.FullName, "PartUsage.md");
            Assert.That(File.Exists(partUsage), Is.True);

            var content = await File.ReadAllTextAsync(partUsage);
            Assert.That(content, Does.StartWith("# PartUsage"));
        }
    }
}
