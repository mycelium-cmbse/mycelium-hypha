// ------------------------------------------------------------------------------------------------
// <copyright file="GeneratedIndex.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Tests
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Nodes;

    // The lookup reads an index through the same strict contract as the query API, so it refuses an
    // index the generator could not have written. A test states only the fields it is about; this fills
    // in the generator's other required fields with neutral values. Text that is not an index object
    // (invalid JSON, a missing 'entries') is left alone, so a test of a broken index still sees it broken.
    internal static class GeneratedIndex
    {
        public static string Complete(string relative, string content)
        {
            var path = relative.Replace('\\', '/');
            var metamodel = path.EndsWith("metamodel/index.json", StringComparison.Ordinal);
            var library = path.EndsWith("model-library/index.json", StringComparison.Ordinal);

            if (!metamodel && !library)
            {
                return content;
            }

            JsonObject? root;

            try
            {
                root = JsonNode.Parse(content) as JsonObject;
            }
            catch (JsonException)
            {
                return content;
            }

            var entries = root?[metamodel ? "entries" : "declarations"] as JsonObject;

            if (root is null || entries is null)
            {
                return content;
            }

            root.TryAdd("schemaVersion", "1.0.0");

            if (metamodel)
            {
                root.TryAdd("metamodel", "SysML");
                root.TryAdd("modelVersionUri", "https://www.omg.org/spec/SysML/20250201");
            }
            else
            {
                root.TryAdd("tag", "2026-09");
                root.TryAdd("counts", new JsonObject { ["files"] = 1, ["declarations"] = entries.Count });
            }

            foreach (var (key, value) in entries)
            {
                if (value is not JsonObject entry)
                {
                    continue;
                }

                if (metamodel)
                {
                    entry.TryAdd("kind", "class");
                    entry.TryAdd("package", "Test");
                    entry.TryAdd("qualifiedName", key);
                    entry.TryAdd("isAbstract", false);
                    entry.TryAdd("summary", string.Empty);
                    entry.TryAdd("extends", new JsonArray());
                }
                else
                {
                    entry.TryAdd("source", "Test.sysml");
                }
            }

            return root.ToJsonString();
        }
    }
}
