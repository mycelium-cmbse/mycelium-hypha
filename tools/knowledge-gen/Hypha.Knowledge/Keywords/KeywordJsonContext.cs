// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Hypha.Knowledge.Keywords
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// The source-generated contract for <c>knowledge/&lt;tag&gt;/textual-notation/keywords.json</c>, so
    /// writing and reading it needs no reflection-based serialization.
    /// </summary>
    [JsonSerializable(typeof(KeywordDocument))]
    internal sealed partial class KeywordJsonContext : JsonSerializerContext
    {
    }
}
