// ------------------------------------------------------------------------------------------------
// <copyright file="XmiModelReader.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.MetamodelGen
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Microsoft.Extensions.Logging.Abstractions;

    using uml4net.xmi;
    using uml4net.xmi.Readers;

    /// <summary>
    /// Thin wrapper over the uml4net XMI reader. Loads a UML/KerML/SysML v2 metamodel from an XMI
    /// document so the generator can be exercised from unit tests (this tool is never run as a CLI).
    /// </summary>
    public static class XmiModelReader
    {
        /// <summary>
        /// Reads the XMI document at <paramref name="path"/> and returns the deserialized result,
        /// including the model's top-level <see cref="XmiReaderResult.Packages"/>.
        /// </summary>
        /// <param name="path">Absolute or relative path to an XMI file.</param>
        /// <param name="pathMaps">
        /// Optional <c>pathmap://</c> URI to local-file mappings used to resolve referenced
        /// libraries (e.g. the UML primitive types).
        /// </param>
        /// <param name="localReferenceBasePath">
        /// Optional base directory used to resolve local (relative) references.
        /// </param>
        /// <returns>The <see cref="XmiReaderResult"/> produced by the uml4net reader.</returns>
        /// <exception cref="ArgumentException">When <paramref name="path"/> is null or whitespace.</exception>
        /// <exception cref="FileNotFoundException">When no file exists at <paramref name="path"/>.</exception>
        public static XmiReaderResult Read(
            string path,
            IReadOnlyDictionary<string, string>? pathMaps = null,
            string? localReferenceBasePath = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);

            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"XMI file not found: '{path}'.", path);
            }

            var scope = XmiReaderBuilder.Create();

            if (pathMaps is not null || localReferenceBasePath is not null)
            {
                scope = scope.UsingSettings(settings =>
                {
                    if (pathMaps is not null)
                    {
                        settings.PathMaps = new Dictionary<string, string>(pathMaps);
                    }

                    if (localReferenceBasePath is not null)
                    {
                        settings.LocalReferenceBasePath = localReferenceBasePath;
                    }
                });
            }

            var reader = scope
                .WithLogger(NullLoggerFactory.Instance)
                .Build();

            return reader.Read(path);
        }
    }
}
