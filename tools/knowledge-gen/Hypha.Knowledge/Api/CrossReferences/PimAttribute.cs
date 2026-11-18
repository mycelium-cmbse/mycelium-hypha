// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Hypha.Knowledge.Api.CrossReferences
{
    /// <summary>A PIM attribute; <see cref="InheritedFrom"/> is <c>null</c> when it is owned.</summary>
    /// <param name="Name">The attribute name.</param>
    /// <param name="Lower">The lower bound of its multiplicity.</param>
    /// <param name="Upper">The upper bound of its multiplicity; negative for unbounded.</param>
    /// <param name="IsDerived">Whether it is derived.</param>
    /// <param name="InheritedFrom">The class it is inherited from; <c>null</c> when it is owned.</param>
    public sealed record PimAttribute(string Name, int Lower, int Upper, bool IsDerived, string? InheritedFrom);
}
