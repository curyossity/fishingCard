"""Launch Deepwater's free, local Earshot tools with hosted critique and credentials disabled."""
from __future__ import annotations

import argparse
import importlib.abc
import json
import os
from pathlib import Path
import re
import runpy
import sys

ROOT = Path(__file__).resolve().parents[2]
INSTALL = ROOT / '.local-tools' / 'earshot'


def configure_environment(*, fetch_models: bool = False) -> None:
    """Drop inherited credentials without reading their values and keep normal audio work offline."""
    for name in tuple(os.environ):
        if re.search(r'API_?KEY|ACCESS_KEY|PRIVATE_KEY|_KEY$|TOKEN|SECRET|PASSWORD|CREDENTIAL|AUTHORIZATION', name, re.IGNORECASE):
            os.environ.pop(name, None)
    os.environ.update({
        'EARSHOT_HOME': str(INSTALL / 'cache'),
        'EARSHOT_CLAP': str(INSTALL / 'models' / 'clap'),
        'EARSHOT_AST': str(INSTALL / 'models' / 'ast'),
        'HF_HOME': str(INSTALL / 'cache' / 'huggingface'),
        'HF_HUB_DISABLE_IMPLICIT_TOKEN': '1',
        'HF_HUB_DISABLE_TELEMETRY': '1',
        'DO_NOT_TRACK': '1',
        'EARSHOT_THREADS': '3',
        'PYTHONUTF8': '1',
        'PYTHONIOENCODING': 'utf-8',
        'PIP_INDEX_URL': 'https://pypi.org/simple',
        'PIP_EXTRA_INDEX_URL': '',
        'PIP_CONFIG_FILE': os.devnull,
        'NETRC': os.devnull,
    })
    for name in ('EARSHOT_OFFLINE', 'HF_HUB_OFFLINE', 'TRANSFORMERS_OFFLINE'):
        os.environ[name] = '0' if fetch_models else '1'
    os.environ['PATH'] = str(INSTALL / 'ffmpeg') + os.pathsep + os.environ.get('PATH', '')


class NoHostedCritique(importlib.abc.MetaPathFinder):
    """Block the optional upload/API module even when called indirectly by a helper."""
    def find_spec(self, fullname, path=None, target=None):
        if fullname == 'earshot.llm' or fullname.startswith('earshot.llm.'):
            raise ImportError('Hosted audio critique is disabled: this installation is free and local only.')
        return None


def main() -> int:
    """Expose local CLI/MCP functions; only an explicit model-fetch command enables public downloads."""
    args = sys.argv[1:]
    if any(arg in {'llm', 'llm_critique', 'llm-critique', '--yes-upload'} for arg in args):
        print('Blocked: cloud critique, audio uploads and API keys are disabled.', file=sys.stderr)
        return 2
    configure_environment()
    sys.meta_path.insert(0, NoHostedCritique())
    if args == ['doctor', '--fetch']:
        runpy.run_path(str(Path(__file__).with_name('fetch_earshot_models.py')), run_name='__main__')
        args = ['doctor']
    if args and args[0] == 'mcp':
        from earshot import mcp_server
        # Filter before server construction: the hosted tool is neither advertised nor callable.
        mcp_server.TOOLS = [entry for entry in mcp_server.TOOLS if not entry[2] and 'llm' not in entry[0].__name__.lower()]
        if '--tools' in args:
            print(json.dumps({'mode':'free-local-only', 'tools':[entry[0].__name__[5:] for entry in mcp_server.TOOLS]}, indent=2))
            return 0
        mcp_server.INSTRUCTIONS = (
            'This installation permits free local audio tools only. No API keys, hosted critique or uploads. '
            'Models are predownloaded and normal work runs offline.\n'
            + '\n'.join(line for line in mcp_server.INSTRUCTIONS.splitlines() if 'llm_critique' not in line and 'Gemini' not in line)
        )
        return mcp_server.serve(argparse.Namespace(tools='--tools' in args, warm='--warm' in args, json=False))
    sys.argv = ['earshot', *args]
    runpy.run_module('earshot', run_name='__main__')
    return 0


if __name__ == '__main__':
    raise SystemExit(main())
