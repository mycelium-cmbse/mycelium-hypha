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
        private const string ModelTableHeader = "**PIM Concept REST/HTTP PSM Model";
        private const string EndpointTableHeader = "**PIM Service REST / HTTP PSM Endpoint";
        private const string TypeProperty = "@type";

        /// <inheritdoc/>
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

        [GeneratedRegex(@"^\*\*(?<name>[A-Za-z][A-Za-z ]*)\*\* - ", RegexOptions.Multiline)]
        private static partial Regex DefinitionPattern();

        [GeneratedRegex(@"\*\*(?<run>[^*\n]+)\*\*")]
        private static partial Regex BoldPattern();

        [GeneratedRegex(@"^\*\*(?<name>\w[\w ]*)\*\*$")]
        private static partial Regex BoldLinePattern();

        [GeneratedRegex(@"^(?<pim>\w+(\.\w+)?) (?<psm>\w+( \[\d+\.\.(\d+|\*)\])?)$")]
        private static partial Regex ModelMappingRowPattern();

        [GeneratedRegex(@"^(?<operation>[A-Za-z]\w*) (?<method>GET|POST|PUT|PATCH|DELETE) ?(?<path>/\S+)$")]
        private static partial Regex EndpointRowPattern();

        [GeneratedRegex(@"\w+")]
        private static partial Regex WordPattern();

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
                // first line that is anything else - a blank one, a caption, or the prose that follows
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

        private static string? OperationId(OpenApiIndexDocument? openApi, string method, string path) =>
            openApi?.Endpoints
                .FirstOrDefault(endpoint =>
                    string.Equals(endpoint.Method, method, StringComparison.Ordinal)
                    && string.Equals(endpoint.Path, path, StringComparison.Ordinal))
                ?.OperationId;

        private static string Multiplicity(int lower, int upper) => string.Create(
            CultureInfo.InvariantCulture,
            $"[{lower}..{(upper < 0 ? "*" : upper.ToString(CultureInfo.InvariantCulture))}]");

        /// <summary>A clause, with what the builder looks for in it worked out once.</summary>
        private sealed class ReadClause
        {
            private readonly string title;

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

            public string Number { get; }

            public string[] Lines { get; }

            public HashSet<string> Definitions { get; }

            public HashSet<string> BoldWords { get; }

            public HashSet<string> Words { get; }

            public bool HasLineStarting(string prefix) =>
                this.Lines.Any(line => line.StartsWith(prefix, StringComparison.Ordinal));

            /// <summary>The strongest thing that links this clause to <paramref name="name"/>, if anything does.</summary>
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
