// ------------------------------------------------------------------------------------------------
// <copyright file="DeclarationScannerTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//
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

    using Hypha.Knowledge.CrossReferences;
    using Hypha.Knowledge.Grammar;
    using Hypha.Knowledge.ModelLibrary;
    using Hypha.Knowledge.TextualNotation;

    /// <summary>
    /// Tests for finding named declarations in KerML/SysML standard-library source, without a real
    /// parser.
    /// </summary>
    [TestFixture]
    public class DeclarationScannerTests
    {
        private static readonly string[] SysmlAttributeForms = ["attribute def", "attribute"];

        // Enough of the real reserved-keyword vocabulary to exercise the tests below - "def" above
        // all, since a bare keyword not registered as its own two-word phrase must never read it as
        // the name it declares.
        private static readonly string[] ReservedWords = ["def", "in", "out", "inout", "all", "from"];

        private readonly DeclarationScanner scanner = new();

        [Test]
        public void Nested_scopes_build_dotted_qualified_names()
        {
            const string text = """
                standard library package Outer {
                    package Inner {
                        attribute def Foo {
                            attribute bar: Integer;
                        }
                    }
                }
                """;

            var declared = this.scanner.Scan(text, SysmlAttributeForms, ReservedWords)
                .Select(declaration => declaration.QualifiedName)
                .ToList();

            Assert.That(declared, Is.EqualTo(new[]
            {
                "Outer",
                "Outer::Inner",
                "Outer::Inner::Foo",
                "Outer::Inner::Foo::bar",
            }));
        }

        [Test]
        public void The_keyword_that_introduced_a_declaration_is_kept_verbatim()
        {
            const string text = "package P { attribute def LengthValue { attribute num: Real; } }";

            var declarations = this.scanner.Scan(text, SysmlAttributeForms, ReservedWords);

            Assert.Multiple(() =>
            {
                Assert.That(
                    declarations.Single(d => d.QualifiedName == "P::LengthValue").Kind,
                    Is.EqualTo("attribute def"));
                Assert.That(
                    declarations.Single(d => d.QualifiedName == "P::LengthValue::num").Kind,
                    Is.EqualTo("attribute"));
            });
        }

        [Test]
        public void A_quoted_name_is_read_without_its_quotes()
        {
            const string text = "package P { attribute def 'Length Value' { } }";

            var declared = this.scanner.Scan(text, SysmlAttributeForms, ReservedWords)
                .Select(declaration => declaration.QualifiedName);

            Assert.That(declared, Does.Contain("P::Length Value"));
        }

        [Test]
        public void A_redefinition_target_is_not_mistaken_for_a_new_declaration()
        {
            // Straight from ISQBase.sysml's shape: ":>>" separates the keyword from a reference to an
            // existing feature, not a fresh name.
            const string text = """
                package P {
                    attribute def LengthUnit {
                        attribute :>> quantityDimension { :>> quantityPowerFactors = lengthPF; }
                    }
                }
                """;

            var declared = this.scanner.Scan(text, SysmlAttributeForms, ReservedWords)
                .Select(declaration => declaration.QualifiedName)
                .ToList();

            Assert.That(declared, Is.EqualTo(new[] { "P", "P::LengthUnit" }));
        }

        [Test]
        public void A_body_less_declaration_does_not_open_a_scope()
        {
            const string text =
                "package P { attribute length: LengthValue[*] nonunique :> scalarQuantities; " +
                "attribute def Next { } }";

            var declared = this.scanner.Scan(text, SysmlAttributeForms, ReservedWords)
                .Select(declaration => declaration.QualifiedName)
                .ToList();

            // "length" must not have swallowed "Next" into its own (non-existent) body.
            Assert.That(declared, Is.EqualTo(new[] { "P", "P::length", "P::Next" }));
        }

        [Test]
        public void A_doc_comment_braces_and_all_does_not_perturb_nesting()
        {
            const string text = """
                package P {
                    attribute def Length {
                        doc
                        /*
                         * a set is written `{1, 2, 3}` in AsciiMath, and this one is unbalanced: {
                         */
                        attribute num: Real;
                    }
                }
                """;

            var declared = this.scanner.Scan(text, SysmlAttributeForms, ReservedWords)
                .Select(declaration => declaration.QualifiedName)
                .ToList();

            Assert.That(declared, Is.EqualTo(new[] { "P", "P::Length", "P::Length::num" }));
        }

        [Test]
        public void A_keyword_immediately_followed_by_another_keyword_declares_nothing_for_the_first()
        {
            // "succession flow x" - "succession" alone must not swallow "flow" as if it were the name;
            // "flow" gets its own turn and reports the real name.
            const string text = "package P { succession flow x { } }";

            var declarations = this.scanner.Scan(text, SysmlAttributeForms, ReservedWords);

            Assert.Multiple(() =>
            {
                Assert.That(declarations.Select(d => d.QualifiedName), Does.Not.Contain("P::flow"));
                Assert.That(declarations.Select(d => d.QualifiedName), Does.Contain("P::x"));
                Assert.That(declarations.Single(d => d.QualifiedName == "P::x").Kind, Is.EqualTo("flow"));
            });
        }

        [Test]
        public void A_bare_keyword_never_reads_the_reserved_word_that_follows_as_its_name()
        {
            // The bug this guards: "flow def X" with "flow def" NOT registered as its own two-word
            // phrase (only bare "flow" is). Without the reserved-word check, bare "flow" would read
            // the literal word "def" as the name it declares.
            const string text = "package P { flow def X { } }";

            var declared = this.scanner.Scan(text, [], ["def"])
                .Select(declaration => declaration.QualifiedName)
                .ToList();

            Assert.That(declared, Is.EqualTo(new[] { "P" }));
        }

        [Test]
        public void Core_kerml_keywords_are_recognised_without_any_surface_forms_supplied()
        {
            const string text = "standard library package ScalarValues { datatype Boolean; }";

            var declared = this.scanner.Scan(text, [], ReservedWords)
                .Select(declaration => declaration.QualifiedName)
                .ToList();

            Assert.That(declared, Is.EqualTo(new[] { "ScalarValues", "ScalarValues::Boolean" }));
        }

        [Test]
        public void The_real_ScalarValues_file_declares_its_numeric_hierarchy()
        {
            var path = Path.Combine(
                Repository.Layout!.Root.FullName,
                "sources", "2026-05", "textual", "sysml.library",
                "Kernel Libraries", "Kernel Data Type Library", "ScalarValues.kerml");

            if (!File.Exists(path))
            {
                Assert.Ignore("the standard library has not been fetched for 2026-05");
                return;
            }

            var declared = this.scanner.Scan(File.ReadAllText(path), [], ReservedWords)
                .Select(declaration => declaration.QualifiedName)
                .ToList();

            Assert.That(declared, Is.EqualTo(new[]
            {
                "ScalarValues",
                "ScalarValues::ScalarValue",
                "ScalarValues::Boolean",
                "ScalarValues::String",
                "ScalarValues::NumericalValue",
                "ScalarValues::Number",
                "ScalarValues::Complex",
                "ScalarValues::Real",
                "ScalarValues::Rational",
                "ScalarValues::Integer",
                "ScalarValues::Natural",
                "ScalarValues::Positive",
            }));
        }

        [Test]
        [Explicit("a one-off sweep of the real corpus, not a repeatable assertion")]
        public void Diagnostic_sweep_of_the_real_2026_05_library()
        {
            var layout = Repository.Layout!;
            var root = layout.ModelLibrarySources("2026-05");

            if (!root.Exists)
            {
                Assert.Ignore("the standard library has not been fetched for 2026-05");
                return;
            }

            var reader = new KnowledgeReader();
            var index = layout.MetamodelIndex("2026-05");
            var forms = new SurfaceForms()
                .Index(reader.ReadElements(index).Elements.Select(element => element.Name))
                .Forms.Values;

            var parser = new GrammarParser();
            var reserved = new HashSet<string>(StringComparer.Ordinal);
            foreach (var grammarFile in new[] { "KerML-textual-bnf.kebnf", "SysML-textual-bnf.kebnf" })
            {
                var file = layout.Grammar("2026-05", grammarFile);
                if (file.Exists)
                {
                    reserved.UnionWith(parser.ReservedKeywords(File.ReadAllText(file.FullName)));
                }
            }

            var byName = new Dictionary<string, string>();
            var duplicates = new List<string>();
            var total = 0;

            foreach (var file in root.EnumerateFiles("*.*", SearchOption.AllDirectories))
            {
                if (file.Extension is not (".sysml" or ".kerml"))
                {
                    continue;
                }

                var declarations = this.scanner.Scan(File.ReadAllText(file.FullName), forms, reserved);
                total += declarations.Count;

                foreach (var declaration in declarations)
                {
                    if (byName.TryGetValue(declaration.QualifiedName, out var existing))
                    {
                        duplicates.Add($"{declaration.QualifiedName}: {existing} vs {file.Name}");
                    }
                    else
                    {
                        byName[declaration.QualifiedName] = file.Name;
                    }
                }
            }

            TestContext.Out.WriteLine($"total declarations: {total}");
            TestContext.Out.WriteLine($"distinct qualified names: {byName.Count}");
            TestContext.Out.WriteLine($"duplicates: {duplicates.Count}");
            foreach (var duplicate in duplicates)
            {
                TestContext.Out.WriteLine(duplicate);
            }
        }
    }
}
