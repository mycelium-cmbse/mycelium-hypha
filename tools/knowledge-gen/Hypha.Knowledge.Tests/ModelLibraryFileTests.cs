// ------------------------------------------------------------------------------------------------
// <copyright file="ModelLibraryFileTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Hypha.Knowledge.ModelLibrary;

    [TestFixture]
    public class ModelLibraryFileTests
    {
        private DirectoryInfo scratch = null!;

        [SetUp]
        public void SetUp() =>
            this.scratch = Directory.CreateDirectory(
                Path.Combine(Path.GetTempPath(), $"hypha-library-file-{Guid.NewGuid():N}"));

        [TearDown]
        public void TearDown() => this.scratch.Delete(recursive: true);

        [Test]
        public void A_rendered_index_reads_back_unchanged()
        {
            var original = new ModelLibraryDocument(
                ModelLibraryDocument.CurrentSchemaVersion,
                "2026-05",
                new ModelLibraryCounts(2, 3),
                new Dictionary<string, ModelLibraryDeclarationEntry>(StringComparer.Ordinal)
                {
                    ["ISQBase::mass"] = new("attribute", "packages/ISQBase.md", "ISQBase.sysml"),
                    ["ScalarValues"] = new("standard library package", "packages/ScalarValues.md", "ScalarValues.kerml"),
                    ["ScalarValues::Real"] = new("datatype", "packages/ScalarValues.md", "ScalarValues.kerml"),
                });
            var path = Path.Combine(this.scratch.FullName, ModelLibraryDocument.FileName);
            File.WriteAllText(path, ModelLibraryFile.Render(original));

            var read = ModelLibraryFile.ReadIfPresent(path);

            Assert.Multiple(() =>
            {
                Assert.That(read, Is.Not.Null);
                Assert.That(read!.Tag, Is.EqualTo("2026-05"));
                Assert.That(read.Counts, Is.EqualTo(original.Counts));
                Assert.That(read.Declarations, Is.EqualTo(original.Declarations));
                Assert.That(ModelLibraryFile.Render(read), Is.EqualTo(ModelLibraryFile.Render(original)));
            });
        }

        [Test]
        public void A_missing_index_reads_as_null()
        {
            Assert.That(ModelLibraryFile.ReadIfPresent(Path.Combine(this.scratch.FullName, "index.json")), Is.Null);
        }

        [TestCase("")]
        [TestCase(" ")]
        public void A_blank_path_is_refused(string path)
        {
            Assert.That(() => ModelLibraryFile.ReadIfPresent(path), Throws.ArgumentException);
        }
    }
}
