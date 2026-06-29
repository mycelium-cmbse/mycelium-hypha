#!/usr/bin/env python3
# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""SessionStart hook: detect whether the OMG specification PDFs are available under sources/specs/.

The PDFs are copyrighted (not shipped with the plugin) but are required to (re)generate the spec
citation knowledge base (knowledge/spec, via tools/spec-extract). If any are missing, emit a message
asking the user to download them. If all are present, stay silent.

Python is used deliberately: a user who is missing the PDFs must run the tools/spec-extract Python
scripts to build the spec knowledge base anyway, so they already have Python available. Uses only the
standard library and avoids version-specific syntax so it runs on any reasonably recent Python 3.
"""

import json
import os
import sys
from pathlib import Path

PDFS = [
    (
        "1-Kernel_Modeling_Language.pdf",
        "https://github.com/Systems-Modeling/SysML-v2-Release/blob/master/doc/1-Kernel_Modeling_Language.pdf",
    ),
    (
        "2a-OMG_Systems_Modeling_Language.pdf",
        "https://github.com/Systems-Modeling/SysML-v2-Release/blob/master/doc/2a-OMG_Systems_Modeling_Language.pdf",
    ),
    (
        "3-Systems_Modeling_API_and_Services.pdf",
        "https://github.com/Systems-Modeling/SysML-v2-Release/blob/master/doc/3-Systems_Modeling_API_and_Services.pdf",
    ),
]


def main():
    # Resolve the plugin root from the env var Claude sets, falling back to this script's location.
    plugin_root = Path(os.environ.get("CLAUDE_PLUGIN_ROOT") or Path(__file__).resolve().parents[1])
    specs_dir = plugin_root / "sources" / "specs"

    missing = [(name, url) for name, url in PDFS if not (specs_dir / name).is_file()]
    if not missing:
        return 0  # all present: nothing to report

    context = "\n".join(
        [
            "Hypha: the OMG specification PDFs are not available, so the normative spec-citation knowledge",
            "base (knowledge/spec) cannot be generated. The metamodel-lookup and SysML-validation features",
            "still work.",
            "",
            "Missing under sources/specs/: " + ", ".join(name for name, _ in missing) + ".",
            "",
            "Download the PDF(s) from the SysML v2 release repository into sources/specs/:",
        ]
        + ["  - " + url for _, url in missing]
        + [
            "",
            "Then build the spec knowledge base by running the tools/spec-extract tests with the PDFs",
            "present (see tools/spec-extract/README.md).",
        ]
    )

    json.dump(
        {"hookSpecificOutput": {"hookEventName": "SessionStart", "additionalContext": context}},
        sys.stdout,
    )
    return 0


if __name__ == "__main__":
    sys.exit(main())
