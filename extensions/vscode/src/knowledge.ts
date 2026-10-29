import { readFile, realpath, stat } from "node:fs/promises";
import path from "node:path";
import { pathToFileURL } from "node:url";

import { parseReleaseManifest } from "./manifest";

export interface LookupInput {
  kind: "metamodel" | "model-library";
  name: string;
  tag?: string;
  startLine?: number;
}

export async function lookupKnowledge(storagePath: string, input: LookupInput): Promise<string> {
  if (!["metamodel", "model-library"].includes(input.kind) || !input.name?.trim()) {
    throw new Error("Specify a metamodel or model-library lookup and a non-empty name.");
  }
  const start = input.startLine ?? 1;
  if (!Number.isInteger(start) || start < 1) {
    throw new Error("startLine must be a positive integer.");
  }
  const manifest = parseReleaseManifest(await readText(path.join(storagePath, "knowledge", "versions.json")));
  const tag = input.tag ?? manifest.defaultTag;
  if (!tag || !manifest.tags.includes(tag)) {
    throw new Error(`The requested release is not installed or no default is selected. Installed: ${manifest.tags.join(", ") || "none"}.`);
  }
  if (!/^[0-9]{4}-[0-9]{2}(?:\.[0-9]+)?$/.test(tag)) {
    throw new Error("The installed manifest contains an invalid release tag.");
  }
  const root = path.join(storagePath, "knowledge", tag, input.kind);
  const index: unknown = JSON.parse(await readText(path.join(root, "index.json")));
  const entries = isRecord(index) ? index[input.kind === "metamodel" ? "entries" : "declarations"] : undefined;
  if (!isRecord(entries)) {
    throw new Error(`Invalid ${input.kind} index for ${tag}. Regenerate the release.`);
  }
  const name = input.name.trim();
  const exact = Object.entries(entries).find(([key]) => key === name);
  const matches = exact ? [exact] : Object.entries(entries).filter(([key, value]) =>
    key.toLowerCase().includes(name.toLowerCase()) ||
    (isRecord(value) && typeof value.qualifiedName === "string" && value.qualifiedName.toLowerCase() === name.toLowerCase())
  );
  if (matches.length !== 1) {
    return JSON.stringify({
      release: tag,
      kind: input.kind,
      message: matches.length === 0 ? "No matching entry. Do not substitute general knowledge." : "Choose an exact name and call this tool again.",
      matches: matches.slice(0, 20).map(([key]) => key),
      totalMatches: matches.length
    });
  }
  const [entryName, entry] = matches[0];
  if (!isRecord(entry) || typeof entry.file !== "string" || !entry.file.endsWith(".md")) {
    throw new Error(`Invalid source path for ${entryName} in the ${tag} index.`);
  }
  const file = await containedFile(root, entry.file);
  const lines = (await readText(file)).split(/\r?\n/);
  if (start > lines.length) {
    throw new Error(`startLine exceeds the ${lines.length} lines in this source.`);
  }
  const end = Math.min(start + 159, lines.length);
  return JSON.stringify({
    release: tag,
    kind: input.kind,
    name: entryName,
    source: pathToFileURL(file).href,
    startLine: start,
    endLine: end,
    totalLines: lines.length,
    nextLine: end < lines.length ? end + 1 : null,
    guidance: "Answer only from this release's source. Cite the source file and release. Inherited features are already expanded: do not reconstruct inheritance. This is source-backed lookup, not compiler validation or normative specification text. Read remaining lines if needed; do not treat a truncated page as complete.",
    content: lines.slice(start - 1, end).join("\n")
  });
}

async function containedFile(root: string, relative: string): Promise<string> {
  if (path.isAbsolute(relative) || relative.includes("\\")) {
    throw new Error("The knowledge index contains an unsafe source path.");
  }
  const [resolvedRoot, file] = await Promise.all([realpath(root), realpath(path.resolve(root, relative))]);
  const difference = path.relative(resolvedRoot, file);
  if (difference.startsWith("..") || path.isAbsolute(difference)) {
    throw new Error("The knowledge index points outside its release directory.");
  }
  return file;
}

async function readText(file: string): Promise<string> {
  if ((await stat(file)).size > 8 * 1024 * 1024) {
    throw new Error("The knowledge source exceeds the 8 MiB read limit.");
  }
  return readFile(file, "utf8");
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === "object" && value !== null && !Array.isArray(value);
}
