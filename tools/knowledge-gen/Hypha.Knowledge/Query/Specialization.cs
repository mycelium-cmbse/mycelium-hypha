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
    /// <param name="Specializes">
    /// Whether <paramref name="Specific"/> is <paramref name="General"/> or has it among the ancestors
    /// the generator computed.
    /// </param>
    /// <param name="Path">
    /// A shortest chain of direct generalizations from <paramref name="Specific"/> to
    /// <paramref name="General"/>, both included, explaining the answer; empty when
    /// <paramref name="Specific"/> does not specialize <paramref name="General"/>.
    /// </param>
    public sealed record Specialization(string Specific, string General, bool Specializes, IReadOnlyList<string> Path);
}
