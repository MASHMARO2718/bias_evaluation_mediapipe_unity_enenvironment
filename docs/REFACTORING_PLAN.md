# Refactoring plan / リファクタリング計画

Bilingual notes for preparing this repository for **GitHub** and long-term maintenance.

---

## Goals / 目的

**EN:** Improve clarity, testability, and contributor onboarding without breaking existing scenes in one giant step.

**JP:** 既存シーンを一度に壊さず、分かりやすさ・テスト容易性・コントリビュートしやすさを上げる。

---

## Phase 0 — Hygiene (done or quick wins) / 衛生（済・すぐできること）

| Item | EN | JP |
|------|----|----|
| Ignore capture output | Add `/Output/` to `.gitignore` so run artifacts are not committed. | 実行生成物をコミットしないよう `/Output/` を無視。 |
| Docs | README + `docs/*` + sample CSV. | README・ドキュメント・CSV サンプル。 |
| Comments vs behavior | Align summaries (e.g. JPG vs PNG). | コメントと実装の一致（例: JPG）。 |
| Null safety | Guard `AutoCaptureManager` when `CaptureSystemManager` is missing. | `CaptureSystemManager` 未設定時のガード。 |

---

## Phase 1 — Naming & folder layout / 命名とフォルダ

| Item | EN | JP |
|------|----|----|
| Script file vs class | `camera_transform.cs` contains `CameraPositionController_TMP` — rename file to match class (update Unity meta if needed). | ファイル名とクラス名を一致（リネーム時は .meta に注意）。 |
| Folder casing | Prefer `Camera` over `camera` for consistency on case-sensitive OS. | 大文字小文字の統一（Linux/mac での混乱防止）。 |
| Design doc | Keep `Trigger Zone Capture System.md` but ensure headings use **`CaptureSystemManager`**, not legacy placeholder names. | 設計メモのクラス名を実装に合わせる。 |

---

## Phase 2 — Assembly definitions / アセンブリ定義

| Item | EN | JP |
|------|----|----|
| `asmdef` | Add e.g. `Assets/Scripts/Unity1019.Capture.asmdef` referencing UGUI/TMP as needed — **exclude** third-party folders. | 自前スクリプトだけ `asmdef` でまとめ、再コンパイル範囲を縮小。 |
| Namespaces | Introduce `Unity1019.Capture`, `Unity1019.Triggers`, etc., gradually. | 名前空間を段階的に導入。 |

---

## Phase 3 — Parsing & paths / パースとパス

| Item | EN | JP |
|------|----|----|
| CSV | Use `float.Parse(..., CultureInfo.InvariantCulture)` for locale-independent runs. | `InvariantCulture` で数値パース（ロケール差の防止）。 |
| CSV module | Extract a small `CameraPathCsvLoader` class (testable, single responsibility). | CSV 読み込みを専用クラスに分離。 |
| Paths | Optional `ICapturePaths` or static config for project root / `Output` to ease tests. | 出力パスを抽象化しテストしやすくする。 |

---

## Phase 4 — Dead code & duplication / 未使用と重複

| Item | EN | JP |
|------|----|----|
| `JointRecorder` | If only `SyncedJointRecorder` is used in scene, document or remove unused component. | シーン未使用なら削除または README に「レガシー」と明記。 |
| Status UI | `UpdateStatus` is duplicated between managers — optional shared helper or small UI service class. | ステータス表示の共通化は任意。 |

---

## Phase 5 — Runtime robustness / 実行時の堅牢性

| Item | EN | JP |
|------|----|----|
| Coroutines | `StopAllCoroutines` in one component can affect others on same GameObject — consider scoped coroutine hosts or `StopCoroutine` handles. | `StopAllCoroutines` の影響範囲に注意。 |
| `FindFirstObjectByType` | `SyncedJointRecorder` uses it in `Awake` — prefer inspector injection from `CaptureSystemManager`. | 可能なら参照は Inspector / 注入に統一。 |

---

## Phase 6 — GitHub checklist / GitHub 公開チェックリスト

- [ ] Add explicit **LICENSE** for your own code.
- [ ] Remove or replace **non-redistributable** assets; list what remains in README.
- [ ] Add **`.gitattributes`** if you need LF normalization for team.
- [ ] Optional: GitHub **Issues** templates for bugs vs feature requests.
- [ ] Optional: **Editor** menu item to validate scene references (missing refs report).

---

## Suggested order / 推奨順序

1. Phase 0 → 1 (low risk)  
2. Phase 3 (CSV + culture) before sharing datasets across machines  
3. Phase 2 when multiple contributors touch scripts  
4. Phase 4–5 when adding tests or new capture modes  

This plan is intentionally incremental so `SampleScene` keeps working between steps.
