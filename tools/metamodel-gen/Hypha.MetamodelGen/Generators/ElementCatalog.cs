// ------------------------------------------------------------------------------------------------
// <copyright file="ElementCatalog.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.MetamodelGen.Generators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using uml4net.CommonStructure;
    using uml4net.Extensions;
    using uml4net.Packages;
    using uml4net.SimpleClassifiers;
    using uml4net.StructuredClassifiers;
    using uml4net.xmi.Readers;

    /// <summary>
    /// Central, deterministic access to the generated elements of a model (metaclasses, enumerations
    /// and primitive types) plus the naming helpers shared by the element-file generators.
    /// </summary>
    public static class ElementCatalog
    {
        /// <summary>Returns all metaclasses (<see cref="IClass"/>) in the model, ordered by name.</summary>
        public static IReadOnlyList<IClass> Metaclasses(XmiReaderResult model) =>
            PackagedElements<IClass>(model);

        /// <summary>Returns all enumerations (<see cref="IEnumeration"/>) in the model, ordered by name.</summary>
        public static IReadOnlyList<IEnumeration> Enumerations(XmiReaderResult model) =>
            PackagedElements<IEnumeration>(model);

        /// <summary>Returns all primitive types (<see cref="IPrimitiveType"/>) in the model, ordered by name.</summary>
        public static IReadOnlyList<IPrimitiveType> PrimitiveTypes(XmiReaderResult model) =>
            PackagedElements<IPrimitiveType>(model);

        /// <summary>The interfaces of <paramref name="model"/>, ordered by name.</summary>
        /// <remarks>Empty for KerML and SysML, which declare none.</remarks>
        public static IReadOnlyList<IInterface> Interfaces(XmiReaderResult model) =>
            PackagedElements<IInterface>(model);

        /// <summary>
        /// The data types of <paramref name="model"/> that are neither a primitive type nor an
        /// enumeration, ordered by name.
        /// </summary>
        /// <remarks>Empty for KerML and SysML, which declare none.</remarks>
        public static IReadOnlyList<IDataType> DataTypes(XmiReaderResult model) =>
            PackagedElements<IDataType>(model)
                .Where(dataType => dataType is not IPrimitiveType and not IEnumeration)
                .ToList();

        /// <summary>
        /// The names of the classifiers that realize <paramref name="interface"/>, ordered.
        /// </summary>
        /// <remarks>
        /// Read from the model's realizations rather than from the classes: a realizing classifier may
        /// live in another model - the API's <c>Data</c> is realized by the KerML <c>Element</c>.
        /// </remarks>
        public static IReadOnlyList<string> RealizedBy(XmiReaderResult model, IInterface @interface)
        {
            ArgumentNullException.ThrowIfNull(@interface);

            return PackagedElements<IRealization>(model)
                .Where(realization => realization.Supplier.Any(supplier => supplier.XmiId == @interface.XmiId))
                .SelectMany(realization => realization.Client)
                .Select(client => client.Name)
                .Where(name => !string.IsNullOrEmpty(name))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToList();
        }

        /// <summary>
        /// Returns the set of element names that have a generated element file (metaclasses,
        /// enumerations, primitive types, interfaces and data types), used to decide whether a feature
        /// type can be linked.
        /// </summary>
        public static IReadOnlySet<string> LinkableTypeNames(XmiReaderResult model)
        {
            ArgumentNullException.ThrowIfNull(model);

            return Metaclasses(model).Select(element => element.Name)
                .Concat(Enumerations(model).Select(element => element.Name))
                .Concat(PrimitiveTypes(model).Select(element => element.Name))
                .Concat(Interfaces(model).Select(element => element.Name))
                .Concat(DataTypes(model).Select(element => element.Name))
                .ToHashSet(StringComparer.Ordinal);
        }

        /// <summary>
        /// Builds the fully qualified name by walking the owning-namespace chain up to the root model
        /// and joining the names with <c>::</c> (e.g. <c>SysML::Systems::Actions::AcceptActionUsage</c>).
        /// </summary>
        public static string FullyQualifiedName(INamedElement element)
        {
            ArgumentNullException.ThrowIfNull(element);

            var parts = new List<string>();

            for (INamedElement current = element; current is not null; current = current.Namespace)
            {
                if (!string.IsNullOrEmpty(current.Name))
                {
                    parts.Add(current.Name);
                }
            }

            parts.Reverse();

            return string.Join("::", parts);
        }

        /// <summary>
        /// Returns the visibility as its lowercase name (e.g. <c>public</c>). An unset visibility (the
        /// XMI omits the attribute) reads as <c>public</c>, the default UML 2.5.1 gives a
        /// <c>PackageableElement</c>.
        /// </summary>
        public static string VisibilityName(VisibilityKind? visibility) =>
            (visibility ?? VisibilityKind.Public).ToString().ToLowerInvariant();

        /// <summary>
        /// Returns the named, ordered packaged elements of type <typeparamref name="T"/> across every
        /// package (recursively), de-duplicated by XMI id.
        /// </summary>
        private static List<T> PackagedElements<T>(XmiReaderResult model)
            where T : class, INamedElement
        {
            ArgumentNullException.ThrowIfNull(model);

            return model.Packages
                .SelectMany(ExpandPackages)
                .DistinctBy(package => package.XmiId)
                .SelectMany(package => package.PackagedElement.OfType<T>())
                .DistinctBy(element => element.XmiId)
                .OrderBy(element => element.Name, StringComparer.Ordinal)
                .ToList();
        }

        /// <summary>Yields a package together with all of its (recursively) nested packages.</summary>
        internal static IEnumerable<IPackage> ExpandPackages(IPackage package)
        {
            yield return package;

            foreach (var nested in package.QueryPackages())
            {
                yield return nested;
            }
        }
    }
}
