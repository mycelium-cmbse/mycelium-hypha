// ------------------------------------------------------------------------------------------------
// <copyright file="XmiModelReaderTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.MetamodelGen.Tests
{
    using System;
    using System.IO;
    using System.Linq;

    using NUnit.Framework;

    /// <summary>
    /// Smoke tests proving the uml4net dependency and the XMI input are wired up correctly.
    /// </summary>
    [TestFixture]
    public class XmiModelReaderTests
    {
        [Test]
        public void Reads_xmi_and_enumerates_top_level_packages()
        {
            var xmiPath = TryLocateXmiFile();
            if (xmiPath is null)
            {
                Assert.Ignore(
                    "No XMI file found under sources/<tag>/xmi/. Fetch a release to enable this test.");
            }

            var result = XmiModelReader.Read(xmiPath!);

            Assert.That(result.Packages, Is.Not.Empty);

            foreach (var package in result.Packages)
            {
                var name = package.GetType().GetProperty("Name")?.GetValue(package) as string;
                Assert.That(string.IsNullOrWhiteSpace(name), Is.False, "Every top-level package should have a name.");
            }
        }

        /// <summary>
        /// Returns the metamodel entry point for the default release, under
        /// <c>sources/&lt;tag&gt;/xmi/</c>: the <c>*.uml</c> model is preferred, falling back to a
        /// <c>*.xmi</c> file (e.g. the shared primitive types). Returns <c>null</c> if none is found.
        /// </summary>
        private static string? TryLocateXmiFile()
        {
            if (Repository.Layout?.DefaultTag is not { } tag)
            {
                return null;
            }

            var xmiDir = Repository.Layout!.Xmi(tag);
            if (!xmiDir.Exists)
            {
                return null;
            }

            var files = Directory.EnumerateFiles(xmiDir.FullName, "*.*", SearchOption.AllDirectories).ToList();

            return files.FirstOrDefault(f => f.EndsWith(".uml", StringComparison.OrdinalIgnoreCase))
                ?? files.FirstOrDefault(f => f.EndsWith(".xmi", StringComparison.OrdinalIgnoreCase));
        }
    }
}
