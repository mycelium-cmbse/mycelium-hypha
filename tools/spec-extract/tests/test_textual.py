# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""Unit tests for the generated textual-notation layer (no network, no real sources)."""

from __future__ import annotations

from pathlib import Path

import pytest

from spec_extract.textual import (
    elements_in,
    example_filename,
    iter_models,
    render_example,
    render_index,
    reserved_keywords,
    surface_form,
    surface_forms,
)

BNF = """
// Clause 8.2.2.1.2 Lexical Structure

RESERVED_KEYWORD =
    'about' | 'abstract' | 'accept' | 'part' | 'def'
    | 'attribute' | 'use' | 'case'

DEFINED_BY  = ':'   | 'defined' 'by'
"""


@pytest.mark.parametrize(
    ("metaclass", "expected"),
    [
        ("PartDefinition", "part def"),
        ("PartUsage", "part"),
        ("AttributeDefinition", "attribute def"),
        ("UseCaseDefinition", "use case def"),
        ("ConnectionUsage", "connection"),
        ("SuccessionFlowUsage", "succession flow"),
    ],
)
def test_surface_form_follows_the_metamodel_naming_convention(metaclass: str, expected: str) -> None:
    assert surface_form(metaclass) == expected


@pytest.mark.parametrize("metaclass", ["Element", "Feature", "Type", "Definition", "Usage"])
def test_metaclasses_without_a_surface_form_are_skipped(metaclass: str) -> None:
    assert surface_form(metaclass) is None


def test_reserved_keywords_are_read_from_the_grammar() -> None:
    assert reserved_keywords(BNF) == ["about", "abstract", "accept", "attribute", "case", "def", "part", "use"]


def test_reserved_keywords_tolerate_a_grammar_without_the_production() -> None:
    assert reserved_keywords("SOMETHING_ELSE = 'x'") == []


def test_elements_in_matches_declarations() -> None:
    forms = surface_forms(["PartDefinition", "PartUsage", "AttributeUsage", "ActionDefinition"])

    found = elements_in("part def Vehicle {\n\tattribute mass : Real;\n}", forms)

    assert found == ["AttributeUsage", "PartDefinition", "PartUsage"]


def test_elements_in_does_not_match_inside_an_identifier() -> None:
    forms = surface_forms(["PartUsage"])

    assert elements_in("attribute counterpart : String;", forms) == []


def test_elements_in_does_not_match_inside_a_quoted_name() -> None:
    forms = surface_forms(["PartUsage"])

    assert elements_in("attribute 'part' : String;", forms) == []


def test_elements_in_returns_nothing_for_an_unrelated_model() -> None:
    forms = surface_forms(["PartDefinition", "ActionDefinition"])

    assert elements_in("package Empty;", forms) == []


def test_example_filename_is_deterministic_and_flat() -> None:
    name = example_filename(Path("sysml/training/02. Part Definitions/Part Definition Example.sysml"))

    assert name == "sysml-training-02-part-definitions-part-definition-example.md"
    assert "/" not in name and "\\" not in name


def test_render_example_embeds_the_source_verbatim() -> None:
    source = "part def Vehicle {\r\n\tattribute mass : Real;\r\n}\r\n"

    page = render_example(Path("sysml/x.sysml"), source, ["PartDefinition"], "SysML")

    assert "```sysml\npart def Vehicle {\n\tattribute mass : Real;\n}\n```" in page
    assert "source: sysml/x.sysml" in page
    assert "elements: [PartDefinition]" in page
    assert "- [PartDefinition](../metamodel/elements/PartDefinition.md)" in page


def test_render_example_omits_the_elements_section_when_there_are_none() -> None:
    page = render_example(Path("kerml/x.kerml"), "package Empty;", [], "KerML")

    assert "## Elements" not in page
    assert "elements: []" in page


def test_render_index_lists_keywords_and_examples() -> None:
    index = render_index(
        "2026-05",
        [("a.md", Path("sysml/a.sysml"))],
        {"SysML": ["part", "def"], "KerML": ["feature"]},
    )

    assert "textual notation — 2026-05" in index
    assert "## KerML keywords (1)" in index
    assert "## SysML keywords (2)" in index
    assert "- [a](examples/a.md) — `sysml/a.sysml`" in index


def test_iter_models_finds_models_recursively_and_sorted(tmp_path: Path) -> None:
    (tmp_path / "sysml" / "deep").mkdir(parents=True)
    (tmp_path / "sysml" / "b.sysml").write_text("x", encoding="utf-8")
    (tmp_path / "sysml" / "deep" / "a.kerml").write_text("x", encoding="utf-8")
    (tmp_path / "sysml" / "ignored.html").write_text("x", encoding="utf-8")

    found = [str(path).replace("\\", "/") for path in iter_models(tmp_path)]

    assert found == ["sysml/b.sysml", "sysml/deep/a.kerml"]
