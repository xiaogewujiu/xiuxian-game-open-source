# ☯ 修仙游戏图标生成与净化处理指南 (Xiuxian Icon Generation & Crop Guide)

本指南详细记录了修仙游戏全套“极简发光线条图标”的 **AI 生成提示词模版** 以及 **Python 图像切割与净化处理脚本**。
当您（或未来的 AI 编码助手）需要为游戏新增图标时，只需遵循此指南即可完美产出风格一致的极简透明发光线条图标。

---

## 🎨 1. AI 绘图提示词配方 (Prompt Formula)

为保持游戏内图标风格的完全统一，请使用以下提示词模版调用 Gemini / Imagen (或 Midjourney) 生成 3x3 的九宫格图：

### 📝 统一提示词模版：
> **A 3x3 grid sheet of 9 minimalist [主题英文] icons for a Chinese cultivation (xiuxian) game. Pure black background, clean line art style. Icons include: [物件1英文描述], [物件2英文描述], [物件3英文描述], [物件4英文描述], [物件5英文描述], [物件6英文描述], [物件7英文描述], [物件8英文描述], and [物件9英文描述]. Simple gold and cyan line art, black background.**

### 💡 核心设计参数：
* **宽高比 (AspectRatio)**: `1:1`
* **主色调 (Colors)**: `gold and cyan` (琥珀金与灵能蓝线条)
* **背景 (Background)**: `pure black background` (纯黑背景，便于算法完美抠图)
* **排版 (Layout)**: `3x3 grid sheet of 9` (标准的 9 宫格排版)
* **描边风格 (Style)**: `clean line art style`, `minimalist` (极简线条画风格，无实心厚重色块，无渐变立体阴影)

---

## 🛠️ 2. 图像切割与自动净化脚本 (Python Processing Script)

大图生成后，我们需要将 3x3 九宫格的网格线、多余的角落装饰框、底部的英文字符完全滤除，并将中心线条图标提取为透明背景、带深色描边的游戏用 `128x128` PNG 图片。

请在项目根目录下创建一个 `crop_icons.py` 脚本，将以下内容写入其中并运行：

```python
import os
import sys
from PIL import Image, ImageFilter, ImageEnhance

def process_and_clean_cell(cell, col, row, threshold=30, contrast_boost=1.3):
    """
    对单个九宫格区域进行背景擦除、去杂线、自动裁剪、发光强化以及描边处理
    """
    cell = cell.convert("RGBA")
    width, height = cell.size
    
    # 1. 估算四角背景色 (实现屏幕混合去底)
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
    
    # 2. 去色底并生成 Alpha 通道
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
    
    # 3. 边界和角边装饰框坐标定位 (用于判定并擦除装饰线)
    x_left_map = [38, 29, 20]
    x_right_map = [319, 313, 304]
    y_top_map = [38, 29, 20]
    y_bottom_map = [319, 312, 303]
    
    xl = x_left_map[col]
    xr = x_right_map[col]
    yt = y_top_map[row]
    yb = y_bottom_map[row]
    
    # 4. 连通域检测与语义清洗 (滤除网格线、花纹边框、底部英文字体)
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
                            if dx == 0 and dy == 0: continue
                            nx, ny = cx + dx, cy + dy
                            if 0 <= nx < width and 0 <= ny < height:
                                if pixels[nx, ny][3] > 0 and not visited[nx][ny]:
                                    visited[nx][ny] = True
                                    queue.append((nx, ny))
                components.append(comp)
                
    for comp in components:
        xs = [p[0] for p in comp]
        ys = [p[1] for p in comp]
        min_x, max_x = min(xs), max(xs)
        min_y, max_y = min(ys), max(ys)
        w_c = max_x - min_x
        h_c = max_y - min_y
        
        should_delete = False
        
        # A. 底部英文文本标签
        if min_y >= yb + 8:
            should_delete = True
        # B. 外层总大图框线
        elif min_x <= 15 or max_x >= 326 or min_y <= 15 or max_y >= 326:
            should_delete = True
        # C. 局部的单元网格线/装饰框
        elif w_c > 230 and h_c > 230 and min_x <= xl + 5 and max_x >= xr - 5:
            should_delete = True
        # D. 四角符文与角落小框
        elif max_x <= xl + 80 and max_y <= yt + 80:
            should_delete = True
        elif max_x <= xl + 80 and min_y >= yb - 80:
            should_delete = True
        elif min_x >= xr - 80 and max_y <= yt + 80:
            should_delete = True
        elif min_x >= xr - 80 and min_y >= yb - 80:
            should_delete = True
            
        if should_delete:
            for cx, cy in comp:
                pixels[cx, cy] = (0, 0, 0, 0)
                
    # 5. 自动包围框裁切与居中重构
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
        
    # 6. 二次保险：擦除缩放后在边缘残留的碎点杂色
    pixels_cl = cell.load()
    w_cl, h_cl = cell.size
    visited_cl = [[False for _ in range(h_cl)] for _ in range(w_cl)]
    components_cl = []
    
    for x in range(w_cl):
        for y in range(h_cl):
            if pixels_cl[x, y][3] > 0 and not visited_cl[x][y]:
                comp = []
                queue = [(x, y)]
                visited_cl[x][y] = True
                head = 0
                while head < len(queue):
                    cx, cy = queue[head]
                    head += 1
                    comp.append((cx, cy))
                    for dx in [-1, 0, 1]:
                        for dy in [-1, 0, 1]:
                            if dx == 0 and dy == 0: continue
                            nx, ny = cx + dx, cy + dy
                            if 0 <= nx < w_cl and 0 <= ny < h_cl:
                                if pixels_cl[nx, ny][3] > 0 and not visited_cl[nx][ny]:
                                    visited_cl[nx][ny] = True
                                    queue.append((nx, ny))
                components_cl.append(comp)
                
    for comp in components_cl:
        xs = [p[0] for p in comp]
        ys = [p[1] for p in comp]
        min_x, max_x = min(xs), max(xs)
        min_y, max_y = min(ys), max(ys)
        if len(comp) < 500:
            if min_x <= 15 or max_x >= 112 or min_y <= 15 or max_y >= 112:
                for cx, cy in comp:
                    pixels_cl[cx, cy] = (0, 0, 0, 0)
                    
    # 7. 琥珀金发光线条色彩饱和度增强
    pixels_cl = cell.load()
    for x in range(w_cl):
        for y in range(h_cl):
            pr, pg, pb, pa = pixels_cl[x, y]
            if pa > 0:
                if pr > 120 and pr > pb * 1.5:
                    nr = min(255, int(pr * 1.15))
                    ng = min(255, int(pg * 1.05))
                    nb = int(pb * 0.8)
                    pixels_cl[x, y] = (nr, ng, nb, pa)
                    
    enhancer = ImageEnhance.Color(cell)
    cell = enhancer.enhance(1.3)
    
    # 8. 叠加深黑保护性外描边 (防止线条在亮色背景下看不清)
    alpha = cell.split()[-1]
    outline_color = (11, 15, 23, 240)
    outline_base = Image.new("RGBA", cell.size, outline_color)
    outline = Image.composite(outline_base, Image.new("RGBA", cell.size, (0, 0, 0, 0)), alpha)
    outline = outline.filter(ImageFilter.MaxFilter(3))
    outline = outline.filter(ImageFilter.GaussianBlur(1))
    
    final_img = Image.alpha_composite(outline, cell)
    return final_img

def main():
    if len(sys.argv) < 3:
        print("用法: python crop_icons.py [3x3原图路径] [输出文件夹] [图标名称1] [图标名称2] ...")
        print("例如: python crop_icons.py sheet.jpg ./src/icons_new item_01 item_02 ...")
        return
        
    image_path = sys.argv[1]
    output_dir = sys.argv[2]
    names = sys.argv[3:]
    
    if not os.path.exists(output_dir):
        os.makedirs(output_dir)
        
    img = Image.open(image_path)
    width, height = img.size
    cell_w = width // 3
    cell_h = height // 3
    
    idx = 0
    for r in range(3):
        for c in range(3):
            left = c * cell_w
            top = r * cell_h
            right = (c + 1) * cell_w
            bottom = (r + 1) * cell_h
            
            cell_img = img.crop((left, top, right, bottom))
            processed = process_and_clean_cell(cell_img, c, r)
            
            name = names[idx] if idx < len(names) else f"icon_{idx+1}"
            out_path = os.path.join(output_dir, f"{name}.png")
            processed.save(out_path, "PNG")
            print(f"✅ 处理完成: {out_path}")
            idx += 1

if __name__ == "__main__":
    main()
```

---

## 📈 3. 新建与注册图标流程

当您想在游戏中增设一批新图标（例如 9 个新法宝）时，流程如下：

1. **第一步**：将上面第一节的**提示词模版**稍作修改，让 AI 产出包含这 9 个法宝的 3x3 纯黑底大图（命名为 `magic_treasure_sheet.jpg`）。
2. **第二步**：将大图放入项目目录，并运行命令行脚本剪切大图：
   ```bash
   python crop_icons.py magic_treasure_sheet.jpg ./src/icons_new fan_01 fan_02 fan_03 fan_04 fan_05 fan_06 fan_07 fan_08 fan_09
   ```
3. **第三步**：打开 [src/icons/index.js](file:///D:/ALL/zijianyouxi/XXXV/xiuxian-game/src/icons/index.js) 注册您的新图标：
   ```javascript
   import fan_01 from '../icons_new/fan_01.png'
   // ... 导入并放入 export 注册表即可
   ```
4. **第四步**：在 Vue 组件中直接使用 `<AssetIcon :source="ICON.fan_01" />` 完美渲染。
