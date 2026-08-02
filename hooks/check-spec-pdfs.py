#!/usr/bin/env python3
# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""SessionStart hook: detect whether the OMG specification PDFs are available for the default release.

The PDFs are copyrighted (not shipped with the plugin) but are required to (re)generate the spec
citation knowledge base (knowledge/<tag>/spec, via tools/spec-extract). Inputs are per release tag,
so this checks the default release recorded in knowledge/versions.json. If any PDF is missing, emit a
message asking the user to download them. If all are present, stay silent.

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


def default_tag(plugin_root):
    """The release tag hypha answers from by default, or None when there is no manifest."""
    manifest = plugin_root / "knowledge" / "versions.json"
    if not manifest.is_file():
        return None
    try:
        return json.loads(manifest.read_text(encoding="utf-8")).get("default")
    except (ValueError, OSError):
        return None


def main():
    # Resolve the plugin root from the env var Claude sets, falling back to this script's location.
    plugin_root = Path(os.environ.get("CLAUDE_PLUGIN_ROOT") or Path(__file__).resolve().parents[1])

    tag = default_tag(plugin_root)
    if tag is None:
        return  # no manifest: nothing meaningful to check

    specs_dir = plugin_root / "sources" / tag / "specs"

    missing = [(name, url) for name, url in PDFS if not (specs_dir / name).is_file()]
    if not missing:
        return  # all present: nothing to report

    # The PDFs are published per release tag, so point at this release's copies rather than master.
    tagged = [(name, url.replace("/blob/master/", "/blob/" + tag + "/")) for name, url in missing]

    context = "\n".join(
        [
            "Hypha: the OMG specification PDFs for release " + tag + " are not available, so the normative",
            "spec-citation knowledge base (knowledge/" + tag + "/spec) cannot be generated. The",
            "metamodel-lookup and SysML-validation features still work, and spec-citation can still name",
            "the governing clause from the committed cross-references.",
            "",
            "Missing under sources/" + tag + "/specs/: " + ", ".join(name for name, _ in tagged) + ".",
            "",
            "Download the PDF(s) for this release into sources/" + tag + "/specs/:",
        ]
        + ["  - " + url for _, url in tagged]
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


if __name__ == "__main__":
    main()
