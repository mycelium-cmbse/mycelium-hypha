// ------------------------------------------------------------------------------------------------
// <copyright file="IDeclarationScanner.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.ModelLibrary
{
    using System.Collections.Generic;

    /// <summary>
    /// Finds the named declarations in one KerML/SysML model, without resolving them.
    /// </summary>
    public interface IDeclarationScanner
    {
        /// <summary>
        /// Every named declaration in <paramref name="modelText"/>, in the order it declares them.
        /// </summary>
        /// <param name="modelText">The verbatim KerML/SysML source.</param>
        /// <param name="surfaceForms">
        /// This release's SysML surface forms (e.g. <c>"part def"</c>, <c>"action"</c>), as derived by
        /// <see cref="TextualNotation.ISurfaceForms"/> from its metamodel index. KerML's own core
        /// vocabulary (<c>package</c>, <c>classifier</c>, <c>datatype</c>, <c>feature</c>, ...) is
        /// always recognised regardless of what is passed here - it does not follow the
        /// Definition/Usage naming convention <see cref="TextualNotation.ISurfaceForms"/> derives from.
        /// </param>
        /// <param name="reservedKeywords">
        /// This release's reserved words, from both grammars' <c>RESERVED_KEYWORD</c> productions (see
        /// <see cref="Grammar.IGrammarParser.ReservedKeywords"/>). A word never opens as a declared
        /// name even when it is not itself a recognised declaration keyword: <c>attribute def</c> not
        /// being registered as a two-word phrase must not make a bare <c>attribute</c> read the
        /// following <c>def</c> as the name it declares - a reserved word can never be an unquoted
        /// identifier in KerML/SysML, so seeing one is proof this is not the name.
        /// </param>
        IReadOnlyList<LibraryDeclaration> Scan(
            string modelText, IEnumerable<string> surfaceForms, IEnumerable<string> reservedKeywords);
    }
}
