using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;

/// <summary>
/// FrameCapturerと完全同期して関節座標を記録するクラス
/// カメラ撮影のタイミングと完全に一致する座標データを取得
/// </summary>
public class SyncedJointRecorder : MonoBehaviour
{
    [Header("Recording Settings")]
    [Tooltip("記録対象のキャラクターのAnimator")]
    public Animator targetAnimator;

    [Tooltip("出力ルート。空欄=プロジェクト直下の Output。FrameCapturer と揃えること")]
    public string outputRootPath = "";

    [Tooltip("出力ルート直下のサブフォルダ名")]
    public string outputFolderName = "CapturedFrames";

    [Tooltip("出力CSVファイル名（記録開始時に自動でタイムスタンプ付きに変更されます）")]
    public string csvFileName = "synced_joint_positions.csv";

    [Header("Status")]
    [SerializeField]
    private bool isRecording = false;

    [SerializeField]
    private int recordedFrameCount = 0;

    // 記録データ
    private List<string> csvLines = new List<string>();
    private HumanBodyBones[] bones;
    private string outputPath;

    // FrameCapturerとの同期用
    private FrameCapturer frameCapturer;
    private int lastCapturedFrameCount = 0;

    private void Awake()
    {
        // 記録対象のボーン一覧を取得
        bones = (HumanBodyBones[])System.Enum.GetValues(typeof(HumanBodyBones));
        
        // FrameCapturerを取得（非推奨警告を回避）
        frameCapturer = FindFirstObjectByType<FrameCapturer>();
        if (frameCapturer == null)
        {
            Debug.LogError("[SyncedJointRecorder] FrameCapturer が見つかりません！", this);
        }
    }

    /// <summary>
    /// 記録を開始
    /// </summary>
    public void StartRecording()
    {
        if (targetAnimator == null)
        {
            Debug.LogError("[SyncedJointRecorder] Target Animator が設定されていません！", this);
            return;
        }

        if (frameCapturer == null)
        {
            Debug.LogError("[SyncedJointRecorder] FrameCapturer が設定されていません！", this);
            return;
        }

        string root = CapturePathUtility.ResolveOutputRoot(outputRootPath);
        string timestampedFileName = $"synced_joint_positions_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv";
        outputPath = Path.Combine(root, outputFolderName, timestampedFileName);

        // データをクリア
        csvLines.Clear();
        recordedFrameCount = 0;
        lastCapturedFrameCount = 0;

        // CSVヘッダーを作成
        CreateCSVHeader();

        isRecording = true;
        Debug.Log("[SyncedJointRecorder] 同期関節座標の記録を開始しました。", this);
    }

    /// <summary>
    /// 記録を停止してCSVを保存
    /// </summary>
    public void StopRecording()
    {
        if (!isRecording) return;

        isRecording = false;

        // CSVファイルを保存
        SaveCSV();

        Debug.Log($"[SyncedJointRecorder] 記録を終了しました。総フレーム数: {recordedFrameCount}", this);
        Debug.Log($"[SyncedJointRecorder] 保存先: {outputPath}", this);
    }

    private void Update()
    {
        if (!isRecording) return;
        if (targetAnimator == null) return;
        if (frameCapturer == null) return;

        // FrameCapturerの撮影フレーム数をチェック
        int currentCapturedFrameCount = frameCapturer.GetCapturedFrameCount();
        
        // 新しいフレームが撮影された場合のみ記録
        if (currentCapturedFrameCount > lastCapturedFrameCount)
        {
            // 撮影されたフレーム数分だけ記録
            for (int i = lastCapturedFrameCount; i < currentCapturedFrameCount; i++)
            {
                RecordFrame(i);
            }
            
            lastCapturedFrameCount = currentCapturedFrameCount;
        }
    }

    /// <summary>
    /// CSVヘッダーを作成
    /// </summary>
    private void CreateCSVHeader()
    {
        StringBuilder header = new StringBuilder();
        header.Append("Frame,Time");

        // 各ボーンのX,Y,Z座標カラムを追加
        foreach (HumanBodyBones bone in bones)
        {
            if (bone == HumanBodyBones.LastBone) continue; // LastBoneはスキップ

            string boneName = bone.ToString();
            header.Append($",{boneName}_X,{boneName}_Y,{boneName}_Z");
        }

        csvLines.Add(header.ToString());
    }

    /// <summary>
    /// 指定フレーム番号の関節座標を記録
    /// </summary>
    private void RecordFrame(int frameNumber)
    {
        StringBuilder line = new StringBuilder();

        // フレーム番号と時間
        line.Append($"{frameNumber},{Time.time:F3}");

        // 各ボーンの座標を取得
        foreach (HumanBodyBones bone in bones)
        {
            if (bone == HumanBodyBones.LastBone) continue;

            Transform boneTransform = targetAnimator.GetBoneTransform(bone);

            if (boneTransform != null)
            {
                // ワールド座標を記録
                Vector3 position = boneTransform.position;
                line.Append($",{position.x:F6},{position.y:F6},{position.z:F6}");
            }
            else
            {
                // ボーンが存在しない場合は空欄
                line.Append(",,,");
            }
        }

        csvLines.Add(line.ToString());
        recordedFrameCount++;
    }

    /// <summary>
    /// CSVファイルを保存
    /// </summary>
    private void SaveCSV()
    {
        try
        {
            // 出力ディレクトリが存在しない場合は作成
            string directory = Path.GetDirectoryName(outputPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // CSVファイルを書き込み
            File.WriteAllLines(outputPath, csvLines, Encoding.UTF8);

            Debug.Log($"[SyncedJointRecorder] CSVファイルを保存しました: {outputPath}", this);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SyncedJointRecorder] CSVファイルの保存に失敗しました: {e.Message}", this);
        }
    }

    /// <summary>
    /// 記録中かどうか
    /// </summary>
    public bool IsRecording()
    {
        return isRecording;
    }

    /// <summary>
    /// 記録フレーム数を取得
    /// </summary>
    public int GetRecordedFrameCount()
    {
        return recordedFrameCount;
    }

    /// <summary>
    /// フレーム数と座標データ数の一致を確認
    /// </summary>
    public void ValidateSync()
    {
        if (frameCapturer == null) return;

        int capturedFrames = frameCapturer.GetCapturedFrameCount();
        int recordedFrames = GetRecordedFrameCount();

        Debug.Log($"[SyncedJointRecorder] 同期確認 - 撮影フレーム数: {capturedFrames}, 記録フレーム数: {recordedFrames}");
        
        if (capturedFrames == recordedFrames)
        {
            Debug.Log("[SyncedJointRecorder] ✅ 完全同期しています！", this);
        }
        else
        {
            Debug.LogWarning($"[SyncedJointRecorder] ⚠️ 同期にずれがあります。差: {Mathf.Abs(capturedFrames - recordedFrames)}", this);
        }
    }

    private void OnDestroy()
    {
        // 記録中なら保存
        if (isRecording)
        {
            StopRecording();
        }
    }
}

