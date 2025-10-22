using UnityEngine;
using System.IO;
using System.Collections;

/// <summary>
/// カメラの映像を連続撮影してPNG形式で保存するクラス
/// </summary>
public class FrameCapturer : MonoBehaviour
{
    [Header("Capture Settings")]
    [Tooltip("撮影するカメラ")]
    public Camera targetCamera;

    [Tooltip("撮影画像の幅")]
    public int captureWidth = 1280;

    [Tooltip("撮影画像の高さ")]
    public int captureHeight = 720;

    [Tooltip("目標フレームレート")]
    public int targetFrameRate = 30;

    [Tooltip("固定フレームレート撮影を使用（Time.captureFramerate）")]
    public bool useFixedFrameRate = true;

    [Header("Output Settings")]
    [Tooltip("出力フォルダ名（Assets/Output/配下）")]
    public string outputFolderName = "CapturedFrames";

    [Header("Status")]
    [SerializeField]
    private bool isCapturing = false;

    [SerializeField]
    private int capturedFrameCount = 0;

    private string outputPath;
    private RenderTexture renderTexture;
    private Texture2D texture2D;

    private void Awake()
    {
        // RenderTexture と Texture2D を作成
        renderTexture = new RenderTexture(captureWidth, captureHeight, 24);
        texture2D = new Texture2D(captureWidth, captureHeight, TextureFormat.RGB24, false);
    }

    /// <summary>
    /// 撮影を開始
    /// </summary>
    public void StartCapture()
    {
        if (targetCamera == null)
        {
            Debug.LogError("[FrameCapturer] Target Camera が設定されていません！", this);
            return;
        }

        // 出力パスを設定（撮影開始時に毎回更新）
        outputPath = Path.Combine(Application.dataPath, "Output", outputFolderName);

        // 出力フォルダを作成
        if (!Directory.Exists(outputPath))
        {
            try
            {
                Directory.CreateDirectory(outputPath);
                Debug.Log($"[FrameCapturer] 出力フォルダを作成しました: {outputPath}", this);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[FrameCapturer] 出力フォルダの作成に失敗しました: {e.Message}", this);
                return;
            }
        }

        // 既存のフレームをクリア（オプション：コメントアウトで上書き防止）
        ClearPreviousFrames();

        // 固定フレームレート設定
        if (useFixedFrameRate)
        {
            Time.captureFramerate = targetFrameRate;
            Debug.Log($"[FrameCapturer] 固定フレームレート {targetFrameRate} FPS を設定しました。", this);
        }

        isCapturing = true;
        capturedFrameCount = 0;

        // 撮影コルーチンを開始
        StartCoroutine(CaptureFramesCoroutine());

        Debug.Log("[FrameCapturer] 撮影を開始しました。", this);
    }

    /// <summary>
    /// 撮影を停止
    /// </summary>
    public void StopCapture()
    {
        if (!isCapturing) return;

        isCapturing = false;

        // 固定フレームレートを解除
        if (useFixedFrameRate)
        {
            Time.captureFramerate = 0;
            Debug.Log("[FrameCapturer] 固定フレームレートを解除しました。", this);
        }

        StopAllCoroutines();

        Debug.Log($"[FrameCapturer] 撮影を終了しました。総フレーム数: {capturedFrameCount}", this);
        Debug.Log($"[FrameCapturer] 保存先: {outputPath}", this);
    }

    /// <summary>
    /// 撮影コルーチン
    /// </summary>
    private IEnumerator CaptureFramesCoroutine()
    {
        while (isCapturing)
        {
            // フレーム終了まで待機
            yield return new WaitForEndOfFrame();

            // 1フレームをキャプチャ
            CaptureFrame();
        }
    }

    /// <summary>
    /// 1フレームをキャプチャして保存
    /// </summary>
    private void CaptureFrame()
    {
        // カメラの描画先を RenderTexture に設定
        RenderTexture currentRT = RenderTexture.active;
        targetCamera.targetTexture = renderTexture;
        targetCamera.Render();

        // RenderTexture から Texture2D に読み込み
        RenderTexture.active = renderTexture;
        texture2D.ReadPixels(new Rect(0, 0, captureWidth, captureHeight), 0, 0);
        texture2D.Apply();

        // 元に戻す
        targetCamera.targetTexture = null;
        RenderTexture.active = currentRT;

        // PNG として保存
        byte[] bytes = texture2D.EncodeToPNG();
        string filename = $"frame_{capturedFrameCount:D4}.png";
        string filepath = Path.Combine(outputPath, filename);

        try
        {
            File.WriteAllBytes(filepath, bytes);
            capturedFrameCount++;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FrameCapturer] フレームの保存に失敗しました: {e.Message}", this);
            StopCapture();
        }
    }

    /// <summary>
    /// 以前のフレームをクリア
    /// </summary>
    private void ClearPreviousFrames()
    {
        if (Directory.Exists(outputPath))
        {
            string[] files = Directory.GetFiles(outputPath, "frame_*.png");
            foreach (string file in files)
            {
                try
                {
                    File.Delete(file);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[FrameCapturer] ファイル削除に失敗: {file} - {e.Message}", this);
                }
            }
            Debug.Log($"[FrameCapturer] 以前のフレーム {files.Length} 個を削除しました。", this);
        }
    }

    /// <summary>
    /// 撮影中かどうか
    /// </summary>
    public bool IsCapturing()
    {
        return isCapturing;
    }

    /// <summary>
    /// 撮影フレーム数を取得
    /// </summary>
    public int GetCapturedFrameCount()
    {
        return capturedFrameCount;
    }

    private void OnDestroy()
    {
        // リソースのクリーンアップ
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }

        if (texture2D != null)
        {
            Destroy(texture2D);
        }

        // 固定フレームレートを解除
        Time.captureFramerate = 0;
    }
}

