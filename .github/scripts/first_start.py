# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""The launcher's first start, as an MCP client makes it, against a published tools release.

Starts ``bin/hypha-launch`` (the extensionless path every client package names: the POSIX script on
macOS and Linux, ``hypha-launch.exe`` found from it on Windows) with nothing cached, then checks:

1. ``initialize`` is answered at once, while ``hypha`` downloads;
2. the client is told its tools changed, and ``tools/list`` then holds the real tools;
3. ``hypha_release_status`` answers;
4. ``hypha_install_release`` installs a release, reporting progress;
5. a query (``hypha_metaclass``) answers from that release;
6. the launcher exits with 0 once the client closes standard input.

Every folder it writes to is under the scratch folder it is given. Exits non-zero on any failure.
"""

import json
import os
import queue
import subprocess
import sys
import threading
import time

FIRST_START_TIMEOUT = 600
INSTALL_TIMEOUT = 1800


def main(launcher: str, scratch: str, release: str) -> int:
    folders = {name: os.path.join(scratch, name) for name in ('home', 'claude', 'config', 'cwd')}
    for folder in folders.values():
        os.makedirs(folder, exist_ok=True)

    env = {name: value for name, value in os.environ.items() if name not in ('HYPHA_BIN', 'HYPHA_HOME')}
    env.update(HYPHA_HOME=folders['home'], CLAUDE_CONFIG_DIR=folders['claude'], XDG_CONFIG_HOME=folders['config'])

    # On Windows a client's Node spawn finds bin/hypha-launch.exe from the extensionless path; Python's
    # CreateProcess would try the POSIX script of that name instead, so name the .exe it finds.
    executable = os.path.abspath(launcher)
    if os.name == 'nt' and not executable.lower().endswith('.exe'):
        executable += '.exe'

    started = time.perf_counter()
    process = subprocess.Popen(
        [executable], stdin=subprocess.PIPE, stdout=subprocess.PIPE, stderr=subprocess.PIPE,
        cwd=folders['cwd'], env=env, text=True, encoding='utf-8')
    messages: queue.Queue = queue.Queue()
    errors: list[str] = []

    def read_output() -> None:
        for line in process.stdout:
            if line.strip():
                messages.put(json.loads(line))

    def read_errors() -> None:
        for line in process.stderr:
            errors.append(line.rstrip())

    threading.Thread(target=read_output, daemon=True).start()
    threading.Thread(target=read_errors, daemon=True).start()
    report: list[str] = []

    def log(text: str) -> None:
        line = f"{time.perf_counter() - started:7.1f}s  {text}"
        print(line, flush=True)
        report.append(line)

    def send(message: dict) -> None:
        process.stdin.write(json.dumps(message) + '\n')
        process.stdin.flush()

    def wait(predicate, timeout: float, what: str) -> dict:
        deadline = time.time() + timeout
        while time.time() < deadline:
            try:
                message = messages.get(timeout=1)
            except queue.Empty:
                if process.poll() is not None:
                    break
                continue
            if predicate(message):
                return message
        raise RuntimeError(f"no {what} within {timeout}s; standard error ends with: {errors[-10:]}")

    def call(request_id: int, tool: str, arguments: dict, timeout: float, token: str | None = None) -> dict:
        params = {'name': tool, 'arguments': arguments}
        if token:
            params['_meta'] = {'progressToken': token}
        send({'jsonrpc': '2.0', 'id': request_id, 'method': 'tools/call', 'params': params})
        progress = 0
        while True:
            message = wait(lambda m: m.get('id') == request_id or m.get('method') == 'notifications/progress',
                           timeout, f'{tool} answer')
            if message.get('id') == request_id:
                if progress:
                    log(f"{tool}: {progress} progress notifications")
                result = message.get('result') or {}
                text = ''.join(block.get('text', '') for block in result.get('content', []))
                if 'error' in message or result.get('isError'):
                    raise RuntimeError(f"{tool} failed: {message.get('error') or text}")
                return json.loads(text)
            progress += 1

    try:
        send({'jsonrpc': '2.0', 'id': 1, 'method': 'initialize',
              'params': {'protocolVersion': '2025-06-18', 'capabilities': {},
                         'clientInfo': {'name': 'first-start-check', 'version': '1'}}})
        init = wait(lambda m: m.get('id') == 1, 60, 'initialize answer')['result']
        setting_up = 'starting for the first time' in init.get('instructions', '')
        log(f"initialize: {init['serverInfo']['name']} {init['serverInfo']['version']}, "
            f"{'downloading' if setting_up else 'already handed over'}")
        send({'jsonrpc': '2.0', 'method': 'notifications/initialized'})

        if setting_up:
            wait(lambda m: m.get('method') == 'notifications/tools/list_changed',
                 FIRST_START_TIMEOUT, 'tools/list_changed')
            log('downloaded, checked and handed over: the client was told its tools changed')

        send({'jsonrpc': '2.0', 'id': 2, 'method': 'tools/list'})
        names = [tool['name'] for tool in wait(lambda m: m.get('id') == 2, 60, 'tools/list')['result']['tools']]
        if 'hypha_metaclass' not in names:
            raise RuntimeError(f"the real tools are not listed: {names}")
        log(f"tools/list: {len(names)} tools")

        status = call(3, 'hypha_release_status', {}, 120)
        log(f"hypha_release_status: {len(status.get('releases', []))} releases installed")

        install = call(4, 'hypha_install_release', {'tag': release, 'includeSpecifications': False},
                       INSTALL_TIMEOUT, 'install')
        log(f"hypha_install_release {release}: done ({json.dumps(install)[:120]})")

        answer = call(5, 'hypha_metaclass', {'question': 'ancestors', 'name': 'PartUsage', 'tag': release}, 120)
        if 'Usage' not in answer.get('value', []) or answer.get('grounding', {}).get('release') != release:
            raise RuntimeError(f"the query did not answer from {release}: {json.dumps(answer)[:300]}")
        log(f"hypha_metaclass ancestors of PartUsage: {len(answer['value'])}, from {release}")

        process.stdin.close()
        code = process.wait(timeout=120)
        if code != 0:
            raise RuntimeError(f"the launcher exited with {code}")
        log('the launcher exited with 0 once standard input closed')
    except Exception as failure:
        log(f"FAILED: {failure}")
        if process.poll() is None:
            process.kill()
        return 1
    finally:
        summary = os.environ.get('GITHUB_STEP_SUMMARY')
        if summary:
            with open(summary, 'a', encoding='utf-8') as file:
                file.write('```\n' + '\n'.join(report) + '\n```\n')

    return 0


if __name__ == '__main__':
    sys.exit(main(*sys.argv[1:4]))
