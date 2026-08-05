// ------------------------------------------------------------------------------------------------
// <copyright file="SurfaceFormsTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Tests
{
    using Hypha.Knowledge.TextualNotation;

    /// <summary>
    /// Tests for deriving the textual-notation surface form of a metaclass, and for matching it
    /// against a model.
    /// </summary>
    [TestFixture]
    public class SurfaceFormsTests
    {
        private ISurfaceForms forms = null!;

        [SetUp]
        public void SetUp() => this.forms = new SurfaceForms();

        [TestCase("PartDefinition", "part def")]
        [TestCase("PartUsage", "part")]
        [TestCase("AttributeDefinition", "attribute def")]
        [TestCase("UseCaseDefinition", "use case def")]
        [TestCase("ConnectionUsage", "connection")]
        [TestCase("SuccessionFlowUsage", "succession flow")]
        public void Follows_the_metamodel_naming_convention(string metaclass, string expected) =>
            Assert.That(this.forms.Of(metaclass), Is.EqualTo(expected));

        [TestCase("Element")]
        [TestCase("Feature")]
        [TestCase("Type")]
        [TestCase("Definition")]
        [TestCase("Usage")]
        public void Metaclasses_without_a_surface_form_are_skipped(string metaclass) =>
            Assert.That(this.forms.Of(metaclass), Is.Null);

        [Test]
        public void Elements_in_matches_declarations()
        {
            var index = this.forms.Index(
                ["PartDefinition", "PartUsage", "AttributeUsage", "ActionDefinition"]);

            var found = index.ElementsIn("part def Vehicle {\n\tattribute mass : Real;\n}");

            Assert.That(found, Is.EqualTo(new[] { "AttributeUsage", "PartDefinition", "PartUsage" }));
        }

        [Test]
        public void Elements_in_does_not_match_inside_an_identifier()
        {
            // "counterpart" is not a "part".
            var index = this.forms.Index(["PartUsage"]);

            Assert.That(index.ElementsIn("attribute counterpart : String;"), Is.Empty);
        }

        [Test]
        public void Elements_in_does_not_match_inside_a_quoted_name()
        {
            // 'part' in quotes is a name, not a declaration.
            var index = this.forms.Index(["PartUsage"]);

            Assert.That(index.ElementsIn("attribute 'part' : String;"), Is.Empty);
        }

        [Test]
        public void Elements_in_returns_nothing_for_an_unrelated_model()
        {
            var index = this.forms.Index(["PartDefinition", "ActionDefinition"]);

            Assert.That(index.ElementsIn("package Empty;"), Is.Empty);
        }

        [Test]
        public void A_two_word_form_is_matched_as_written()
        {
            // "succession flow" carries a space, so the boundary guard has to hold across it.
            var index = this.forms.Index(["SuccessionFlowUsage"]);

            Assert.Multiple(() =>
            {
                Assert.That(index.ElementsIn("succession flow f;"), Is.EqualTo(new[] { "SuccessionFlowUsage" }));
                Assert.That(index.ElementsIn("attribute successionflowing;"), Is.Empty);
            });
        }

        [Test]
        public void The_index_keeps_only_metaclasses_that_have_a_form()
        {
            // The mapping can never invent an element: only the names handed in are considered, and
            // only those the convention applies to are kept.
            var index = this.forms.Index(["PartUsage", "Element", "Feature"]);

            Assert.That(index.Forms.Keys, Is.EqualTo(new[] { "PartUsage" }));
        }
    }
}
