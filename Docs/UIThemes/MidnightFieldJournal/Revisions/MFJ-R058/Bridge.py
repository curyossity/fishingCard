"""Use the existing local MCP session without changing Penpot or Unity connection configuration."""
import json
import os
from pathlib import Path
import subprocess
import sys

folder = Path(__file__).resolve().parent

def call(name, arguments):
    command = ['python', os.environ['TEMP'] + '/penpot-modifier-tooltip-mcp.py']
    result = subprocess.run(command, input=json.dumps({'name': name, 'arguments': arguments}), text=True, capture_output=True, timeout=75)
    if result.returncode:
        raise RuntimeError(result.stderr[:1500])
    response = json.loads(result.stdout)
    if 'error' in response:
        raise RuntimeError(str(response['error']))
    content = response['result']['content'][0]['text']
    if response['result'].get('isError') or content.startswith('Tool execution failed:'):
        raise RuntimeError(content)
    try:
        value = json.loads(content)
    except ValueError:
        return content
    return value.get('result', value)

if __name__ == '__main__':
    if sys.argv[1] == 'inspect':
        print(json.dumps(call('execute_code', {'code': "const s=penpot.currentPage.getShapeById('75555645-d407-8067-8008-de9d62501389');return {page:penpot.currentPage.id,board:penpotUtils.shapeStructure(s,1)};"})))
    elif sys.argv[1] == 'run':
        print(json.dumps(call('execute_code', {'code': (folder / sys.argv[2]).read_text(encoding='utf-8')})))
