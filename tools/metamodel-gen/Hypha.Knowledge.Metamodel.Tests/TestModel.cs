// ------------------------------------------------------------------------------------------------
// <copyright file="TestModel.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Metamodel.Tests
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

    internal static class TestModel
    {
        public const string FixtureTag = "2026-05";

        // The pathmap URI the SysML metamodel uses to reference the UML primitive types library.
        private const string PrimitiveTypesPathMap = "pathmap://UML_LIBRARIES/UMLPrimitiveTypes.library.uml";

        private static readonly Lazy<XmiReaderResult?> LazyModel = new(Load);

        public static IReadOnlyList<string> Tags => [FixtureTag];

        public static XmiReaderResult? Model => LazyModel.Value;

        public static XmiReaderResult? ModelFor(string tag) =>
            string.Equals(tag, FixtureTag, StringComparison.Ordinal) ? Model : null;

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

        public static IReadOnlyList<IClass> QueryInterestingMetaclasses(XmiReaderResult model)
        {
            ArgumentNullException.ThrowIfNull(model);

            var inspector = new ModelInspector(NullLoggerFactory.Instance);

            return model.Packages
                .SelectMany(package => inspector.QueryInterestingClasses(package, includeOperations: true))
                .OfType<IClass>()
                .DistinctBy(@class => @class.XmiId)
                .OrderBy(@class => @class.Name, StringComparer.Ordinal)
                .ToList();
        }
    }
}
