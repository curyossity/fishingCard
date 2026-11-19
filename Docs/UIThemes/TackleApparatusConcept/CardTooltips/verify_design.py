"""Verify native SVG structure and record the completed local browser checks."""
from pathlib import Path
from html import unescape
import json
import re
import xml.etree.ElementTree as ET

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[3]


def browser_report(name):
    source = ROOT / 'Temp/TooltipResearch' / name
    content = source.read_text(encoding='utf-8-sig')
    match = re.search(r'<pre id="report">(.*?)</pre>', content, re.S)
    if not match:
        raise ValueError('No completed browser report in ' + name)
    return json.loads(unescape(match.group(1)))


def luminance(color):
    values = [int(color[i:i+2], 16) / 255 for i in (1, 3, 5)]
    linear = [v / 12.92 if v <= .04045 else ((v + .055) / 1.055) ** 2.4 for v in values]
    return sum(v * weight for v, weight in zip(linear, (.2126, .7152, .0722)))


def contrast(a, b):
    x, y = sorted((luminance(a), luminance(b)))
    return round((y + .05) / (x + .05), 2)


def verify():
    sources = sorted(HERE.glob('*.svg'))
    text_count = 0
    for path in sources:
        tree = ET.parse(path)
        ids = [node.attrib['id'] for node in tree.iter() if 'id' in node.attrib]
        if len(ids) != len(set(ids)):
            raise ValueError('Duplicate SVG IDs in ' + path.name)
        text_count += sum(node.tag.endswith('}text') for node in tree.iter())
    layout = browser_report('QA-final-dom.txt')
    interaction = browser_report('QA-review-final-dom.txt')
    assert not layout['overflows'] and all(f['status'] == 'loaded' for f in layout['fonts'])
    assert interaction['passed'] and all(c['passed'] for c in interaction['checks'])
    colors = {name: contrast(color, '#202725') for name, color in
              [('body', '#F2E5C4'), ('secondary', '#BEBAA8'), ('coefficient', '#E9BC63'), ('positiveContext', '#B9DDC0')]}
    assert min(colors.values()) >= 4.5
    report = dict(date='2026-11-19', svgFiles=len(sources), editableTextElements=text_count,
                  localLayout=layout, localInteraction=interaction, contrastRatios=colors,
                  penpotStatus='Blocked: HTTP 400 / Server not initialized',
                  unityIntegration='Not part of this design pass')
    (HERE / 'QAReport.json').write_text(json.dumps(report, indent=2) + '\n', encoding='utf-8')
    print(json.dumps(dict(svgFiles=len(sources), textBoundsChecked=layout['checked'], overflows=0,
                          interactionChecks=len(interaction['checks']), contrastRatios=colors), indent=2))


if __name__ == '__main__':
    verify()
