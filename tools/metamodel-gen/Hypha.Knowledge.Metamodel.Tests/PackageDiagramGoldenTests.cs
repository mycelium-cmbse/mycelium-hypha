// ------------------------------------------------------------------------------------------------
// <copyright file="PackageDiagramGoldenTests.cs" company="Starion Group S.A.">
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
    using System.Text;

    using Hypha.Knowledge.Metamodel.Generators;

    using NUnit.Framework;

    [TestFixture]
    public class PackageDiagramGoldenTests
    {
        private static IEnumerable<string> PackageNames()
        {
            var model = TestModel.Model;
            if (model is null)
            {
                yield break;
            }

            foreach (var payload in PackageDiagramGenerator.CreatePayloads(model))
            {
                yield return payload.Package;
            }
        }

        [TestCaseSource(nameof(PackageNames))]
        public void Generated_diagram_matches_expected(string packageName)
        {
            var expectedPath = Path.Combine(DiagramDirectory(), $"{packageName}.md");
            Assert.That(File.Exists(expectedPath), Is.True, $"Missing expected diagram: {expectedPath}");

            var expected = Normalize(File.ReadAllText(expectedPath));

            Assert.That(Normalize(Build(packageName)), Is.EqualTo(expected));
        }

        [Test]
        [Explicit("Regenerates the committed expected diagrams; run manually after an intended format change.")]
        public void Bless_expected_files()
        {
            var model = TestModel.Model;
            if (model is null)
            {
                return;
            }

            var directory = Path.Combine(
                Repository.Layout!.Root.FullName,
                "tools", "metamodel-gen", "Hypha.Knowledge.Metamodel.Tests", "Expected", "metamodel",
                PackageDiagramGenerator.DiagramDirectoryName);
            Directory.CreateDirectory(directory);

            foreach (var payload in PackageDiagramGenerator.CreatePayloads(model))
            {
                File.WriteAllText(
                    Path.Combine(directory, $"{payload.Package}.md"),
                    Build(payload.Package),
                    new UTF8Encoding(false));
            }
        }

        private static string Build(string packageName)
        {
            var model = TestModel.Model;
            Assert.That(model, Is.Not.Null, "No SysML model found in the fixture.");

            var generator = new PackageDiagramGenerator();

            foreach (var payload in PackageDiagramGenerator.CreatePayloads(model!))
            {
                if (payload.Package == packageName)
                {
                    return generator.GenerateDiagram(payload);
                }
            }

            Assert.Fail($"The generator produced no payload for package '{packageName}'.");
            return string.Empty;
        }

        private static string DiagramDirectory() =>
            Path.Combine(
                TestContext.CurrentContext.TestDirectory, "Expected", "metamodel",
                PackageDiagramGenerator.DiagramDirectoryName);

        private static string Normalize(string text) => text.Replace("\r\n", "\n").TrimEnd('\n');
    }
}
