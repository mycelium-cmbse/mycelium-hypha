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
    using System.Globalization;

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
    }
}
