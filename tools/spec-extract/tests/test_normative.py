# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""Normative vs informative classification, including losslessness."""

from __future__ import annotations

from spec_extract.model import Block, Clause
from spec_extract.normative import classify


def _clause(body: str) -> Clause:
    return Clause(
        number="7.1",
        title="Example",
        document="KerML",
        version="1.0",
        page_start=1,
        page_end=1,
        blocks=[Block("text", body)],
    )


def test_splits_text_note_and_example_and_flags_normative() -> None:
    body = "\n".join(
        [
            "The element shall conform to the rules.",
            "It provides additional behaviour.",
            "NOTE This explanation is informative.",
            "More note text continues here.",
            "EXAMPLE 1 A short illustration.",
            "example body line.",
        ]
    )

    clause = classify(_clause(body))

    assert [block.kind for block in clause.blocks] == ["text", "note", "example"]
    assert clause.is_normative is True
    # Lossless: concatenating the blocks reproduces the input exactly.
    assert clause.text == body


def test_capitalised_note_word_is_not_a_marker() -> None:
    clause = classify(_clause("Note that this paragraph is ordinary prose."))

    assert [block.kind for block in clause.blocks] == ["text"]
    assert clause.is_normative is False
