# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""Command-line entry point: extract specification text into markdown.

Two modes, one per version axis:

* ``--tag <release>`` extracts one language release's KerML + SysML specifications from
  ``sources/<release>/specs/`` into ``knowledge/<release>/spec/{kerml,sysml2}/``.
* ``--api-version <version>`` extracts the Systems Modeling API and Services specification from
  ``sources/api/<version>/specs/`` into ``knowledge/api/<version>/spec/``. The API is versioned apart
  from the language releases, so it is neither read from nor written into a release's folder.

Calls the exact same pipeline functions ``tests/test_generate.py`` already calls, so this and the
maintainer pytest flow can never drift apart. This is what an installed plugin runs, via
``uv run --project tools/spec-extract python -m spec_extract --repo-root <root> --tag <tag>``, so that
spec-citation can quote verbatim text without a provisioned ``.venv``. The pytest flow remains the
maintainer-facing way to regenerate every installed release at once.
"""

from __future__ import annotations

import argparse
import sys
from pathlib import Path

from spec_extract.pipeline import DocMeta, extract_document, write_clauses, write_index, write_index_json

KERML_PDF = "1-Kernel_Modeling_Language.pdf"
SYSML_PDF = "2a-OMG_Systems_Modeling_Language.pdf"
API_PDF = "3-Systems_Modeling_API_and_Services.pdf"


def main(argv: list[str] | None = None) -> int:
    """Extracts one release's, or one API version's, spec text; returns a process exit code."""
    args = _parse_args(argv)
    out_root = args.out_root or args.repo_root

    if args.api_version:
        version = args.api_version
        specs_dir = args.repo_root / "sources" / "api" / version / "specs"
        out_base = out_root / "knowledge" / "api" / version / "spec"
        targets = [(specs_dir / API_PDF, DocMeta("SystemsModelingAPI", "1.0", ""))]
    else:
        version = args.tag
        specs_dir = args.repo_root / "sources" / version / "specs"
        out_base = out_root / "knowledge" / version / "spec"
        targets = [
            (specs_dir / KERML_PDF, DocMeta("KerML", "1.0", "kerml")),
            (specs_dir / SYSML_PDF, DocMeta("SysML", "2.0", "sysml2")),
        ]

    missing = [pdf.name for pdf, _ in targets if not pdf.is_file()]
    if missing:
        print(f"missing OMG spec PDF(s) under {specs_dir}: {', '.join(missing)}", file=sys.stderr)
        return 1

    for pdf, meta in targets:
        clauses = extract_document(pdf, meta)
        if not clauses:
            print(f"no clauses extracted from {pdf.name} for {version}", file=sys.stderr)
            return 1

        out_dir = out_base / meta.out_subdir if meta.out_subdir else out_base
        write_clauses(clauses, out_dir)
        write_index(clauses, out_dir)
        write_index_json(clauses, out_dir)

    return 0


def _parse_args(argv: list[str] | None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        prog="python -m spec_extract",
        description=(
            "Extract one release's KerML + SysML spec text into knowledge/<tag>/spec/, or one API "
            "version's Systems Modeling API spec text into knowledge/api/<version>/spec/."
        ),
    )
    parser.add_argument(
        "--repo-root",
        required=True,
        type=Path,
        help="the repository root (reads sources/ from here)",
    )
    version = parser.add_mutually_exclusive_group(required=True)
    version.add_argument("--tag", help="the language release tag, e.g. 2026-05")
    version.add_argument("--api-version", help="the Systems Modeling API version, e.g. 2026-04")
    parser.add_argument(
        "--out-root",
        type=Path,
        default=None,
        help="where knowledge/ is written; defaults to --repo-root",
    )
    return parser.parse_args(argv)


if __name__ == "__main__":
    sys.exit(main())
