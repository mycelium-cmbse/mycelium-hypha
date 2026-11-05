// ------------------------------------------------------------------------------------------------
// <copyright file="ApiClause.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api.CrossReferences
{
    /// <summary>
    /// One clause of the API specification, as the cross-references read it.
    /// </summary>
    /// <remarks>
    /// The text is read to find what the clause defines and maps; none of it is carried into the
    /// cross-reference document.
    /// </remarks>
    /// <param name="Number">The clause number, e.g. <c>7.1.2</c>.</param>
    /// <param name="Title">The clause title.</param>
    /// <param name="Text">The extracted clause text.</param>
    public sealed record ApiClause(string Number, string Title, string Text);
}
