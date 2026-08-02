// ------------------------------------------------------------------------------------------------
// <copyright file="MetamodelJsonGoldenTests.cs" company="Starion Group S.A.">
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
    /// Golden-file tests for the JSON sidecar. The committed <c>knowledge/metamodel/metamodel.json</c>
    /// and <c>index.json</c> ARE the golden files (regenerated in-memory and compared), so the large
    /// rich graph is not duplicated under <c>Expected/</c>.
    /// </summary>
    [TestFixture]
    public class MetamodelJsonGoldenTests
    {
        /// <summary>The installed release tags; each is golden-tested in its own right.</summary>
        private static IEnumerable<string> Tags() => TestModel.Tags;

        [TestCaseSource(nameof(Tags))]
        public void Generated_metamodel_json_matches_committed(string tag)
        {
            AssertMatchesCommitted(tag, "metamodel.json", BuildMetamodelJson(tag));
        }

        [TestCaseSource(nameof(Tags))]
        public void Generated_index_json_matches_committed(string tag)
        {
            AssertMatchesCommitted(tag, "index.json", BuildIndexJson(tag));
        }

        [Test]
        [Explicit("Regenerates the committed knowledge-base JSON; run manually after an intended format change.")]
        public void Bless_committed_files()
        {
            foreach (var tag in TestModel.Tags)
            {
                var directory = JsonSidecarTestSupport.KnowledgeDirectory(tag).FullName;
                Directory.CreateDirectory(directory);

                File.WriteAllText(
                    Path.Combine(directory, "metamodel.json"), BuildMetamodelJson(tag), new UTF8Encoding(false));
                File.WriteAllText(
                    Path.Combine(directory, "index.json"), BuildIndexJson(tag), new UTF8Encoding(false));
            }
        }

        private static string BuildMetamodelJson(string tag)
        {
            var model = TestModel.ModelFor(tag);
            Assert.That(model, Is.Not.Null, $"No SysML model found under sources/{tag}/xmi/.");

            var document = MetamodelJsonGenerator.BuildDocument(
                model!, JsonSidecarTestSupport.ComputeSourceHash(tag));
            return MetamodelJsonGenerator.Serialize(document);
        }

        private static string BuildIndexJson(string tag)
        {
            var model = TestModel.ModelFor(tag);
            Assert.That(model, Is.Not.Null, $"No SysML model found under sources/{tag}/xmi/.");

            var document = MetamodelJsonGenerator.BuildDocument(
                model!, JsonSidecarTestSupport.ComputeSourceHash(tag));
            return MetamodelJsonGenerator.Serialize(MetamodelJsonGenerator.BuildIndex(document));
        }

        private static void AssertMatchesCommitted(string tag, string fileName, string generated)
        {
            var committedPath = Path.Combine(JsonSidecarTestSupport.KnowledgeDirectory(tag).FullName, fileName);
            Assert.That(File.Exists(committedPath), Is.True, $"Missing committed file: {committedPath}");

            var committed = JsonSidecarTestSupport.Normalize(File.ReadAllText(committedPath));
            Assert.That(JsonSidecarTestSupport.Normalize(generated), Is.EqualTo(committed));
        }
    }
}
