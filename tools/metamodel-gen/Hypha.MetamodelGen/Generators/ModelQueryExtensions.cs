// ------------------------------------------------------------------------------------------------
// <copyright file="ModelQueryExtensions.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.MetamodelGen.Generators
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Linq;

    using uml4net.CommonStructure;
    using uml4net.Values;

    /// <summary>
    /// Deterministic queries over the uml4net model shared by more than one generator.
    /// </summary>
    public static class ModelQueryExtensions
    {
        /// <summary>
        /// Converts a uml4net upper-multiplicity bound (a string, where <c>*</c> denotes unbounded and
        /// an empty value denotes the UML default of 1) to a number, encoding unbounded as <c>-1</c>.
        /// </summary>
        public static int QueryUpperBound(string? rawUpper)
        {
            if (string.IsNullOrEmpty(rawUpper))
            {
                return 1;
            }

            return rawUpper == "*" ? -1 : int.Parse(rawUpper, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Extracts the textual (OCL) body of a constraint specification. The specification is held in
        /// a container; the contained opaque expression carries the body. Returns an empty string when
        /// there is no opaque-expression body.
        /// </summary>
        public static string QueryConstraintBody(this IConstraint constraint)
        {
            ArgumentNullException.ThrowIfNull(constraint);

            if (constraint.Specification is IEnumerable specifications)
            {
                var expression = specifications.OfType<IOpaqueExpression>().FirstOrDefault();

                if (expression is not null)
                {
                    return string.Join("\n", expression.Body).Replace("\r\n", "\n").Trim();
                }
            }

            return string.Empty;
        }
    }
}
