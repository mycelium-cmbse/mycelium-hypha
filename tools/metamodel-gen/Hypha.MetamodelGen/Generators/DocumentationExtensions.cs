// ------------------------------------------------------------------------------------------------
// <copyright file="DocumentationExtensions.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.MetamodelGen.Generators
{
    using System;
    using System.Text.RegularExpressions;

    using uml4net.CommonStructure;
    using uml4net.Extensions;

    /// <summary>
    /// Documentation extraction shared by the element-file and index generators.
    /// </summary>
    public static class DocumentationExtensions
    {
        // Convention: every Regex in this tool is constructed with an explicit match timeout to
        // guard against catastrophic backtracking (ReDoS). Reuse RegexTimeout for any new patterns.
        private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(2);

        private static readonly Regex WhitespaceRegex =
            new(@"\s+", RegexOptions.Compiled, RegexTimeout);

        /// <summary>
        /// Returns the element's documentation as a single clean paragraph: the comment-body
        /// fragments are joined and runs of whitespace collapsed to single spaces.
        /// </summary>
        public static string QueryDocumentationText(this IElement element)
        {
            ArgumentNullException.ThrowIfNull(element);

            var joined = string.Join(" ", element.QueryDocumentation());

            return WhitespaceRegex.Replace(joined, " ").Trim();
        }

        /// <summary>
        /// Returns a one-line summary of the element's documentation: the first sentence, truncated
        /// to <paramref name="maxLength"/> characters with an ellipsis when longer.
        /// </summary>
        public static string QuerySummary(this IElement element, int maxLength = 160)
        {
            var text = element.QueryDocumentationText();

            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            var sentenceEnd = text.IndexOf(". ", StringComparison.Ordinal);
            var sentence = sentenceEnd >= 0 ? text[..(sentenceEnd + 1)] : text;

            if (sentence.Length > maxLength)
            {
                sentence = sentence[..maxLength].TrimEnd() + "…";
            }

            return sentence;
        }
    }
}
