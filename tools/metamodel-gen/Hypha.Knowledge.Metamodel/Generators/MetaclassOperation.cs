// ------------------------------------------------------------------------------------------------
// <copyright file="MetaclassOperation.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Metamodel.Generators
{
    /// <summary>
    /// An owned operation of a metaclass, as rendered in its element file.
    /// </summary>
    public sealed class MetaclassOperation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetaclassOperation"/> class.
        /// </summary>
        /// <param name="name">The operation name.</param>
        /// <param name="signature">The UML-style signature.</param>
        /// <param name="isQuery">Whether the operation is a query.</param>
        /// <param name="documentation">The operation documentation (may be empty).</param>
        /// <param name="body">The OCL of the operation's <c>bodyCondition</c> (may be empty).</param>
        public MetaclassOperation(string name, string signature, bool isQuery, string documentation, string body)
        {
            this.Name = name;
            this.Signature = signature;
            this.IsQuery = isQuery;
            this.Documentation = documentation;
            this.Body = body;
        }

        /// <summary>Gets the operation name.</summary>
        public string Name { get; }

        /// <summary>
        /// Gets the UML-style signature, e.g. <c>namesOf(element : Element) : String [0..*]</c>.
        /// </summary>
        public string Signature { get; }

        /// <summary>Gets a value indicating whether the operation is a query.</summary>
        public bool IsQuery { get; }

        /// <summary>Gets the operation documentation (may be empty).</summary>
        public string Documentation { get; }

        /// <summary>Gets the OCL of the operation's <c>bodyCondition</c> (may be empty).</summary>
        public string Body { get; }
    }
}
