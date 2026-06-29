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
    using System.IO;
    using System.Text;

    using Hypha.MetamodelGen.Generators;

    using NUnit.Framework;

    /// <summary>
    /// Golden-file tests for the JSON sidecar. The committed <c>knowledge/sysml2/metamodel.json</c>
    /// and <c>index.json</c> ARE the golden files (regenerated in-memory and compared), so the large
    /// rich graph is not duplicated under <c>Expected/</c>.
    /// </summary>
    [TestFixture]
    public class MetamodelJsonGoldenTests
    {
        [Test]
        public void Generated_metamodel_json_matches_committed()
        {
            AssertMatchesCommitted("metamodel.json", BuildMetamodelJson());
        }

        [Test]
        public void Generated_index_json_matches_committed()
        {
            AssertMatchesCommitted("index.json", BuildIndexJson());
        }

        [Test]
        [Explicit("Regenerates the committed knowledge-base JSON; run manually after an intended format change.")]
        public void Bless_committed_files()
        {
            var directory = JsonSidecarTestSupport.KnowledgeDirectory().FullName;
            File.WriteAllText(Path.Combine(directory, "metamodel.json"), BuildMetamodelJson(), new UTF8Encoding(false));
            File.WriteAllText(Path.Combine(directory, "index.json"), BuildIndexJson(), new UTF8Encoding(false));
        }

        private static string BuildMetamodelJson()
        {
            var model = TestModel.Model;
            Assert.That(model, Is.Not.Null, "No SysML model found under sources/xmi/.");

            var document = MetamodelJsonGenerator.BuildDocument(model!, JsonSidecarTestSupport.ComputeSourceHash());
            return MetamodelJsonGenerator.Serialize(document);
        }

        private static string BuildIndexJson()
        {
            var model = TestModel.Model;
            Assert.That(model, Is.Not.Null, "No SysML model found under sources/xmi/.");

            var document = MetamodelJsonGenerator.BuildDocument(model!, JsonSidecarTestSupport.ComputeSourceHash());
            return MetamodelJsonGenerator.Serialize(MetamodelJsonGenerator.BuildIndex(document));
        }

        private static void AssertMatchesCommitted(string fileName, string generated)
        {
            var committedPath = Path.Combine(JsonSidecarTestSupport.KnowledgeDirectory().FullName, fileName);
            Assert.That(File.Exists(committedPath), Is.True, $"Missing committed file: {committedPath}");

            var committed = JsonSidecarTestSupport.Normalize(File.ReadAllText(committedPath));
            Assert.That(JsonSidecarTestSupport.Normalize(generated), Is.EqualTo(committed));
        }
    }
}
