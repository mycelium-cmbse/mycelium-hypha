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
    /// files under <c>knowledge/metamodel/elements/</c>.
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
            if (TestModel.Model is null)
            {
                Assert.Ignore("No SysML model found in the fixture.");
            }

            DirectoryInfo? outputDirectory = null;

            // Every installed release is regenerated, not just the default one.
            foreach (var tag in TestModel.Tags)
            {
                var model = TestModel.ModelFor(tag);
                Assert.That(model, Is.Not.Null, $"No model found for release {tag}.");

                outputDirectory = TestOutput.Directory(tag, "elements");

                await this.generator.GenerateAsync(model!, outputDirectory);
            }

            var partUsage = Path.Combine(outputDirectory!.FullName, "PartUsage.md");
            Assert.That(File.Exists(partUsage), Is.True);

            var content = await File.ReadAllTextAsync(partUsage);
            Assert.Multiple(() =>
            {
                Assert.That(content, Does.StartWith("---\nname: PartUsage").IgnoreCase.Or.StartWith("---\r\nname: PartUsage"));
                Assert.That(content, Does.Contain("\n# PartUsage"));
            });
        }

        [Test]
        public void Element_files_render_every_operation_with_its_body_condition()
        {
            var model = TestModel.Model;
            if (model is null)
            {
                Assert.Ignore("No SysML model found in the fixture.");
            }

            var metaclasses = MetaclassFileGenerator.QueryMetaclasses(model!);
            var subtypeIndex = MetaclassFileGenerator.BuildSubtypeIndex(metaclasses);
            var linkableTypeNames = ElementCatalog.LinkableTypeNames(model!);
            var withBody = 0;

            foreach (var metaclass in metaclasses.Where(@class => @class.OwnedOperation.Count > 0))
            {
                var content = this.generator.GenerateElement(metaclass, subtypeIndex, linkableTypeNames).Replace("\r\n", "\n");

                Assert.That(content, Does.Contain("\n## Operations\n"), metaclass.Name);

                foreach (var operation in metaclass.OwnedOperation)
                {
                    Assert.That(content, Does.Contain($"\n### {operation.Name}\n"), $"{metaclass.Name}::{operation.Name}");

                    var body = operation.QueryBodyConditionText();
                    if (body.Length > 0)
                    {
                        Assert.That(content, Does.Contain($"```ocl\n{body}\n```"), $"{metaclass.Name}::{operation.Name}");
                        withBody++;
                    }
                }
            }

            Assert.That(withBody, Is.GreaterThan(0), "The fixture has no operation with a bodyCondition to check.");
        }
    }
}
