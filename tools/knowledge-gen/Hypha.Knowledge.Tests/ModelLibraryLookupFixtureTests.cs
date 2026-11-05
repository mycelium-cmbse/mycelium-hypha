// ------------------------------------------------------------------------------------------------
// <copyright file="ModelLibraryLookupFixtureTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using System.Text.RegularExpressions;

    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Lookup;
    using Hypha.Knowledge.ModelLibrary;

    /// <summary>
    /// <see cref="KnowledgeLookup"/> against real standard-library files, laid out the way
    /// <c>hypha generate</c> lays them out: declarations from <see cref="DeclarationScanner"/>, the page
    /// from <see cref="ModelLibraryRenderer"/>.
    /// </summary>
    /// <remarks>
    /// The fixtures are two unmodified files of Systems-Modeling/SysML-v2-Release (EPL-2.0, see
    /// NOTICE), chosen because synthetic pages missed what they contain: <c>feature all x</c> and
    /// <c>assoc all X</c> (Occurrences.kerml), and members whose enclosing name is also used in
    /// <c>specializes</c> before its own declaration (SysML.sysml).
    /// </remarks>
    [TestFixture]
    public class ModelLibraryLookupFixtureTests
    {
        /// <summary>The release the fixture files are installed and generated as.</summary>
        private const string Tag = "2026-09";

        /// <summary>SysML surface forms the fixture uses; generation reads them from the grammar.</summary>
        private static readonly string[] SurfaceForms = ["metadata def", "attribute", "item", "ref"];

        /// <summary>The KerML / SysML reserved words the scanner must not take for a declared name.</summary>
        private static readonly string[] Reserved =
        [
            "def", "abstract", "about", "alias", "all", "and", "as", "assign", "assoc", "behavior", "binding",
            "bool", "by", "chains", "class", "classifier", "comment", "composite", "conjugate", "conjugates",
            "conjugation", "connector", "const", "crosses", "datatype", "default", "dependency", "derived",
            "differences", "disjoining", "disjoint", "doc", "else", "end", "expr", "false", "feature",
            "featured", "featuring", "filter", "first", "flow", "for", "from", "function", "hastype", "if",
            "implies", "import", "in", "inout", "interaction", "intersects", "inv", "inverse", "inverting",
            "istype", "language", "library", "locale", "member", "meta", "metaclass", "metadata",
            "multiplicity", "namespace", "new", "nonunique", "not", "null", "of", "or", "ordered", "out",
            "package", "portion", "predicate", "private", "protected", "public", "redefined", "redefines",
            "redefinition", "references", "rep", "return", "specialization", "specializes", "standard",
            "step", "struct", "subclassifier", "subset", "subsets", "subtype", "succession", "then", "to",
            "true", "type", "typed", "typing", "unions", "var", "xor", "part", "attribute", "action", "state",
            "ref", "item", "port",
        ];

        /// <summary>The scratch repository root the fixture's model-library knowledge is generated into.</summary>
        private DirectoryInfo root = null!;

        /// <summary>The knowledge layout over <see cref="root"/>.</summary>
        private KnowledgeLayout layout = null!;

        /// <summary>The lookup under test, reading through <see cref="layout"/>.</summary>
        private KnowledgeLookup lookup = null!;

        /// <summary>Creates a scratch repository root with <see cref="Tag"/> installed, and the lookup over it.</summary>
        [SetUp]
        public void SetUp()
        {
            this.root = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "hypha-library-fixture-" + Guid.NewGuid().ToString("N")));
            this.root.Create();
            this.layout = new KnowledgeLayout(this.root);
            this.lookup = new KnowledgeLookup(this.layout);
            KnowledgeLookupTests.Install(this.layout, Tag);
        }

        /// <summary>Deletes the scratch repository root, if a test left it behind.</summary>
        [TearDown]
        public void TearDown()
        {
            this.root.Refresh();
            if (this.root.Exists)
            {
                this.root.Delete(recursive: true);
            }
        }

        [TestCase("Occurrences.kerml", "KerML")]
        [TestCase("SysML.sysml", "SysML")]
        public void Every_declaration_starts_on_its_own_line_that_declares_it(string file, string language)
        {
            var declarations = this.Generate(file, language);
            var wrong = new List<string>();
            var lines = new Dictionary<int, string>();

            foreach (var declaration in declarations)
            {
                var result = this.lookup.Lookup(new LookupRequest(KnowledgeKind.ModelLibrary, declaration.QualifiedName, MaxLines: 1));
                var line = result.DeclarationLine;

                if (line is null || !Declares(result.Content!, declaration))
                {
                    wrong.Add($"{declaration.QualifiedName} [{declaration.Kind}] -> {line}: {result.Content?.Trim()}");
                }
                else if (!lines.TryAdd(line.Value, declaration.QualifiedName))
                {
                    wrong.Add($"{declaration.QualifiedName} shares line {line} with {lines[line.Value]}");
                }
            }

            Assert.That(wrong, Is.Empty, $"{wrong.Count} of {declarations.Count} declarations");
        }

        [Test]
        public void A_feature_declared_with_all_is_found_in_its_own_association()
        {
            this.Generate("Occurrences.kerml", "KerML");

            var result = this.lookup.Lookup(new LookupRequest(
                KnowledgeKind.ModelLibrary, "Occurrences::SelfSameLifeLink::sourceOccurrence", MaxLines: 1));

            Assert.That(result.Content!.Trim(), Is.EqualTo(Source("Occurrences.kerml")[719].Trim()));
        }

        [Test]
        public void A_member_is_not_found_in_a_definition_that_specializes_its_scope()
        {
            var declarations = this.Generate("SysML.sysml", "SysML");
            var isReference = declarations.Single(declaration =>
                declaration.QualifiedName.EndsWith("Systems::Usage::isReference", StringComparison.Ordinal));

            var result = this.lookup.Lookup(new LookupRequest(KnowledgeKind.ModelLibrary, isReference.QualifiedName, MaxLines: 1));

            // Line 443 of SysML.sysml, inside `metadata def Usage`; AttributeUsage's redefinition at
            // line 61 comes first and was taken for it while `specializes Usage` counted as the scope.
            Assert.That(result.Content!.Trim(), Is.EqualTo(Source("SysML.sysml")[442].Trim()));
        }

        /// <summary>
        /// Tells whether a source line declares the given declaration: its kind's keywords, then (after an optional
        /// <c>all</c> and short name) its simple name, quoted or not, as a whole word.
        /// </summary>
        /// <param name="line">The source line the lookup returned as the declaration's line.</param>
        /// <param name="declaration">The declaration the line is expected to declare.</param>
        /// <returns><see langword="true"/> when the line declares that kind and name; otherwise <see langword="false"/>.</returns>
        private static bool Declares(string line, LibraryDeclaration declaration)
        {
            var keyword = string.Join(@"\s+", declaration.Kind.Split(' ').Select(Regex.Escape));
            var name = Regex.Escape(declaration.QualifiedName.Split("::")[^1].Trim('\''));

            return Regex.IsMatch(line, $@"(?<![\w]){keyword}\s+(?:all\s+)?(?:<[^>]*>\s*)?'?{name}'?(?![\w])");
        }

        /// <summary>Reads a fixture file as LF-normalized lines, so a test can index a known line of it.</summary>
        /// <param name="file">The fixture file's name under <c>Fixtures/model-library/</c>.</param>
        /// <returns>The file's lines, zero-based.</returns>
        private static string[] Source(string file) =>
            File.ReadAllText(Fixture(file)).Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');

        /// <summary>Resolves a fixture file copied next to the test assembly.</summary>
        /// <param name="file">The fixture file's name under <c>Fixtures/model-library/</c>.</param>
        /// <returns>The fixture file's full path.</returns>
        private static string Fixture(string file) =>
            Path.Combine(TestContext.CurrentContext.TestDirectory, "Fixtures", "model-library", file);

        /// <summary>Writes the page and the index the generators would write; returns the unique declarations.</summary>
        /// <param name="file">The fixture file to scan and render, under <c>Fixtures/model-library/</c>.</param>
        /// <param name="language">The language the fixture is written in, <c>KerML</c> or <c>SysML</c>, as the page names it.</param>
        /// <returns>
        /// The declarations whose qualified name is unique in the file – the only ones the index records, since a
        /// duplicated name cannot be looked up unambiguously.
        /// </returns>
        private List<LibraryDeclaration> Generate(string file, string language)
        {
            var text = File.ReadAllText(Fixture(file)).Replace("\r\n", "\n", StringComparison.Ordinal);
            var scanned = new DeclarationScanner().Scan(text, SurfaceForms, Reserved);
            var unique = scanned
                .GroupBy(declaration => declaration.QualifiedName, StringComparer.Ordinal)
                .Where(group => group.Count() == 1)
                .Select(group => group.Single())
                .ToList();

            var page = Path.GetFileNameWithoutExtension(file) + ".md";
            var folder = this.layout.ModelLibrary(Tag);
            Directory.CreateDirectory(Path.Combine(folder.FullName, "packages"));
            File.WriteAllText(
                Path.Combine(folder.FullName, "packages", page),
                new ModelLibraryRenderer().RenderPackage("sysml.library/" + file, text, scanned, language));

            var declarations = unique.ToDictionary(
                declaration => declaration.QualifiedName,
                declaration => new { kind = declaration.Kind, file = "packages/" + page, source = file });
            File.WriteAllText(
                this.layout.ModelLibraryIndex(Tag).FullName,
                JsonSerializer.Serialize(new { schemaVersion = "1.0.0", tag = Tag, declarations }));

            return unique;
        }
    }
}
