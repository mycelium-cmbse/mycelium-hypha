// ------------------------------------------------------------------------------------------------
// <copyright file="ApiCrossReferenceBuilder.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api.CrossReferences
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;

    using Hypha.Knowledge.Api.OpenApi;
    using Hypha.Knowledge.Grammar;

    /// <summary>
    /// Lines up the PIM, the specification clauses and the REST/HTTP PSM.
    /// </summary>
    /// <remarks>
    /// A pure function of its three inputs. Wherever one of them <b>states</b> a link, that is what is
    /// used: the specification's two mapping tables, a clause that opens a paragraph by defining a
    /// type, a request schema whose <c>@type</c> is fixed to a type's name. A shared name is the
    /// fallback, and the edge says so.
    /// </remarks>
    public sealed partial class ApiCrossReferenceBuilder : IApiCrossReferenceBuilder
    {
        // The header rows of the two REST/HTTP mapping tables, as the extracted text carries them. The
        // OSLC PSM has tables of the same build under other headers, which must not be read as these.
        /// <summary>The line that opens the PIM-to-PSM model mapping table.</summary>
        private const string ModelTableHeader = "**PIM Concept REST/HTTP PSM Model";

        /// <summary>The line that opens the PIM-to-PSM endpoint mapping table.</summary>
        private const string EndpointTableHeader = "**PIM Service REST / HTTP PSM Endpoint";

        /// <summary>The member a request schema fixes to the name of the PIM type it carries.</summary>
        private const string TypeProperty = "@type";

        /// <summary>
        /// Builds the cross-references for one API version: links each PIM type to the clauses that
        /// cover it and to its PSM schemas, compares its effective attributes with the PSM models'
        /// members, and maps each PIM service operation to its clauses and endpoints.
        /// </summary>
        /// <param name="apiVersion">The API version.</param>
        /// <param name="pim">The PIM types.</param>
        /// <param name="openApi">
        /// The REST/HTTP PSM index; <c>null</c> when it was not generated, in which case no type has a
        /// schema and no endpoint has an operation identifier.
        /// </param>
        /// <param name="clauses">
        /// The specification clauses, in any order; empty when the text was not generated, in which case
        /// nothing is linked to a clause or a mapping table.
        /// </param>
        /// <returns>The cross-reference document, its types and operations ordered by key.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="apiVersion"/> is <c>null</c>, empty or white space.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="pim"/> or <paramref name="clauses"/> is <c>null</c>.
        /// </exception>
        public ApiCrossReferenceDocument Build(
            string apiVersion, PimGraph pim, OpenApiIndexDocument? openApi, IReadOnlyList<ApiClause> clauses)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(apiVersion);
            ArgumentNullException.ThrowIfNull(pim);
            ArgumentNullException.ThrowIfNull(clauses);

            var ordered = clauses
                .OrderBy(clause => clause.Number, ClauseNumberComparer.Instance)
                .Select(clause => new ReadClause(clause))
                .ToList();

            var classes = pim.Classes ?? [];
            var services = classes
                .Where(@class => (@class.OwnedOperations ?? []).Count > 0)
                .ToDictionary(
                    @class => @class.Name,
                    @class => (@class.OwnedOperations ?? []).Select(operation => operation.Name)
                        .ToHashSet(StringComparer.Ordinal),
                    StringComparer.Ordinal);

            var modelMapping = ordered.SelectMany(ModelMappingRows).ToList();

            // A row that maps one whole type to one whole model. Rows that map a member
            // ("MergeResult.conflict") or carry a multiplicity are kept as written but name no schema.
            var mapped = modelMapping
                .Where(row => !row.Pim.Contains('.', StringComparison.Ordinal) && !row.Psm.Contains(' ', StringComparison.Ordinal))
                .GroupBy(row => row.Pim, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First().Psm, StringComparer.Ordinal);

            var types = new SortedDictionary<string, ApiTypeEntry>(StringComparer.Ordinal);

            foreach (var @class in classes)
            {
                types[@class.Name] = Type("class", @class.Name, @class, ordered, mapped, openApi);
            }

            foreach (var (kind, named) in new[]
                     {
                         ("interface", pim.Interfaces), ("enumeration", pim.Enumerations), ("dataType", pim.DataTypes),
                     })
            {
                foreach (var type in named ?? [])
                {
                    types[type.Name] = Type(kind, type.Name, null, ordered, mapped, openApi);
                }
            }

            var endpoints = ordered
                .Where(clause => clause.HasLineStarting(EndpointTableHeader))
                .ToList();

            var operations = new SortedDictionary<string, ApiOperationEntry>(StringComparer.Ordinal);

            foreach (var (service, names) in services)
            {
                foreach (var name in names)
                {
                    operations[$"{service}.{name}"] = new ApiOperationEntry(
                        service,
                        ordered
                            .Where(clause => clause.BoldWords.Contains(service) && clause.HasLineStarting(name + "("))
                            .Select(clause => new ApiClauseEdge(clause.Number, ApiCrossReferenceDocument.SignatureMethod))
                            .ToList(),
                        endpoints
                            .SelectMany(clause => EndpointRows(clause, service, name, services))
                            .Distinct()
                            .Select(row => new ApiEndpointEdge(row.Method, row.Path, OperationId(openApi, row.Method, row.Path)))
                            .ToList(),
                        endpoints.FirstOrDefault(clause => clause.Words.Contains(name))?.Number);
                }
            }

            return new ApiCrossReferenceDocument(
                ApiCrossReferenceDocument.CurrentSchemaVersion,
                apiVersion,
                new ApiCrossReferenceSources(true, openApi is not null, ordered.Count > 0),
                new ApiCrossReferenceCounts(
                    types.Count,
                    types.Values.Count(type => type.Clauses.Count > 0),
                    types.Values.Count(type => type.Schema is not null),
                    operations.Count,
                    operations.Values.Count(operation => operation.Endpoints.Count > 0)),
                types,
                operations,
                modelMapping);
        }

        /// <summary>
        /// A line that opens a paragraph by defining a name in bold, e.g. <c>**Commit** - </c>.
        /// </summary>
        /// <returns>The pattern; its <c>name</c> group is the defined name.</returns>
        [GeneratedRegex(@"^\*\*(?<name>[A-Za-z][A-Za-z ]*)\*\* - ", RegexOptions.Multiline)]
        private static partial Regex DefinitionPattern();

        /// <summary>A run of bold text on one line.</summary>
        /// <returns>The pattern; its <c>run</c> group is the bold text.</returns>
        [GeneratedRegex(@"\*\*(?<run>[^*\n]+)\*\*")]
        private static partial Regex BoldPattern();

        /// <summary>A line that is nothing but a bold name, as a mapping table sets a service's name.</summary>
        /// <returns>The pattern; its <c>name</c> group is the bold name.</returns>
        [GeneratedRegex(@"^\*\*(?<name>\w[\w ]*)\*\*$")]
        private static partial Regex BoldLinePattern();

        /// <summary>
        /// A row of the model mapping table: a PIM type or <c>Type.member</c>, then a PSM model,
        /// optionally with a multiplicity.
        /// </summary>
        /// <returns>The pattern; its <c>pim</c> and <c>psm</c> groups are the two cells.</returns>
        [GeneratedRegex(@"^(?<pim>\w+(\.\w+)?) (?<psm>\w+( \[\d+\.\.(\d+|\*)\])?)$")]
        private static partial Regex ModelMappingRowPattern();

        /// <summary>A row of the endpoint mapping table: an operation, an HTTP method and a path.</summary>
        /// <returns>The pattern; its <c>operation</c>, <c>method</c> and <c>path</c> groups are the cells.</returns>
        [GeneratedRegex(@"^(?<operation>[A-Za-z]\w*) (?<method>GET|POST|PUT|PATCH|DELETE) ?(?<path>/\S+)$")]
        private static partial Regex EndpointRowPattern();

        /// <summary>One word.</summary>
        /// <returns>The pattern.</returns>
        [GeneratedRegex(@"\w+")]
        private static partial Regex WordPattern();

        /// <summary>Lines up one PIM type with the clauses that cover it and its PSM schemas.</summary>
        /// <param name="kind"><c>class</c>, <c>interface</c>, <c>enumeration</c> or <c>dataType</c>.</param>
        /// <param name="name">The type's name.</param>
        /// <param name="class">
        /// The type when it is a class, whose effective attributes are compared with the PSM models'
        /// members; <c>null</c> for any other kind.
        /// </param>
        /// <param name="clauses">The specification clauses, in clause order.</param>
        /// <param name="mapped">The PSM model the model mapping table names for each whole PIM type.</param>
        /// <param name="openApi">The REST/HTTP PSM index; <c>null</c> when it was not generated.</param>
        /// <returns>
        /// The type's entry. Its members and PSM-only members are <c>null</c> unless it is a class with a
        /// response schema that has members.
        /// </returns>
        private static ApiTypeEntry Type(
            string kind,
            string name,
            PimClass? @class,
            List<ReadClause> clauses,
            Dictionary<string, string> mapped,
            OpenApiIndexDocument? openApi)
        {
            var edges = clauses
                .Select(clause => (clause.Number, Method: clause.MethodFor(name)))
                .Where(edge => edge.Method is not null)
                .Select(edge => new ApiClauseEdge(edge.Number, edge.Method!))
                .ToList();

            var schema = openApi is null ? null : Schema(name, mapped, openApi);

            IReadOnlyList<ApiMemberEntry>? members = null;
            IReadOnlyList<string>? psmOnly = null;

            if (@class is not null
                && schema?.Response is { } responseName
                && openApi!.Schemas[responseName].Properties is { } responseProperties)
            {
                var response = responseProperties.Select(property => property.Name).ToHashSet(StringComparer.Ordinal);
                var request = schema.Request is { } requestName
                    ? (openApi.Schemas[requestName].Properties ?? [])
                        .Select(property => property.Name).ToHashSet(StringComparer.Ordinal)
                    : [];

                var attributes = (@class.OwnedAttributes ?? [])
                    .Concat(@class.InheritedAttributes ?? [])
                    .OrderBy(attribute => attribute.Name, StringComparer.Ordinal)
                    .ToList();

                members = attributes
                    .Select(attribute => new ApiMemberEntry(
                        attribute.Name,
                        Multiplicity(attribute.Lower, attribute.Upper),
                        attribute.IsDerived,
                        attribute.InheritedFrom,
                        response.Contains(attribute.Name),
                        request.Contains(attribute.Name)))
                    .ToList();

                var pimNames = attributes.Select(attribute => attribute.Name).ToHashSet(StringComparer.Ordinal);

                psmOnly = response
                    .Where(property => !pimNames.Contains(property))
                    .OrderBy(property => property, StringComparer.Ordinal)
                    .ToList();
            }

            return new ApiTypeEntry(kind, $"pim/elements/{name}.md", edges, schema, members, psmOnly);
        }

        /// <summary>
        /// Finds a PIM type's PSM schemas: the response model the mapping table names (or, failing that,
        /// the API schema of the same name), and the request schema whose <c>@type</c> is fixed to the
        /// type's name.
        /// </summary>
        /// <param name="name">The PIM type's name.</param>
        /// <param name="mapped">The PSM model the model mapping table names for each whole PIM type.</param>
        /// <param name="openApi">The REST/HTTP PSM index.</param>
        /// <returns>The schemas and what links each, or <c>null</c> when neither is found.</returns>
        private static ApiSchemaEdge? Schema(string name, Dictionary<string, string> mapped, OpenApiIndexDocument openApi)
        {
            string? response = null;
            string? responseMethod = null;

            if (mapped.TryGetValue(name, out var stated) && openApi.Schemas.ContainsKey(stated))
            {
                (response, responseMethod) = (stated, ApiCrossReferenceDocument.MappingTableMethod);
            }
            else if (openApi.Schemas.TryGetValue(name, out var same)
                     && string.Equals(same.Namespace, OpenApiNames.ApiNamespace, StringComparison.Ordinal))
            {
                (response, responseMethod) = (name, ApiCrossReferenceDocument.SameNameMethod);
            }

            // The request model is the one the document fixes to this type: a schema used in request
            // bodies whose "@type" may only be the type's name.
            var constant = $"const \"{name}\"";
            var request = openApi.Schemas
                .Where(schema => !string.Equals(schema.Key, response, StringComparison.Ordinal))
                .Where(schema => schema.Value.Role is OpenApiNames.RequestRole or OpenApiNames.BothRoles)
                .Where(schema => (schema.Value.Properties ?? []).Any(property =>
                    string.Equals(property.Name, TypeProperty, StringComparison.Ordinal)
                    && string.Equals(property.Type, constant, StringComparison.Ordinal)))
                .Select(schema => schema.Key)
                .OrderBy(key => key, StringComparer.Ordinal)
                .FirstOrDefault();

            return response is null && request is null
                ? null
                : new ApiSchemaEdge(
                    response,
                    responseMethod,
                    request,
                    request is null ? null : ApiCrossReferenceDocument.TypeConstantMethod);
        }

        /// <summary>The rows of the model mapping table a clause holds; none when it holds no such table.</summary>
        /// <param name="clause">The clause.</param>
        /// <returns>The rows, as written, in the order the clause holds them.</returns>
        private static IEnumerable<ApiModelMappingRow> ModelMappingRows(ReadClause clause)
        {
            var inTable = false;

            foreach (var line in clause.Lines)
            {
                if (line.StartsWith(ModelTableHeader, StringComparison.Ordinal))
                {
                    inTable = true;
                    continue;
                }

                if (!inTable)
                {
                    continue;
                }

                // A row is two names, the second optionally with a multiplicity. The table ends at the
                // first line that is anything else: a blank one, a caption, or the prose that follows
                // it without a blank line in the extracted text. Being this strict is what keeps
                // specification wording out of a document that may carry identifiers only.
                var row = ModelMappingRowPattern().Match(line);

                if (!row.Success)
                {
                    inTable = false;
                    continue;
                }

                yield return new ApiModelMappingRow(row.Groups["pim"].Value, row.Groups["psm"].Value, clause.Number);
            }
        }

        /// <summary>
        /// The endpoints a clause's mapping table gives for one operation, on rows that hold the whole
        /// mapping. The table groups its rows under the owning service, set in bold on a line of its own.
        /// </summary>
        /// <param name="clause">The clause holding the endpoint mapping table.</param>
        /// <param name="service">The service that owns the operation.</param>
        /// <param name="operation">The operation's name.</param>
        /// <param name="services">The operation names of every PIM service, by service name.</param>
        /// <returns>The method and path of each matching row, in the order the clause holds them.</returns>
        private static IEnumerable<(string Method, string Path)> EndpointRows(
            ReadClause clause, string service, string operation, Dictionary<string, HashSet<string>> services)
        {
            string? current = null;

            foreach (var line in clause.Lines)
            {
                var heading = BoldLinePattern().Match(line);

                if (heading.Success)
                {
                    // Any bold heading ends the previous group, also one that names no PIM service:
                    // its rows must not be attributed to the service above it. The specification sets
                    // some service names in several words ("Project Usage Service").
                    var name = heading.Groups["name"].Value.Replace(" ", string.Empty, StringComparison.Ordinal);
                    current = services.ContainsKey(name) ? name : null;
                    continue;
                }

                var row = EndpointRowPattern().Match(line);

                if (row.Success
                    && string.Equals(current, service, StringComparison.Ordinal)
                    && string.Equals(row.Groups["operation"].Value, operation, StringComparison.Ordinal))
                {
                    yield return (row.Groups["method"].Value, row.Groups["path"].Value);
                }
            }
        }

        /// <summary>The OpenAPI operation with <paramref name="method"/> and <paramref name="path"/>.</summary>
        /// <param name="openApi">The REST/HTTP PSM index; <c>null</c> when it was not generated.</param>
        /// <param name="method">The HTTP method, upper case.</param>
        /// <param name="path">The path template.</param>
        /// <returns>
        /// The operation identifier; <c>null</c> without an index or when it has no such endpoint.
        /// </returns>
        private static string? OperationId(OpenApiIndexDocument? openApi, string method, string path) =>
            openApi?.Endpoints
                .FirstOrDefault(endpoint =>
                    string.Equals(endpoint.Method, method, StringComparison.Ordinal)
                    && string.Equals(endpoint.Path, path, StringComparison.Ordinal))
                ?.OperationId;

        /// <summary>Writes a multiplicity in the PIM's notation.</summary>
        /// <param name="lower">The lower bound.</param>
        /// <param name="upper">The upper bound; negative for unbounded.</param>
        /// <returns>The multiplicity, e.g. <c>[0..*]</c> or <c>[1..1]</c>.</returns>
        private static string Multiplicity(int lower, int upper) => string.Create(
            CultureInfo.InvariantCulture,
            $"[{lower}..{(upper < 0 ? "*" : upper.ToString(CultureInfo.InvariantCulture))}]");

        /// <summary>A clause, with what the builder looks for in it worked out once.</summary>
        private sealed class ReadClause
        {
            /// <summary>The clause title.</summary>
            private readonly string title;

            /// <summary>
            /// Initializes a new instance of the <see cref="ReadClause"/> class, reading
            /// <paramref name="clause"/>'s text once into lines, definitions, bold words and words.
            /// </summary>
            /// <param name="clause">The clause to read.</param>
            public ReadClause(ApiClause clause)
            {
                var text = clause.Text.Replace("\r\n", "\n", StringComparison.Ordinal);

                this.Number = clause.Number;
                this.title = clause.Title;
                this.Lines = text.Split('\n');

                this.Definitions = DefinitionPattern().Matches(text)
                    .Select(match => match.Groups["name"].Value.Replace(" ", string.Empty, StringComparison.Ordinal))
                    .ToHashSet(StringComparer.Ordinal);

                // Each word of a bold run, and the run as one name: a figure sets several type names
                // in one run ("DataDifference MergeResult"), while a heading may set one name in
                // several words ("Project Usage Service").
                this.BoldWords = BoldPattern().Matches(text)
                    .SelectMany(match => WordPattern().Matches(match.Groups["run"].Value)
                        .Select(word => word.Value)
                        .Append(match.Groups["run"].Value.Replace(" ", string.Empty, StringComparison.Ordinal)))
                    .ToHashSet(StringComparer.Ordinal);

                this.Words = WordPattern().Matches(text).Select(word => word.Value).ToHashSet(StringComparer.Ordinal);
            }

            /// <summary>Gets the clause number, e.g. <c>7.1.2</c>.</summary>
            public string Number { get; }

            /// <summary>Gets the clause text, split into lines.</summary>
            public string[] Lines { get; }

            /// <summary>
            /// Gets the names a paragraph of the clause opens by defining in bold, with spaces removed.
            /// </summary>
            public HashSet<string> Definitions { get; }

            /// <summary>
            /// Gets each word of every bold run of the clause, and each run as one name with its spaces
            /// removed.
            /// </summary>
            public HashSet<string> BoldWords { get; }

            /// <summary>Gets every word of the clause text.</summary>
            public HashSet<string> Words { get; }

            /// <summary>Whether a line of the clause starts with <paramref name="prefix"/>.</summary>
            /// <param name="prefix">The text a line must start with, compared ordinally.</param>
            /// <returns><c>true</c> when at least one line starts with <paramref name="prefix"/>.</returns>
            public bool HasLineStarting(string prefix) =>
                this.Lines.Any(line => line.StartsWith(prefix, StringComparison.Ordinal));

            /// <summary>The strongest thing that links this clause to <paramref name="name"/>, if anything does.</summary>
            /// <param name="name">The PIM type's name.</param>
            /// <returns>
            /// <see cref="ApiCrossReferenceDocument.TitleMethod"/>,
            /// <see cref="ApiCrossReferenceDocument.DefinitionMethod"/> or
            /// <see cref="ApiCrossReferenceDocument.BoldMentionMethod"/>, strongest first; <c>null</c> when
            /// nothing links them.
            /// </returns>
            public string? MethodFor(string name)
            {
                if (string.Equals(this.title, name, StringComparison.Ordinal))
                {
                    return ApiCrossReferenceDocument.TitleMethod;
                }

                if (this.Definitions.Contains(name))
                {
                    return ApiCrossReferenceDocument.DefinitionMethod;
                }

                return this.BoldWords.Contains(name) ? ApiCrossReferenceDocument.BoldMentionMethod : null;
            }
        }
    }
}
