// ------------------------------------------------------------------------------------------------
// <copyright file="SurfaceFormIndex.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.TextualNotation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// The surface forms of one release's metaclasses, ready to be matched against a model.
    /// </summary>
    /// <remarks>
    /// Built once per release rather than per model: a release ships around 300 models and has around
    /// 100 metaclasses with a surface form, so compiling the patterns on every lookup would mean tens
    /// of thousands of redundant compilations.
    /// </remarks>
    public sealed class SurfaceFormIndex
    {
        private readonly IReadOnlyList<(string Metaclass, Regex Declaration)> declarations;

        /// <summary>
        /// Initializes a new instance of the <see cref="SurfaceFormIndex"/> class.
        /// </summary>
        /// <param name="forms">Metaclass name to surface form.</param>
        internal SurfaceFormIndex(IReadOnlyDictionary<string, string> forms)
        {
            this.Forms = forms;

            this.declarations = forms
                .Select(entry => (entry.Key, Declaration: DeclarationPattern(entry.Value)))
                .ToList();
        }

        /// <summary>Each metaclass that has a surface form, mapped to it.</summary>
        public IReadOnlyDictionary<string, string> Forms { get; }

        /// <summary>The metaclasses whose surface form appears as a declaration in the model, sorted.</summary>
        public IReadOnlyList<string> ElementsIn(string modelText)
        {
            ArgumentNullException.ThrowIfNull(modelText);

            return this.declarations
                .Where(entry => entry.Declaration.IsMatch(modelText))
                .Select(entry => entry.Metaclass)
                .Order(StringComparer.Ordinal)
                .ToList();
        }

        /// <summary>
        /// A declaration is the keyword followed by a name, a body or a typing colon - never a
        /// substring of a longer identifier, and never a quoted name.
        /// </summary>
        /// <remarks>
        /// <c>counterpart</c> is not a <c>part</c>, and <c>'part'</c> in quotes is a name. Unlike the
        /// grammar patterns this one leaves <c>\w</c> Unicode-aware, matching the Python it replaces:
        /// model text is user-written and may well contain non-ASCII identifiers.
        /// </remarks>
        private static Regex DeclarationPattern(string form) =>
            new($@"(?<![\w']){Regex.Escape(form)}(?![\w'])", RegexOptions.None);
    }
}
