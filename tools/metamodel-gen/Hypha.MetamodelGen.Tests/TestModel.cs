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
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Microsoft.Extensions.Logging.Abstractions;

    using uml4net.Reporting.Generators;
    using uml4net.StructuredClassifiers;
    using uml4net.xmi.Readers;

    /// <summary>
    /// Helpers for locating the (committed) XMI inputs from the test output directory, loading a
    /// release's model, and querying it via uml4net.
    /// </summary>
    /// <remarks>
    /// Inputs live under <c>sources/&lt;tag&gt;/xmi/</c>, one folder per release tag, with the model
    /// loaded and cached per tag. <c>PrimitiveTypes.xmi</c> is shared at <c>sources/</c>: it is the
    /// OMG UML primitives library, published by neither upstream and identical for every release.
    /// </remarks>
    internal static class TestModel
    {
        // The pathmap URI the SysML metamodel uses to reference the UML primitive types library.
        private const string PrimitiveTypesPathMap = "pathmap://UML_LIBRARIES/UMLPrimitiveTypes.library.uml";

        private static readonly ConcurrentDictionary<string, XmiReaderResult?> Cache = new(StringComparer.Ordinal);

        /// <summary>Gets the installed release tags, newest first.</summary>
        public static IReadOnlyList<string> Tags => KnowledgeVersions.Tags;

        /// <summary>
        /// Gets the model for the default release tag, loaded once (or <c>null</c> when no model is
        /// present). Tests that only exercise generator behaviour can use this and ignore versioning.
        /// </summary>
        public static XmiReaderResult? Model =>
            KnowledgeVersions.DefaultTag is { } tag ? ModelFor(tag) : null;

        /// <summary>
        /// Walks up from the test output directory to the repository root (the directory that
        /// contains both <c>sources</c> and <c>knowledge</c>), or <c>null</c> if not found.
        /// </summary>
        public static DirectoryInfo? FindRepoRoot()
        {
            for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
            {
                if (Directory.Exists(Path.Combine(dir.FullName, "sources"))
                    && Directory.Exists(Path.Combine(dir.FullName, "knowledge")))
                {
                    return dir;
                }
            }

            return null;
        }

        /// <summary>Loads (and caches) the combined KerML + SysML model for one release tag.</summary>
        public static XmiReaderResult? ModelFor(string tag) =>
            Cache.GetOrAdd(tag, LoadSysmlModel);

        /// <summary>
        /// Loads the full SysML v2 metamodel for <paramref name="tag"/> from
        /// <c>sources/&lt;tag&gt;/xmi/SysML_only_xmi.uml</c>. The SysML document references the KerML
        /// abstract syntax (resolved as a local file in the same folder) and the UML primitive types
        /// (resolved via a path map to the shared <c>sources/PrimitiveTypes.xmi</c>), so reading SysML
        /// as the root yields the complete KerML + SysML model with fully resolved generalization
        /// chains. Returns <c>null</c> if the inputs for that tag are not present.
        /// </summary>
        public static XmiReaderResult? LoadSysmlModel(string tag)
        {
            var root = FindRepoRoot();
            if (root is null)
            {
                return null;
            }

            var xmiDirectory = KnowledgeVersions.XmiDirectory(tag).FullName;
            var modelPath = Path.Combine(xmiDirectory, "SysML_only_xmi.uml");

            if (!File.Exists(modelPath))
            {
                return null;
            }

            var pathMaps = new Dictionary<string, string>
            {
                [PrimitiveTypesPathMap] = Path.Combine(root.FullName, "sources", "PrimitiveTypes.xmi"),
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
