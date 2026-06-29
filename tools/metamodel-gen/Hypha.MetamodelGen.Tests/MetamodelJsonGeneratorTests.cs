// ------------------------------------------------------------------------------------------------
// <copyright file="MetamodelJsonGeneratorTests.cs" company="Starion Group S.A.">
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
    /// Tests for <see cref="MetamodelJsonGenerator"/>. Running these regenerates
    /// <c>knowledge/sysml2/metamodel.json</c> and <c>index.json</c>.
    /// </summary>
    [TestFixture]
    public class MetamodelJsonGeneratorTests
    {
        private MetamodelJsonGenerator generator = null!;

        [SetUp]
        public void SetUp()
        {
            this.generator = new MetamodelJsonGenerator();
        }

        [Test]
        public async Task Generates_deterministic_sidecar_files()
        {
            var model = TestModel.LoadSysmlModel();
            if (model is null)
            {
                Assert.Ignore("No SysML *.uml model found under sources/xmi/.");
            }

            var hash = JsonSidecarTestSupport.ComputeSourceHash();

            var document = MetamodelJsonGenerator.BuildDocument(model!, hash);

            Assert.Multiple(() =>
            {
                Assert.That(document.Metamodel, Is.EqualTo("SysML"));
                Assert.That(document.ModelVersionUri, Does.StartWith("https://www.omg.org/spec/SysML"));
                Assert.That(document.Counts.Classes, Is.GreaterThan(0));
                Assert.That(document.Classes.Any(@class => @class.Name == "PartUsage"), Is.True);
                Assert.That(document.Counts.Classes, Is.EqualTo(document.Classes.Count));
            });

            // Deterministic: the same model serializes byte-for-byte identically.
            var first = MetamodelJsonGenerator.Serialize(MetamodelJsonGenerator.BuildDocument(model!, hash));
            var second = MetamodelJsonGenerator.Serialize(MetamodelJsonGenerator.BuildDocument(model!, hash));
            Assert.That(second, Is.EqualTo(first));

            // Produce the committed knowledge-base files.
            var outputDirectory = JsonSidecarTestSupport.KnowledgeDirectory();
            await this.generator.GenerateAsync(model!, outputDirectory, hash);

            Assert.Multiple(() =>
            {
                Assert.That(File.Exists(Path.Combine(outputDirectory.FullName, "metamodel.json")), Is.True);
                Assert.That(File.Exists(Path.Combine(outputDirectory.FullName, "index.json")), Is.True);
            });
        }
    }
}
