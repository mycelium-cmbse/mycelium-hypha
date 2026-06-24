// ------------------------------------------------------------------------------------------------
// <copyright file="HandleBarsGenerator.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.MetamodelGen.Generators
{
    using System.Collections.Generic;
    using System.IO;

    using HandlebarsDotNet;

    /// <summary>
    /// Abstract base class from which all Handlebars-based generators derive.
    /// </summary>
    public abstract class HandleBarsGenerator : Generator
    {
        /// <summary>
        /// The <see cref="IHandlebars"/> instance used to compile and render templates.
        /// </summary>
        protected readonly IHandlebars Handlebars;

        /// <summary>
        /// Initializes a new instance of the <see cref="HandleBarsGenerator"/> class, compiling and
        /// registering the named templates.
        /// </summary>
        /// <param name="templateNames">
        /// The template (file) names, without extension, to compile from <see cref="Generator.TemplateFolderPath"/>.
        /// </param>
        protected HandleBarsGenerator(params string[] templateNames)
        {
            this.Templates = new Dictionary<string, HandlebarsTemplate<object, object>>();
            this.Handlebars = HandlebarsDotNet.Handlebars.Create();

            foreach (var name in templateNames)
            {
                this.RegisterTemplate(name);
            }
        }

        /// <summary>
        /// Gets the registered, compiled templates keyed by template (file) name.
        /// </summary>
        public Dictionary<string, HandlebarsTemplate<object, object>> Templates { get; }

        /// <summary>
        /// Compiles and registers a Handlebars template located at
        /// <c>{TemplateFolderPath}/{name}.hbs</c>.
        /// </summary>
        /// <param name="name">The template file name without extension.</param>
        protected void RegisterTemplate(string name)
        {
            var templatePath = Path.Combine(this.TemplateFolderPath, $"{name}.hbs");
            var template = File.ReadAllText(templatePath);

            this.Templates.Add(name, this.Handlebars.Compile(template));
        }
    }
}
