# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""Cross-reference the knowledge trees: metamodel elements <-> spec clauses <-> grammar <-> examples.

Only clause *identifiers* are recorded, never clause text. ``knowledge/spec/`` is git-ignored because the
OMG license forbids redistributing the specification, but an edge to a clause number carries none of that
text and is safe to commit. That is what lets ``spec-citation`` still name the governing clause when the
PDFs are absent, instead of refusing to answer at all.

Every edge carries the provenance tier it belongs to and the method that produced it, so a reader can tell
a fact read out of a source from one this repository inferred.

Each function here is pure (takes parsed data, returns data); only ``write_cross_references`` touches disk.
"""

from __future__ import annotations

import json
import re
from pathlib import Path

SCHEMA_VERSION = "1.0.0"

PROVENANCE_TIERS = {
    "NORMATIVE": "Verbatim specification text, clause-anchored. Lives only in the git-ignored knowledge/spec tree.",
    "MODEL": "Read directly from the metamodel XMI by tools/metamodel-gen.",
    "DERIVED": "Computed or asserted by this repository: a closure, a name match, or a curated cross-reference.",
}

_ELEMENTS_LINE = re.compile(r"^elements:\s*\[(?P<items>[^\]]*)\]\s*$", re.MULTILINE)
_PRODUCTION = re.compile(r"^(?P<name>[A-Za-z][A-Za-z0-9_]*)\s*=", re.MULTILINE)


def clause_sort_key(clause: str) -> tuple[tuple[int, int, str], ...]:
    """Order clause numbers numerically, so 8.3.6.10 follows 8.3.6.3 rather than preceding it."""
    parts = []
    for part in clause.split("."):
        if part.isdigit():
            parts.append((0, int(part), ""))
        else:
            parts.append((1, 0, part))
    return tuple(parts)


def clause_edges(element_names: list[str], clause_titles: dict[str, dict[str, str]]) -> dict[str, list[dict]]:
    """Match element names against clause titles, exactly.

    ``clause_titles`` maps a document key (``kerml`` / ``sysml2``) to ``{clause number: title}``. An element
    may legitimately be treated in several clauses; every match is kept rather than guessing which one is
    the defining occurrence, and the caller can present them all.
    """
    wanted = set(element_names)
    edges: dict[str, list[dict]] = {name: [] for name in element_names}
    for document in sorted(clause_titles):
        for clause, title in clause_titles[document].items():
            if title in wanted:
                edges[title].append(
                    {
                        "document": document,
                        "clause": clause,
                        "provenance": "DERIVED",
                        "method": "exact-title-match",
                    }
                )
    for name in edges:
        edges[name].sort(key=lambda edge: (edge["document"], clause_sort_key(edge["clause"])))
    return edges


def grammar_edges(element_names: list[str], productions: dict[str, list[str]]) -> dict[str, list[dict]]:
    """Match element names against BNF production names, exactly.

    ``productions`` maps a grammar key (``kerml`` / ``sysml``) to its production names.
    """
    wanted = set(element_names)
    edges: dict[str, list[dict]] = {name: [] for name in element_names}
    for grammar in sorted(productions):
        for production in productions[grammar]:
            if production in wanted:
                edges[production].append(
                    {
                        "grammar": grammar,
                        "production": production,
                        "provenance": "DERIVED",
                        "method": "exact-name-match",
                    }
                )
    for name in edges:
        edges[name].sort(key=lambda edge: (edge["grammar"], edge["production"]))
    return edges


def example_edges(element_names: list[str], examples: dict[str, list[str]]) -> dict[str, list[dict]]:
    """Invert the curated ``elements:`` front matter of the worked examples into element -> example edges.

    ``examples`` maps an example path (relative to ``knowledge/``) to the element names it declares.
    """
    wanted = set(element_names)
    edges: dict[str, list[dict]] = {name: [] for name in element_names}
    for path in sorted(examples):
        for name in examples[path]:
            if name in wanted:
                edges[name].append(
                    {
                        "file": path,
                        "provenance": "DERIVED",
                        "method": "curated-front-matter",
                    }
                )
    for name in edges:
        edges[name].sort(key=lambda edge: edge["file"])
    return edges


def build_cross_references(
    elements: list[dict],
    clause_titles: dict[str, dict[str, str]],
    documents: dict[str, dict[str, str]],
    productions: dict[str, list[str]],
    examples: dict[str, list[str]],
    model_version_uri: str,
) -> dict:
    """Assemble the full cross-reference document. Pure: no disk access, no ordering surprises."""
    names = sorted(element["name"] for element in elements)
    by_name = {element["name"]: element for element in elements}

    clauses = clause_edges(names, clause_titles)
    grammar = grammar_edges(names, productions)
    worked = example_edges(names, examples)

    entries = {}
    for name in names:
        entries[name] = {
            "kind": by_name[name]["kind"],
            "element": "metamodel/" + by_name[name]["file"],
            "clauses": clauses[name],
            "grammar": grammar[name],
            "examples": worked[name],
        }

    return {
        "schemaVersion": SCHEMA_VERSION,
        "modelVersionUri": model_version_uri,
        "documents": {key: documents[key] for key in sorted(documents)},
        "provenanceTiers": PROVENANCE_TIERS,
        "counts": {
            "elements": len(names),
            "withClauses": sum(1 for name in names if clauses[name]),
            "withoutClauses": sum(1 for name in names if not clauses[name]),
            "withGrammar": sum(1 for name in names if grammar[name]),
            "withExamples": sum(1 for name in names if worked[name]),
            "clauseEdges": sum(len(clauses[name]) for name in names),
            "grammarEdges": sum(len(grammar[name]) for name in names),
            "exampleEdges": sum(len(worked[name]) for name in names),
        },
        "entries": entries,
    }


def render(document: dict) -> str:
    """Render deterministically: stable key order from the builder, two-space indent, trailing newline."""
    return json.dumps(document, indent=2, ensure_ascii=False) + "\n"


def load_elements(metamodel_index_path: str | Path) -> tuple[list[dict], str]:
    """Read ``knowledge/metamodel/index.json``; returns the element records and the model version URI."""
    index = json.loads(Path(metamodel_index_path).read_text(encoding="utf-8"))
    elements = [
        {"name": name, "kind": entry["kind"], "file": entry["file"]} for name, entry in index["entries"].items()
    ]
    return elements, index["modelVersionUri"]


def load_clause_titles(spec_index_path: str | Path) -> tuple[dict[str, str], dict[str, str]]:
    """Read a ``knowledge/spec/<doc>/index.json``; returns ``{clause: title}`` and the document metadata."""
    index = json.loads(Path(spec_index_path).read_text(encoding="utf-8"))
    titles = {clause: entry["title"] for clause, entry in index["entries"].items()}
    return titles, {"document": index["document"], "version": index["version"]}


def load_productions(bnf_path: str | Path) -> list[str]:
    """Read the production names defined in a ``.kebnf`` grammar file."""
    text = Path(bnf_path).read_text(encoding="utf-8", errors="replace")
    return [match.group("name") for match in _PRODUCTION.finditer(text)]


def load_examples(examples_dir: str | Path, prefix: str) -> dict[str, list[str]]:
    """Read the ``elements:`` front matter of every worked example under ``examples_dir``.

    ``prefix`` is prepended to each file name to make the recorded path relative to ``knowledge/``.
    """
    result: dict[str, list[str]] = {}
    for path in sorted(Path(examples_dir).glob("*.md")):
        match = _ELEMENTS_LINE.search(path.read_text(encoding="utf-8"))
        if not match:
            continue
        names = [item.strip().strip("\"'") for item in match.group("items").split(",")]
        result[prefix + path.name] = [name for name in names if name]
    return result


def write_cross_references(document: dict, path: str | Path) -> Path:
    """Write the rendered cross-reference document to ``path`` with LF endings."""
    path = Path(path)
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(render(document), encoding="utf-8", newline="\n")
    return path
