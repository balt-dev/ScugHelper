import math
import numpy as np
from PIL import Image

WIDTH: int = 96
HEIGHT: int = 96
RADIUS: int = 48
FRAMES: int = 120
WOBBLE_STRENGTH: float = 8
DISTANCE_WOBBLE: float = 2

def main():
    def lerped(strength: float) -> float:
        if (strength < 0.5):
            return 1 - lerped(1 - strength)
        norm_strength: float = strength * 2 - 1
        if norm_strength == 1:
            return 1
        return 1 - 2 ** (-5 ** norm_strength)
    
    def get_pixel_color(x: int, y: int, t: float) -> tuple[int, int, int]:
        x_dist = x - WIDTH / 2
        y_dist = y - HEIGHT / 2
        distance: float = (x_dist ** 2 + y_dist ** 2) ** 0.5
        if (distance == 0):
            return (0x80, 0x80, 0xFF)
        strength = max(0, RADIUS - distance) / RADIUS
        x_angle = x_dist / distance
        y_angle = y_dist / distance
        wobble = lerped((math.sin(distance / DISTANCE_WOBBLE - t * 2 * math.pi) + 1) / 2) * WOBBLE_STRENGTH
        x_offset = x_angle * wobble
        y_offset = y_angle * wobble
        return (0x80 + int(x_offset), 0x80 + int(y_offset), int(0xFF * strength))
    
    buf = np.zeros((HEIGHT, WIDTH, 3), dtype = np.uint8)
    for t in range(0, FRAMES):
        for y in range(0, HEIGHT):
            for x in range(0, WIDTH):
                buf[y, x] = get_pixel_color(x, y, t / FRAMES)
        print(f"\r{(t+1)/FRAMES*100:3.0f}%", end="")
        Image.fromarray(buf, "RGB").save(f"distort{t:03}.png")
    print()

if __name__ == "__main__":
    main()