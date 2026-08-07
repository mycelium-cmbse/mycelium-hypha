// ------------------------------------------------------------------------------------------------
// <copyright file="Generator.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.MetamodelGen.Generators
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Abstract base class from which all knowledge-base generators derive.
    /// </summary>
    public abstract class Generator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Generator"/> class, resolving the folder
        /// that holds the (output-copied) Handlebars templates.
        /// </summary>
        protected Generator()
        {
            // AppContext.BaseDirectory rather than Assembly.Location: in a single-file publish - the
            // no-SDK delivery of the CLI - the assembly has no location on disk, while the templates
            // are still copied next to the executable.
            this.TemplateFolderPath = Path.Combine(AppContext.BaseDirectory, "Templates");
        }

        /// <summary>
        /// Gets the path where the Handlebars templates are stored.
        /// </summary>
        public string TemplateFolderPath { get; }

        /// <summary>
        /// Writes generated content to <paramref name="fileName"/> in <paramref name="outputDirectory"/>
        /// using UTF-8 (no BOM). The directory is created if it does not exist.
        /// </summary>
        protected static async Task WriteAsync(string generatedContent, DirectoryInfo outputDirectory, string fileName)
        {
            ArgumentException.ThrowIfNullOrEmpty(generatedContent);
            ArgumentNullException.ThrowIfNull(outputDirectory);
            ArgumentException.ThrowIfNullOrEmpty(fileName);

            if (!outputDirectory.Exists)
            {
                outputDirectory.Create();
            }

            var filePath = Path.Combine(outputDirectory.FullName, fileName);
            await File.WriteAllTextAsync(filePath, generatedContent, new UTF8Encoding(false));
        }
    }
}
