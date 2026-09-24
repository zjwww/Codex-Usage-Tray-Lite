"""Encode deterministic browser frames into README GIFs and check each output."""
from pathlib import Path
from PIL import Image
import json

repo=Path(__file__).resolve().parents[1]
source=repo/'artifacts/validation/readme-menu-demo-v0.2.26'
target=repo/'docs/images'
reports=[]
for locale in ('en','zh-cn'):
    for theme in ('light','dark'):
        prefix=f'{locale}-{theme}'
        manifest=json.loads((source/f'{prefix}.json').read_text())
        frames=[Image.open(source/f['file']).convert('RGB') for f in manifest]
        assert len({im.size for im in frames})==1
        # A shared palette keeps text and backgrounds stable between frames.
        sample=Image.new('RGB',(frames[0].width,frames[0].height*len(frames)))
        for i,im in enumerate(frames): sample.paste(im,(0,i*im.height))
        palette=sample.quantize(colors=128,method=Image.Quantize.MEDIANCUT)
        indexed=[im.quantize(palette=palette,dither=Image.Dither.NONE) for im in frames]
        dest=target/f'menu-demo-{prefix}.gif'
        indexed[0].save(dest,save_all=True,append_images=indexed[1:],duration=[f['duration'] for f in manifest],loop=0,optimize=True,disposal=1)
        with Image.open(dest) as check:
            assert check.is_animated and check.n_frames==len(manifest)
            assert check.info.get('loop')==0
            duration=0
            for i in range(check.n_frames):check.seek(i);duration+=check.info['duration']
            assert duration==sum(f['duration'] for f in manifest)
            check.seek(6);check.convert('RGB').save(source/f'{prefix}-gif-tools-check.png')
            reports.append({'file':dest.name,'frames':check.n_frames,'durationMs':duration,'size':check.size,'bytes':dest.stat().st_size})
        assert dest.stat().st_size<2_000_000
(source/'gif-validation.json').write_text(json.dumps(reports,indent=2))
print(json.dumps(reports,indent=2))
