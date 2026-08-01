// ------------------------------------------------------------------------------------------------
// <copyright file="PackageDiagramGoldenTests.cs" company="Starion Group S.A.">
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
    using System.Text;

    using Hypha.MetamodelGen.Generators;

    using NUnit.Framework;

    /// <summary>
    /// Golden-file tests for the package diagrams. As with the JSON sidecar, the committed
    /// <c>knowledge/metamodel/diagrams/*.md</c> ARE the golden files (regenerated in-memory and
    /// compared), so 41 diagrams are not duplicated under <c>Expected/</c>.
    /// </summary>
    [TestFixture]
    public class PackageDiagramGoldenTests
    {
        /// <summary>The package names to golden-test: whatever the generator produces, not a fixed list.</summary>
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
        public void Generated_diagram_matches_committed(string packageName)
        {
            var committedPath = Path.Combine(DiagramDirectory(), $"{packageName}.md");
            Assert.That(File.Exists(committedPath), Is.True, $"Missing committed diagram: {committedPath}");

            var committed = Normalize(File.ReadAllText(committedPath));

            Assert.That(Normalize(Build(packageName)), Is.EqualTo(committed));
        }

        [Test]
        [Explicit("Regenerates the committed knowledge-base diagrams; run manually after an intended format change.")]
        public void Bless_committed_files()
        {
            var directory = DiagramDirectory();
            Directory.CreateDirectory(directory);

            foreach (var packageName in PackageNames())
            {
                File.WriteAllText(
                    Path.Combine(directory, $"{packageName}.md"), Build(packageName), new UTF8Encoding(false));
            }
        }

        private static string Build(string packageName)
        {
            var model = TestModel.Model;
            Assert.That(model, Is.Not.Null, "No SysML model found under sources/xmi/.");

            var payloads = PackageDiagramGenerator.CreatePayloads(model!);
            var generator = new PackageDiagramGenerator();

            foreach (var payload in payloads)
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
                TestModel.FindRepoRoot()!.FullName,
                "knowledge",
                "metamodel",
                PackageDiagramGenerator.DiagramDirectoryName);

        private static string Normalize(string text) => text.Replace("\r\n", "\n").TrimEnd('\n');
    }
}
