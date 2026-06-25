// ------------------------------------------------------------------------------------------------
// <copyright file="EnumerationAndPrimitiveGoldenTests.cs" company="Starion Group S.A.">
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
    /// Golden-file tests for the enumeration and primitive-type element files. Every enumeration and
    /// primitive type in the model is compared byte-for-byte against a committed expected file under
    /// <c>Expected/sysml2/elements/</c>.
    /// </summary>
    [TestFixture]
    public class EnumerationAndPrimitiveGoldenTests
    {
        /// <summary>The enumeration names to golden-test (every enumeration in the model).</summary>
        public static IEnumerable<string> EnumerationNames()
        {
            var model = TestModel.Model;
            if (model is null)
            {
                yield break;
            }

            foreach (var enumeration in ElementCatalog.Enumerations(model))
            {
                yield return enumeration.Name;
            }
        }

        /// <summary>The primitive-type names to golden-test (every primitive type in the model).</summary>
        public static IEnumerable<string> PrimitiveTypeNames()
        {
            var model = TestModel.Model;
            if (model is null)
            {
                yield break;
            }

            foreach (var primitiveType in ElementCatalog.PrimitiveTypes(model))
            {
                yield return primitiveType.Name;
            }
        }

        [TestCaseSource(nameof(EnumerationNames))]
        public void Generated_enumeration_matches_expected(string enumerationName)
        {
            var model = TestModel.Model;
            Assert.That(model, Is.Not.Null, "No SysML model found under sources/xmi/.");

            var enumeration = ElementCatalog.Enumerations(model!).Single(e => e.Name == enumerationName);
            var generated = Normalize(new EnumerationFileGenerator().GenerateElement(enumeration));

            AssertMatchesExpected(enumerationName, generated);
        }

        [TestCaseSource(nameof(PrimitiveTypeNames))]
        public void Generated_primitive_type_matches_expected(string primitiveTypeName)
        {
            var model = TestModel.Model;
            Assert.That(model, Is.Not.Null, "No SysML model found under sources/xmi/.");

            var primitiveType = ElementCatalog.PrimitiveTypes(model!).Single(p => p.Name == primitiveTypeName);
            var generated = Normalize(new PrimitiveTypeFileGenerator().GenerateElement(primitiveType));

            AssertMatchesExpected(primitiveTypeName, generated);
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

            var enumerationGenerator = new EnumerationFileGenerator();
            foreach (var enumeration in ElementCatalog.Enumerations(model!))
            {
                File.WriteAllText(
                    Path.Combine(expectedDirectory, $"{enumeration.Name}.md"),
                    enumerationGenerator.GenerateElement(enumeration),
                    new UTF8Encoding(false));
            }

            var primitiveGenerator = new PrimitiveTypeFileGenerator();
            foreach (var primitiveType in ElementCatalog.PrimitiveTypes(model!))
            {
                File.WriteAllText(
                    Path.Combine(expectedDirectory, $"{primitiveType.Name}.md"),
                    primitiveGenerator.GenerateElement(primitiveType),
                    new UTF8Encoding(false));
            }
        }

        private static void AssertMatchesExpected(string name, string generated)
        {
            var expectedPath = Path.Combine(
                TestContext.CurrentContext.TestDirectory, "Expected", "sysml2", "elements", $"{name}.md");

            Assert.That(File.Exists(expectedPath), Is.True, $"Missing expected golden file: {expectedPath}");

            var expected = Normalize(File.ReadAllText(expectedPath));

            Assert.That(generated, Is.EqualTo(expected));
        }

        private static string Normalize(string text)
        {
            return text.Replace("\r\n", "\n").TrimEnd('\n');
        }
    }
}
