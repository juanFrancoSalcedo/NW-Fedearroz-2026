from PIL import Image, ImageDraw, ImageFont
import numpy as np
import os

OUT = r'C:\Users\USER\Documents\Newrona\Repositories\FedeArroz 2026\U.Fedearroz2026\Assets\Art\2D\UI\Holobox\Generated'
FONT = r'C:\Users\USER\Documents\Newrona\Repositories\FedeArroz 2026\U.Fedearroz2026\Assets\Art\Fonts\Roboto Static\Roboto-Regular.ttf'

os.makedirs(OUT, exist_ok=True)

# ---------------- HALO BLANCO ----------------
# 840x840, centro (540,940) del mockup => rect (120,520)
S = 840
W = S
img = Image.new('RGBA', (W, W), (0, 0, 0, 0))
px = np.zeros((W, W, 4), dtype=np.float64)
c = W / 2.0
yy, xx = np.mgrid[0:W, 0:W]
d = np.sqrt((xx - c + 0.5) ** 2 + (yy - c + 0.5) ** 2)

alpha = np.ones_like(d)
hard = 340.0
soft = 358.0
tail_scale = 26.0
tail_amp = 0.38

fade = np.clip((soft - d) / (soft - hard), 0.0, 1.0)
tail = tail_amp * np.exp(-np.maximum(d - soft, 0) / tail_scale)
alpha = np.where(d <= hard, 1.0, np.where(d <= soft, fade, tail))
alpha = np.clip(alpha, 0.0, 1.0)

px[..., 0] = 254
px[..., 1] = 249
px[..., 2] = 227
px[..., 3] = alpha * 255
Image.fromarray(px.astype(np.uint8), 'RGBA').save(os.path.join(OUT, 'Halo Beneficio.png'))
print('halo ok')

# ---------------- TERMOMETRO ----------------
# region del mockup: x 896..968 (72), y 494..722 (228)
OX, OY, OW, OH = 896, 494, 72, 228
K = 4
tw, th = OW * K, OH * K
timg = Image.new('RGBA', (tw, th), (0, 0, 0, 0))
dr = ImageDraw.Draw(timg)

WHITE = (255, 255, 255, 255)
GREEN = (96, 203, 84, 255)


def rect(x0, y0, x1, y1, color, r):
    dr.rounded_rectangle([(x0 - OX) * K, (y0 - OY) * K, (x1 - OX) * K, (y1 - OY) * K], radius=r * K, fill=color)


def circle(cx, cy, rad, color):
    dr.ellipse([(cx - OX - rad) * K, (cy - OY - rad) * K, (cx - OX + rad) * K, (cy - OY + rad) * K], fill=color)


# cuerpo blanco
rect(910, 496, 937, 660, WHITE, 13.5)
circle(923.5, 691, 28, WHITE)
# relleno verde
rect(917, 613, 930, 691, GREEN, 6.5)
circle(923.5, 691, 21, GREEN)

# ticks y numeros
top_y = 663.0
bottom_y = 525.0
font = ImageFont.truetype(FONT, 8 * K)
for v in range(0, 71, 2):
    y = top_y - (top_y - bottom_y) * (v / 70.0)
    long = (v % 10 == 0)
    x0 = 939 if long else 941
    x1 = 951 if long else 946
    dr.rectangle([(x0 - OX) * K, (y - OY) * K - K // 2, (x1 - OX) * K, (y - OY) * K + K // 2], fill=WHITE)

for v in range(0, 71, 10):
    y = top_y - (top_y - bottom_y) * (v / 70.0)
    dr.text(((954 - OX) * K, (y - OY) * K), str(v), font=font, fill=WHITE, anchor='lm')

timg = timg.resize((OW, OH), Image.LANCZOS)
timg.save(os.path.join(OUT, 'Termometro.png'))
print('thermometer ok', timg.size)

# ---------------- RADAR SOLO LINEAS (para el detalle) ----------------
radar = Image.open(r'C:\Users\USER\Documents\Newrona\Repositories\FedeArroz 2026\U.Fedearroz2026\Assets\Art\2D\UI\Holobox\Radar.png').convert('RGBA')
ra = np.asarray(radar).copy()
Hh, Ww = ra.shape[:2]
cxx, cyy = (Ww - 1) / 2.0, (Hh - 1) / 2.0
yyg, xxg = np.mgrid[0:Hh, 0:Ww]
rr = np.sqrt((xxg - cxx) ** 2 + (yyg - cyy) ** 2)
Gm = ra[..., 1].astype(int)
Rm = ra[..., 0].astype(int)
Bm = ra[..., 2].astype(int)
keep = (rr >= 344) | ((rr >= 282) & (rr <= 294) & (Gm - Bm > 40))
ra[~keep, 3] = 0
Image.fromarray(ra, 'RGBA').save(os.path.join(OUT, 'Radar Detalle.png'))
print('radar detalle ok')
