"""Copy unchanged generated sources and register their measured alpha bounds."""
from pathlib import Path
import hashlib
import json
import shutil
from PIL import Image

ROOT = Path(__file__).resolve().parent
LIBRARY = ROOT.parent
jobs = json.loads((ROOT / 'GenerationJobs.json').read_text(encoding='utf-8'))['jobs']
manifest_path = LIBRARY / 'Manifest.json'
manifest = json.loads(manifest_path.read_text(encoding='utf-8'))
report = []
for job in jobs:
    source = Path(job['source'])
    target = LIBRARY / 'Sprites' / (job['id'] + '.png')
    if target.exists() and target.read_bytes() != source.read_bytes():
        raise ValueError(f'Refusing to replace existing sprite: {target}')
    shutil.copyfile(source, target)
    with Image.open(target) as image:
        assert image.mode == 'RGBA', (job['id'], image.mode)
        alpha = image.getchannel('A')
        raw_bounds = alpha.getbbox()
        # Ignore only near-transparent generation noise when measuring the silhouette.
        cutoff = 16
        x0, y0, x1, y1 = alpha.point(lambda value: 255 if value > cutoff else 0).getbbox()
        assert alpha.getextrema() == (0, 255)
        assert 0 < x0 < x1 < image.width and 0 < y0 < y1 < image.height
        entry = {key: job[key] for key in ['id','name','definition']}
        entry.update(filename=target.name, width=image.width, height=image.height,
                     alphaMin=0, alphaMax=255, bounds=[x0,y0,x1-x0,y1-y0],
                     clearMargin=True, alphaCutoff=cutoff, rawAlphaBounds=list(raw_bounds), sha256=hashlib.sha256(target.read_bytes()).hexdigest(),
                     provenance='Supplement20261119/GenerationJobs.json')
        previous = next((item for item in manifest['sprites'] if item['id'] == job['id']), None)
        if previous is not None:
            assert previous == entry, f'Existing registration changed: {job["id"]}'
        else:
            manifest['sprites'].append(entry)
        report.append(entry)
supplement = {'date':'2026-11-19', 'ids':[job['id'] for job in jobs],
              'provenance':'Supplement20261119/GenerationJobs.json', 'penpotPublished':False}
manifest.setdefault('supplements', [])
if supplement not in manifest['supplements']:
    manifest['supplements'].append(supplement)
manifest_path.write_text(json.dumps(manifest, indent=2) + '\n', encoding='utf-8')
(ROOT / 'Validation.json').write_text(json.dumps({'passed':True,'sprites':report}, indent=2) + '\n', encoding='utf-8')
print(json.dumps({'registered':len(report),'libraryTotal':len(manifest['sprites']),
                  'sprites':[{key:item[key] for key in ['id','width','height','bounds']} for item in report]}))
