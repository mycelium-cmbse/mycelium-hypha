// ------------------------------------------------------------------------------------------------
// <copyright file="MetaclassOperation.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.MetamodelGen.Generators
{
    /// <summary>
    /// An owned operation of a metaclass, as rendered in its element file.
    /// </summary>
    public sealed class MetaclassOperation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetaclassOperation"/> class.
        /// </summary>
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
