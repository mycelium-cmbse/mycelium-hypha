// ------------------------------------------------------------------------------------------------
// <copyright file="JsonOutput.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Commands
{
    using System;
    using System.CommandLine;
    using System.Linq;
    using System.Text.Encodings.Web;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;

    using Spectre.Console;

    /// <summary>
    /// The rules every verb's <c>--json</c> output follows, so a host can rely on them for all verbs.
    /// </summary>
    /// <remarks>
    /// Standard output carries exactly one line holding one JSON document: no banner, no log lines
    /// (they go to standard error), no wrapping. A failure is a document too,
    /// <c>{"error": "..."}</c>, with a non-zero exit code, whether the verb reports it or
    /// <c>Program</c> does, before or around the verb.
    /// </remarks>
    public static class JsonOutput
    {
        /// <summary>The option name every JSON-capable verb uses.</summary>
        public const string OptionName = "--json";

        /// <summary>The serializer settings of every <c>--json</c> document.</summary>
        /// <remarks>
        /// Absent values are written as <c>null</c> rather than left out, so every property of a
        /// document type is always there for a host to read. Text is not HTML-escaped (an apostrophe
        /// stays an apostrophe): the reader is a program, never a browser.
        /// </remarks>
        public static readonly JsonSerializerOptions Format = new(JsonSerializerDefaults.General)
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        /// <summary>Creates the <c>--json</c> option for a verb.</summary>
        /// <param name="description">The help text the verb shows for its <c>--json</c> option.</param>
        /// <returns>
        /// A new, non-recursive <c>--json</c> flag that defaults to <c>false</c>, for the verb to add to its options.
        /// </returns>
        public static Option<bool> CreateOption(string description) =>
            new(OptionName)
            {
                Description = description,
                DefaultValueFactory = _ => false,
            };

        /// <summary>Whether the parsed verb was asked for JSON.</summary>
        /// <param name="parseResult">The parsed command line; its innermost command is the verb that runs.</param>
        /// <returns>
        /// <c>true</c> when that verb declares a <c>--json</c> flag and it was set; <c>false</c> when it was not
        /// set or the verb has no such flag.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="parseResult"/> is <c>null</c>.</exception>
        public static bool IsRequested(ParseResult parseResult)
        {
            ArgumentNullException.ThrowIfNull(parseResult);

            return parseResult.CommandResult.Command.Options
                .OfType<Option<bool>>()
                .Any(option => string.Equals(option.Name, OptionName, StringComparison.Ordinal)
                               && parseResult.GetValue(option));
        }

        /// <summary>Writes one document as one line.</summary>
        /// <typeparam name="T">The document type; its JSON property names are the contract with the host.</typeparam>
        /// <param name="document">The document to serialize with <see cref="Format"/> to standard output.</param>
        /// <remarks>
        /// Straight to the underlying writer, not <c>AnsiConsole.Write(IRenderable)</c>: a Text widget
        /// wraps to the console width, which would break a long line of JSON across several lines.
        /// Still goes through <see cref="AnsiConsole.Console"/>, so a test that redirects it can
        /// capture this the same way it captures everything else.
        /// </remarks>
        /// <returns>A task that completes once the line has been written and flushed.</returns>
        public static async Task WriteAsync<T>(T document)
        {
            await AnsiConsole.Console.Profile.Out.Writer.WriteLineAsync(JsonSerializer.Serialize(document, Format));
            await AnsiConsole.Console.Profile.Out.Writer.FlushAsync();
        }

        /// <summary>Writes a failure as <c>{"error": "..."}</c>.</summary>
        /// <param name="message">What went wrong, written as is as the value of <c>error</c>.</param>
        /// <returns>A task that completes once the line has been written and flushed.</returns>
        public static Task WriteErrorAsync(string message) => WriteAsync(new JsonError(message));

        /// <summary>The document a failed <c>--json</c> verb prints.</summary>
        /// <param name="Error">What went wrong and, where there is one, what to do about it.</param>
        private sealed record JsonError([property: JsonPropertyName("error")] string Error);
    }
}
