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

    using uml4net.Reporting.Generators;
    using uml4net.StructuredClassifiers;
    using uml4net.xmi.Readers;

    /// <summary>
    /// Helpers for locating the (committed) XMI inputs from the test output directory, loading the
    /// model, and querying it via uml4net.
    /// </summary>
    internal static class TestModel
    {
        // The pathmap URI the SysML metamodel uses to reference the UML primitive types library.
        private const string PrimitiveTypesPathMap = "pathmap://UML_LIBRARIES/UMLPrimitiveTypes.library.uml";

        private static XmiReaderResult? cachedModel;
        private static bool modelLoaded;

        /// <summary>
        /// Gets the SysML model, loaded once per test run (or <c>null</c> if no model is present).
        /// </summary>
        public static XmiReaderResult? Model
        {
            get
            {
                if (!modelLoaded)
                {
                    cachedModel = LoadSysmlModel();
                    modelLoaded = true;
                }

                return cachedModel;
            }
        }

        /// <summary>
        /// Walks up from the test output directory to the repository root (the directory that
        /// contains both <c>sources/xmi</c> and <c>knowledge</c>), or <c>null</c> if not found.
        /// </summary>
        public static DirectoryInfo? FindRepoRoot()
        {
            for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
            {
                if (Directory.Exists(Path.Combine(dir.FullName, "sources", "xmi"))
                    && Directory.Exists(Path.Combine(dir.FullName, "knowledge")))
                {
                    return dir;
                }
            }

            return null;
        }

        /// <summary>
        /// Loads the SysML metamodel from <c>sources/xmi/*.uml</c>, resolving the referenced UML
        /// primitive types via a path map. Returns <c>null</c> if no model is present.
        /// </summary>
        public static XmiReaderResult? LoadSysmlModel()
        {
            var root = FindRepoRoot();
            if (root is null)
            {
                return null;
            }

            var xmiDirectory = Path.Combine(root.FullName, "sources", "xmi");

            var modelPath = Directory
                .EnumerateFiles(xmiDirectory, "*.uml", SearchOption.AllDirectories)
                .FirstOrDefault();

            if (modelPath is null)
            {
                return null;
            }

            var pathMaps = new Dictionary<string, string>
            {
                [PrimitiveTypesPathMap] = Path.Combine(xmiDirectory, "PrimitiveTypes.xmi"),
            };

            return XmiModelReader.Read(modelPath, pathMaps, xmiDirectory);
        }

        /// <summary>
        /// Returns the "interesting" metaclasses (the minimal set covering property/type variations,
        /// concrete and abstract) as determined by uml4net's <see cref="ModelInspector"/>. The
        /// expected metaclasses are derived from uml4net, never hard-coded.
        /// </summary>
        public static IReadOnlyList<IClass> QueryInterestingMetaclasses(XmiReaderResult model)
        {
            ArgumentNullException.ThrowIfNull(model);

            var inspector = new ModelInspector(NullLoggerFactory.Instance);

            return model.Packages
                .SelectMany(inspector.QueryInterestingClasses)
                .DistinctBy(@class => @class.XmiId)
                .OrderBy(@class => @class.Name, StringComparer.Ordinal)
                .ToList();
        }
    }
}
