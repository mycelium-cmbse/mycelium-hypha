// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Hypha.Knowledge.Metamodel.Generators
{
    /// <summary>
    /// A single element entry in the index (metaclass, enumeration or primitive type).
    /// </summary>
    public sealed class IndexEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IndexEntry"/> class.
        /// </summary>
        /// <param name="name">The element name.</param>
        /// <param name="summary">The one-line summary of the element documentation (may be empty).</param>
        public IndexEntry(string name, string summary)
        {
            this.Name = name;
            this.Summary = summary;
        }

        /// <summary>Gets the element name (also the element file stem).</summary>
        public string Name { get; }

        /// <summary>Gets the one-line summary of the element documentation (may be empty).</summary>
        public string Summary { get; }
    }
}
