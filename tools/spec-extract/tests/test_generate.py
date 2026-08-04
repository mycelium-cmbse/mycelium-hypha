# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""Regenerate the knowledge base for every installed release.

This mirrors how the C# metamodel-gen tests write the knowledge base: running pytest with the OMG
PDFs present under ``sources/<tag>/specs/`` populates ``knowledge/<tag>/spec/{kerml,sysml2}/`` and
the committed ``knowledge/<tag>/cross-references.json``; without them it skips.

The clause text is git-ignored (OMG terms). The cross-references *are* committed: they record clause
identifiers only, never clause text.
"""

from __future__ import annotations

import json
from pathlib import Path

import pytest

from conftest import require_pdfs
from spec_extract.crossrefs import (
    build_cross_references,
    load_clause_titles,
    load_elements,
    load_examples,
    render,
    write_cross_references,
)
from spec_extract.grammar import clause_links, feature_links, metaclass_links, parse_graphical, render_graphical
from spec_extract.grammar import parse as parse_grammar
from spec_extract.grammar import render as render_grammar
from spec_extract.pipeline import DocMeta, extract_document, write_clauses, write_index, write_index_json
from spec_extract.textual import (
    elements_in,
    example_filename,
    iter_models,
    render_example,
    render_index,
    reserved_keywords,
    surface_forms,
)
from spec_extract.upstream import RELEASE_REPO


def test_regenerates_spec_knowledge_base(repo_root: Path, installed_tags: list[str]) -> None:
    for tag in installed_tags:
        kerml_pdf, sysml_pdf = require_pdfs(repo_root, tag)
        targets = [
            (kerml_pdf, DocMeta("KerML", "1.0", "kerml")),
            (sysml_pdf, DocMeta("SysML", "2.0", "sysml2")),
        ]

        for pdf_path, meta in targets:
            clauses = extract_document(pdf_path, meta)
            out_dir = repo_root / "knowledge" / tag / "spec" / meta.out_subdir
            paths = write_clauses(clauses, out_dir)
            index_path = write_index(clauses, out_dir)
            index_json_path = write_index_json(clauses, out_dir)

            assert paths, f"no clauses extracted from {pdf_path.name} at {tag}"
            assert all(path.read_text(encoding="utf-8").startswith("---\n") for path in paths)
            assert index_path.read_text(encoding="utf-8").startswith("---\n")

            catalog = json.loads(index_json_path.read_text(encoding="utf-8"))
            assert catalog["clauses"] == len(clauses)
            assert len(catalog["entries"]) == len(clauses)


def test_regenerates_textual_notation(repo_root: Path, installed_tags: list[str]) -> None:
    """Write ``knowledge/<tag>/textual-notation/`` from that release's own models and grammar.

    Nothing here is hand-written: every model shipped at the tag becomes an example page, the
    keywords come from that tag's BNF, and the element links are derived from the metamodel naming
    convention.
    """
    for tag in installed_tags:
        textual_root = repo_root / "sources" / tag / "textual"
        if not textual_root.is_dir():
            pytest.skip(f"no textual sources fetched for {tag}")

        index = json.loads((repo_root / "knowledge" / tag / "metamodel" / "index.json").read_text("utf-8"))
        forms = surface_forms(list(index["entries"]))

        out_dir = repo_root / "knowledge" / tag / "textual-notation"
        examples_dir = out_dir / "examples"
        examples_dir.mkdir(parents=True, exist_ok=True)
        for stale in examples_dir.glob("*.md"):
            stale.unlink()

        written: list[tuple[str, Path]] = []
        for relative in iter_models(textual_root):
            text = (textual_root / relative).read_text(encoding="utf-8", errors="replace")
            language = "KerML" if relative.suffix == ".kerml" else "SysML"
            page = render_example(relative, text, elements_in(text, forms), language)

            file_name = example_filename(relative)
            (examples_dir / file_name).write_text(page, encoding="utf-8", newline="\n")
            written.append((file_name, relative))

        keywords = {}
        for grammar in ("KerML", "SysML"):
            text = (textual_root / "bnf" / f"{grammar}-textual-bnf.kebnf").read_text("utf-8")
            keywords[grammar] = reserved_keywords(text)

            # The grammar reference: every production with the metaclass it builds, the clause it is
            # defined in and the metamodel features it populates - all stated by the grammar itself.
            productions = parse_grammar(text)
            (out_dir / f"grammar-{grammar.lower()}.md").write_text(
                render_grammar(grammar, tag, productions), encoding="utf-8", newline="\n"
            )
            assert productions, f"no productions parsed from the {grammar} grammar"
            assert all(production.clause for production in productions), (
                f"every {grammar} production should be attributed to a clause"
            )

        # The graphical notation is mostly pictures; the images are linked at this tag, not copied.
        graphical = parse_graphical(
            (textual_root / "bnf" / "SysML-graphical-bnf.kgbnf").read_text("utf-8")
        )
        (out_dir / "grammar-graphical.md").write_text(
            render_graphical(tag, RELEASE_REPO, graphical), encoding="utf-8", newline="\n"
        )
        assert graphical, "no productions parsed from the graphical grammar"
        assert any(production.images for production in graphical), "no notation images referenced"

        (out_dir / "index.md").write_text(
            render_index(tag, written, keywords), encoding="utf-8", newline="\n"
        )

        assert len(written) > 100, f"expected every model at {tag} to be written"
        assert len({name for name, _ in written}) == len(written), "example file names must be unique"
        assert keywords["SysML"], "no reserved keywords read from the SysML grammar"


def test_regenerates_cross_references(repo_root: Path, installed_tags: list[str]) -> None:
    """Write the committed ``knowledge/<tag>/cross-references.json`` for each installed release.

    Needs that release's clause catalogs to exist, so it skips when they have not been generated.
    """
    for tag in installed_tags:
        knowledge = repo_root / "knowledge" / tag
        spec_indexes = {doc: knowledge / "spec" / doc / "index.json" for doc in ("kerml", "sysml2")}
        missing = [str(path) for path in spec_indexes.values() if not path.is_file()]
        if missing:
            pytest.skip("spec clause catalog not generated (git-ignored): " + ", ".join(missing))

        elements, model_version_uri = load_elements(knowledge / "metamodel" / "index.json")

        clause_titles: dict[str, dict[str, str]] = {}
        documents: dict[str, dict[str, str]] = {}
        for doc, path in spec_indexes.items():
            clause_titles[doc], documents[doc] = load_clause_titles(path)

        # Grammar and examples are both per release now. The grammar link uses the metaclass each
        # production *declares* it builds, which covers far more elements than matching names.
        bnf_dir = repo_root / "sources" / tag / "textual" / "bnf"
        element_names = {element["name"] for element in elements}
        parsed = {
            key: parse_grammar((bnf_dir / f"{grammar}-textual-bnf.kebnf").read_text("utf-8"))
            for key, grammar in (("kerml", "KerML"), ("sysml", "SysML"))
        }
        productions = {key: metaclass_links(value, element_names) for key, value in parsed.items()}

        # The grammar states which clause each production belongs to and which metamodel features it
        # populates; both are read rather than inferred.
        grammar_clauses = {key: clause_links(value, element_names) for key, value in parsed.items()}
        grammar_features = {key: feature_links(value, element_names) for key, value in parsed.items()}
        grammar_documents = {"kerml": "kerml", "sysml": "sysml2"}
        examples = load_examples(
            knowledge / "textual-notation" / "examples", "textual-notation/examples/"
        )

        document = build_cross_references(
            elements,
            clause_titles,
            documents,
            productions,
            examples,
            model_version_uri,
            grammar_clauses=grammar_clauses,
            grammar_documents=grammar_documents,
            grammar_features=grammar_features,
        )
        path = write_cross_references(document, knowledge / "cross-references.json")

        written = path.read_text(encoding="utf-8")
        assert written == render(document), "written file must match the rendered document"
        assert document["counts"]["elements"] == len(elements)
        assert document["counts"]["withClauses"] > 0, f"no element matched a clause title at {tag}"
        assert document["counts"]["withExamples"] > 0, f"no element matched a worked example at {tag}"
        assert '"title"' not in written, "cross-references must never carry OMG clause titles"
