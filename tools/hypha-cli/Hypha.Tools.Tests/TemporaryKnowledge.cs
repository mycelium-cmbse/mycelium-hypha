// ------------------------------------------------------------------------------------------------
// <copyright file="TemporaryKnowledge.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Tests
{
    using System;
    using System.IO;
    using System.Linq;

    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Releases;

    /// <summary>
    /// A repository root in a temporary folder, with releases recorded as installed and generated
    /// files written where the generators write them. Deleted on dispose.
    /// </summary>
    internal sealed class TemporaryKnowledge : IDisposable
    {
        public TemporaryKnowledge()
        {
            this.Root = new DirectoryInfo(
                Path.Combine(Path.GetTempPath(), "hypha-cli-tests-" + Guid.NewGuid().ToString("N")));
            this.Root.Create();
            this.Layout = new KnowledgeLayout(this.Root);
        }

        public DirectoryInfo Root { get; }

        public KnowledgeLayout Layout { get; }

        /// <summary>Records <paramref name="tags"/> as installed, the first as the default.</summary>
        public TemporaryKnowledge Install(params string[] tags)
        {
            VersionManifestFile.Write(
                VersionManifest.Build(
                    tags[0],
                    tags.Select(tag => new InstalledVersion(
                        tag,
                        new UpstreamReference(Upstream.ReleaseRepository, "abc"),
                        new UpstreamReference(Upstream.PilotRepository, "def")))),
                this.Layout.VersionManifest.FullName);

            return this;
        }

        /// <summary>Writes a file under <c>knowledge/&lt;tag&gt;/</c>.</summary>
        public TemporaryKnowledge Write(string tag, string relative, string content)
        {
            var path = Path.Combine(
                this.Layout.Knowledge(tag).FullName, relative.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, content);

            return this;
        }

        public void Dispose()
        {
            this.Root.Refresh();
            if (this.Root.Exists)
            {
                this.Root.Delete(recursive: true);
            }
        }
    }
}
