# Unity Trigger-Zone Walk Capture / トリガーゾーン歩行キャプチャ

**English** | [日本語は下へ](#概要-日本語)

## Overview (English)

Unity project for **recording a humanoid character walking through invisible trigger zones** while saving **synchronized camera frames (JPG)** and **per-frame joint positions (CSV)**. Designed as a front-end capture pipeline for tools such as **MediaPipe** pose estimation.

Core idea: **spatial, event-driven capture** instead of fragile frame-by-frame synchronization code. Three `TriggerZone` colliders mark *start capture*, *end capture*, and *stop / cleanup*.

An optional **`AutoCaptureManager`** reads camera positions from a **CSV** at the project root and runs the capture sequence repeatedly for multi-view datasets.

---

## Requirements

| Item | Version / notes |
|------|------------------|
| Unity Editor | **6000.0.60f1** (Unity 6) — see `ProjectSettings/ProjectVersion.txt` |
| Render pipeline | **URP** (`com.unity.render-pipelines.universal` 17.x) |
| Character | Humanoid **Animator** + **Root Motion** walk clip (see scene setup) |

---

## Repository layout (main scripts)

| Path | Role |
|------|------|
| `Assets/Scripts/Core/CaptureSystemManager.cs` | Orchestrates UI, spawn, triggers, `FrameCapturer`, `SyncedJointRecorder` |
| `Assets/Scripts/Capture/CapturePathUtility.cs` | Resolves output root and input file paths |
| `Assets/Scripts/Capture/FrameCapturer.cs` | Renders camera to texture; saves `frame_XXXX.jpg` under `{outputRoot}/{folder}/` |
| `Assets/Scripts/Capture/SyncedJointRecorder.cs` | Joint CSV aligned with captured frames |
| `Assets/Scripts/Capture/AutoCaptureManager.cs` | Batch runs from CSV camera path list |
| `Assets/Scripts/Triggers/TriggerZone.cs` | Trigger volumes + `UnityEvent<GameObject>` |
| `Assets/Scripts/Character/CharacterAnimationController.cs` | Plays clip, root motion |
| `Assets/Scripts/camera/camera_transform.cs` | TMP UI → camera position / look-at (class: `CameraPositionController_TMP`) |

Concept note: `Assets/Scripts/Triggers/Trigger Zone Capture System.md` (design memo; some early names differ from final class names).

---

## Quick start

1. Open the project in **Unity 6** (matching `ProjectVersion.txt`).
2. Open **`Assets/Scenes/SampleScene.unity`** (or your configured scene).
3. Ensure **CaptureSystemManager** references are set: character prefab, animation clip, capture camera, three trigger zones, UI.
4. Character instance must be tag **`Player`** (set automatically on spawn by `CaptureSystemManager`).
5. Press **Play**, then the **start** button. By default, output goes to **`Output/`** next to `Assets` (see `.gitignore`). Set **`captureOutputRootPath`** on `CaptureSystemManager` to use another folder (relative to project root or absolute).

**Auto multi-view:** Set **`csvFilePath`** on `AutoCaptureManager` (relative to project root or absolute path). See [docs/SETUP_AND_USAGE.md](docs/SETUP_AND_USAGE.md) and [docs/samples/camera_coordinates_sample.csv](docs/samples/camera_coordinates_sample.csv).

---

## Documentation

- [docs/SETUP_AND_USAGE.md](docs/SETUP_AND_USAGE.md) — Setup, CSV format, output layout (EN + JP)
- [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) — Component flow and data paths (EN + JP)
- [docs/REFACTORING_PLAN.md](docs/REFACTORING_PLAN.md) — Roadmap before / after GitHub publication (EN + JP)

---

## Third-party assets / redistribution

This repo may include **Unity packages** (e.g. TextMesh Pro) and **store or sample assets** (e.g. under `Assets/npc_casual_set_00/`). **Do not redistribute** licensed assets unless your license allows it. For a minimal public repo, keep only your scripts and replace characters with a CC0 / your own humanoid.

---

## License

Specify your license in a `LICENSE` file. Code authored in this repository can be MIT or your choice; **bundled assets follow their own terms**.

---

<a id="概要-日本語"></a>

## 概要（日本語）

Unity で **ヒューマノイドがトリガーゾーンを通過する区間だけ** カメラ連写し、**フレーム画像（JPG）** と **関節座標（CSV）** を同期して書き出すプロジェクトです。**MediaPipe** などの姿勢推定パイプラインの入力用として想定しています。

考え方は **「空間イベントで撮影を開始・終了する」** 方式で、フレーム単位の複雑な同期より調整とデバッグがしやすくなります。任意で **`AutoCaptureManager`** により、**CSV（パスは相対または絶対で指定）** で定義した複数カメラ位置から連続バッチ撮影できます。出力先は **`captureOutputRootPath`** で変更できます。

---

## 要件（日本語）

- **Unity 6**（`ProjectSettings/ProjectVersion.txt` のエディタ版に合わせる）
- **URP**
- **Humanoid Animator** と **Root Motion** 歩行クリップ（シーンで割り当て）

---

## ドキュメント（日本語）

上記と同じファイルが **英語・日本語の両方** で書いてあります。

- [セットアップと使い方](docs/SETUP_AND_USAGE.md)
- [アーキテクチャ](docs/ARCHITECTURE.md)
- [リファクタリング計画](docs/REFACTORING_PLAN.md)

---

## アセット公開時の注意（日本語）

**Asset Store やサードパーティのモデル** をそのまま GitHub に載せないでください。ライセンス上、スクリプトのみ公開しキャラクターは差し替える構成が安全です。
