// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Hypha.Knowledge.Api.CrossReferences
{
    /// <summary>A PIM type that is known by name only.</summary>
    /// <param name="Name">The type name.</param>
    public sealed record PimNamed(string Name);
}
