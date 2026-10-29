import { spawn } from "node:child_process";
import { access, constants } from "node:fs/promises";
import path from "node:path";
import * as vscode from "vscode";

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

export async function runCli(
  executable: string,
  storagePath: string,
  args: string[],
  output: vscode.OutputChannel,
  token: vscode.CancellationToken
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
        env: {
          ...process.env,
          UV_CACHE_DIR: path.join(storagePath, "toolchain", "uv-cache"),
          UV_PYTHON_INSTALL_DIR: path.join(storagePath, "toolchain", "python")
        }
      }
    );
    let stdout = "";
    const cancellation = token.onCancellationRequested(() => child.kill());
    child.stdout.setEncoding("utf8");
    child.stdout.on("data", (chunk: string) => {
      output.append(chunk);
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
        reject(new Error(`The Hypha CLI exited with code ${code ?? `after signal ${signal}`}. See the Hypha output channel for details.`));
      } else {
        resolve(stdout);
      }
    });
  });
}
