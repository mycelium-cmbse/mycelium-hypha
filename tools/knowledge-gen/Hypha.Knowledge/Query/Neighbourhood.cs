// ------------------------------------------------------------------------------------------------
// <copyright file="Neighbourhood.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Query
{
    using System.Collections.Generic;

    /// <summary>
    /// The metaclasses around one metaclass: those its own features point to, and those whose own
    /// features point to it.
    /// </summary>
    /// <param name="Class">The metaclass asked about.</param>
    /// <param name="Outgoing">
    /// The features the metaclass owns that are typed by a metaclass, sorted by feature.
    /// </param>
    /// <param name="Incoming">
    /// The features other metaclasses own that are typed by this one, sorted by metaclass and then feature.
    /// </param>
    public sealed record Neighbourhood(string Class, IReadOnlyList<TypedFeature> Outgoing, IReadOnlyList<TypedFeature> Incoming);
}
