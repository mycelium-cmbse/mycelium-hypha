// ------------------------------------------------------------------------------------------------
// <copyright file="MetamodelModelLoader.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.MetamodelGen.Generation
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;

    using Hypha.Knowledge.Layout;

    using uml4net.xmi.Readers;

    /// <summary>
    /// Loads one release's metamodel from its XMI.
    /// </summary>
    /// <remarks>
    /// The SysML document references the KerML abstract syntax (a local file beside it) and the UML
    /// primitive types (via a path map to the shared <c>sources/PrimitiveTypes.xmi</c>), so reading
    /// SysML as the root yields the complete KerML + SysML model with resolved generalization chains.
    /// <para>
    /// Cached per tag: reading is the expensive part of generation, and both the generator and the
    /// golden tests want the same model.
    /// </para>
    /// </remarks>
    public sealed class MetamodelModelLoader : IMetamodelModelLoader
    {
        /// <summary>
        /// The path map the SysML metamodel uses to reference the UML primitive types.
        /// </summary>
        /// <remarks>
        /// Not a location we choose. It is the literal string the OMG XMI contains, and it is the key
        /// we have to answer to for resolution to succeed; the value it maps <i>to</i> is what varies,
        /// and that comes from the layout.
        /// </remarks>
        [SuppressMessage(
            "Major Code Smell",
            "S1075:URIs should not be hardcoded",
            Justification = "Fixed identifier appearing in the upstream XMI, not a configurable path.")]
        private const string PrimitiveTypesPathMap =
            "pathmap://UML_LIBRARIES/UMLPrimitiveTypes.library.uml";

        /// <summary>The root document; KerML is pulled in by reference.</summary>
        public const string RootDocument = "SysML_only_xmi.uml";

        /// <summary>
        /// The model read for each tag, or <c>null</c> for a tag whose XMI was absent, keyed ordinally.
        /// </summary>
        private readonly ConcurrentDictionary<string, XmiReaderResult?> cache =
            new(StringComparer.Ordinal);

        /// <summary>Resolves a release's XMI folder and the shared primitive types.</summary>
        private readonly IKnowledgeLayout layout;

        /// <summary>
        /// Initializes a new instance of the <see cref="MetamodelModelLoader"/> class.
        /// </summary>
        /// <param name="layout">Resolves a release's XMI folder and the shared primitive types.</param>
        public MetamodelModelLoader(IKnowledgeLayout layout) => this.layout = layout;

        /// <summary>
        /// The combined KerML + SysML model for <paramref name="tag"/>, read on the first call for that
        /// tag and served from the cache afterwards.
        /// </summary>
        /// <param name="tag">The release whose metamodel is loaded.</param>
        /// <returns>The model, or <c>null</c> when that release's XMI has not been fetched.</returns>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is <c>null</c>, empty or white space.</exception>
        public XmiReaderResult? Load(string tag)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(tag);

            return this.cache.GetOrAdd(tag, this.Read);
        }

        /// <summary>
        /// Reads <paramref name="tag"/>'s model from <see cref="RootDocument"/>, mapping the UML
        /// primitive types to the shared copy.
        /// </summary>
        /// <param name="tag">The release whose metamodel is read.</param>
        /// <returns>The model, or <c>null</c> when the root document does not exist.</returns>
        private XmiReaderResult? Read(string tag)
        {
            var xmiDirectory = this.layout.Xmi(tag);
            var model = new FileInfo(Path.Combine(xmiDirectory.FullName, RootDocument));

            if (!model.Exists)
            {
                return null;
            }

            var pathMaps = new Dictionary<string, string>
            {
                [PrimitiveTypesPathMap] = this.layout.SharedPrimitiveTypes.FullName,
            };

            return XmiModelReader.Read(model.FullName, pathMaps, xmiDirectory.FullName);
        }
    }
}
