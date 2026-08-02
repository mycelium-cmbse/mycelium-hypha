# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""Unit tests for the cross-reference layer, on synthetic data (no PDFs, no knowledge base required)."""

from __future__ import annotations

import json
from pathlib import Path

from spec_extract.crossrefs import (
    build_cross_references,
    clause_edges,
    clause_sort_key,
    example_edges,
    grammar_edges,
    load_examples,
    merge_clause_edges,
    render,
)

ELEMENTS = [
    {"name": "PartUsage", "kind": "class", "file": "elements/PartUsage.md"},
    {"name": "Feature", "kind": "class", "file": "elements/Feature.md"},
    {"name": "VisibilityKind", "kind": "enumeration", "file": "elements/VisibilityKind.md"},
]

CLAUSE_TITLES = {
    "kerml": {"7.4.2": "Feature", "8.3.3.1.8": "Feature", "2": "Conformance"},
    "sysml2": {"8.3.6.3": "PartUsage", "1": "Scope"},
}

DOCUMENTS = {"kerml": {"document": "KerML", "version": "1.0"}, "sysml2": {"document": "SysML", "version": "2.0"}}


def test_clause_sort_key_orders_numerically_not_lexically() -> None:
    ordered = sorted(["8.3.6.10", "8.3.6.3", "8.3.6.2"], key=clause_sort_key)
    assert ordered == ["8.3.6.2", "8.3.6.3", "8.3.6.10"]


def test_clause_sort_key_tolerates_non_numeric_parts() -> None:
    # Must not raise: annex-style identifiers sort after numeric ones rather than blowing up.
    assert sorted(["A.1", "8.1"], key=clause_sort_key) == ["8.1", "A.1"]


def test_clause_edges_matches_titles_exactly_and_keeps_every_match() -> None:
    edges = clause_edges(["PartUsage", "Feature", "VisibilityKind"], CLAUSE_TITLES)

    assert [edge["clause"] for edge in edges["Feature"]] == ["7.4.2", "8.3.3.1.8"]
    assert [edge["document"] for edge in edges["PartUsage"]] == ["sysml2"]
    assert edges["VisibilityKind"] == []


def test_clause_edges_are_tagged_with_provenance_and_method() -> None:
    edge = clause_edges(["PartUsage"], CLAUSE_TITLES)["PartUsage"][0]

    assert edge["provenance"] == "DERIVED"
    assert edge["method"] == "exact-title-match"


def test_clause_edges_sort_documents_before_clause_numbers() -> None:
    titles = {"sysml2": {"1.1": "Feature"}, "kerml": {"9.9": "Feature"}}

    edges = clause_edges(["Feature"], titles)["Feature"]

    assert [(edge["document"], edge["clause"]) for edge in edges] == [("kerml", "9.9"), ("sysml2", "1.1")]


def test_grammar_edges_use_the_declared_production() -> None:
    links = {
        "sysml": {"PartUsage": ["PartUsage", "PartUsageDeclaration"]},
        "kerml": {"Feature": ["Feature"]},
    }

    edges = grammar_edges(["PartUsage", "Feature", "VisibilityKind"], links)

    assert [edge["production"] for edge in edges["PartUsage"]] == ["PartUsage", "PartUsageDeclaration"]
    assert edges["Feature"][0]["grammar"] == "kerml"
    assert edges["VisibilityKind"] == []


def test_grammar_edges_distinguish_a_declared_production_from_a_name_match() -> None:
    links = {"sysml": {"PartUsage": ["PartUsage", "PartUsageDeclaration"]}}

    methods = {edge["production"]: edge["method"] for edge in grammar_edges(["PartUsage"], links)["PartUsage"]}

    assert methods["PartUsage"] == "production-name"
    assert methods["PartUsageDeclaration"] == "declared-production"


def test_grammar_edges_are_model_provenance() -> None:
    # The grammar states what a production builds; this is read, not inferred.
    edges = grammar_edges(["Feature"], {"kerml": {"Feature": ["Feature"]}})

    assert edges["Feature"][0]["provenance"] == "MODEL"


def test_a_grammar_stated_clause_upgrades_a_title_match() -> None:
    # The same clause found both ways must not stay labelled as inferred.
    title_matched = {"Feature": [{"document": "kerml", "clause": "7.4.2", "provenance": "DERIVED",
                                 "method": "exact-title-match"}]}

    merged = merge_clause_edges(title_matched, {"kerml": {"Feature": ["7.4.2"]}}, {"kerml": "kerml"})

    assert len(merged["Feature"]) == 1, "the same clause must not be listed twice"
    assert merged["Feature"][0]["provenance"] == "MODEL"
    assert merged["Feature"][0]["method"] == "grammar-clause"


def test_a_grammar_stated_clause_is_added_when_the_title_never_matched() -> None:
    merged = merge_clause_edges(
        {"Feature": []}, {"kerml": {"Feature": ["8.3.1"]}}, {"kerml": "kerml"}
    )

    assert [edge["clause"] for edge in merged["Feature"]] == ["8.3.1"]


def test_merged_clause_edges_stay_numerically_ordered() -> None:
    merged = merge_clause_edges(
        {"Feature": []}, {"kerml": {"Feature": ["8.3.10", "8.3.2"]}}, {"kerml": "kerml"}
    )

    assert [edge["clause"] for edge in merged["Feature"]] == ["8.3.2", "8.3.10"]


def test_example_edges_invert_the_curated_front_matter() -> None:
    examples = {
        "textual-notation/examples/part-definitions.md": ["PartUsage", "Feature"],
        "textual-notation/examples/generalization.md": ["Feature"],
    }

    edges = example_edges(["PartUsage", "Feature", "VisibilityKind"], examples)

    assert [edge["file"] for edge in edges["Feature"]] == [
        "textual-notation/examples/generalization.md",
        "textual-notation/examples/part-definitions.md",
    ]
    assert edges["VisibilityKind"] == []


def test_build_counts_coverage_and_shapes_entries() -> None:
    document = build_cross_references(
        ELEMENTS,
        CLAUSE_TITLES,
        DOCUMENTS,
        {"sysml": {"PartUsage": ["PartUsage"]}},
        {"textual-notation/examples/part-definitions.md": ["PartUsage"]},
        "https://www.omg.org/spec/SysML/20250201",
    )

    assert document["counts"] == {
        "elements": 3,
        "withClauses": 2,
        "withoutClauses": 1,
        "withGrammar": 1,
        "withFeatures": 0,
        "withExamples": 1,
        "clauseEdges": 3,
        "statedClauseEdges": 0,
        "grammarEdges": 1,
        "featureEdges": 0,
        "exampleEdges": 1,
    }
    assert document["entries"]["PartUsage"]["element"] == "metamodel/elements/PartUsage.md"
    assert document["entries"]["VisibilityKind"]["kind"] == "enumeration"


def test_build_orders_entries_by_name() -> None:
    document = build_cross_references(ELEMENTS, CLAUSE_TITLES, DOCUMENTS, {}, {}, "uri")

    assert list(document["entries"]) == ["Feature", "PartUsage", "VisibilityKind"]


def test_build_is_deterministic_for_reordered_input() -> None:
    first = build_cross_references(ELEMENTS, CLAUSE_TITLES, DOCUMENTS, {}, {}, "uri")
    second = build_cross_references(list(reversed(ELEMENTS)), CLAUSE_TITLES, DOCUMENTS, {}, {}, "uri")

    assert render(first) == render(second)


def test_render_emits_no_clause_titles() -> None:
    # The licensing guarantee: an edge records a clause *number*, never the OMG wording it points at.
    titles = {"sysml2": {"8.3.6.3": "PartUsage", "4": "Terms and Definitions"}}

    text = render(build_cross_references(ELEMENTS, titles, DOCUMENTS, {}, {}, "uri"))

    assert "Terms and Definitions" not in text
    assert '"title"' not in text


def test_render_ends_with_a_single_newline() -> None:
    text = render(build_cross_references(ELEMENTS, CLAUSE_TITLES, DOCUMENTS, {}, {}, "uri"))

    assert text.endswith("}\n")
    assert json.loads(text)["schemaVersion"]


def test_built_document_matches_the_committed_schema_shape(repo_root: Path) -> None:
    """Keep the hand-written schema honest without pulling in a JSON-Schema validator.

    Checks the shape the builder actually produces against the ``required`` lists the schema declares,
    so adding a field to one and not the other fails here.
    """
    schema = json.loads((repo_root / "knowledge" / "cross-references.schema.json").read_text(encoding="utf-8"))
    document = build_cross_references(
        ELEMENTS,
        CLAUSE_TITLES,
        DOCUMENTS,
        {"sysml": {"PartUsage": ["PartUsage"]}},
        {"textual-notation/examples/part-definitions.md": ["PartUsage"]},
        "uri",
    )

    assert sorted(document) == sorted(schema["required"])
    assert sorted(document["counts"]) == sorted(schema["properties"]["counts"]["required"])

    entry = document["entries"]["PartUsage"]
    assert sorted(entry) == sorted(schema["$defs"]["entry"]["required"])
    assert sorted(entry["clauses"][0]) == sorted(schema["$defs"]["clauseEdge"]["required"])
    assert sorted(entry["grammar"][0]) == sorted(schema["$defs"]["grammarEdge"]["required"])
    assert sorted(entry["examples"][0]) == sorted(schema["$defs"]["exampleEdge"]["required"])

    tiers = set(schema["$defs"]["provenance"]["enum"])
    assert set(document["provenanceTiers"]) == tiers
    assert entry["clauses"][0]["provenance"] in tiers


def test_load_examples_reads_the_elements_front_matter(tmp_path: Path) -> None:
    (tmp_path / "parts.md").write_text(
        '---\nname: Parts\nelements: [PartUsage, "PartDefinition"]\n---\n\n# Parts\n', encoding="utf-8"
    )
    (tmp_path / "no-elements.md").write_text("---\nname: Nothing\n---\n", encoding="utf-8")

    loaded = load_examples(tmp_path, "textual-notation/examples/")

    assert loaded == {"textual-notation/examples/parts.md": ["PartUsage", "PartDefinition"]}
