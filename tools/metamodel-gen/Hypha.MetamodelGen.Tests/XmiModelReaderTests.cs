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
                Assert.Ignore("No XMI file found under sources/xmi/. Add the KerML/SysML v2 XMI there to enable this test.");
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
        /// Walks up from the test output directory to the repository root and returns the metamodel
        /// entry point under <c>sources/xmi/</c>: the <c>*.uml</c> model is preferred, falling back to
        /// a <c>*.xmi</c> file (e.g. referenced primitive types). Returns <c>null</c> if none is found.
        /// </summary>
        private static string? TryLocateXmiFile()
        {
            for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
            {
                var xmiDir = Path.Combine(dir.FullName, "sources", "xmi");
                if (Directory.Exists(xmiDir))
                {
                    var files = Directory.EnumerateFiles(xmiDir, "*.*", SearchOption.AllDirectories).ToList();

                    return files.FirstOrDefault(f => f.EndsWith(".uml", StringComparison.OrdinalIgnoreCase))
                        ?? files.FirstOrDefault(f => f.EndsWith(".xmi", StringComparison.OrdinalIgnoreCase));
                }
            }

            return null;
        }
    }
}
