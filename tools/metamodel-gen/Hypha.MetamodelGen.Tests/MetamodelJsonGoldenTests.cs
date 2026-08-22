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
    /// Golden-file tests for the JSON sidecar, compared against a committed expected file under
    /// <c>Expected/metamodel/</c> - the fixture model (see <see cref="TestModel"/>) is the only one
    /// exercised now that nothing per-release is committed.
    /// </summary>
    [TestFixture]
    public class MetamodelJsonGoldenTests
    {
        [Test]
        public void Generated_metamodel_json_matches_expected()
        {
            AssertMatchesExpected("metamodel.json", BuildMetamodelJson());
        }

        [Test]
        public void Generated_index_json_matches_expected()
        {
            AssertMatchesExpected("index.json", BuildIndexJson());
        }

        [Test]
        [Explicit("Regenerates the committed expected JSON; run manually after an intended format change.")]
        public void Bless_expected_files()
        {
            var directory = Path.Combine(
                Repository.Layout!.Root.FullName,
                "tools", "metamodel-gen", "Hypha.MetamodelGen.Tests", "Expected", "metamodel");
            Directory.CreateDirectory(directory);

            File.WriteAllText(
                Path.Combine(directory, "metamodel.json"), BuildMetamodelJson(), new UTF8Encoding(false));
            File.WriteAllText(
                Path.Combine(directory, "index.json"), BuildIndexJson(), new UTF8Encoding(false));
        }

        private static string BuildMetamodelJson()
        {
            var model = TestModel.Model;
            Assert.That(model, Is.Not.Null, "No SysML model found in the fixture.");

            var document = MetamodelJsonGenerator.BuildDocument(
                model!, JsonSidecarTestSupport.ComputeSourceHash());
            return MetamodelJsonGenerator.Serialize(document);
        }

        private static string BuildIndexJson()
        {
            var model = TestModel.Model;
            Assert.That(model, Is.Not.Null, "No SysML model found in the fixture.");

            var document = MetamodelJsonGenerator.BuildDocument(
                model!, JsonSidecarTestSupport.ComputeSourceHash());
            return MetamodelJsonGenerator.Serialize(MetamodelJsonGenerator.BuildIndex(document));
        }

        private static void AssertMatchesExpected(string fileName, string generated)
        {
            var expectedPath = Path.Combine(JsonSidecarTestSupport.ExpectedDirectory, fileName);
            Assert.That(File.Exists(expectedPath), Is.True, $"Missing expected golden file: {expectedPath}");

            var expected = JsonSidecarTestSupport.Normalize(File.ReadAllText(expectedPath));
            Assert.That(JsonSidecarTestSupport.Normalize(generated), Is.EqualTo(expected));
        }
    }
}
