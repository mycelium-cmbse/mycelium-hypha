// ------------------------------------------------------------------------------------------------
// <copyright file="MetaclassFileGoldenTests.cs" company="Starion Group S.A.">
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
    using System.Text;

    using Hypha.MetamodelGen.Generators;

    using NUnit.Framework;

    /// <summary>
    /// Golden-file tests: the generated element markdown for each "interesting" metaclass (as
    /// determined by uml4net's <see cref="uml4net.Reporting.Generators.ModelInspector"/>) is
    /// compared byte-for-byte against a committed expected file under <c>Expected/sysml2/elements/</c>.
    /// </summary>
    [TestFixture]
    public class MetaclassFileGoldenTests
    {
        /// <summary>
        /// The metaclass names to golden-test: the ModelInspector's interesting classes (not hard-coded).
        /// </summary>
        private static IEnumerable<string> InterestingMetaclassNames()
        {
            var model = TestModel.Model;
            if (model is null)
            {
                yield break;
            }

            foreach (var metaclass in TestModel.QueryInterestingMetaclasses(model))
            {
                yield return metaclass.Name;
            }
        }

        [TestCaseSource(nameof(InterestingMetaclassNames))]
        public void Generated_element_matches_expected(string metaclassName)
        {
            var model = TestModel.Model;
            Assert.That(model, Is.Not.Null, "No SysML model found under sources/xmi/.");

            var metaclasses = MetaclassFileGenerator.QueryMetaclasses(model!);
            var subtypeIndex = MetaclassFileGenerator.BuildSubtypeIndex(metaclasses);
            var linkableTypeNames = ElementCatalog.LinkableTypeNames(model!);
            var metaclass = metaclasses.Single(@class => @class.Name == metaclassName);
            var generated = Normalize(new MetaclassFileGenerator().GenerateElement(metaclass, subtypeIndex, linkableTypeNames));

            var expectedPath = Path.Combine(
                TestContext.CurrentContext.TestDirectory, "Expected", "sysml2", "elements", $"{metaclassName}.md");

            Assert.That(File.Exists(expectedPath), Is.True, $"Missing expected golden file: {expectedPath}");

            var expected = Normalize(File.ReadAllText(expectedPath));

            Assert.That(generated, Is.EqualTo(expected));
        }

        [Test]
        [Explicit("Regenerates the committed expected golden files; run manually after an intended format change.")]
        public void Bless_expected_files()
        {
            var model = TestModel.Model;
            Assert.That(model, Is.Not.Null);

            var expectedDirectory = Path.Combine(
                TestModel.FindRepoRoot()!.FullName,
                "tools", "metamodel-gen", "Hypha.MetamodelGen.Tests", "Expected", "sysml2", "elements");

            Directory.CreateDirectory(expectedDirectory);

            var generator = new MetaclassFileGenerator();
            var metaclasses = MetaclassFileGenerator.QueryMetaclasses(model!);
            var subtypeIndex = MetaclassFileGenerator.BuildSubtypeIndex(metaclasses);
            var linkableTypeNames = ElementCatalog.LinkableTypeNames(model!);

            foreach (var name in InterestingMetaclassNames())
            {
                var metaclass = metaclasses.Single(@class => @class.Name == name);
                var content = generator.GenerateElement(metaclass, subtypeIndex, linkableTypeNames);

                File.WriteAllText(Path.Combine(expectedDirectory, $"{name}.md"), content, new UTF8Encoding(false));
            }
        }

        private static string Normalize(string text)
        {
            return text.Replace("\r\n", "\n").TrimEnd('\n');
        }
    }
}
