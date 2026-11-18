// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Hypha.Knowledge.Migration
{
    /// <summary>
    /// A release or API version a client had installed that the migration did not copy into
    /// <c>HYPHA_HOME</c>, because it would answer wrongly there.
    /// </summary>
    /// <param name="Kind"><see cref="MigratedVersion.Release"/> or <see cref="MigratedVersion.Api"/>.</param>
    /// <param name="Tag">The release tag or API version.</param>
    /// <param name="From">The client whose folder it was in, such as <c>the Claude plugin</c>.</param>
    /// <param name="Reason">Why it was left out, such as the older format one of its files is in.</param>
    public sealed record LeftOutVersion(string Kind, string Tag, string From, string Reason);
}
