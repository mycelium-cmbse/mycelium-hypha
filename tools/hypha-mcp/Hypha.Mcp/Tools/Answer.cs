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
    using System.Text.Json;
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
        private static T Ask<T>(Func<T> question)
        {
            ArgumentNullException.ThrowIfNull(question);

            try
            {
                return question();
            }
            catch (Exception exception) when (exception is KnowledgeLookupException or ArgumentException or FormatException or InvalidOperationException)
            {
                // The SDK hides the message of any other exception from the client; these messages name
                // what is missing and how to fix it, which is what the model needs to correct itself.
                // A manifest that is not a version manifest is an InvalidOperationException.
                throw new McpException(exception.Message, exception);
            }
            catch (Exception exception) when (exception is JsonException or IOException or UnauthorizedAccessException)
            {
                // A file the question reads (knowledge/versions.json most often) is corrupt, or is
                // being rewritten by another hypha at this moment.
                throw new McpException($"A knowledge file could not be read: {exception.Message} {UnreadableHint}", exception);
            }
        }
    }
}
