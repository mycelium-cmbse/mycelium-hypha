import { spawn } from "node:child_process";
import { access, constants } from "node:fs/promises";
import path from "node:path";
import * as vscode from "vscode";

/** Input passed from the Copilot lookup tool to `hypha lookup --json`. */
export interface LookupInput {
  /** Selects the generated index to search. */
  kind: "metamodel" | "model-library";
  /** Exact name or search fragment to resolve. */
  name: string;
  /** Optional installed release; the CLI default is used when omitted. */
  tag?: string;
  /** Optional 1-based first line for a paginated lookup. */
  startLine?: number;
}

/** JSON result returned by `hypha lookup --json`. */
export interface LookupResult {
  /** Installed release that supplied this result. */
  release: string;
  /** Index that was searched. */
  kind: string;
  /** Whether the request found a source, ambiguous matches, suggestions, or nothing. */
  status: "found" | "ambiguous" | "candidates" | "not-found";
  /** Index key when a source was found. */
  name: string | null;
  /** Absolute path of the source file when found. */
  path: string | null;
  /** Declaration line, when the CLI located one in a model-library page. */
  declarationLine: number | null;
  /** First source line returned, 1-based. */
  startLine: number | null;
  /** Last source line returned, 1-based and inclusive. */
  endLine: number | null;
  /** Total number of lines in the source page. */
  totalLines: number | null;
  /** First line of the next page, or null when the source is complete. */
  nextLine: number | null;
  /** Returned source text when the lookup found one entry. */
  content: string | null;
  /** Names to choose from when the result is ambiguous or has suggestions. */
  matches: string[];
  /** Number of matches before the CLI's result cap. */
  totalMatches: number;
}

/** JSON result returned by `hypha check --json`. */
export interface CheckResult {
  /** Installed release tags, newest first. */
  installedTags: string[];
  /** Configured default release, or null when none is selected. */
  defaultTag: string | null;
  /** Newest releases that can be installed from upstream. */
  availableOnline: string[];
}

/** JSON result returned by `hypha list --json`. */
export interface ListResult {
  /** Configured default release, or null when none is selected. */
  defaultTag: string | null;
  /** Installed releases and their generated-artifact readiness. */
  releases: ReleaseStatus[];
}

/** One installed release and its artifact state, as reported by the CLI. */
export interface ReleaseStatus {
  /** Release tag. */
  tag: string;
  /** Whether this is the configured default release. */
  isDefault: boolean;
  /** Whether release input sources are present. */
  sourcesFetched: boolean;
  /** Generated artifacts, as determined by the CLI's layout implementation. */
  artifacts: {
    /** Metamodel index is present. */
    metamodel: boolean;
    /** Textual-notation index is present. */
    textualNotation: boolean;
    /** Model-library index is present. */
    modelLibrary: boolean;
    /** Cross-reference data is present. */
    crossReferences: boolean;
    /** Both normative specification catalogs are present. */
    specification: boolean;
    /** All core generated artifacts are present. */
    core: boolean;
  };
}

/**
 * Resolves the executable bundled for the current extension host.
 * @param extensionPath Root folder of the installed extension.
 * @returns Absolute path to the host-specific Hypha CLI executable.
 * @throws If the executable is missing or not executable.
 */
export async function ensureCli(extensionPath: string): Promise<string> {
  const executable = path.join(
    extensionPath, "out", "cli", process.platform === "win32" ? "hypha.exe" : "hypha"
  );
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

/**
 * Runs the bundled CLI with extension-owned data and cache roots.
 *
 * On POSIX, the child starts in its own process group so cancellation can stop
 * descendants as well as `hypha`. Windows uses `taskkill /t` for the same reason.
 *
 * @param executable Absolute path to the bundled CLI.
 * @param storagePath Extension-global storage used for repository and toolchain data.
 * @param args CLI command and arguments, before the shared root options are appended.
 * @param output Channel for command output and diagnostics.
 * @param token VS Code cancellation token for the command.
 * @param captureStdout Whether successful stdout is also shown in the output channel.
 * @returns Captured standard output.
 * @throws CancellationError when cancelled, or an Error when the process cannot start or exits unsuccessfully.
 */
export async function runCli(
  executable: string,
  storagePath: string,
  args: string[],
  output: vscode.OutputChannel,
  token: vscode.CancellationToken,
  captureStdout = true
): Promise<string> {
  if (token.isCancellationRequested) {
    throw new vscode.CancellationError();
  }
  return new Promise<string>((resolve, reject) => {
    const child = spawn(
      executable,
      [...args, "--repository-root", storagePath, "--toolchain-cache-root", path.join(storagePath, "toolchain"), "--no-logo"],
      {
        cwd: storagePath,
        windowsHide: true,
        detached: process.platform !== "win32",
        env: {
          ...process.env,
          UV_CACHE_DIR: path.join(storagePath, "toolchain", "uv-cache"),
          UV_PYTHON_INSTALL_DIR: path.join(storagePath, "toolchain", "python")
        }
      }
    );
    let stdout = "";
    const cancellation = token.onCancellationRequested(() => {
      const pid = child.pid;
      if (!pid) {
        child.kill();
        return;
      }

      if (process.platform === "win32") {
        const killer = spawn("taskkill", ["/pid", String(pid), "/t", "/f"], {
          windowsHide: true,
          stdio: "ignore"
        });
        killer.on("error", () => child.kill());
        killer.on("close", (code) => {
          if (code !== 0) {
            child.kill();
          }
        });
        killer.unref();
        return;
      }

      try {
        process.kill(-pid, "SIGTERM");
      } catch {
        child.kill();
      }
    });
    child.stdout.setEncoding("utf8");
    child.stdout.on("data", (chunk: string) => {
      if (captureStdout) {
        output.append(chunk);
      }
      stdout += chunk;
    });
    child.stderr.setEncoding("utf8");
    child.stderr.on("data", (chunk: string) => output.append(chunk));
    child.on("error", (error) => {
      cancellation.dispose();
      reject(new Error(`Could not start the Hypha CLI: ${error.message}`, { cause: error }));
    });
    child.on("close", (code, signal) => {
      cancellation.dispose();
      if (token.isCancellationRequested) {
        reject(new vscode.CancellationError());
      } else if (code !== 0) {
        if (!captureStdout && stdout) {
          output.append(stdout);
        }
        const detail = !captureStdout ? readJsonError(stdout) : undefined;
        reject(new Error(detail
          ? `The Hypha CLI failed: ${detail}`
          : `The Hypha CLI exited with code ${code ?? `after signal ${signal}`}. See the Hypha output channel for details.`));
      } else {
        resolve(stdout);
      }
    });
  });
}

/**
 * Runs a CLI command that writes a single JSON document to standard output.
 * The generic type describes the CLI's versioned JSON contract; JSON parsing is
 * checked here, while field validation remains the CLI contract's responsibility.
 *
 * @param executable Absolute path to the bundled CLI.
 * @param storagePath Extension-global storage used for repository and toolchain data.
 * @param args CLI command and arguments.
 * @param output Channel for command diagnostics.
 * @param token VS Code cancellation token for the command.
 * @returns The parsed CLI document.
 * @throws If the command fails or stdout is not valid JSON.
 */
export async function runCliJson<T>(
  executable: string,
  storagePath: string,
  args: string[],
  output: vscode.OutputChannel,
  token: vscode.CancellationToken
): Promise<T> {
  const text = await runCli(executable, storagePath, args, output, token, false);
  try {
    return JSON.parse(text) as T;
  } catch (error) {
    throw new Error("The Hypha CLI returned invalid JSON.", { cause: error });
  }
}

/** Returns a structured CLI error when a failed JSON command reports one. */
function readJsonError(stdout: string): string | undefined {
  try {
    const result: unknown = JSON.parse(stdout);
    if (typeof result === "object" && result !== null && "error" in result &&
        typeof result.error === "string" && result.error.length > 0) {
      return result.error;
    }
  } catch {
    // Keep the process-exit diagnostic when stdout was not a JSON error document.
  }
  return undefined;
}
