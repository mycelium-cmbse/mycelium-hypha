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
        /// <summary>
        /// Every (release tag, package) pair to golden-test. Both dimensions are discovered – the
        /// tags from the manifest, the packages from the generator – never hard-coded.
        /// </summary>
        private static IEnumerable<TestCaseData> TaggedPackages()
        {
            foreach (var tag in TestModel.Tags)
            {
                var model = TestModel.ModelFor(tag);
                if (model is null)
                {
                    continue;
                }

                foreach (var payload in PackageDiagramGenerator.CreatePayloads(model))
                {
                    yield return new TestCaseData(tag, payload.Package).SetName(
                        $"Generated_diagram_matches_committed({tag},{payload.Package})");
                }
            }
        }

        [TestCaseSource(nameof(TaggedPackages))]
        public void Generated_diagram_matches_committed(string tag, string packageName)
        {
            var committedPath = Path.Combine(DiagramDirectory(tag), $"{packageName}.md");
            Assert.That(File.Exists(committedPath), Is.True, $"Missing committed diagram: {committedPath}");

            var committed = Normalize(File.ReadAllText(committedPath));

            Assert.That(Normalize(Build(tag, packageName)), Is.EqualTo(committed));
        }

        [Test]
        [Explicit("Regenerates the committed knowledge-base diagrams; run manually after an intended format change.")]
        public void Bless_committed_files()
        {
            foreach (var tag in TestModel.Tags)
            {
                var model = TestModel.ModelFor(tag);
                if (model is null)
                {
                    continue;
                }

                var directory = DiagramDirectory(tag);
                Directory.CreateDirectory(directory);

                foreach (var payload in PackageDiagramGenerator.CreatePayloads(model))
                {
                    File.WriteAllText(
                        Path.Combine(directory, $"{payload.Package}.md"),
                        Build(tag, payload.Package),
                        new UTF8Encoding(false));
                }
            }
        }

        private static string Build(string tag, string packageName)
        {
            var model = TestModel.ModelFor(tag);
            Assert.That(model, Is.Not.Null, $"No SysML model found under sources/{tag}/xmi/.");

            var generator = new PackageDiagramGenerator();

            foreach (var payload in PackageDiagramGenerator.CreatePayloads(model!))
            {
                if (payload.Package == packageName)
                {
                    return generator.GenerateDiagram(payload);
                }
            }

            Assert.Fail($"The generator produced no payload for package '{packageName}' at {tag}.");
            return string.Empty;
        }

        private static string DiagramDirectory(string tag) =>
            Path.Combine(
                KnowledgeVersions.MetamodelDirectory(tag).FullName,
                PackageDiagramGenerator.DiagramDirectoryName);

        private static string Normalize(string text) => text.Replace("\r\n", "\n").TrimEnd('\n');
    }
}
