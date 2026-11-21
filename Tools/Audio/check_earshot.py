"""Verify the real local MCP transport, CPU inference, generation and the no-hosted-API configuration."""
import asyncio
import importlib
import json
import os
from pathlib import Path
import sys

from earshot_local import ROOT, INSTALL, NoHostedCritique, configure_environment

configure_environment()
sys.meta_path.insert(0, NoHostedCritique())


def unpack(result):
    """Decode Earshot's full MCP result while retaining useful diagnostics on a failed tool call."""
    text = '\n'.join(getattr(part, 'text', '') for part in result.content)
    if getattr(result, 'is_error', getattr(result, 'isError', False)):
        raise RuntimeError(text[:1600])
    if 'JSON: ' not in text:
        raise RuntimeError('Expected a full Earshot JSON response: ' + text[:600])
    return json.loads(text.rsplit('JSON: ', 1)[1])


async def check():
    """Use a disposable sample outside Assets; no game content is modified."""
    from mcp import ClientSession, StdioServerParameters
    from mcp.client.stdio import stdio_client
    blocked = False
    try:
        importlib.import_module('earshot.llm')
    except ImportError:
        blocked = True
    assert blocked, 'Hosted module was not blocked'
    parameters = StdioServerParameters(command=sys.executable,
        args=[str(ROOT / 'Tools/Audio/earshot_local.py'), 'mcp'], env=os.environ.copy(), cwd=str(ROOT))
    with (INSTALL / 'mcp-check-stderr.log').open('w', encoding='utf-8') as errors:
        async with stdio_client(parameters, errlog=errors) as (read, write):
            async with ClientSession(read, write, read_timeout_seconds=300) as session:
                await session.initialize()
                names = [tool.name for tool in (await session.list_tools()).tools]
                assert len(names) == 19 and not any('llm' in name for name in names)
                print('MCP handshake passed; 19 local tools, no cloud tool.', flush=True)
                doctor = unpack(await session.call_tool('doctor', {'load':True, 'full':True}))
                assert not doctor['problems'], doctor['problems']
                assert not doctor['torch_cuda_used'] and doctor['offline']
                assert not any(doctor['llm_keys_present'].values())
                print('Both CPU models loaded offline; no API keys present in the tool environment.', flush=True)
                sample = INSTALL / 'smoke-click.wav'
                generated = unpack(await session.call_tool('make', {'want':'a short soft wooden click', 'out':str(sample), 'full':True}))
                assert sample.exists() and sample.stat().st_size > 44
                measured = unpack(await session.call_tool('measure', {'path':str(sample), 'full':True}))
                print('Original sample generation and audio measurement passed.', flush=True)
                description = unpack(await session.call_tool('describe', {'path':str(sample), 'full':True}))
                assert description.get('summary') or description.get('text')
                report = {'passed':True, 'localTools':names, 'offline':True,
                    'hostedModuleBlocked':blocked, 'cloudToolAdvertised':False, 'apiKeysSupplied':False,
                    'doctor':doctor, 'generation':generated, 'measurement':measured, 'description':description}
                (INSTALL / 'verification.json').write_text(json.dumps(report, indent=2) + '\n', encoding='utf-8')
                print('Local audio inference passed. Full report: .local-tools/earshot/verification.json', flush=True)


if __name__ == '__main__':
    asyncio.run(check())
