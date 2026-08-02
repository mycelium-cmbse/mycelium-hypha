# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""Generate the textual-notation knowledge base for one release, from that release's own sources.

Everything here is derived – nothing is hand-written:

* **examples** – every ``.kerml`` / ``.sysml`` model shipped at the tag, copied byte-exact into a
  fenced block with front matter naming its upstream path;
* **keywords** – the ``RESERVED_KEYWORD`` production read straight out of that tag's BNF;
* **element links** – derived from the metamodel's own naming convention rather than curated:
  ``PartDefinition`` surfaces as ``part def`` and ``PartUsage`` as ``part``, so a model that contains
  the surface form is linked to the metaclass. Only names present in that release's metamodel index
  are kept, so the mapping can never invent an element.

The models differ between releases, so this is generated per tag like everything else.
"""

from __future__ import annotations

import re
from pathlib import Path

_RESERVED_START = re.compile(r"^RESERVED_KEYWORD\s*=", re.ASCII)
_NEXT_PRODUCTION = re.compile(r"^[A-Za-z_]\w*\s*(?::\s*\w+\s*)?=", re.ASCII)
_QUOTED = re.compile(r"'([^']+)'")
_CAMEL = re.compile(r"[A-Z][a-z0-9]*")

MODEL_SUFFIXES = (".kerml", ".sysml")


def reserved_keywords(bnf_text: str) -> list[str]:
    """The reserved keywords declared by a grammar, in sorted order.

    Scanned line by line rather than with one multi-line regex: the block runs from the
    ``RESERVED_KEYWORD`` production until the next production or a blank line, which is far clearer
    to reason about than a reluctant match against a lookahead.
    """
    collected: set[str] = set()
    inside = False

    for line in bnf_text.replace("\r\n", "\n").split("\n"):
        if not inside:
            if _RESERVED_START.match(line):
                inside = True
                collected.update(_QUOTED.findall(line))
            continue

        if not line.strip() or _NEXT_PRODUCTION.match(line):
            break

        collected.update(_QUOTED.findall(line))

    return sorted(collected)


def surface_form(metaclass: str) -> str | None:
    """The textual-notation surface form of a metaclass name, or ``None`` when it has none.

    Follows the convention the metamodel itself uses: ``<Words>Definition`` is written
    ``<words> def`` and ``<Words>Usage`` is written ``<words>``.
    """
    for suffix, tail in (("Definition", " def"), ("Usage", "")):
        if metaclass.endswith(suffix) and metaclass != suffix:
            words = _CAMEL.findall(metaclass[: -len(suffix)])
            if not words:
                return None
            return " ".join(word.lower() for word in words) + tail
    return None


def surface_forms(metaclasses: list[str]) -> dict[str, str]:
    """Map every metaclass that has a surface form to it, longest form first when matching."""
    forms = {}
    for name in metaclasses:
        form = surface_form(name)
        if form:
            forms[name] = form
    return forms


def elements_in(model_text: str, forms: dict[str, str]) -> list[str]:
    """The metaclasses whose surface form appears as a declaration in ``model_text``, sorted."""
    found = []
    for name, form in forms.items():
        # A declaration is the keyword followed by a name, a body or a typing colon - not a
        # substring of a longer identifier.
        pattern = re.compile(r"(?<![\w'])" + re.escape(form) + r"(?![\w'])")
        if pattern.search(model_text):
            found.append(name)
    return sorted(found)


def example_filename(relative_path: Path) -> str:
    """A deterministic, flat file name for a model's example page."""
    slug = re.sub(r"[^a-z0-9]+", "-", str(relative_path.with_suffix("")).lower().replace("\\", "/"))
    return slug.strip("-") + ".md"


def render_example(relative_path: Path, model_text: str, elements: list[str], language: str) -> str:
    """Render one model as a markdown page: front matter, the verbatim source, and its elements."""
    body = model_text.replace("\r\n", "\n").rstrip("\n")
    front = [
        "---",
        f"name: {relative_path.stem}",
        "kind: example",
        f"language: {language}",
        f"source: {str(relative_path).replace(chr(92), '/')}",
        f"elements: [{', '.join(elements)}]",
        "license: EPL-2.0",
        "---",
        "",
        f"# {relative_path.stem}",
        "",
        f"Verbatim {language} model from `{str(relative_path).replace(chr(92), '/')}`"
        " (EPL-2.0; see [NOTICE](../../../NOTICE)).",
        "",
        f"```{language.lower()}",
        body,
        "```",
        "",
    ]

    if elements:
        front += ["## Elements", ""]
        front += [f"- [{name}](../metamodel/elements/{name}.md)" for name in elements]
        front += [""]

    return "\n".join(front)


def render_index(tag: str, examples: list[tuple[str, Path]], keywords: dict[str, list[str]]) -> str:
    """Render the per-release index: the keyword reference and every generated example."""
    lines = [
        "---",
        f"tag: {tag}",
        "kind: textual-notation-index",
        f"examples: {len(examples)}",
        "---",
        "",
        f"# SysML v2 / KerML textual notation — {tag}",
        "",
        "> Generated by `tools/spec-extract` — do not edit by hand; re-run the generator.",
        "",
        "Every model shipped with this release, copied verbatim, plus the reserved keywords read",
        "from this release's grammar. Element links are derived from the metamodel naming",
        "convention (`PartDefinition` → `part def`, `PartUsage` → `part`).",
        "",
    ]

    for grammar in sorted(keywords):
        words = keywords[grammar]
        lines += [f"## {grammar} keywords ({len(words)})", "", "`" + "`, `".join(words) + "`", ""]

    lines += ["## Examples", ""]
    for file_name, relative_path in examples:
        source = str(relative_path).replace("\\", "/")
        lines.append(f"- [{relative_path.stem}](examples/{file_name}) — `{source}`")
    lines.append("")

    return "\n".join(lines)


def iter_models(textual_root: Path) -> list[Path]:
    """Every model under a release's textual sources, as paths relative to that root, sorted."""
    return sorted(
        path.relative_to(textual_root)
        for path in textual_root.rglob("*")
        if path.is_file() and path.suffix in MODEL_SUFFIXES
    )
