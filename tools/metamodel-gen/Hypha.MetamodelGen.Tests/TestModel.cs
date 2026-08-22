// ------------------------------------------------------------------------------------------------
// <copyright file="TestModel.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.MetamodelGen.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Microsoft.Extensions.Logging.Abstractions;

    using NUnit.Framework;

    using uml4net.Reporting.Generators;
    using uml4net.StructuredClassifiers;
    using uml4net.xmi.Readers;

    /// <summary>
    /// Helpers for loading the committed test fixture's SysML v2 model and querying it via uml4net.
    /// </summary>
    /// <remarks>
    /// Every per-release input under <c>sources/&lt;tag&gt;/</c> is fetched and generated locally now
    /// (see <c>CLAUDE.md</c>) - nothing there is committed for tests to read any more. One real
    /// release's XMI is committed here instead, under <c>Fixtures/xmi/</c>: a fixture for this test
    /// project alone (never part of what a plugin install fetches or ships), captured from
    /// <see cref="FixtureTag"/>. <c>PrimitiveTypes.xmi</c> stays shared at the repository's
    /// <c>sources/</c> root: it is the OMG UML primitives library, tag-independent and unrelated to
    /// any one release.
    /// </remarks>
    internal static class TestModel
    {
        /// <summary>The release the committed fixture XMI was captured from.</summary>
        public const string FixtureTag = "2026-05";

        // The pathmap URI the SysML metamodel uses to reference the UML primitive types library.
        private const string PrimitiveTypesPathMap = "pathmap://UML_LIBRARIES/UMLPrimitiveTypes.library.uml";

        private static readonly Lazy<XmiReaderResult?> LazyModel = new(Load);

        /// <summary>
        /// Gets <see cref="FixtureTag"/> as a single-element list, so tests written to loop over every
        /// installed release keep working unchanged against the one committed fixture.
        /// </summary>
        public static IReadOnlyList<string> Tags => [FixtureTag];

        /// <summary>
        /// Gets the fixture model, loaded once (or <c>null</c> when the fixture is somehow absent).
        /// </summary>
        public static XmiReaderResult? Model => LazyModel.Value;

        /// <summary>Gets the fixture model for <paramref name="tag"/>, or <c>null</c> for any other tag.</summary>
        public static XmiReaderResult? ModelFor(string tag) =>
            string.Equals(tag, FixtureTag, StringComparison.Ordinal) ? Model : null;

        /// <summary>
        /// Loads the fixture's SysML v2 metamodel from <c>Fixtures/xmi/SysML_only_xmi.uml</c>. The
        /// SysML document references the KerML abstract syntax (resolved as a local file in the same
        /// folder) and the UML primitive types (resolved via a path map to the shared
        /// <c>sources/PrimitiveTypes.xmi</c>), so reading SysML as the root yields the complete
        /// KerML + SysML model with fully resolved generalization chains. Returns <c>null</c> if the
        /// fixture is not present.
        /// </summary>
        private static XmiReaderResult? Load()
        {
            if (Repository.Layout is not { } layout)
            {
                return null;
            }

            var xmiDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "Fixtures", "xmi");
            var modelPath = Path.Combine(xmiDirectory, "SysML_only_xmi.uml");

            if (!File.Exists(modelPath))
            {
                return null;
            }

            var pathMaps = new Dictionary<string, string>
            {
                [PrimitiveTypesPathMap] = layout.SharedPrimitiveTypes.FullName,
            };

            return XmiModelReader.Read(modelPath, pathMaps, xmiDirectory);
        }

        /// <summary>
        /// Returns the "interesting" metaclasses (the minimal set covering property/type variations
        /// as well as operation argument/return-type variations, concrete and abstract) as determined
        /// by uml4net's <see cref="ModelInspector"/>. The expected metaclasses are derived from
        /// uml4net, never hard-coded.
        /// </summary>
        public static IReadOnlyList<IClass> QueryInterestingMetaclasses(XmiReaderResult model)
        {
            ArgumentNullException.ThrowIfNull(model);

            var inspector = new ModelInspector(NullLoggerFactory.Instance);

            return model.Packages
                .SelectMany(package => inspector.QueryInterestingClasses(package, includeOperations: true))
                .DistinctBy(@class => @class.XmiId)
                .OrderBy(@class => @class.Name, StringComparer.Ordinal)
                .ToList();
        }
    }
}
