import * as vscode from "vscode";

export async function releaseReadiness(globalStorageUri: vscode.Uri, tag: string): Promise<string> {
  const artifacts = [
    ["metamodel", "metamodel/index.md"],
    ["textual notation", "textual-notation/index.md"],
    ["model library", "model-library/index.md"],
    ["cross-references", "cross-references.json"]
  ] as const;
  const present: string[] = [];
  const missing: string[] = [];

  for (const [name, relativePath] of artifacts) {
    if (await fileExists(vscode.Uri.joinPath(globalStorageUri, "knowledge", tag, relativePath))) {
      present.push(name);
    } else {
      missing.push(name);
    }
  }

  const specAvailable =
    await fileExists(vscode.Uri.joinPath(globalStorageUri, "knowledge", tag, "spec", "kerml", "index.md")) &&
    await fileExists(vscode.Uri.joinPath(globalStorageUri, "knowledge", tag, "spec", "sysml2", "index.md"));
  const coreSummary = missing.length === 0
    ? `Core generated artifacts are present (${present.join(", ")}).`
    : `Core generated artifacts are incomplete. Present: ${present.join(", ") || "none"}. Missing: ${missing.join(", ")}.`;
  const specSummary = specAvailable
    ? " Normative specification text is present."
    : " Normative specification text is not present; spec answers may be limited to clause references.";

  return `Core readiness for ${tag}: ${coreSummary}${specSummary}`;
}

async function fileExists(uri: vscode.Uri): Promise<boolean> {
  try {
    const stat = await vscode.workspace.fs.stat(uri);
    return stat.size > 0;
  } catch (error) {
    if (error instanceof vscode.FileSystemError && error.code === "FileNotFound") {
      return false;
    }
    throw error;
  }
}
