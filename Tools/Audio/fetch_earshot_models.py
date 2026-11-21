"""Download only Earshot's pinned public safetensors/config files, with no Hugging Face credentials."""
from pathlib import Path
import ast
import hashlib
import json
import re
import time
import urllib.request
import urllib.error

root = Path(__file__).resolve().parents[2]
tool = root / '.local-tools/earshot'
source = Path(json.loads((tool / 'source.json').read_text())['source'])
tree = ast.parse((source / 'earshot/config.py').read_text())
pins = next(ast.literal_eval(node.value) for node in tree.body if isinstance(node, ast.Assign)
            and any(isinstance(t, ast.Name) and t.id == 'PINS' for t in node.targets))
report = {}
for repo, spec in pins.items():
    short = 'clap' if repo.startswith('laion/') else 'ast'
    folder = tool / 'models' / short
    folder.mkdir(parents=True, exist_ok=True)
    report[short] = {'repository':repo, 'revision':spec['revision'], 'files':{}}
    for name in [*spec['files'], spec['weights']]:
        path = folder / name
        part = path.with_suffix(path.suffix + '.part')
        if not path.exists():
            url = 'https://huggingface.co/' + repo + '/resolve/' + spec['revision'] + '/' + name
            for attempt in range(4):
                offset = part.stat().st_size if part.exists() else 0
                headers = {'User-Agent':'Deepwater-local-audio-setup', 'Accept-Encoding':'identity'}
                if offset:
                    headers['Range'] = 'bytes=' + str(offset) + '-'
                request = urllib.request.Request(url, headers=headers)
                print(json.dumps({'model':short, 'file':name, 'resumeMB':round(offset/1e6, 1), 'attempt':attempt + 1}), flush=True)
                try:
                    with urllib.request.urlopen(request, timeout=90) as response:
                        append = offset > 0 and response.status == 206
                        if append:
                            assert response.headers['Content-Range'].startswith('bytes ' + str(offset) + '-')
                        with part.open('ab' if append else 'wb') as output:
                            while chunk := response.read(1024 * 1024):
                                output.write(chunk)
                    break
                except urllib.error.HTTPError as error:
                    complete = re.fullmatch(r'bytes \*/(\d+)', error.headers.get('Content-Range', ''))
                    if error.code == 416 and complete and int(complete[1]) == offset:
                        break
                    if attempt == 3:
                        raise RuntimeError('Public model download failed: HTTP ' + str(error.code)) from None
                except (TimeoutError, OSError):
                    if attempt == 3:
                        raise RuntimeError('Public model download interrupted; partial file preserved for resumption') from None
                time.sleep(1)
            part.replace(path)
        with path.open('rb') as downloaded:
            digest = hashlib.file_digest(downloaded, 'sha256').hexdigest()
        report[short]['files'][name] = {'bytes':path.stat().st_size, 'sha256':digest}
    print(json.dumps({'model':short, 'status':'download complete'}), flush=True)
(tool / 'models.json').write_text(json.dumps(report, indent=2) + '\n', encoding='utf-8')
