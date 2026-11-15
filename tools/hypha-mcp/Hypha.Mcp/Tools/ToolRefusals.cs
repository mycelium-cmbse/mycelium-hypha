// ------------------------------------------------------------------------------------------------
// <copyright file="ToolRefusals.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Mcp.Tools
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Text.Json;

    using Microsoft.Extensions.Logging;

    using ModelContextProtocol;
    using ModelContextProtocol.Protocol;
    using ModelContextProtocol.Server;

    /// <summary>
    /// Turns a tool's refusal into the tool error the client reads, before the SDK logs it as a failure.
    /// </summary>
    /// <remarks>
    /// A tool refuses by throwing an <see cref="McpException"/> whose message says what is wrong and how to
    /// fix it: a release that is not installed, the default release asked to be removed, a name the
    /// metamodel does not have. The SDK reports any exception a tool throws as one that "threw an unhandled
    /// exception", at <see cref="LogLevel.Error"/>, which fills the client's log with errors for answers that
    /// went exactly as intended. This filter answers with the same tool error the SDK would have sent, a
    /// result with <see cref="CallToolResult.IsError"/> set and the message as its text.
    /// <para>
    /// How it is logged depends on what the <see cref="McpException"/> carries. A refusal, one with no inner
    /// exception or one translated from a lookup, argument, format or operation error, is logged at
    /// <see cref="LogLevel.Information"/>: the answer went as intended. A failure translated from a file that
    /// could not be read or an upstream that could not be reached is logged at <see cref="LogLevel.Warning"/>,
    /// with the exception, so that the server's log still says what went wrong. A protocol error
    /// (<see cref="McpProtocolException"/>), such as an unknown tool, and every other exception still reach
    /// the SDK.
    /// </para>
    /// </remarks>
    internal static class ToolRefusals
    {
        /// <summary>The log category refusals are written under.</summary>
        internal const string Category = "Hypha.Mcp.Tools";

        /// <summary>The compiled <c>Refused</c> message (event 1, information).</summary>
        private static readonly Action<ILogger, string, string, Exception?> RefusedMessage =
            LoggerMessage.Define<string, string>(
                LogLevel.Information, new EventId(1, "Refused"), "{ToolName} refused: {Reason}");

        /// <summary>The compiled <c>Failed</c> message (event 2, warning), logged with the exception.</summary>
        private static readonly Action<ILogger, string, string, Exception?> FailedMessage =
            LoggerMessage.Define<string, string>(
                LogLevel.Warning, new EventId(2, "Failed"), "{ToolName} failed: {Reason}");

        /// <summary>Wraps the call-tool handler so that a tool's refusal becomes a tool error.</summary>
        /// <param name="next">The handler that calls the tool.</param>
        /// <returns>The wrapping handler.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="next"/> is <c>null</c>.</exception>
        public static McpRequestHandler<CallToolRequestParams, CallToolResult> AsToolErrors(
            McpRequestHandler<CallToolRequestParams, CallToolResult> next)
        {
            ArgumentNullException.ThrowIfNull(next);

            return async (context, cancellationToken) =>
            {
                try
                {
                    return await next(context, cancellationToken);
                }
                catch (McpException exception) when (exception is not McpProtocolException)
                {
                    var name = context.Params?.Name ?? "(unnamed)";
                    var logger = context.Services?.GetService(typeof(ILoggerFactory)) is ILoggerFactory loggers
                        ? loggers.CreateLogger(Category)
                        : null;

                    if (logger is not null && IsFailure(exception))
                    {
                        FailedMessage(logger, name, exception.Message, exception);
                    }
                    else if (logger is not null)
                    {
                        RefusedMessage(logger, name, exception.Message, null);
                    }

                    return Refusal(exception.Message);
                }
            };
        }

        /// <summary>
        /// Whether an <see cref="McpException"/> reports a failure, rather than a refusal: one translated from a
        /// file that could not be read, or from an upstream that could not be reached.
        /// </summary>
        /// <param name="exception">The exception the tool threw.</param>
        /// <returns><c>true</c> for a failure.</returns>
        internal static bool IsFailure(McpException exception) =>
            exception.InnerException is IOException or UnauthorizedAccessException or JsonException or HttpRequestException;

        /// <summary>The tool error a refusal is answered with.</summary>
        /// <param name="message">What is wrong, and how to fix it.</param>
        /// <returns>A result marked as an error, holding the message as its one text block.</returns>
        internal static CallToolResult Refusal(string message) =>
            new() { IsError = true, Content = [new TextContentBlock { Text = message }] };
    }
}
