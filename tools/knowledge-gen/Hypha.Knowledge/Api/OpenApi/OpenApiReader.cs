// ------------------------------------------------------------------------------------------------
// <copyright file="OpenApiReader.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api.OpenApi
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;

    /// <summary>
    /// Reads the REST/HTTP PSM out of an OpenAPI 3.1 document.
    /// </summary>
    /// <remarks>
    /// Only what the document <b>states</b> is read. Whether a schema is a request or a response model
    /// comes from which endpoints refer to it, not from a <c>Request</c> suffix in its name; whether it
    /// belongs to the API or mirrors a metamodel element comes from its <c>$id</c>.
    /// </remarks>
    public sealed class OpenApiReader : IOpenApiReader
    {
        private const string SchemaReferencePrefix = "#/components/schemas/";

        private static readonly string[] Methods = ["get", "post", "put", "patch", "delete"];

        /// <inheritdoc/>
        public OpenApiModel Read(string json)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(json);

            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            if (!root.TryGetProperty("paths", out var paths)
                || !root.TryGetProperty("components", out var components)
                || !components.TryGetProperty("schemas", out var schemas))
            {
                throw new InvalidOperationException(
                    "not an OpenAPI document hypha can read: it has no paths or no components/schemas");
            }

            var endpoints = ReadEndpoints(paths, components);
            var roles = Roles(paths, components, schemas);

            var read = schemas.EnumerateObject()
                .Select(schema => ReadSchema(schema.Name, schema.Value, roles))
                .OrderBy(schema => schema.Name, StringComparer.Ordinal)
                .ToList();

            var info = root.TryGetProperty("info", out var declared) ? declared : default;

            return new OpenApiModel(
                Text(info, "title"),
                Text(info, "version"),
                Text(root, "openapi"),
                root.TryGetProperty("tags", out var tags) && tags.ValueKind == JsonValueKind.Array
                    ? tags.EnumerateArray().Select(tag => Text(tag, "name")).ToList()
                    : [],
                read,
                endpoints);
        }

        /// <summary>
        /// A node's type in a compact notation, whether it allows <c>null</c>, and - for a reference
        /// by identifier - the type the document says it points at.
        /// </summary>
        internal static (string Type, bool Nullable, string? RefersTo) Describe(JsonElement node)
        {
            if (node.ValueKind != JsonValueKind.Object)
            {
                return ("any", false, null);
            }

            if (node.TryGetProperty("$ref", out var reference))
            {
                return (
                    ReferenceName(reference.GetString()),
                    false,
                    node.TryGetProperty("$comment", out var comment) ? LastSegment(comment.GetString()) : null);
            }

            foreach (var keyword in new[] { "oneOf", "anyOf" })
            {
                if (node.TryGetProperty(keyword, out var alternatives) && alternatives.ValueKind == JsonValueKind.Array)
                {
                    var described = alternatives.EnumerateArray().Select(Describe).ToList();
                    var present = described.Where(alternative => alternative.Type != "null").ToList();

                    return (
                        present.Count == 0 ? "null" : string.Join(" | ", present.Select(alternative => alternative.Type)),
                        described.Count != present.Count || present.Any(alternative => alternative.Nullable),
                        present.Select(alternative => alternative.RefersTo).FirstOrDefault(target => target is not null));
                }
            }

            if (node.TryGetProperty("const", out var constant))
            {
                return ($"const {constant.GetRawText()}", false, null);
            }

            if (node.TryGetProperty("enum", out var literals) && literals.ValueKind == JsonValueKind.Array)
            {
                return (
                    $"enum ({string.Join(", ", literals.EnumerateArray().Select(literal => literal.ToString()))})",
                    false,
                    null);
            }

            var type = node.TryGetProperty("type", out var declared) ? declared : default;

            if (type.ValueKind == JsonValueKind.String && type.GetString() == "array")
            {
                var (itemType, _, itemTarget) = node.TryGetProperty("items", out var items)
                    ? Describe(items)
                    : ("any", false, null);

                return ($"{(itemType.Contains(' ', StringComparison.Ordinal) ? $"({itemType})" : itemType)}[]", false, itemTarget);
            }

            if (type.ValueKind == JsonValueKind.Array)
            {
                var names = type.EnumerateArray().Select(name => name.GetString() ?? "any").ToList();
                var present = names.Where(name => name != "null").ToList();

                return (present.Count == 0 ? "null" : string.Join(" | ", present), names.Count != present.Count, null);
            }

            if (type.ValueKind == JsonValueKind.String)
            {
                var name = type.GetString()!;

                return (
                    node.TryGetProperty("format", out var format) ? $"{name} ({format.GetString()})" : name,
                    false,
                    null);
            }

            return (node.TryGetProperty("properties", out _) ? OpenApiNames.ObjectKind : "any", false, null);
        }

        private static OpenApiSchema ReadSchema(string name, JsonElement schema, Dictionary<string, string> roles)
        {
            var id = Text(schema, "$id");
            var required = schema.TryGetProperty("required", out var listed) && listed.ValueKind == JsonValueKind.Array
                ? listed.EnumerateArray().Select(item => item.GetString() ?? string.Empty).ToHashSet(StringComparer.Ordinal)
                : [];

            var properties = schema.TryGetProperty("properties", out var members) && members.ValueKind == JsonValueKind.Object
                ? members.EnumerateObject()
                    .Select(member =>
                    {
                        var (type, nullable, refersTo) = Describe(member.Value);
                        return new OpenApiProperty(member.Name, type, required.Contains(member.Name), nullable, refersTo);
                    })
                    .OrderBy(property => property.Name, StringComparer.Ordinal)
                    .ToList()
                : [];

            var variants = new[] { "oneOf", "anyOf" }
                .Where(keyword => schema.TryGetProperty(keyword, out var value) && value.ValueKind == JsonValueKind.Array)
                .SelectMany(keyword => schema.GetProperty(keyword).EnumerateArray())
                .Where(alternative => alternative.ValueKind == JsonValueKind.Object && alternative.TryGetProperty("$ref", out _))
                .Select(alternative => ReferenceName(alternative.GetProperty("$ref").GetString()))
                .ToList();

            var values = schema.TryGetProperty("enum", out var literals) && literals.ValueKind == JsonValueKind.Array
                ? literals.EnumerateArray().Select(literal => literal.ToString()).ToList()
                : [];

            return new OpenApiSchema(
                name,
                id,
                id.Contains(OpenApiNames.ApiIdMarker, StringComparison.Ordinal)
                    ? OpenApiNames.ApiNamespace
                    : OpenApiNames.MetamodelNamespace,
                Kind(schema, properties.Count, variants.Count, values.Count),
                roles.GetValueOrDefault(name, OpenApiNames.NoRole),
                schema.TryGetProperty("additionalProperties", out var additional)
                && additional.ValueKind is JsonValueKind.True or JsonValueKind.False
                    ? additional.GetBoolean()
                    : null,
                properties,
                variants,
                values);
        }

        private static string Kind(JsonElement schema, int properties, int variants, int values)
        {
            if (values > 0)
            {
                return OpenApiNames.EnumerationKind;
            }

            if (properties > 0)
            {
                return OpenApiNames.ObjectKind;
            }

            if (variants > 0)
            {
                return OpenApiNames.UnionKind;
            }

            return Describe(schema).Type;
        }

        private static List<OpenApiEndpoint> ReadEndpoints(JsonElement paths, JsonElement components)
        {
            var endpoints = new List<OpenApiEndpoint>();

            foreach (var path in paths.EnumerateObject())
            {
                foreach (var method in Methods)
                {
                    if (!path.Value.TryGetProperty(method, out var operation))
                    {
                        continue;
                    }

                    endpoints.Add(new OpenApiEndpoint(
                        method.ToUpperInvariant(),
                        path.Name,
                        Text(operation, "operationId"),
                        Text(operation, "summary"),
                        operation.TryGetProperty("tags", out var tags) && tags.ValueKind == JsonValueKind.Array
                            ? tags.EnumerateArray().Select(tag => tag.GetString() ?? string.Empty).ToList()
                            : [],
                        [.. Parameters(path.Value, components), .. Parameters(operation, components)],
                        operation.TryGetProperty("requestBody", out var body) ? BodyType(Resolve(body, components)) : null,
                        operation.TryGetProperty("responses", out var responses) && responses.ValueKind == JsonValueKind.Object
                            ? responses.EnumerateObject()
                                .Select(response =>
                                {
                                    var resolved = Resolve(response.Value, components);
                                    return new OpenApiResponse(response.Name, Text(resolved, "description"), BodyType(resolved));
                                })
                                .ToList()
                            : []));
                }
            }

            return endpoints;
        }

        /// <summary>
        /// The parameters a path item or an operation declares. A path item's apply to every operation
        /// on it, so an endpoint's parameters are its path's followed by its own.
        /// </summary>
        private static IEnumerable<OpenApiParameter> Parameters(JsonElement owner, JsonElement components)
        {
            if (!owner.TryGetProperty("parameters", out var parameters) || parameters.ValueKind != JsonValueKind.Array)
            {
                return [];
            }

            return parameters.EnumerateArray()
                .Select(parameter => Resolve(parameter, components))
                .Select(parameter => new OpenApiParameter(
                    Text(parameter, "name"),
                    Text(parameter, "in"),
                    parameter.TryGetProperty("required", out var must) && must.ValueKind == JsonValueKind.True,
                    parameter.TryGetProperty("schema", out var schema) ? Describe(schema).Type : "any",
                    Text(parameter, "description")));
        }

        /// <summary>
        /// Which schemas the endpoints use in a request body, in a response, or both - directly or
        /// through the schemas those refer to.
        /// </summary>
        private static Dictionary<string, string> Roles(JsonElement paths, JsonElement components, JsonElement schemas)
        {
            var requested = new HashSet<string>(StringComparer.Ordinal);
            var returned = new HashSet<string>(StringComparer.Ordinal);

            foreach (var path in paths.EnumerateObject())
            {
                foreach (var method in Methods)
                {
                    if (!path.Value.TryGetProperty(method, out var operation))
                    {
                        continue;
                    }

                    if (operation.TryGetProperty("requestBody", out var body))
                    {
                        Collect(Resolve(body, components), requested);
                    }

                    if (operation.TryGetProperty("responses", out var responses) && responses.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var response in responses.EnumerateObject())
                        {
                            Collect(Resolve(response.Value, components), returned);
                        }
                    }
                }
            }

            Close(requested, schemas);
            Close(returned, schemas);

            return requested.Union(returned, StringComparer.Ordinal).ToDictionary(
                name => name,
                name => requested.Contains(name) && returned.Contains(name)
                    ? OpenApiNames.BothRoles
                    : requested.Contains(name) ? OpenApiNames.RequestRole : OpenApiNames.ResponseRole,
                StringComparer.Ordinal);
        }

        /// <summary>Extends <paramref name="names"/> with every schema reachable from it.</summary>
        private static void Close(HashSet<string> names, JsonElement schemas)
        {
            var pending = new Queue<string>(names);

            while (pending.Count > 0)
            {
                if (!schemas.TryGetProperty(pending.Dequeue(), out var schema))
                {
                    continue;
                }

                var found = new HashSet<string>(StringComparer.Ordinal);
                Collect(schema, found);

                foreach (var name in found.Where(names.Add))
                {
                    pending.Enqueue(name);
                }
            }
        }

        /// <summary>Every schema <paramref name="node"/> refers to, at any depth within the node.</summary>
        private static void Collect(JsonElement node, HashSet<string> names)
        {
            switch (node.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var property in node.EnumerateObject())
                    {
                        if (property.NameEquals("$ref") && property.Value.ValueKind == JsonValueKind.String
                            && property.Value.GetString()!.StartsWith(SchemaReferencePrefix, StringComparison.Ordinal))
                        {
                            names.Add(ReferenceName(property.Value.GetString()));
                        }
                        else if (!property.NameEquals("examples"))
                        {
                            Collect(property.Value, names);
                        }
                    }

                    break;

                case JsonValueKind.Array:
                    foreach (var item in node.EnumerateArray())
                    {
                        Collect(item, names);
                    }

                    break;
            }
        }

        /// <summary>The type of a request or response body: its first media type's schema.</summary>
        private static string? BodyType(JsonElement body)
        {
            if (body.ValueKind != JsonValueKind.Object
                || !body.TryGetProperty("content", out var content)
                || content.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            foreach (var media in content.EnumerateObject())
            {
                if (media.Value.TryGetProperty("schema", out var schema))
                {
                    return Describe(schema).Type;
                }
            }

            return null;
        }

        /// <summary>Follows a <c>#/components/...</c> reference one step; anything else is returned as is.</summary>
        private static JsonElement Resolve(JsonElement node, JsonElement components)
        {
            if (node.ValueKind != JsonValueKind.Object || !node.TryGetProperty("$ref", out var reference))
            {
                return node;
            }

            var segments = (reference.GetString() ?? string.Empty).Split('/');

            return segments.Length == 4
                   && segments[1] == "components"
                   && components.TryGetProperty(segments[2], out var section)
                   && section.TryGetProperty(segments[3], out var target)
                ? target
                : node;
        }

        private static string ReferenceName(string? reference) => LastSegment(reference) ?? string.Empty;

        private static string? LastSegment(string? value) =>
            string.IsNullOrEmpty(value) ? null : value[(value.LastIndexOf('/') + 1)..];

        private static string Text(JsonElement node, string property) =>
            node.ValueKind == JsonValueKind.Object
            && node.TryGetProperty(property, out var value)
            && value.ValueKind == JsonValueKind.String
                ? value.GetString() ?? string.Empty
                : string.Empty;
    }
}
