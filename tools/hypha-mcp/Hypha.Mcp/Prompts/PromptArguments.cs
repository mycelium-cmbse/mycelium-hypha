// ------------------------------------------------------------------------------------------------
// <copyright file="PromptArguments.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Mcp.Prompts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;

    using ModelContextProtocol;
    using ModelContextProtocol.Protocol;
    using ModelContextProtocol.Server;

    /// <summary>
    /// Refuses a prompt request that leaves out a required argument, with a message naming it.
    /// </summary>
    /// <remarks>
    /// The SDK binds a prompt's arguments to its method's parameters before the method runs, so a missing
    /// argument fails in the binding, and the client gets the SDK's generic "An error occurred." rather
    /// than the prompt's own check. This filter runs before the binding and checks the arguments the
    /// prompt declares as required.
    /// </remarks>
    internal static class PromptArguments
    {
        /// <summary>
        /// Wraps the get-prompt handler so that a request without a required argument is refused first.
        /// </summary>
        /// <param name="next">The handler that binds the arguments and renders the prompt.</param>
        /// <returns>The wrapped handler.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="next"/> is <c>null</c>.</exception>
        public static McpRequestHandler<GetPromptRequestParams, GetPromptResult> RequireArguments(
            McpRequestHandler<GetPromptRequestParams, GetPromptResult> next)
        {
            ArgumentNullException.ThrowIfNull(next);

            return (context, cancellationToken) =>
            {
                if (context.MatchedPrimitive is McpServerPrompt prompt && context.Params is { } request)
                {
                    var missing = Missing(prompt.ProtocolPrompt, request.Arguments);

                    if (missing.Count > 0)
                    {
                        throw new McpException(Refusal(prompt.ProtocolPrompt.Name, missing));
                    }
                }

                return next(context, cancellationToken);
            };
        }

        /// <summary>The message refusing a prompt request that leaves out required arguments.</summary>
        /// <param name="prompt">The prompt's name.</param>
        /// <param name="missing">The arguments left out, at least one.</param>
        /// <returns>The message, naming the prompt and every argument it needs.</returns>
        internal static string Refusal(string prompt, IReadOnlyList<string> missing) =>
            $"The prompt '{prompt}' needs the argument{(missing.Count > 1 ? "s" : string.Empty)} "
            + $"{string.Join(", ", missing.Select(name => $"'{name}'"))}.";

        /// <summary>The required arguments of a prompt that a request leaves out or leaves blank.</summary>
        /// <param name="prompt">The prompt, as the client lists it.</param>
        /// <param name="arguments">The request's arguments, or <c>null</c> when it has none.</param>
        /// <returns>The names of the missing arguments, in the order the prompt declares them.</returns>
        internal static IReadOnlyList<string> Missing(Prompt prompt, IDictionary<string, JsonElement>? arguments) =>
            [.. (prompt.Arguments ?? [])
                .Where(argument => argument.Required == true)
                .Where(argument => arguments is null
                                   || !arguments.TryGetValue(argument.Name, out var value)
                                   || value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined
                                   || (value.ValueKind == JsonValueKind.String && string.IsNullOrWhiteSpace(value.GetString())))
                .Select(argument => argument.Name)];
    }
}
