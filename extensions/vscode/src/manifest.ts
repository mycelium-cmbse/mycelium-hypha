export interface ReleaseManifest {
  defaultTag: string | null;
  tags: string[];
}

export function parseReleaseManifest(content: string): ReleaseManifest {
  let value: unknown;

  try {
    value = JSON.parse(content);
  } catch (error) {
    throw new Error("The release manifest is not valid JSON.", { cause: error });
  }

  if (!isRecord(value) || !Array.isArray(value.versions)) {
    throw new Error("The release manifest must contain a versions array.");
  }

  const tags = value.versions.map((release: unknown) => {
    if (!isRecord(release) || typeof release.tag !== "string" || release.tag.length === 0) {
      throw new Error("Every release in the manifest must have a non-empty tag.");
    }

    return release.tag;
  });

  const defaultTag = value.default;
  if (defaultTag !== null && typeof defaultTag !== "string") {
    throw new Error("The default release tag must be a string or null.");
  }

  if (defaultTag !== null && !tags.includes(defaultTag)) {
    throw new Error("The default release tag must identify an installed release.");
  }

  return { defaultTag, tags };
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === "object" && value !== null && !Array.isArray(value);
}
