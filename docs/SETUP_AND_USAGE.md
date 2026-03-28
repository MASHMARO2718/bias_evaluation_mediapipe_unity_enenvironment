# Setup & usage / セットアップと使い方

## English

### 1. Open the project

- Use Unity Editor version from **`ProjectSettings/ProjectVersion.txt`** (currently **6000.0.60f1**).
- Open **`Assets/Scenes/SampleScene.unity`** (adjust if your workflow uses another scene).

### 2. CaptureSystemManager inspector

Assign at minimum:

| Field | Purpose |
|-------|---------|
| Start button | Starts one capture run |
| Status text | Optional UI feedback |
| Character prefab | Humanoid with **Animator** |
| Animation clip | Walk (typically **Apply Root Motion**) |
| Capture camera | Renders to disk |
| Capture / End / Stop zones | Three **TriggerZone** objects with **BoxCollider.isTrigger** |
| **Capture output root** (`captureOutputRootPath`) | Optional. Empty = **`Output/`** next to `Assets`. Relative paths are from the **project root**; absolute paths (e.g. `D:\Captures`) are allowed. Same root is used for frames (JPG) and synced joint CSV. |

### 3. Play mode

1. Enter Play.
2. Click **Start**.
3. Find output under **`<captureOutputRootPath>/<subfolder>/`** (default: **`Output/<subfolder>/`** at repo root).

### 4. AutoCaptureManager (optional)

- Add component or use existing GameObject.
- Set **`csvFilePath`** to a path **relative to project root** (e.g. `data/cameras.csv`) or an **absolute** path.
- CSV columns (header required):

```text
X,Y,Z,Radius,FolderName
```

- **X, Y, Z:** Camera world position.
- **Radius:** Currently unused by loader (may be reserved); still present in files.
- **FolderName:** Logical name / subfolder key; camera **rotation** is computed as **look-at world origin (0,0,0)**.

Sample rows: [samples/camera_coordinates_sample.csv](samples/camera_coordinates_sample.csv).

### 5. CameraPositionController_TMP

- Binds **TMP_InputField** (X/Y/Z) to the transform; **LookAt** target defaults to origin if unset.
- Useful for manual tuning before exporting camera paths.

### 6. Troubleshooting

| Issue | Check |
|-------|--------|
| Triggers never fire | Layer matrix, **isTrigger**, **Player** tag, Rigidbody on character if needed |
| Empty output folder | Write permissions, disk space, `CaptureSystemManager` validation errors in Console |
| Wrong frame count vs CSV | Use **fixed** capture framerate; see `FrameCapturer.useFixedFrameRate` |

---

## 日本語

### 1. プロジェクトを開く

- **`ProjectSettings/ProjectVersion.txt`** と同じ Unity 6 エディタで開いてください。
- 通常は **`Assets/Scenes/SampleScene.unity`** を使用します。

### 2. CaptureSystemManager の割り当て

上表と同様に、ボタン・キャラPrefab・クリップ・カメラ・3つのトリガーを設定します。トリガーには **BoxCollider（Trigger）** が必要です。

- **`captureOutputRootPath`**: 空欄で **`Output/`**（`Assets` と並ぶ）。相対はプロジェクトルート基準、**絶対パス**も可。画像と同期関節CSVの共通ルート。

### 3. 実行

Play → スタートボタン → 上記ルート配下のサブフォルダに出力。

### 4. AutoCaptureManager（任意）

- **`csvFilePath`**: プロジェクトルートからの**相対**、または**絶対パス**。
- 形式は **1行目ヘッダー**、`X,Y,Z,Radius,FolderName`。回転は **原点を向く** ように自動計算されます。
- サンプル: [samples/camera_coordinates_sample.csv](samples/camera_coordinates_sample.csv)

### 5. トラブルシュート

- トリガーが反応しない → **タグ Player**、**Is Trigger**、レイヤー衝突、キャラに **Rigidbody** が要る場合あり。
- 出力がない → Console のエラー、フォルダ書き込み権限。

---

## MediaPipe / downstream (brief)

**EN:** Export **fixed FPS** sequences; resolution **1280×720** or **640×480** depending on your model. JPG is lossy — if artifacts hurt pose estimation, switch encoder to PNG in `FrameCapturer` and update docs.

**JP:** フレームレートは固定推奨。JPG の圧縮が推定に悪影響なら `FrameCapturer` で PNG に変更し、README を更新してください。
