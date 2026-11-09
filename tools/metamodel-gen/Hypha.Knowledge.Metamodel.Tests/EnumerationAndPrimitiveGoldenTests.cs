// ------------------------------------------------------------------------------------------------
// <copyright file="EnumerationAndPrimitiveGoldenTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Metamodel.Tests
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Hypha.Knowledge.Metamodel.Generators;

    using NUnit.Framework;

    [TestFixture]
    public class EnumerationAndPrimitiveGoldenTests
    {
        private static IEnumerable<string> EnumerationNames()
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

        private static IEnumerable<string> PrimitiveTypeNames()
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
            Assert.That(model, Is.Not.Null, "No SysML model found in the fixture.");

            var enumeration = ElementCatalog.Enumerations(model!).Single(e => e.Name == enumerationName);
            var generated = Normalize(new EnumerationFileGenerator().GenerateElement(enumeration));

            AssertMatchesExpected(enumerationName, generated);
        }

        [TestCaseSource(nameof(PrimitiveTypeNames))]
        public void Generated_primitive_type_matches_expected(string primitiveTypeName)
        {
            var model = TestModel.Model;
            Assert.That(model, Is.Not.Null, "No SysML model found in the fixture.");

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
                Repository.Layout!.Root.FullName,
                "tools", "metamodel-gen", "Hypha.Knowledge.Metamodel.Tests", "Expected", "metamodel", "elements");

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
                TestContext.CurrentContext.TestDirectory, "Expected", "metamodel", "elements", $"{name}.md");

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
