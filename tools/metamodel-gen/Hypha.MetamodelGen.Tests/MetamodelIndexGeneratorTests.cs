// ------------------------------------------------------------------------------------------------
// <copyright file="MetamodelIndexGeneratorTests.cs" company="Starion Group S.A.">
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
    /// Tests for <see cref="MetamodelIndexGenerator"/>. Running these regenerates
    /// <c>knowledge/sysml2/index.md</c> from the SysML metamodel XMI.
    /// </summary>
    [TestFixture]
    public class MetamodelIndexGeneratorTests
    {
        private MetamodelIndexGenerator generator = null!;

        [SetUp]
        public void SetUp()
        {
            this.generator = new MetamodelIndexGenerator();
        }

        [Test]
        public async Task Generates_a_deterministic_sysml2_index()
        {
            var model = TestModel.LoadSysmlModel();
            if (model is null)
            {
                Assert.Ignore("No SysML *.uml model found under sources/xmi/.");
            }

            // The metaclasses to verify come from uml4net's ModelInspector, not a hard-coded list.
            var interestingMetaclasses = TestModel.QueryInterestingMetaclasses(model!);

            var content = this.generator.GenerateIndex(model!, "SysML v2");

            Assert.Multiple(() =>
            {
                Assert.That(content, Does.StartWith("# SysML v2 metamodel index"));
                Assert.That(content, Does.Contain("metaclasses across"));

                Assert.That(interestingMetaclasses, Is.Not.Empty, "ModelInspector returned no interesting classes");

                foreach (var metaclass in interestingMetaclasses)
                {
                    Assert.That(
                        content,
                        Does.Contain($"[{metaclass.Name}](elements/{metaclass.Name}.md)"),
                        $"the index should contain an entry for the " +
                        $"{(metaclass.IsAbstract ? "abstract" : "concrete")} metaclass '{metaclass.Name}'");
                }
            });

            // Output must be deterministic for diff-friendly regeneration.
            Assert.That(this.generator.GenerateIndex(model!, "SysML v2"), Is.EqualTo(content));

            // Produce the committed knowledge base file.
            var outputDirectory = new DirectoryInfo(
                Path.Combine(TestModel.FindRepoRoot()!.FullName, "knowledge", "sysml2"));

            await this.generator.GenerateAsync(model!, outputDirectory, "SysML v2");

            Assert.That(File.Exists(Path.Combine(outputDirectory.FullName, "index.md")), Is.True);
        }
    }
}
