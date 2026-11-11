// ------------------------------------------------------------------------------------------------
// <copyright file="KeywordMetaclass.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Keywords
{
    using System.Collections.Generic;

    /// <summary>A metaclass whose syntax includes a keyword or operator, and how it was reached.</summary>
    /// <param name="Name">The metaclass.</param>
    /// <param name="Via">
    /// <see cref="Direct"/> when a production that builds the metaclass writes the literal itself, and
    /// <see cref="Helper"/> when it is written in a production called from one.
    /// </param>
    /// <param name="Productions">The productions building the metaclass that reach the literal, ordered ordinally.</param>
    /// <param name="Through">The productions reached from them that write the literal, ordered ordinally.</param>
    public sealed record KeywordMetaclass(
        string Name,
        string Via,
        IReadOnlyList<string> Productions,
        IReadOnlyList<string> Through)
    {
        /// <summary>The <see cref="Via"/> of a literal written by a production that builds the metaclass.</summary>
        public const string Direct = "direct";

        /// <summary>The <see cref="Via"/> of a literal written in a production called from one.</summary>
        public const string Helper = "helper";
    }
}
