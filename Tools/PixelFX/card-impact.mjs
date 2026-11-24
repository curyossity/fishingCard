// Original Deepwater choreography, authored with PixelFX's MIT painter and seeded RNG.
import { Rng, fit, smooth } from '../../.local-tools/pixelfx/src/core/index.js';

export const meta = {
  name: 'Deepwater card impact',
  desc: 'Water squeezed out of a vertical card seam: foam ribbons tear into shaded drops, followed by two small rising bubbles.',
  tags: ['water', 'impact', 'ui', 'one-shot'], size: [96, 144], origin: [48, 72],
  duration: .4, loop: false, fps: 60, bg: 'dark',
  options: { palette: ['#163e48', '#246576', '#4096a6', '#70c4cb', '#b6e5df', '#f6fff0'], seed: 19, power: .65 },
};

/** A tapered, bent sheet of water, outlined against both pale cards and dark glass. */
function ribbon(p, ex, ey, width, C) {
  if (width < 1.2) return;
  const left = [], right = [];
  for (let j = 0; j <= 8; j++) {
    const u = j / 8, x = ex * Math.sin(u * Math.PI * .5), y = ey * u, w = width * (.3 + .7 * Math.sin(u * Math.PI * .8));
    left.push([x - w, y]); right.unshift([x + w, y]);
  }
  p.poly([...left, ...right], C[0]);
  p.poly(left.map(([x,y],i) => [x + .9, y]).concat(right.map(([x,y]) => [x - .9,y])), C[2]);
  p.polyline(left.map(([x,y]) => [x + 1,y]), C[4]);
  p.circle(ex,ey,width*.7,C[2]); p.circle(ex-.5,ey-.8,width*.48,C[4]);
  for (let j = 2; j <= 6; j += 2) {
    const u=j/8, x=ex*Math.sin(u*Math.PI*.5)-width*.45, y=ey*u;
    p.circle(x,y,1.2,C[5]);
  }
}

/** Analytic sampling keeps the same frame stable when Unity skips or seeks in the scoring timeline. */
export function draw(p, t, o) {
  if (t < 0 || t >= meta.duration) return;
  const C = fit(o.palette, 6), power = o.power, R = new Rng(o.seed);
  const strength = .52 + power * .48;
  if (t < .13) {
    const reach = (18 + 27 * (1 - Math.exp(-t * 80))) * strength;
    for (let side of [-1, 1]) for (let finger = 0; finger < (power < .4 ? 1 : 2); finger++) {
      if (reach < 2) continue;
      const x = (power < .4 ? 0 : finger === 0 ? -1 : 1) * (8 + power * 8) * (1 - Math.exp(-(t + .015) * 65));
      ribbon(p, x + (side < 0 ? -3 : 4), side * reach * (finger === 1 ? 1 : .70) * (side < 0 ? 1 : .78), (3.5 + power * 1.2) * (1 - smooth(.04,.10,t)), C);
    }
    if (t < .045) {
      const h = (10 + power * 5) * (1 - t / .06);
      p.ellipse(0,0,3,h,C[1]); p.ellipse(-.5,0,1.7,h-1,C[5]);
      for (let j=0;j<5;j++) p.circle((j%2?2:-2), (j-2)*3, 1.8, C[5]);
    }
  }
  // Water drops have drag, a rounded head, a dark lower rim and a small foam highlight.
  const drops = 4 + Math.round(power * 3);
  for (let i = 0; i < drops; i++) {
    const delay = R.range(.018,.065), life = R.range(.14,.24), vx = R.range(-90,90), vy = (i % 2 ? -1 : 1) * R.range(95,180);
    const size = R.range(1.2,2.6) * strength, u = t - delay;
    if (u < 0 || u >= life) continue;
    const drag = (1 - Math.exp(-u * 5)) / 5;
    const x = vx * drag, y = Math.sign(vy) * (10 + power * 5) + vy * drag + 70 * u * u, radius = size * (1 - smooth(life * .65, life, u));
    if (radius < .5) continue;
    p.beam(x - vx * .014, y - vy * .014, x, y, radius * 1.5, C[0]);
    p.circle(x,y,radius,C[2]); p.circle(x-.4,y-.5,radius*.67,C[4]); p.px(x-1,y-1,C[5]);
  }
  // Two or three hollow bubbles follow the fast spray rather than competing with its first frame.
  for (let i = 0; i < (power > .75 ? 3 : 2); i++) {
    const u = t - .09 - i * .018;
    if (u < 0 || u >= .22) continue;
    const x = (i % 2 ? -1 : 1) * (9 + i * 5) + Math.sin(u * 16 + i) * 2, y = -8 - i * 6 - u * 70;
    const r = (1.6 + i * .5) * (1 - smooth(.17,.22,u));
    if (r < .7) continue;
    const pts = Array.from({length: 13}, (_, k) => { const a = k / 12 * Math.PI * 2; return [x + Math.cos(a) * r, y + Math.sin(a) * r]; });
    p.polyline(pts, C[1]); p.line(x-r*.6,y-r*.7,x+.3,y-r,C[4]); p.px(x-1,y-r,C[5]);
  }
}
