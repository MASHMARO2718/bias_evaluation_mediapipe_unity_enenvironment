# 288カメラの完全な座標リストを生成し、CSV形式で全て出力

radii = [3, 4, 5, 6]
ys = [1.0, 2.0]

coordinates = []

for r in radii:
    for x in range(-r, r + 1):
        for z in range(-r, r + 1):
            # 境界上のみ（正方形の輪郭）
            if abs(x) == r or abs(z) == r:
                for y in ys:
                    coordinates.append((x, y, z))

# 結果の表示
print(f"総カメラ数: {len(coordinates)}")
print("\n=== 半径別の内訳 ===")
for r in radii:
    count_y1 = sum(1 for x, y, z in coordinates if max(abs(x), abs(z)) == r and y == 1.0)
    count_y2 = sum(1 for x, y, z in coordinates if max(abs(x), abs(z)) == r and y == 2.0)
    print(f"r={r}: Y=1で{count_y1}点, Y=2で{count_y2}点 (計{count_y1+count_y2}点)")

print("\n=== Y別の内訳 ===")
y1_coords = [(x, y, z) for x, y, z in coordinates if y == 1.0]
y2_coords = [(x, y, z) for x, y, z in coordinates if y == 2.0]
print(f"S₁ (Y=1): {len(y1_coords)}点")
print(f"S₂ (Y=2): {len(y2_coords)}点")

# 完全なリストをCSV形式で全て出力
print("\n" + "="*80)
print("=== 全288座標のCSV完全リスト ===")
print("="*80)
print("X,Y,Z,Radius,FolderName")

for x, y, z in sorted(coordinates):
    r = max(abs(x), abs(z))
    folder_name = f"CapturedFrames_{float(x):.1f}_{float(y):.1f}_{float(z):.1f}"
    print(f"{x},{y},{z},{r},{folder_name}")

print("="*80)
print(f"合計: {len(coordinates)}行")

# ファイルに保存する場合
save_to_file = True  # Falseにすると保存しない
if save_to_file:
    with open("camera_coordinates_288.csv", "w", encoding="utf-8") as f:
        f.write("X,Y,Z,Radius,FolderName\n")
        for x, y, z in sorted(coordinates):
            r = max(abs(x), abs(z))
            folder_name = f"CapturedFrames_{float(x):.1f}_{float(y):.1f}_{float(z):.1f}"
            f.write(f"{x},{y},{z},{r},{folder_name}\n")
    print(f"\nCSVファイルとして 'camera_coordinates_288.csv' に保存しました。")