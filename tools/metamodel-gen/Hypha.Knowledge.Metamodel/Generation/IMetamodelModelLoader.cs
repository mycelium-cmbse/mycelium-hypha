// ------------------------------------------------------------------------------------------------
// <copyright file="IMetamodelModelLoader.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Metamodel.Generation
{
    using uml4net.xmi.Readers;

    /// <summary>
    /// Loads a release's metamodel from the committed XMI.
    /// </summary>
    public interface IMetamodelModelLoader
    {
        /// <summary>
        /// The combined KerML + SysML model for <paramref name="tag"/>, or <c>null</c> when that
        /// release's XMI has not been fetched.
        /// </summary>
        /// <param name="tag">The release whose metamodel is loaded.</param>
        /// <returns>The model, or <c>null</c> when the release's XMI is absent.</returns>
        XmiReaderResult? Load(string tag);
    }
}
