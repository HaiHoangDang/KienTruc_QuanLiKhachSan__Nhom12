import sys
sys.stdout.reconfigure(encoding='utf-8')
from pathlib import Path
lines = Path('Controllers/HotelController.cs').read_text(encoding='utf-8').splitlines()
for idx, line in enumerate(lines, 1):
    if 600 <= idx <= 780:
        print(f'{idx:04d}: {line}')
