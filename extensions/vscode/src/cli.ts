import { access, constants } from "node:fs/promises";
import path from "node:path";

/**
 * The path of the `hypha` executable bundled for an extension host, whether or not it is there.
 * @param extensionPath Root folder of the installed extension.
 * @param platform The host's platform, as `process.platform` reports it.
 * @returns Absolute path of the host-specific executable.
 */
export function cliPath(extensionPath: string, platform: string = process.platform): string {
  return path.join(extensionPath, "out", "cli", platform === "win32" ? "hypha.exe" : "hypha");
}

/**
 * Resolves the executable bundled for the current extension host.
 * @param extensionPath Root folder of the installed extension.
 * @returns Absolute path to the host-specific Hypha CLI executable.
 * @throws If the executable is missing or not executable.
 */
export async function ensureCli(extensionPath: string): Promise<string> {
  const executable = cliPath(extensionPath);
  try {
    await access(executable, constants.X_OK);
  } catch (error) {
    throw new Error(
      "The bundled Hypha CLI is missing or not executable. Install the VSIX for this extension host, or run npm run build:cli before launching the development host.",
      { cause: error }
    );
  }
  return executable;
}
