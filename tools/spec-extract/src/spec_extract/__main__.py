# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""Command-line entry point: extract one release's KerML + SysML spec text into markdown.

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


def main(argv: list[str] | None = None) -> int:
    """Extracts one release's spec text; returns a process exit code."""
    args = _parse_args(argv)
    out_root = args.out_root or args.repo_root
    specs_dir = args.repo_root / "sources" / args.tag / "specs"

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
            print(f"no clauses extracted from {pdf.name} for {args.tag}", file=sys.stderr)
            return 1

        out_dir = out_root / "knowledge" / args.tag / "spec" / meta.out_subdir
        write_clauses(clauses, out_dir)
        write_index(clauses, out_dir)
        write_index_json(clauses, out_dir)

    return 0


def _parse_args(argv: list[str] | None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        prog="python -m spec_extract",
        description="Extract one release's KerML + SysML spec text into knowledge/<tag>/spec/.",
    )
    parser.add_argument(
        "--repo-root",
        required=True,
        type=Path,
        help="the repository root (reads sources/<tag>/specs/ from here)",
    )
    parser.add_argument("--tag", required=True, help="the release tag, e.g. 2026-05")
    parser.add_argument(
        "--out-root",
        type=Path,
        default=None,
        help="where knowledge/<tag>/spec/ is written; defaults to --repo-root",
    )
    return parser.parse_args(argv)


if __name__ == "__main__":
    sys.exit(main())
