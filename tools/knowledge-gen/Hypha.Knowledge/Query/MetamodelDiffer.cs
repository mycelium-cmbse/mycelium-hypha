// ------------------------------------------------------------------------------------------------
// <copyright file="MetamodelDiffer.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Query
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using Hypha.Knowledge.Metamodel;

    /// <summary>
    /// Compares two <c>metamodel.json</c> documents field by field. An element or member is matched on
    /// its XMI id first, so a rename is reported as one, and on its name when the ids differ or are
    /// missing. Inherited members are not compared: they follow from what their declaring metaclass owns.
    /// </summary>
    public sealed class MetamodelDiffer : IMetamodelDiffer
    {
        /// <summary>Every change between two documents.</summary>
        /// <param name="from">The older document.</param>
        /// <param name="to">The newer document.</param>
        /// <returns>The changes, sorted by element, element kind, member, member kind, kind and then field.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="from"/> or <paramref name="to"/> is <c>null</c>.</exception>
        public IReadOnlyList<MetamodelChange> Compare(MetamodelDocument from, MetamodelDocument to)
        {
            ArgumentNullException.ThrowIfNull(from);
            ArgumentNullException.ThrowIfNull(to);

            var changes = new List<MetamodelChange>();

            Elements(changes, "class", from.Classes, to.Classes, node => node.XmiId, node => node.Name, CompareClass);
            Elements(changes, "enumeration", from.Enumerations, to.Enumerations, node => node.XmiId, node => node.Name, CompareEnumeration);
            Elements(
                changes, "primitiveType", from.PrimitiveTypes, to.PrimitiveTypes, node => node.XmiId, node => node.Name, ComparePrimitiveType);

            return changes
                .OrderBy(change => change.Element, StringComparer.Ordinal)
                .ThenBy(change => change.ElementKind, StringComparer.Ordinal)
                .ThenBy(change => change.Member ?? string.Empty, StringComparer.Ordinal)
                .ThenBy(change => change.MemberKind ?? string.Empty, StringComparer.Ordinal)
                .ThenBy(change => change.Kind, StringComparer.Ordinal)
                .ThenBy(change => change.Field ?? string.Empty, StringComparer.Ordinal)
                .ToList();
        }

        /// <summary>Pairs the items of two lists: on a shared XMI id first, then on a shared name.</summary>
        /// <typeparam name="T">The kind of item.</typeparam>
        /// <param name="from">The older items.</param>
        /// <param name="to">The newer items.</param>
        /// <param name="id">The XMI id of an item, empty when it has none.</param>
        /// <param name="name">The name of an item.</param>
        /// <returns>
        /// Each pair, with <c>null</c> on the newer side for an item removed and on the older side for one
        /// added.
        /// </returns>
        private static List<(T? From, T? To)> Match<T>(IReadOnlyList<T> from, IReadOnlyList<T> to, Func<T, string> id, Func<T, string> name)
            where T : class
        {
            var pairs = new List<(T? From, T? To)>();
            var matched = new HashSet<T>(ReferenceEqualityComparer.Instance);
            var byId = to
                .Where(item => id(item).Length > 0)
                .GroupBy(id, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
            var unmatched = new List<T>();

            foreach (var item in from)
            {
                if (id(item).Length > 0 && byId.Remove(id(item), out var match))
                {
                    pairs.Add((item, match));
                    matched.Add(match);
                }
                else
                {
                    unmatched.Add(item);
                }
            }

            var byName = to
                .Where(item => !matched.Contains(item))
                .GroupBy(name, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => new Queue<T>(group), StringComparer.Ordinal);

            foreach (var item in unmatched)
            {
                if (byName.TryGetValue(name(item), out var candidates) && candidates.Count > 0)
                {
                    var match = candidates.Dequeue();
                    pairs.Add((item, match));
                    matched.Add(match);
                }
                else
                {
                    pairs.Add((item, null));
                }
            }

            pairs.AddRange(to.Where(item => !matched.Contains(item)).Select(item => ((T?)null, (T?)item)));

            return pairs;
        }

        /// <summary>Compares the elements of one kind.</summary>
        /// <typeparam name="T">The kind of element.</typeparam>
        /// <param name="changes">The changes found so far, added to.</param>
        /// <param name="kind">The element kind, for the changes.</param>
        /// <param name="from">The older elements.</param>
        /// <param name="to">The newer elements.</param>
        /// <param name="id">The XMI id of an element.</param>
        /// <param name="name">The name of an element.</param>
        /// <param name="compare">Compares the fields and members of an element found in both.</param>
        private static void Elements<T>(
            List<MetamodelChange> changes,
            string kind,
            IReadOnlyList<T> from,
            IReadOnlyList<T> to,
            Func<T, string> id,
            Func<T, string> name,
            Action<Scope, T, T> compare)
            where T : class
        {
            foreach (var (older, newer) in Match(from, to, id, name))
            {
                if (newer is null)
                {
                    new Scope(changes, name(older!), kind, null, null).Add(MetamodelChange.Removed);
                    continue;
                }

                var scope = new Scope(changes, name(newer), kind, null, null);

                if (older is null)
                {
                    scope.Add(MetamodelChange.Added);
                    continue;
                }

                scope.Field(MetamodelChange.Renamed, "name", name(older), name(newer));
                compare(scope, older, newer);
            }
        }

        /// <summary>Compares the members of one kind that an element owns.</summary>
        /// <typeparam name="T">The kind of member.</typeparam>
        /// <param name="element">The element's scope.</param>
        /// <param name="kind">The member kind, for the changes.</param>
        /// <param name="from">The older members.</param>
        /// <param name="to">The newer members.</param>
        /// <param name="id">The XMI id of a member.</param>
        /// <param name="name">The name of a member.</param>
        /// <param name="label">How a member's name is written in a change.</param>
        /// <param name="compare">Compares the fields of a member found in both.</param>
        private static void Members<T>(
            Scope element,
            string kind,
            IReadOnlyList<T> from,
            IReadOnlyList<T> to,
            Func<T, string> id,
            Func<T, string> name,
            Func<string, string> label,
            Action<Scope, T, T> compare)
            where T : class
        {
            foreach (var (older, newer) in Match(from, to, id, name))
            {
                if (newer is null)
                {
                    element.Member(label(name(older!)), kind).Add(MetamodelChange.Removed);
                    continue;
                }

                var scope = element.Member(label(name(newer)), kind);

                if (older is null)
                {
                    scope.Add(MetamodelChange.Added);
                    continue;
                }

                scope.Field(MetamodelChange.Renamed, "name", name(older), name(newer));
                compare(scope, older, newer);
            }
        }

        /// <summary>Compares a metaclass found in both documents.</summary>
        /// <param name="scope">The metaclass's scope.</param>
        /// <param name="from">The older metaclass.</param>
        /// <param name="to">The newer metaclass.</param>
        private static void CompareClass(Scope scope, ClassNode from, ClassNode to)
        {
            scope.Field("package", Namespace(from.QualifiedName), Namespace(to.QualifiedName));
            scope.Field("isAbstract", Text(from.IsAbstract), Text(to.IsAbstract));
            scope.Field("visibility", from.Visibility, to.Visibility);
            scope.Field("extends", List(from.Extends), List(to.Extends));
            scope.Documentation(from.Documentation, to.Documentation);

            Members(scope, "attribute", from.OwnedAttributes, to.OwnedAttributes, node => node.XmiId, node => node.Name, Same, CompareAttribute);
            Members(scope, "operation", from.OwnedOperations, to.OwnedOperations, node => node.XmiId, node => node.Name, Same, CompareOperation);
            Members(scope, "constraint", from.Constraints, to.Constraints, node => node.XmiId, node => node.Name, Same, CompareConstraint);
        }

        /// <summary>Compares an attribute found in both documents.</summary>
        /// <param name="scope">The attribute's scope.</param>
        /// <param name="from">The older attribute.</param>
        /// <param name="to">The newer attribute.</param>
        private static void CompareAttribute(Scope scope, AttributeNode from, AttributeNode to)
        {
            scope.Field("type", from.Type, to.Type);
            scope.Field("lower", Text(from.Lower), Text(to.Lower));
            scope.Field("upper", Text(from.Upper), Text(to.Upper));
            scope.Field("isDerived", Text(from.IsDerived), Text(to.IsDerived));
            scope.Field("isDerivedUnion", Text(from.IsDerivedUnion), Text(to.IsDerivedUnion));
            scope.Field("isComposite", Text(from.IsComposite), Text(to.IsComposite));
            scope.Field("isOrdered", Text(from.IsOrdered), Text(to.IsOrdered));
            scope.Field("isUnique", Text(from.IsUnique), Text(to.IsUnique));
            scope.Field("isReadOnly", Text(from.IsReadOnly), Text(to.IsReadOnly));
            scope.Field("defaultValue", from.DefaultValue, to.DefaultValue);
            scope.Field("visibility", from.Visibility, to.Visibility);
            scope.Field("redefines", List(from.Redefines.Select(Text)), List(to.Redefines.Select(Text)));
            scope.Field("subsets", List(from.Subsets.Select(Text)), List(to.Subsets.Select(Text)));
            scope.Field("opposite", from.Opposite is null ? null : Text(from.Opposite), to.Opposite is null ? null : Text(to.Opposite));
            scope.Documentation(from.Documentation, to.Documentation);
        }

        /// <summary>Compares an operation found in both documents, and its parameters.</summary>
        /// <param name="scope">The operation's scope.</param>
        /// <param name="from">The older operation.</param>
        /// <param name="to">The newer operation.</param>
        private static void CompareOperation(Scope scope, OperationNode from, OperationNode to)
        {
            scope.Field("returnType", from.ReturnType, to.ReturnType);
            scope.Field("returnLower", Text(from.ReturnLower), Text(to.ReturnLower));
            scope.Field("returnUpper", Text(from.ReturnUpper), Text(to.ReturnUpper));
            scope.Field("isQuery", Text(from.IsQuery), Text(to.IsQuery));
            scope.Field(
                "redefines",
                List(from.Redefines.Select(reference => $"{reference.Class}::{reference.Operation}")),
                List(to.Redefines.Select(reference => $"{reference.Class}::{reference.Operation}")));
            scope.Field("body", from.Body, to.Body);
            scope.Documentation(from.Documentation, to.Documentation);

            var operation = scope.Parent();

            Members(
                operation,
                "parameter",
                from.Parameters,
                to.Parameters,
                node => node.XmiId,
                node => node.Name,
                parameter => $"{to.Name}({parameter})",
                (parameter, older, newer) =>
                {
                    parameter.Field("type", older.Type, newer.Type);
                    parameter.Field("direction", older.Direction, newer.Direction);
                    parameter.Field("lower", Text(older.Lower), Text(newer.Lower));
                    parameter.Field("upper", Text(older.Upper), Text(newer.Upper));
                    parameter.Field(
                        "position",
                        Text(IndexOf(from.Parameters, older)),
                        Text(IndexOf(to.Parameters, newer)));
                });
        }

        /// <summary>Compares a constraint found in both documents.</summary>
        /// <param name="scope">The constraint's scope.</param>
        /// <param name="from">The older constraint.</param>
        /// <param name="to">The newer constraint.</param>
        private static void CompareConstraint(Scope scope, ConstraintNode from, ConstraintNode to)
        {
            scope.Field("ocl", from.Ocl, to.Ocl);
            scope.Field(
                "constrainedElements",
                List(from.ConstrainedElements.Select(Text)),
                List(to.ConstrainedElements.Select(Text)));
            scope.Documentation(from.Documentation, to.Documentation);
        }

        /// <summary>Compares an enumeration found in both documents, and its literals.</summary>
        /// <param name="scope">The enumeration's scope.</param>
        /// <param name="from">The older enumeration.</param>
        /// <param name="to">The newer enumeration.</param>
        private static void CompareEnumeration(Scope scope, EnumerationNode from, EnumerationNode to)
        {
            scope.Field("package", Namespace(from.QualifiedName), Namespace(to.QualifiedName));
            scope.Field("visibility", from.Visibility, to.Visibility);
            scope.Documentation(from.Documentation, to.Documentation);

            Members(
                scope,
                "literal",
                from.Literals,
                to.Literals,
                node => node.XmiId,
                node => node.Name,
                Same,
                (literal, older, newer) => literal.Documentation(older.Documentation, newer.Documentation));
        }

        /// <summary>Compares a primitive type found in both documents.</summary>
        /// <param name="scope">The primitive type's scope.</param>
        /// <param name="from">The older primitive type.</param>
        /// <param name="to">The newer primitive type.</param>
        private static void ComparePrimitiveType(Scope scope, PrimitiveTypeNode from, PrimitiveTypeNode to)
        {
            scope.Field("package", Namespace(from.QualifiedName), Namespace(to.QualifiedName));
            scope.Field("visibility", from.Visibility, to.Visibility);
            scope.Documentation(from.Documentation, to.Documentation);
        }

        /// <summary>A member's name as it is written in a change: unchanged.</summary>
        /// <param name="name">The name.</param>
        /// <returns>The name.</returns>
        private static string Same(string name) => name;

        /// <summary>The qualified name of the package an element is in.</summary>
        /// <param name="qualifiedName">The element's qualified name.</param>
        /// <returns>Everything before the last <c>::</c>, or empty for an element outside any package.</returns>
        private static string Namespace(string qualifiedName)
        {
            var separator = qualifiedName.LastIndexOf("::", StringComparison.Ordinal);

            return separator < 0 ? string.Empty : qualifiedName[..separator];
        }

        /// <summary>The position of an item in a list, by reference.</summary>
        /// <typeparam name="T">The kind of item.</typeparam>
        /// <param name="items">The list.</param>
        /// <param name="item">The item.</param>
        /// <returns>Its zero-based position.</returns>
        private static int IndexOf<T>(IReadOnlyList<T> items, T item)
            where T : class
        {
            for (var index = 0; index < items.Count; index++)
            {
                if (ReferenceEquals(items[index], item))
                {
                    return index;
                }
            }

            return -1;
        }

        /// <summary>A list of values as one text, sorted so that a reordering is not a change.</summary>
        /// <param name="values">The values.</param>
        /// <returns>The values sorted ordinally and joined by <c>, </c>; <c>null</c> for an empty list.</returns>
        private static string? List(IEnumerable<string> values)
        {
            var sorted = values.Order(StringComparer.Ordinal).ToList();

            return sorted.Count == 0 ? null : string.Join(", ", sorted);
        }

        /// <summary>A flag as text.</summary>
        /// <param name="value">The flag.</param>
        /// <returns><c>true</c> or <c>false</c>.</returns>
        private static string Text(bool value) => value ? "true" : "false";

        /// <summary>A number as text, independent of the culture.</summary>
        /// <param name="value">The number.</param>
        /// <returns>The number in invariant digits.</returns>
        private static string Text(int value) => value.ToString(CultureInfo.InvariantCulture);

        /// <summary>A reference to a feature as text.</summary>
        /// <param name="reference">The reference.</param>
        /// <returns><c>Owner::feature</c>, the owner being the class or the association.</returns>
        private static string Text(FeatureReference reference) => $"{reference.Class ?? reference.Association}::{reference.Feature}";

        /// <summary>A reference to a constrained element as text.</summary>
        /// <param name="reference">The reference.</param>
        /// <returns>The kind, then <c>Owner::name</c>, or the name alone for an element a package owns.</returns>
        private static string Text(ElementReference reference) =>
            reference.Owner is null ? $"{reference.Kind} {reference.Name}" : $"{reference.Kind} {reference.Owner}::{reference.Name}";

        /// <summary>Where changes are recorded: one element, or one member of it.</summary>
        private sealed class Scope
        {
            /// <summary>The changes found so far.</summary>
            private readonly List<MetamodelChange> changes;

            /// <summary>The element's name.</summary>
            private readonly string element;

            /// <summary>The element's kind.</summary>
            private readonly string elementKind;

            /// <summary>The member's name, or <c>null</c> for the element itself.</summary>
            private readonly string? member;

            /// <summary>The member's kind, or <c>null</c> for the element itself.</summary>
            private readonly string? memberKind;

            /// <summary>Initializes a new instance of the <see cref="Scope"/> class.</summary>
            /// <param name="changes">The changes found so far, added to.</param>
            /// <param name="element">The element's name.</param>
            /// <param name="elementKind">The element's kind.</param>
            /// <param name="member">The member's name, or <c>null</c> for the element itself.</param>
            /// <param name="memberKind">The member's kind, or <c>null</c> for the element itself.</param>
            public Scope(List<MetamodelChange> changes, string element, string elementKind, string? member, string? memberKind)
            {
                this.changes = changes;
                this.element = element;
                this.elementKind = elementKind;
                this.member = member;
                this.memberKind = memberKind;
            }

            /// <summary>The scope of one member of this scope's element.</summary>
            /// <param name="name">The member's name, as it is written in a change.</param>
            /// <param name="kind">The member's kind.</param>
            /// <returns>The member's scope.</returns>
            public Scope Member(string name, string kind) => new(this.changes, this.element, this.elementKind, name, kind);

            /// <summary>The scope of this scope's element, so a member's own parts are written beside it.</summary>
            /// <returns>The element's scope.</returns>
            public Scope Parent() => new(this.changes, this.element, this.elementKind, null, null);

            /// <summary>Records a change with no field: an addition or a removal.</summary>
            /// <param name="kind">The kind of change.</param>
            public void Add(string kind) =>
                this.changes.Add(new MetamodelChange(this.element, this.elementKind, this.member, this.memberKind, kind, null, null, null));

            /// <summary>Records a field change when the two values differ.</summary>
            /// <param name="field">The field.</param>
            /// <param name="from">The older value.</param>
            /// <param name="to">The newer value.</param>
            public void Field(string field, string? from, string? to) => this.Field(MetamodelChange.Changed, field, from, to);

            /// <summary>Records a change of a given kind to a field when the two values differ.</summary>
            /// <param name="kind">The kind of change.</param>
            /// <param name="field">The field.</param>
            /// <param name="from">The older value.</param>
            /// <param name="to">The newer value.</param>
            public void Field(string kind, string field, string? from, string? to)
            {
                if (!string.Equals(from, to, StringComparison.Ordinal))
                {
                    this.changes.Add(new MetamodelChange(this.element, this.elementKind, this.member, this.memberKind, kind, field, from, to));
                }
            }

            /// <summary>Records a documentation change, without the texts, when the two texts differ.</summary>
            /// <param name="from">The older text.</param>
            /// <param name="to">The newer text.</param>
            public void Documentation(string from, string to)
            {
                if (!string.Equals(from, to, StringComparison.Ordinal))
                {
                    this.Add(MetamodelChange.Documentation);
                }
            }
        }
    }
}
