using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;

/// <summary>
/// キャラクターの全関節座標をCSVに記録するクラス
/// </summary>
public class JointRecorder : MonoBehaviour
{
    [Header("Recording Settings")]
    [Tooltip("記録対象のキャラクターのAnimator")]
    public Animator targetAnimator;

    [Tooltip("出力フォルダ名")]
    public string outputFolderName = "CapturedFrames";

    [Tooltip("出力CSVファイル名（記録開始時に自動でタイムスタンプ付きに変更されます）")]
    public string csvFileName = "joint_positions.csv";

    [Tooltip("記録するフレームレート（0で毎フレーム）")]
    public int recordFrameRate = 30;

    [Header("Status")]
    [SerializeField]
    private bool isRecording = false;

    [SerializeField]
    private int recordedFrameCount = 0;

    // 記録データ
    private List<string> csvLines = new List<string>();
    private HumanBodyBones[] bones;
    private float recordInterval;
    private float lastRecordTime;
    private string outputPath;

    private void Awake()
    {
        // 記録間隔を計算
        recordInterval = recordFrameRate > 0 ? 1f / recordFrameRate : 0f;

        // 記録対象のボーン一覧を取得
        bones = (HumanBodyBones[])System.Enum.GetValues(typeof(HumanBodyBones));
    }

    /// <summary>
    /// 記録を開始
    /// </summary>
    public void StartRecording()
    {
        if (targetAnimator == null)
        {
            Debug.LogError("[JointRecorder] Target Animator が設定されていません！", this);
            return;
        }

        // 出力パスを設定（記録開始時に毎回新しいファイル名を生成）
        string timestampedFileName = $"joint_positions_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv";
        outputPath = Path.Combine(Application.dataPath, "Output", outputFolderName, timestampedFileName);

        // データをクリア
        csvLines.Clear();
        recordedFrameCount = 0;
        lastRecordTime = Time.time;

        // CSVヘッダーを作成
        CreateCSVHeader();

        isRecording = true;
        Debug.Log("[JointRecorder] 関節座標の記録を開始しました。", this);
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

        Debug.Log($"[JointRecorder] 記録を終了しました。総フレーム数: {recordedFrameCount}", this);
        Debug.Log($"[JointRecorder] 保存先: {outputPath}", this);
    }

    private void Update()
    {
        if (!isRecording) return;
        if (targetAnimator == null) return;

        // 記録間隔をチェック
        if (recordInterval > 0 && Time.time - lastRecordTime < recordInterval)
        {
            return;
        }

        // 関節座標を記録
        RecordFrame();
        lastRecordTime = Time.time;
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
    /// 1フレーム分の関節座標を記録
    /// </summary>
    private void RecordFrame()
    {
        StringBuilder line = new StringBuilder();

        // フレーム番号と時間
        line.Append($"{recordedFrameCount},{Time.time:F3}");

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

            Debug.Log($"[JointRecorder] CSVファイルを保存しました: {outputPath}", this);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[JointRecorder] CSVファイルの保存に失敗しました: {e.Message}", this);
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

    private void OnDestroy()
    {
        // 記録中なら保存
        if (isRecording)
        {
            StopRecording();
        }
    }
}
