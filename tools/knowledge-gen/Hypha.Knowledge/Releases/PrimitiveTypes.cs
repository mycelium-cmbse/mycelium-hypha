// ------------------------------------------------------------------------------------------------
// <copyright file="PrimitiveTypes.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Releases
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Layout;

    /// <summary>
    /// The OMG UML primitives library, <c>sources/PrimitiveTypes.xmi</c>, which every release's metamodel and
    /// every API version's PIM refer to and no upstream repository holds.
    /// </summary>
    internal static class PrimitiveTypes
    {
        /// <summary>The manifest resource name of the copy this assembly carries.</summary>
        private const string Resource = "Hypha.Knowledge.Releases.PrimitiveTypes.xmi";

        /// <summary>Writes the library from the copy this assembly carries, when it is not there yet.</summary>
        /// <remarks>
        /// A source checkout commits the file; a knowledge folder anywhere else (the home folder, a
        /// plugin's) would otherwise have none, and neither a metamodel nor a PIM could be generated. A file
        /// that is there is kept as it is. Both the release and the API installer call this, since either
        /// can be the first thing installed in a knowledge folder.
        /// </remarks>
        /// <param name="shared">Where the library goes: <see cref="IKnowledgeLayout.SharedPrimitiveTypes"/>.</param>
        /// <param name="cancellationToken">Cancels the write.</param>
        /// <returns>A task that completes once the file is there.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="shared"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">The assembly does not carry the file, which is a build fault.</exception>
        public static async Task ProvideAsync(FileInfo shared, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(shared);

            shared.Refresh();

            if (shared.Exists)
            {
                return;
            }

            await using var resource = typeof(PrimitiveTypes).Assembly.GetManifestResourceStream(Resource)
                ?? throw new InvalidOperationException($"this build of hypha does not carry {Resource}");
            using var reader = new StreamReader(resource);

            await ReaderSafeFile.WriteAllTextAsync(shared.FullName, await reader.ReadToEndAsync(cancellationToken), cancellationToken);
        }
    }
}
