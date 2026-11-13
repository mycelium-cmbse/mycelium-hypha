// ------------------------------------------------------------------------------------------------
// <copyright file="FallbackServer.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Launcher
{
    using System;
    using System.IO;
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// The MCP server the launcher runs itself when it has no <c>hypha</c> to start: its one tool,
    /// <c>hypha_status</c>, says why and how to fix it, so the model can tell the user instead of the
    /// server silently failing to start.
    /// </summary>
    /// <remarks>
    /// Just enough of the protocol for that, written by hand so the launcher stays small and NativeAOT-safe:
    /// <c>initialize</c> (whose <c>instructions</c> carry the reason too), <c>ping</c>, <c>tools/list</c>
    /// and <c>tools/call</c>, one JSON-RPC message per line. Notifications are read and ignored, and any
    /// other request is answered "method not found".
    /// </remarks>
    internal static class FallbackServer
    {
        /// <summary>The one tool's name.</summary>
        public const string ToolName = "hypha_status";

        /// <summary>The protocol version answered when the client asks for none.</summary>
        private const string DefaultProtocolVersion = "2025-06-18";

        /// <summary>Serves until the client closes standard input.</summary>
        /// <param name="input">The client's messages, one per line.</param>
        /// <param name="output">Where the answers go, one per line.</param>
        /// <param name="failure">Why there is no <c>hypha</c> to start, and how to fix it.</param>
        /// <param name="version">The launcher's version, reported as the server's.</param>
        /// <param name="cancellationToken">Stops the server.</param>
        /// <returns>A task that completes once the client has gone.</returns>
        /// <exception cref="ArgumentNullException">Any argument is <c>null</c>.</exception>
        public static async Task RunAsync(TextReader input, TextWriter output, string failure, string version, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(input);
            ArgumentNullException.ThrowIfNull(output);
            ArgumentNullException.ThrowIfNull(failure);
            ArgumentNullException.ThrowIfNull(version);

            while (await input.ReadLineAsync(cancellationToken) is { } line)
            {
                if (Answer(line, failure, version) is { } answer)
                {
                    await output.WriteAsync(answer.ToJsonString() + "\n");
                    await output.FlushAsync(cancellationToken);
                }
            }
        }

        /// <summary>The answer to one message, or <c>null</c> for a notification or a line that is not one.</summary>
        /// <param name="line">The message.</param>
        /// <param name="failure">Why there is no <c>hypha</c>.</param>
        /// <param name="version">The launcher's version.</param>
        /// <returns>The answer, or <c>null</c>.</returns>
        public static JsonObject? Answer(string line, string failure, string version)
        {
            JsonObject? request;
            try
            {
                request = JsonNode.Parse(line) as JsonObject;
            }
            catch (JsonException)
            {
                return Error(null, -32700, "Parse error");
            }

            // A notification has no id and gets no answer.
            if (request is null || !request.TryGetPropertyValue("id", out var id) || id is null)
            {
                return null;
            }

            var method = request["method"]?.GetValueKind() == JsonValueKind.String ? request["method"]!.GetValue<string>() : null;

            return method switch
            {
                "initialize" => Result(id, new JsonObject
                {
                    ["protocolVersion"] = request["params"]?["protocolVersion"]?.DeepClone() ?? DefaultProtocolVersion,
                    ["capabilities"] = new JsonObject { ["tools"] = new JsonObject() },
                    ["serverInfo"] = new JsonObject { ["name"] = "hypha", ["version"] = version },
                    ["instructions"] = $"hypha could not start, so its tools are not available. Call {ToolName} for why, and tell the user: {failure}",
                }),
                "ping" => Result(id, new JsonObject()),
                "tools/list" => Result(id, new JsonObject
                {
                    ["tools"] = new JsonArray(new JsonObject
                    {
                        ["name"] = ToolName,
                        ["title"] = "hypha status",
                        ["description"] = "Says why the hypha SysML v2 / KerML server could not start, and how to fix it. Call it, and tell the user what it says.",
                        ["inputSchema"] = new JsonObject { ["type"] = "object", ["properties"] = new JsonObject() },
                        ["annotations"] = new JsonObject { ["readOnlyHint"] = true, ["openWorldHint"] = false },
                    }),
                }),
                "tools/call" when request["params"]?["name"]?.GetValueKind() == JsonValueKind.String
                                  && request["params"]!["name"]!.GetValue<string>() == ToolName => Result(id, new JsonObject
                {
                    ["content"] = new JsonArray(new JsonObject { ["type"] = "text", ["text"] = $"hypha could not start: {failure}" }),
                    ["isError"] = false,
                }),
                "tools/call" => Error(id, -32602, $"Unknown tool; the only one is {ToolName}."),
                _ => Error(id, -32601, $"Method not found: hypha could not start, and only {ToolName} answers."),
            };
        }

        /// <summary>A successful answer.</summary>
        /// <param name="id">The request's id.</param>
        /// <param name="result">The result.</param>
        /// <returns>The answer.</returns>
        private static JsonObject Result(JsonNode id, JsonObject result) =>
            new() { ["jsonrpc"] = "2.0", ["id"] = id.DeepClone(), ["result"] = result };

        /// <summary>An error answer.</summary>
        /// <param name="id">The request's id, or <c>null</c> when it could not be read.</param>
        /// <param name="code">The JSON-RPC error code.</param>
        /// <param name="message">What went wrong.</param>
        /// <returns>The answer.</returns>
        private static JsonObject Error(JsonNode? id, int code, string message) =>
            new() { ["jsonrpc"] = "2.0", ["id"] = id?.DeepClone(), ["error"] = new JsonObject { ["code"] = code, ["message"] = message } };
    }
}
