// ------------------------------------------------------------------------------------------------
// <copyright file="Specialization.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Query
{
    using System.Collections.Generic;

    /// <summary>Whether one metaclass specializes another, and through which supertypes.</summary>
    /// <param name="Specific">The metaclass asked about.</param>
    /// <param name="General">The metaclass it may specialize.</param>
    /// <param name="Path">
    /// A shortest chain of direct generalizations from <paramref name="Specific"/> to
    /// <paramref name="General"/>, both included; empty when <paramref name="Specific"/> does not
    /// specialize <paramref name="General"/>.
    /// </param>
    public sealed record Specialization(string Specific, string General, IReadOnlyList<string> Path)
    {
        /// <summary>Gets a value indicating whether <see cref="Specific"/> specializes <see cref="General"/>.</summary>
        public bool Specializes => this.Path.Count > 0;
    }
}
