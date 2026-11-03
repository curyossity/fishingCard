import subprocess,json
from pathlib import Path
target=Path('Temp/Sync013Compile');target.mkdir(exist_ok=True)
results=[]
for assembly in ['Assembly-CSharp','Assembly-CSharp-Editor']:
 source=Path('Library/Bee/artifacts/1900b0aEDbg.dag')/(assembly+'.rsp')
 lines=[]
 for line in source.read_text(encoding='utf-8-sig').splitlines():
  if line.startswith(('-out:','-refout:','-pdb:','-analyzer:','-analyzerconfig:','-additionalfile:','/additionalfile:')):continue
  if assembly.endswith('-Editor') and line.startswith('-r:') and ('/Assembly-CSharp.dll' in line or '/Assembly-CSharp.ref.dll' in line):
   line='-r:"'+(target/'Assembly-CSharp.dll').as_posix()+'"'
  lines.append(line)
 lines.append('-out:"'+(target/(assembly+'.dll')).as_posix()+'"')
 response=target/(assembly+'.rsp');response.write_text('\n'.join(lines),encoding='utf-8')
 run=subprocess.run(['C:/Program Files/dotnet/dotnet.exe','C:/Program Files/dotnet/sdk/9.0.304/Roslyn/bincore/csc.dll','@'+str(response)],capture_output=True,text=True)
 (target/(assembly+'.log')).write_text(run.stdout+run.stderr,encoding='utf-8')
 results.append({'assembly':assembly,'passed':run.returncode==0,'log':str(target/(assembly+'.log'))})
 print(json.dumps(results[-1]))
 if run.returncode:print((run.stdout+run.stderr)[-3000:])
Path('Docs/UIThemes/MidnightFieldJournal/Sync013/Compilation.json').write_text(json.dumps({'backend':'Unity-generated Editor response files / .NET SDK Roslyn; output isolated under Temp','results':results},indent=2))
