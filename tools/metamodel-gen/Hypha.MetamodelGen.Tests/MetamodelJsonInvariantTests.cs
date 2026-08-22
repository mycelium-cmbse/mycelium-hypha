// ------------------------------------------------------------------------------------------------
// <copyright file="MetamodelJsonInvariantTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.MetamodelGen.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;

    using Hypha.MetamodelGen.Generators;

    using NUnit.Framework;

    /// <summary>
    /// Validates the generated JSON, round-tripped through serialization back into the strongly-typed
    /// records (the records are the schema), and asserts structural invariants. Built fresh from the
    /// fixture model each time - nothing per-release is committed for this to read any more.
    /// </summary>
    [TestFixture]
    public class MetamodelJsonInvariantTests
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
        };

        private static MetamodelDocument ReadDocument()
        {
            var model = TestModel.Model;
            Assert.That(model, Is.Not.Null, "No SysML model found in the fixture.");

            var document = MetamodelJsonGenerator.BuildDocument(model!, JsonSidecarTestSupport.ComputeSourceHash());
            var json = MetamodelJsonGenerator.Serialize(document);

            return JsonSerializer.Deserialize<MetamodelDocument>(json, Options)!;
        }

        private static MetamodelLookup ReadIndex()
        {
            var model = TestModel.Model;
            Assert.That(model, Is.Not.Null, "No SysML model found in the fixture.");

            var document = MetamodelJsonGenerator.BuildDocument(model!, JsonSidecarTestSupport.ComputeSourceHash());
            var index = MetamodelJsonGenerator.BuildIndex(document);
            var json = MetamodelJsonGenerator.Serialize(index);

            return JsonSerializer.Deserialize<MetamodelLookup>(json, Options)!;
        }

        [Test]
        public void Type_and_inheritance_references_resolve_to_a_class()
        {
            var document = ReadDocument();
            var classNames = document.Classes.Select(@class => @class.Name).ToHashSet();

            foreach (var @class in document.Classes)
            {
                foreach (var name in @class.Extends.Concat(@class.AllAncestors)
                             .Concat(@class.DirectSubclasses).Concat(@class.AllDescendants))
                {
                    Assert.That(classNames, Does.Contain(name), $"{@class.Name} references unknown class {name}");
                }

                foreach (var inherited in @class.InheritedAttributes)
                {
                    Assert.That(classNames, Does.Contain(inherited.InheritedFrom));
                }
            }
        }

        [Test]
        public void Multiplicity_bounds_are_sane()
        {
            var document = ReadDocument();

            foreach (var @class in document.Classes)
            {
                foreach (var (lower, upper) in @class.OwnedAttributes.Select(a => (a.Lower, a.Upper))
                             .Concat(@class.InheritedAttributes.Select(a => (a.Lower, a.Upper)))
                             .Concat(@class.OwnedOperations.Select(o => (o.ReturnLower, o.ReturnUpper))))
                {
                    Assert.Multiple(() =>
                    {
                        Assert.That(lower, Is.GreaterThanOrEqualTo(0));
                        Assert.That(upper, Is.GreaterThanOrEqualTo(-1));
                    });
                }
            }
        }

        [Test]
        public void Counts_match_array_lengths()
        {
            var document = ReadDocument();
            Assert.Multiple(() =>
            {
                Assert.That(document.Counts.Classes, Is.EqualTo(document.Classes.Count));
                Assert.That(document.Counts.Enumerations, Is.EqualTo(document.Enumerations.Count));
                Assert.That(document.Counts.PrimitiveTypes, Is.EqualTo(document.PrimitiveTypes.Count));
                Assert.That(document.Counts.Packages, Is.EqualTo(document.Packages.Count));
            });
        }

        [Test]
        public void Index_covers_exactly_classes_enumerations_and_primitive_types()
        {
            var document = ReadDocument();
            var index = ReadIndex();

            var expected = document.Classes.Select(c => c.Name)
                .Concat(document.Enumerations.Select(e => e.Name))
                .Concat(document.PrimitiveTypes.Select(p => p.Name))
                .ToHashSet();

            Assert.That(index.Entries, Has.Count.EqualTo(expected.Count), "index entry count must equal classes + enums + primitives (no name collisions)");

            foreach (var name in expected)
            {
                Assert.That(index.Entries.ContainsKey(name), Is.True, $"index is missing an entry for {name}");
            }
        }

    }
}
