// ------------------------------------------------------------------------------------------------
// <copyright file="IApiSelector.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api
{
    using Hypha.Knowledge.Releases;

    /// <summary>Switches the default API version.</summary>
    public interface IApiSelector
    {
        /// <summary>Makes <paramref name="tag"/> the default API version.</summary>
        /// <param name="tag">An installed API version.</param>
        /// <returns>
        /// The new default and the one before it. When <paramref name="tag"/> already was the default,
        /// nothing is written and <see cref="ReleaseSelection.Changed"/> is <c>false</c>.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// <paramref name="tag"/> is <c>null</c>, empty or white space, or is not installed; the message
        /// names the installed API versions, or says that none is installed.
        /// </exception>
        ReleaseSelection Use(string tag);
    }
}
