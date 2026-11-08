// ------------------------------------------------------------------------------------------------
// <copyright file="ModelCatalog.cs" company="Starion Group S.A.">
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
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Finds the <c>.kerml</c> / <c>.sysml</c> models under a release's textual sources.
    /// </summary>
    public sealed class ModelCatalog : IModelCatalog
    {
        /// <summary>The two model extensions; everything else in the tree is grammar or chrome.</summary>
        public static IReadOnlyList<string> ModelExtensions { get; } = [".kerml", ".sysml"];

        /// <summary>
        /// Finds every <c>.kerml</c> and <c>.sysml</c> file under <paramref name="textualRoot"/>, at any
        /// depth, whatever the extension's case.
        /// </summary>
        /// <param name="textualRoot">The release's textual sources, or its standard-library folder.</param>
        /// <returns>
        /// The models' paths relative to <paramref name="textualRoot"/>, with <c>/</c> separators, ordered
        /// segment by segment and case-insensitively. Empty when the folder does not exist.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="textualRoot"/> is <c>null</c>.</exception>
        public IReadOnlyList<string> Discover(DirectoryInfo textualRoot)
        {
            ArgumentNullException.ThrowIfNull(textualRoot);

            if (!textualRoot.Exists)
            {
                return [];
            }

            var root = textualRoot.FullName;

            return textualRoot
                .EnumerateFiles("*", SearchOption.AllDirectories)
                .Where(file => ModelExtensions.Contains(file.Extension.ToLowerInvariant()))
                .Select(file => Path.GetRelativePath(root, file.FullName).Replace('\\', '/'))
                .Order(PathOrder.Instance)
                .ToList();
        }

        /// <summary>
        /// Orders paths segment by segment, case-insensitively.
        /// </summary>
        /// <remarks>
        /// This is the order the committed pages were generated in, and it is not the same as an
        /// ordinal sort of the whole string: comparing segments makes a shorter path sort before a
        /// longer one that extends it, regardless of where the separator falls in the character set.
        /// Case-insensitivity keeps <c>KerML Spec Annex A Examples</c> next to its lowercase siblings
        /// rather than ahead of all of them.
        /// </remarks>
        private sealed class PathOrder : IComparer<string>
        {
            /// <summary>Gets the shared instance; the comparer holds no state.</summary>
            public static PathOrder Instance { get; } = new();

            /// <summary>
            /// Compares two relative paths segment by segment, ordinally on their lowercase forms; a path that is
            /// a prefix of the other sorts first, and <c>null</c> sorts before any path.
            /// </summary>
            /// <param name="x">The first path.</param>
            /// <param name="y">The second path.</param>
            /// <returns>
            /// Less than zero when <paramref name="x"/> sorts first, zero when they are equal, and greater than
            /// zero when <paramref name="y"/> sorts first.
            /// </returns>
            public int Compare(string? x, string? y)
            {
                if (x is null)
                {
                    return y is null ? 0 : -1;
                }

                if (y is null)
                {
                    return 1;
                }

                var left = x.Split('/');
                var right = y.Split('/');

                for (var index = 0; index < Math.Min(left.Length, right.Length); index++)
                {
                    var comparison = string.CompareOrdinal(
                        left[index].ToLowerInvariant(), right[index].ToLowerInvariant());

                    if (comparison != 0)
                    {
                        return comparison;
                    }
                }

                return left.Length.CompareTo(right.Length);
            }
        }
    }
}
