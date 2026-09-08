import os
from PIL import Image, ImageFilter, ImageEnhance

def process_and_clean_cell(cell, col, row, threshold=30, contrast_boost=1.3):
    cell = cell.convert("RGBA")
    width, height = cell.size
    
    # Estimate background from corners
    edge_pixels = []
    for x in range(width):
        edge_pixels.append(cell.getpixel((x, 0)))
        edge_pixels.append(cell.getpixel((x, height - 1)))
    for y in range(height):
        edge_pixels.append(cell.getpixel((0, y)))
        edge_pixels.append(cell.getpixel((width - 1, y)))
        
    bg_r = sum(p[0] for p in edge_pixels) // len(edge_pixels)
    bg_g = sum(p[1] for p in edge_pixels) // len(edge_pixels)
    bg_b = sum(p[2] for p in edge_pixels) // len(edge_pixels)
    
    # Step 1: Screen blend de-multiplication
    datas = cell.getdata()
    new_data = []
    for item in datas:
        r, g, b, a = item
        fr = max(0, r - bg_r)
        fg = max(0, g - bg_g)
        fb = max(0, b - bg_b)
        
        max_val = max(fr, fg, fb)
        if max_val < threshold:
            new_data.append((0, 0, 0, 0))
        else:
            alpha_f = max_val / 255.0
            alpha_f = min(1.0, alpha_f * contrast_boost)
            alpha = int(255 * alpha_f)
            
            denom = max(0.05, alpha_f)
            ur = min(255, int(fr / denom))
            ug = min(255, int(fg / denom))
            ub = min(255, int(fb / denom))
            new_data.append((ur, ug, ub, alpha))
            
    cell.putdata(new_data)
    pixels = cell.load()
    
    # Step 2: Use the exact mathematically verified frame coordinates
    x_left_map = [53, 44, 36]
    x_right_map = [305, 297, 288]
    y_top_map = [54, 44, 34]
    y_bottom_map = [307, 296, 286]
    
    xl = x_left_map[col]
    xr = x_right_map[col]
    yt = y_top_map[row]
    yb = y_bottom_map[row]
    
    # Erase a 6px wide band around the frame boundaries to disconnect them
    for x in range(width):
        for y in range(height):
            # Left boundary band
            if xl - 3 <= x <= xl + 3:
                pixels[x, y] = (0, 0, 0, 0)
            # Right boundary band
            if xr - 3 <= x <= xr + 3:
                pixels[x, y] = (0, 0, 0, 0)
            # Top boundary band
            if yt - 3 <= y <= yt + 3:
                pixels[x, y] = (0, 0, 0, 0)
            # Bottom boundary band
            if yb - 3 <= y <= yb + 3:
                pixels[x, y] = (0, 0, 0, 0)
                
    # Step 3: Connected Component Semantic Cleaning
    visited = [[False for _ in range(height)] for _ in range(width)]
    components = []
    
    for x in range(width):
        for y in range(height):
            if pixels[x, y][3] > 0 and not visited[x][y]:
                comp = []
                queue = [(x, y)]
                visited[x][y] = True
                head = 0
                while head < len(queue):
                    cx, cy = queue[head]
                    head += 1
                    comp.append((cx, cy))
                    for dx in [-1, 0, 1]:
                        for dy in [-1, 0, 1]:
                            if dx == 0 and dy == 0:
                                continue
                            nx, ny = cx + dx, cy + dy
                            if 0 <= nx < width and 0 <= ny < height:
                                if pixels[nx, ny][3] > 0 and not visited[nx][ny]:
                                    visited[nx][ny] = True
                                    queue.append((nx, ny))
                components.append(comp)
                
    # Filter out remaining frame/corner/text components
    for comp in components:
        xs = [p[0] for p in comp]
        ys = [p[1] for p in comp]
        min_x, max_x = min(xs), max(xs)
        min_y, max_y = min(ys), max(ys)
        w_c = max_x - min_x
        h_c = max_y - min_y
        
        should_delete = False
        
        # 1. Text labels at the bottom (starts at yb + 8)
        if min_y >= yb + 8:
            should_delete = True
        # 2. General outer gridlines
        elif min_x <= 15 or max_x >= 326 or min_y <= 15 or max_y >= 326:
            should_delete = True
        # 3. Top-left corner decorations (relative to xl, yt)
        elif max_x <= xl + 80 and max_y <= yt + 80:
            should_delete = True
        # 4. Bottom-left corner decorations (relative to xl, yb)
        elif max_x <= xl + 80 and min_y >= yb - 80:
            should_delete = True
        # 5. Top-right corner decorations (relative to xr, yt)
        elif min_x >= xr - 80 and max_y <= yt + 80:
            should_delete = True
        # 6. Bottom-right corner decorations (relative to xr, yb)
        elif min_x >= xr - 80 and min_y >= yb - 80:
            should_delete = True
            
        if should_delete:
            for cx, cy in comp:
                pixels[cx, cy] = (0, 0, 0, 0)
                
    # Step 4: Bounding box re-trimming and auto-centering
    bbox = cell.getbbox()
    if bbox:
        cropped = cell.crop(bbox)
        w_cr, h_cr = cropped.size
        new_size = max(w_cr, h_cr) + 30
        padded = Image.new("RGBA", (new_size, new_size), (0, 0, 0, 0))
        padded.paste(cropped, ((new_size - w_cr) // 2, (new_size - h_cr) // 2))
        cell = padded.resize((128, 128), Image.Resampling.LANCZOS)
    else:
        return Image.new("RGBA", (128, 128), (0, 0, 0, 0))
        
    # Step 5: Color boosting for gold lines
    pixels = cell.load()
    w_cl, h_cl = cell.size
    for x in range(w_cl):
        for y in range(h_cl):
            pr, pg, pb, pa = pixels[x, y]
            if pa > 0:
                if pr > 120 and pr > pb * 1.5:
                    nr = min(255, int(pr * 1.15))
                    ng = min(255, int(pg * 1.05))
                    nb = int(pb * 0.8)
                    pixels[x, y] = (nr, ng, nb, pa)
                    
    enhancer = ImageEnhance.Color(cell)
    cell = enhancer.enhance(1.3)
    
    # Step 6: Add charcoal outline (3px width, 1px blur)
    alpha = cell.split()[-1]
    outline_color = (11, 15, 23, 240)
    outline_base = Image.new("RGBA", cell.size, outline_color)
    outline = Image.composite(outline_base, Image.new("RGBA", cell.size, (0, 0, 0, 0)), alpha)
    outline = outline.filter(ImageFilter.MaxFilter(3))
    outline = outline.filter(ImageFilter.GaussianBlur(1))
    
    final_img = Image.alpha_composite(outline, cell)
    return final_img

def main():
    image_path = "D:/ALL/zijianyouxi/XXXV/xiuxian-game/ui_line_icons.jpg"
    output_dir = "D:/ALL/zijianyouxi/XXXV/xiuxian-game/src/icons_new"
    
    if not os.path.exists(output_dir):
        os.makedirs(output_dir)
        
    try:
        img = Image.open(image_path)
    except Exception as e:
        print(f"Failed to open image: {e}")
        return
        
    width, height = img.size
    cell_w = width // 3
    cell_h = height // 3
    
    names = [
        "ui_compass", "ui_calendar", "ui_mail",
        "ui_quest", "ui_rank", "ui_book",
        "ui_chat", "ui_setting", "ui_guild"
    ]
    
    idx = 0
    for r in range(3):
        for c in range(3):
            left = c * cell_w
            top = r * cell_h
            right = (c + 1) * cell_w
            bottom = (r + 1) * cell_h
            
            cell_img = img.crop((left, top, right, bottom))
            processed = process_and_clean_cell(cell_img, c, r)
            
            name = names[idx]
            out_path = os.path.join(output_dir, f"{name}.png")
            processed.save(out_path, "PNG")
            
            final_bbox = processed.getbbox()
            print(f"Processed UI Perfected: {name}.png, BBox: {final_bbox}")
            idx += 1

if __name__ == "__main__":
    main()
