// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Hypha.Knowledge.Query
{
    using System.Collections.Generic;

    /// <summary>One package of the metamodel and the metaclasses in it.</summary>
    /// <param name="Name">The package name.</param>
    /// <param name="QualifiedName">The package's qualified name, such as <c>KerML::Core::Features</c>.</param>
    /// <param name="Classes">The metaclasses the package contains directly, sorted.</param>
    /// <param name="Packages">The qualified names of the packages nested directly in it, sorted.</param>
    /// <param name="AllClasses">The metaclasses it contains directly or through nested packages, sorted.</param>
    public sealed record PackageContents(
        string Name,
        string QualifiedName,
        IReadOnlyList<string> Classes,
        IReadOnlyList<string> Packages,
        IReadOnlyList<string> AllClasses);
}
