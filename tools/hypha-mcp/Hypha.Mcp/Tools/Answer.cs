// ------------------------------------------------------------------------------------------------
// <copyright file="Answer.cs" company="Starion Group S.A.">
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
    using System.Threading.Tasks;
    using System.Text.Json.Serialization.Metadata;

    using Hypha.Knowledge.Lookup;
    using Hypha.Knowledge.Query;

    using ModelContextProtocol;

    /// <summary>
    /// Turns a query into a tool's answer: the result as one JSON document, or a tool error whose
    /// message tells the model what to do about it.
    /// </summary>
    internal static class Answer
    {
        /// <summary>
        /// Runs a question and serializes its result through <see cref="QueryJsonContext.Readable"/>, the
        /// same document <c>hypha query --json</c> prints: the value, its grounding and, when part of the
        /// knowledge was never generated, why.
        /// </summary>
        /// <typeparam name="T">The result's type, which <see cref="QueryJsonContext"/> must hold.</typeparam>
        /// <param name="question">Asks the question.</param>
        /// <returns>The result as JSON.</returns>
        /// <exception cref="McpException">
        /// The question could not be answered: the release is not installed or generated, an argument is
        /// missing or invalid. The message says which, and is what the model sees.
        /// </exception>
        public static string Of<T>(Func<QueryResult<T>> question)
            where T : class
        {
            var result = Ask(question);

            return JsonSerializer.Serialize(
                result,
                QueryJsonContext.Readable.GetTypeInfo(typeof(QueryResult<T>)) as JsonTypeInfo<QueryResult<T>>
                ?? throw new InvalidOperationException($"QueryJsonContext does not hold QueryResult<{typeof(T).Name}>."));
        }

        /// <summary>
        /// Runs a question and serializes its result through <paramref name="typeInfo"/>.
        /// </summary>
        /// <typeparam name="T">The result's type.</typeparam>
        /// <param name="question">Asks the question.</param>
        /// <param name="typeInfo">How to serialize the result.</param>
        /// <returns>The result as JSON.</returns>
        /// <exception cref="McpException">The question could not be answered; the message says why.</exception>
        public static string Of<T>(Func<T> question, JsonTypeInfo<T> typeInfo)
        {
            ArgumentNullException.ThrowIfNull(typeInfo);

            return JsonSerializer.Serialize(Ask(question), typeInfo);
        }

        /// <summary>
        /// Runs an operation and serializes its result through <paramref name="typeInfo"/>.
        /// </summary>
        /// <typeparam name="T">The result's type.</typeparam>
        /// <param name="operation">Runs the operation.</param>
        /// <param name="typeInfo">How to serialize the result.</param>
        /// <returns>The result as JSON.</returns>
        /// <exception cref="McpException">The operation failed in a way the model can act on; the message says how.</exception>
        /// <exception cref="OperationCanceledException">The client cancelled the call.</exception>
        public static async Task<string> OfAsync<T>(Func<Task<T>> operation, JsonTypeInfo<T> typeInfo)
        {
            ArgumentNullException.ThrowIfNull(operation);
            ArgumentNullException.ThrowIfNull(typeInfo);

            T result;
            try
            {
                result = await operation();
            }
            catch (Exception exception) when (Translate(exception) is { } error)
            {
                throw error;
            }

            return JsonSerializer.Serialize(result, typeInfo);
        }

        /// <summary>The value of an argument that the chosen question needs.</summary>
        /// <param name="value">The argument's value.</param>
        /// <param name="argument">The argument's name, as the tool's schema gives it.</param>
        /// <param name="question">The question that needs it.</param>
        /// <returns><paramref name="value"/>, which is neither empty nor blank.</returns>
        /// <exception cref="ArgumentException"><paramref name="value"/> is <c>null</c>, empty or blank.</exception>
        public static string Required(string? value, string argument, string question) =>
            string.IsNullOrWhiteSpace(value)
                ? throw new ArgumentException($"The question '{question}' needs the argument '{argument}'.")
                : value;

        /// <summary>What the model can do about an upstream that could not be reached.</summary>
        internal const string UpstreamHint =
            "An unauthenticated GitHub API allows 60 requests an hour; set GITHUB_TOKEN for the server if that is the limit it met.";

        /// <summary>What the model can do about a knowledge file that could not be read.</summary>
        internal const string UnreadableHint =
            "If another hypha command is running, try again once it has finished; otherwise fetch or generate the release again.";

        /// <summary>
        /// Runs a question, turning the failures a caller can cause, and a knowledge file that cannot be
        /// read, into a tool error.
        /// </summary>
        /// <typeparam name="T">The result's type.</typeparam>
        /// <param name="question">Asks the question.</param>
        /// <returns>The result.</returns>
        /// <exception cref="McpException">The question could not be answered; the message says why.</exception>
        public static T Ask<T>(Func<T> question)
        {
            ArgumentNullException.ThrowIfNull(question);

            try
            {
                return question();
            }
            catch (Exception exception) when (Translate(exception) is { } error)
            {
                throw error;
            }
        }

        /// <summary>
        /// The tool error for a failure the model can act on, or <c>null</c> for any other, whose message
        /// the SDK keeps from the client.
        /// </summary>
        /// <param name="exception">The failure.</param>
        /// <returns>The tool error, or <c>null</c>.</returns>
        private static McpException? Translate(Exception exception) => exception switch
        {
            McpException => null,

            // These messages name what is missing and how to fix it, which is what the model needs to
            // correct itself. A manifest that is not a version manifest, and a release lock another
            // client holds, are InvalidOperationExceptions.
            KnowledgeLookupException or ArgumentException or FormatException or InvalidOperationException =>
                new McpException(exception.Message, exception),

            // A file the call reads (knowledge/versions.json most often) is corrupt, or is being
            // rewritten by another hypha at this moment.
            JsonException or IOException or UnauthorizedAccessException =>
                new McpException($"A knowledge file could not be read: {exception.Message} {UnreadableHint}", exception),

            HttpRequestException =>
                new McpException($"The upstream could not be reached: {exception.Message} {UpstreamHint}", exception),

            _ => null,
        };
    }
}
