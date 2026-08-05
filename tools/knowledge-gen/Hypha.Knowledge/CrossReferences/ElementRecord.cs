// ------------------------------------------------------------------------------------------------
// <copyright file="ElementRecord.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.CrossReferences
{
    /// <summary>
    /// One metamodel element, as far as the cross-references need to know it.
    /// </summary>
    /// <param name="Name">The element's simple name, which is also its key in the output.</param>
    /// <param name="Kind"><c>class</c>, <c>enumeration</c> or <c>primitiveType</c>.</param>
    /// <param name="File">Its page, relative to <c>knowledge/&lt;tag&gt;/metamodel/</c>.</param>
    public sealed record ElementRecord(string Name, string Kind, string File);
}
