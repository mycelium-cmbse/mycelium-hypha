// ------------------------------------------------------------------------------------------------
// <copyright file="ModelVocabulary.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.MetamodelGen.Generators
{
    /// <summary>
    /// The words a generated page uses for the model it describes.
    /// </summary>
    /// <remarks>
    /// The KerML and SysML models are metamodels, so their classes are metaclasses. The Systems
    /// Modeling API PIM is an ordinary model, and calling its <c>Commit</c> a metaclass would be wrong.
    /// </remarks>
    /// <param name="ModelNoun">What the model is, e.g. <c>metamodel</c>.</param>
    /// <param name="ClassNoun">What one of its classes is, e.g. <c>metaclass</c>.</param>
    /// <param name="ClassNounPlural">The plural of <paramref name="ClassNoun"/>.</param>
    public sealed record ModelVocabulary(string ModelNoun, string ClassNoun, string ClassNounPlural)
    {
        /// <summary>The words for a metamodel - the default, and what KerML and SysML are.</summary>
        public static ModelVocabulary Metamodel { get; } = new("metamodel", "metaclass", "metaclasses");

        /// <summary>The words for an ordinary model, such as the Systems Modeling API PIM.</summary>
        public static ModelVocabulary Model { get; } = new("model", "class", "classes");
    }
}
