using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// 複数箇所の自動撮影を管理するスクリプト
/// カメラ位置を自動で変更して連続撮影を実行
/// </summary>
public class AutoCaptureManager : MonoBehaviour
{
    [Header("Auto Capture Settings")]
    [Tooltip("CSVファイルのパス（プロジェクト直下）")]
    public string csvFilePath = "camera_coordinates_288.csv";

    [Tooltip("各撮影間の待機時間（秒）")]
    public float waitBetweenCaptures = 2f;

    [Tooltip("撮影開始ボタン")]
    public UnityEngine.UI.Button autoCaptureButton;

    [Tooltip("ステータス表示テキスト")]
    public UnityEngine.UI.Text statusText;

    [Header("References")]
    [Tooltip("撮影カメラ")]
    public Camera captureCamera;

    [Tooltip("CaptureSystemManager")]
    public CaptureSystemManager captureSystemManager;

    [Header("Status")]
    [SerializeField]
    private bool isAutoCapturing = false;

    [SerializeField]
    private int currentCaptureIndex = 0;

    [SerializeField]
    private int totalCaptures = 0;

    [System.Serializable]
    public class CameraPosition
    {
        [Tooltip("位置名（フォルダ名に使用）")]
        public string positionName;

        [Tooltip("カメラの位置")]
        public Vector3 position;

        [Tooltip("カメラの回転")]
        public Vector3 rotation;

        [Tooltip("この位置で撮影するか")]
        public bool enabled = true;
    }

    // CSVから読み込んだ座標データ
    private List<CameraPosition> loadedPositions = new List<CameraPosition>();

    private void Start()
    {
        // ボタンイベント設定
        if (autoCaptureButton != null)
        {
            autoCaptureButton.onClick.AddListener(OnAutoCaptureButtonClicked);
        }

        // CSVファイルから座標を読み込み
        LoadCameraPositionsFromCSV();

        totalCaptures = loadedPositions.Count;
        UpdateStatus($"待機中 - {totalCaptures}箇所の撮影準備完了");
    }

    /// <summary>
    /// CSVファイルからカメラ位置を読み込み
    /// </summary>
    private void LoadCameraPositionsFromCSV()
    {
        loadedPositions.Clear();

        try
        {
            // プロジェクト直下のCSVファイルパス
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string csvPath = Path.Combine(projectRoot, csvFilePath);

            if (!File.Exists(csvPath))
            {
                Debug.LogError($"[AutoCaptureManager] CSVファイルが見つかりません: {csvPath}", this);
                return;
            }

            string[] lines = File.ReadAllLines(csvPath);
            
            // ヘッダー行をスキップ（1行目）
            for (int i = 1; i < lines.Length; i++)
            {
                string[] values = lines[i].Split(',');
                
                if (values.Length >= 5)
                {
                    // X,Y,Z,Radius,FolderName
                    float x = float.Parse(values[0]);
                    float y = float.Parse(values[1]);
                    float z = float.Parse(values[2]);
                    string folderName = values[4];

                    // カメラをキャラクターの方向に向ける（原点(0,0,0)を向く）
                    Vector3 cameraPos = new Vector3(x, y, z);
                    Vector3 lookDirection = Vector3.zero - cameraPos;
                    Quaternion rotation = Quaternion.LookRotation(lookDirection);

                    loadedPositions.Add(new CameraPosition
                    {
                        positionName = folderName,
                        position = cameraPos,
                        rotation = rotation.eulerAngles,
                        enabled = true
                    });
                }
            }

            Debug.Log($"[AutoCaptureManager] CSVから {loadedPositions.Count} 箇所の撮影位置を読み込みました。", this);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[AutoCaptureManager] CSVファイルの読み込みに失敗しました: {e.Message}", this);
        }
    }

    /// <summary>
    /// 自動撮影ボタンが押された
    /// </summary>
    public void OnAutoCaptureButtonClicked()
    {
        if (isAutoCapturing)
        {
            Debug.LogWarning("[AutoCaptureManager] 既に自動撮影中です。", this);
            return;
        }

        if (loadedPositions.Count == 0)
        {
            Debug.LogError("[AutoCaptureManager] 撮影位置が読み込まれていません！", this);
            return;
        }

        StartCoroutine(AutoCaptureSequence());
    }

    /// <summary>
    /// 自動撮影シーケンス
    /// </summary>
    private IEnumerator AutoCaptureSequence()
    {
        isAutoCapturing = true;
        currentCaptureIndex = 0;

        var enabledPositions = loadedPositions.FindAll(p => p.enabled);
        totalCaptures = enabledPositions.Count;

        UpdateStatus($"自動撮影開始 - {totalCaptures}箇所");

        for (int i = 0; i < enabledPositions.Count; i++)
        {
            var position = enabledPositions[i];
            currentCaptureIndex = i + 1;

            UpdateStatus($"撮影中 ({currentCaptureIndex}/{totalCaptures}) - {position.positionName}");

            // カメラ位置を設定
            SetCameraPosition(position);

            // 少し待機（カメラ移動の安定化）
            yield return new WaitForSeconds(0.5f);

            // 撮影実行
            yield return StartCoroutine(ExecuteSingleCapture(position.positionName));

            // 撮影間の待機
            if (i < enabledPositions.Count - 1)
            {
                UpdateStatus($"待機中... ({waitBetweenCaptures}秒)");
                yield return new WaitForSeconds(waitBetweenCaptures);
            }
        }

        UpdateStatus($"自動撮影完了 - {totalCaptures}箇所すべて完了");
        isAutoCapturing = false;

        Debug.Log("[AutoCaptureManager] 自動撮影シーケンスが完了しました。", this);
    }

    /// <summary>
    /// カメラ位置を設定
    /// </summary>
    private void SetCameraPosition(CameraPosition position)
    {
        if (captureCamera == null)
        {
            Debug.LogError("[AutoCaptureManager] Capture Camera が設定されていません！", this);
            return;
        }

        captureCamera.transform.position = position.position;
        captureCamera.transform.rotation = Quaternion.Euler(position.rotation);

        Debug.Log($"[AutoCaptureManager] カメラ位置を設定: {position.positionName} - {position.position}", this);
    }

    /// <summary>
    /// 単一撮影を実行
    /// </summary>
    private IEnumerator ExecuteSingleCapture(string positionName)
    {
        // CaptureSystemManagerの撮影開始
        if (captureSystemManager != null)
        {
            captureSystemManager.OnStartButtonClicked();
        }

        // 撮影完了まで待機（トリガーゾーン通過を待つ）
        yield return new WaitUntil(() => !captureSystemManager.IsRunning());

        Debug.Log($"[AutoCaptureManager] {positionName} の撮影が完了しました。", this);
    }

    /// <summary>
    /// ステータス更新
    /// </summary>
    private void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log($"[AutoCaptureManager] {message}");
    }

    /// <summary>
    /// 自動撮影中かどうか
    /// </summary>
    public bool IsAutoCapturing()
    {
        return isAutoCapturing;
    }

    /// <summary>
    /// 現在の撮影インデックス
    /// </summary>
    public int GetCurrentCaptureIndex()
    {
        return currentCaptureIndex;
    }

    /// <summary>
    /// 総撮影数
    /// </summary>
    public int GetTotalCaptures()
    {
        return totalCaptures;
    }

    /// <summary>
    /// CSVファイルを再読み込み
    /// </summary>
    public void ReloadCameraPositions()
    {
        LoadCameraPositionsFromCSV();
        totalCaptures = loadedPositions.Count;
        UpdateStatus($"再読み込み完了 - {totalCaptures}箇所の撮影準備完了");
    }

    private void OnDestroy()
    {
        if (autoCaptureButton != null)
        {
            autoCaptureButton.onClick.RemoveListener(OnAutoCaptureButtonClicked);
        }
    }
}
