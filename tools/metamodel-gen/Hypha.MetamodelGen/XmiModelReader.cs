// -------------------------------------------------------------------------------------------------
//  <copyright file="XmiModelReader.cs" company="Starion Group S.A.">
//    Copyright 2026 Starion Group S.A.
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//  </copyright>
// -------------------------------------------------------------------------------------------------

namespace Hypha.MetamodelGen
{
    using System;
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
        /// <returns>The <see cref="XmiReaderResult"/> produced by the uml4net reader.</returns>
        /// <exception cref="ArgumentException">When <paramref name="path"/> is null or whitespace.</exception>
        /// <exception cref="FileNotFoundException">When no file exists at <paramref name="path"/>.</exception>
        public static XmiReaderResult Read(string path)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);

            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"XMI file not found: '{path}'.", path);
            }

            var reader = XmiReaderBuilder.Create()
                .WithLogger(NullLoggerFactory.Instance)
                .Build();

            return reader.Read(path);
        }
    }
}
