// ------------------------------------------------------------------------------------------------
// <copyright file="PimFixture.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.MetamodelGen.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Hypha.Knowledge.Api;
    using Hypha.Knowledge.Layout;

    using NUnit.Framework;

    using uml4net.xmi.Readers;

    /// <summary>
    /// Helpers for the hand-written model under <c>Fixtures/pim/</c>, which stands in for the Systems
    /// Modeling API PIM.
    /// </summary>
    /// <remarks>
    /// The published PIM is an OMG document and is not committed. The fixture is synthetic and small,
    /// but it is shaped like the real thing where that matters to the generators: it declares an
    /// interface, realizations and data types that are neither primitive nor an enumeration - none of
    /// which KerML or SysML have - and it refers to the UML primitive types and to the KerML abstract
    /// syntax by the same literal references the published model uses. The KerML it refers to is the
    /// fixture already committed under <c>Fixtures/xmi/</c>.
    /// </remarks>
    internal static class PimFixture
    {
        /// <summary>The API version the fixture is installed as.</summary>
        public const string Tag = "2026-04";

        /// <summary>The name of the fixture's root model.</summary>
        public const string ModelName = "FixtureApi";

        /// <summary>The name of the one package the fixture declares its elements in.</summary>
        public const string PackageName = "ProjectDataVersioning";

        // The literal references the PIM model contains, and that PimGenerator answers to.
        private const string PrimitiveTypesReference = "http://www.omg.org/spec/UML/20161101/PrimitiveTypes.xmi";
        private const string KermlReference = "KerML Abstract Syntax.xml";

        private static readonly Lazy<XmiReaderResult?> LazyModel = new(Load);

        /// <summary>
        /// Gets the fixture model, loaded once, or <c>null</c> when the tests run outside a checkout.
        /// </summary>
        public static XmiReaderResult? Model => LazyModel.Value;

        /// <summary>The fixture PIM model, in the test output.</summary>
        public static string PimPath =>
            Path.Combine(TestContext.CurrentContext.TestDirectory, "Fixtures", "pim", ApiInputs.PimModel);

        /// <summary>The committed KerML fixture the PIM refers to, in the test output.</summary>
        public static string KermlPath =>
            Path.Combine(TestContext.CurrentContext.TestDirectory, "Fixtures", "xmi", ApiInputs.KermlXmi);

        /// <summary>
        /// Lays the fixture out under <paramref name="root"/> the way <c>hypha api fetch</c> would
        /// have: the PIM and the KerML model side by side under <c>sources/api/&lt;tag&gt;/xmi/</c>, and
        /// the shared primitive types above the tag folders.
        /// </summary>
        /// <param name="root">The folder standing in for the repository root.</param>
        /// <param name="outputRoot">Where generated output goes; <paramref name="root"/> when omitted.</param>
        /// <param name="tag">The API version to install the fixture as.</param>
        public static ApiLayout Install(DirectoryInfo root, DirectoryInfo? outputRoot = null, string tag = Tag)
        {
            ArgumentNullException.ThrowIfNull(root);

            var layout = new ApiLayout(new KnowledgeLayout(root, outputRoot));

            var xmi = layout.Xmi(tag);
            xmi.Create();

            File.Copy(PimPath, layout.PimModel(tag).FullName, overwrite: true);
            File.Copy(KermlPath, Path.Combine(xmi.FullName, ApiInputs.KermlXmi), overwrite: true);

            var primitives = layout.SharedPrimitiveTypes;
            primitives.Directory!.Create();
            File.Copy(Repository.Layout!.SharedPrimitiveTypes.FullName, primitives.FullName, overwrite: true);

            return layout;
        }

        /// <summary>
        /// Reads the fixture with the path maps <c>PimGenerator</c> uses, so the generators can be
        /// exercised on the model without going through the file layout.
        /// </summary>
        private static XmiReaderResult? Load()
        {
            if (Repository.Layout is not { } layout || !File.Exists(PimPath) || !File.Exists(KermlPath))
            {
                return null;
            }

            return XmiModelReader.Read(
                PimPath,
                new Dictionary<string, string>
                {
                    [PrimitiveTypesReference] = layout.SharedPrimitiveTypes.FullName,
                    [KermlReference] = KermlPath,
                },
                Path.GetDirectoryName(KermlPath));
        }
    }
}
