# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""Unit tests for the BNF parser, on grammar text in the shape the .kebnf files really use."""

from __future__ import annotations

from spec_extract.grammar import metaclass_links, parse, render

GRAMMAR = """
// Clause 8.2.2.4 Annotations

TextualRepresentation =
    ( 'rep' Identification )?
    'language' language = STRING_VALUE body = REGULAR_COMMENT

// Clause 8.2.2.5.1 Packages

Package =
    ( ownedRelationship += PrefixMetadataMember )*
    PackageDeclaration PackageBody

PackageDeclaration : Package =
    'package' Identification

LibraryPackage =
    ( isStandard ?= 'standard' ) 'library'
    PackageDeclaration
"""


def test_parses_every_production() -> None:
    names = [production.name for production in parse(GRAMMAR)]

    assert names == ["TextualRepresentation", "Package", "PackageDeclaration", "LibraryPackage"]


def test_captures_the_declared_metaclass() -> None:
    by_name = {production.name: production for production in parse(GRAMMAR)}

    assert by_name["PackageDeclaration"].produces == "Package"
    assert by_name["Package"].produces is None, "an untyped production declares no metaclass"


def test_attributes_each_production_to_its_clause() -> None:
    by_name = {production.name: production for production in parse(GRAMMAR)}

    assert by_name["TextualRepresentation"].clause == "8.2.2.4"
    assert by_name["TextualRepresentation"].clause_title == "Annotations"
    assert by_name["PackageDeclaration"].clause == "8.2.2.5.1"


def test_extracts_the_features_a_production_populates() -> None:
    by_name = {production.name: production for production in parse(GRAMMAR)}

    assert by_name["TextualRepresentation"].features == (("body", "="), ("language", "="))
    assert by_name["Package"].features == (("ownedRelationship", "+="),)
    assert by_name["LibraryPackage"].features == (("isStandard", "?="),), "?= is a boolean flag"


def test_body_keeps_the_production_verbatim() -> None:
    by_name = {production.name: production for production in parse(GRAMMAR)}

    assert by_name["PackageDeclaration"].body == "PackageDeclaration : Package =\n    'package' Identification"


def test_metaclass_links_prefer_the_declared_type_over_the_name() -> None:
    links = metaclass_links(parse(GRAMMAR), {"Package", "TextualRepresentation"})

    # PackageDeclaration is typed ": Package", so it links to Package, not to a metaclass of its own name.
    assert links["Package"] == ["Package", "PackageDeclaration"]
    assert links["TextualRepresentation"] == ["TextualRepresentation"]


def test_feature_links_follow_untyped_helper_productions() -> None:
    from spec_extract.grammar import feature_links

    grammar = """
// Clause 1 Parts

PartUsage =
    PartUsageDeclaration

PartUsageDeclaration =
    'part' declaredName = NAME

OtherUsage : Feature =
    isReadOnly ?= 'readonly'
"""
    links = feature_links(parse(grammar), {"PartUsage", "Feature"})

    # The assignment lives in the untyped helper, but belongs to the element the caller builds.
    assert [(entry["feature"], entry["operator"]) for entry in links["PartUsage"]] == [("declaredName", "=")]
    assert links["PartUsage"][0]["productions"] == ["PartUsageDeclaration"]


def test_feature_links_follow_helpers_transitively() -> None:
    from spec_extract.grammar import feature_links

    grammar = """
// Clause 1 Parts

PartUsage =
    PartUsageDeclaration

PartUsageDeclaration =
    'part' PartUsageName

PartUsageName =
    declaredName = NAME
"""
    links = feature_links(parse(grammar), {"PartUsage"})

    # Two hops: PartUsage -> PartUsageDeclaration -> PartUsageName.
    assert [entry["feature"] for entry in links["PartUsage"]] == ["declaredName"]
    assert links["PartUsage"][0]["productions"] == ["PartUsageName"]


def test_feature_links_do_not_follow_a_helper_shared_by_two_elements() -> None:
    from spec_extract.grammar import feature_links

    grammar = """
// Clause 1 Parts

PartUsage =
    SharedDeclaration

ItemUsage =
    SharedDeclaration

SharedDeclaration =
    declaredName = NAME
"""
    links = feature_links(parse(grammar), {"PartUsage", "ItemUsage"})

    # The helper is reachable from both, so attributing it to either would be a guess.
    assert links == {}


def test_feature_links_survive_a_self_referencing_grammar() -> None:
    from spec_extract.grammar import feature_links

    grammar = """
// Clause 1 Parts

PartUsage =
    Nested

Nested =
    Nested | declaredName = NAME
"""
    links = feature_links(parse(grammar), {"PartUsage"})

    assert [entry["feature"] for entry in links["PartUsage"]] == ["declaredName"]


def test_feature_links_do_not_follow_a_typed_production() -> None:
    from spec_extract.grammar import feature_links

    grammar = """
// Clause 1 Parts

PartUsage =
    OtherUsage

OtherUsage : Feature =
    isReadOnly ?= 'readonly'
"""
    links = feature_links(parse(grammar), {"PartUsage", "Feature"})

    # OtherUsage declares its own metaclass, so its assignment must not be attributed to PartUsage.
    assert "PartUsage" not in links
    assert links["Feature"][0]["feature"] == "isReadOnly"


def test_clause_links_use_the_grammars_own_attribution() -> None:
    from spec_extract.grammar import clause_links

    links = clause_links(parse(GRAMMAR), {"Package", "TextualRepresentation"})

    assert links["TextualRepresentation"] == ["8.2.2.4"]
    assert links["Package"] == ["8.2.2.5.1"]


def test_metaclass_links_never_invent_an_element() -> None:
    links = metaclass_links(parse(GRAMMAR), {"Package"})

    assert set(links) == {"Package"}, "only metaclasses the metamodel actually has may appear"


def test_render_groups_by_clause_and_links_the_element() -> None:
    text = render("SysML", "2026-05", parse(GRAMMAR))

    assert "## 8.2.2.5.1 Packages" in text
    assert "produces [Package](../metamodel/elements/Package.md)" in text
    assert "clause `8.2.2.5.1`" in text
    assert "```kebnf" in text
    assert "productions: 4" in text


def test_parse_tolerates_a_grammar_with_no_clause_comments() -> None:
    productions = parse("Foo =\n    'foo'\n")

    assert productions[0].clause is None
    assert render("KerML", "2026-05", productions).count("## Lexical") == 1
