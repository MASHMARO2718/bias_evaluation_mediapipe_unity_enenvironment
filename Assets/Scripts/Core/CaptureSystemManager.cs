using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Trigger Zone Capture System の統括マネージャー
/// 全体の流れを制御し、各コンポーネントを連携させる
/// </summary>
public class CaptureSystemManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("撮影開始ボタン")]
    public Button startButton;

    [Tooltip("ステータス表示用テキスト")]
    public Text statusText;

    [Header("Character Setup")]
    [Tooltip("生成するキャラクターのPrefab")]
    public GameObject characterPrefab;

    [Tooltip("再生するアニメーションクリップ（差し替え可能）")]
    public AnimationClip animationClip;

    [Tooltip("キャラクターの初期生成位置")]
    public Vector3 spawnPosition = new Vector3(0, 0, -5);

    [Header("Animation Settings")]
    [Tooltip("アニメーション再生速度（1.0 = 通常速度）")]
    public float animationSpeed = 1.0f;

    [Header("Capture Settings")]
    [Tooltip("撮影に使用するカメラ")]
    public Camera captureCamera;

    [Tooltip("撮影画像の幅")]
    public int captureWidth = 1280;

    [Tooltip("撮影画像の高さ")]
    public int captureHeight = 720;

    [Tooltip("目標フレームレート")]
    public int targetFrameRate = 30;

    [Tooltip("固定フレームレート撮影を使用")]
    public bool useFixedFrameRate = true;

    [Tooltip("出力フォルダ名")]
    public string outputFolderName = "CapturedFrames";

    [Header("Trigger Zones")]
    [Tooltip("撮影開始トリガー（Z=-3）")]
    public TriggerZone captureStartZone;

    [Tooltip("撮影終了トリガー（Z=3）")]
    public TriggerZone captureEndZone;

    [Tooltip("全停止トリガー（Z=5）")]
    public TriggerZone stopZone;

    // 内部状態
    private GameObject currentCharacter;
    private CharacterAnimationController animationController;
    private FrameCapturer frameCapturer;
    private JointRecorder jointRecorder;
    private bool isRunning = false;

    private void Start()
    {
        // UIボタンの設定
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }
        else
        {
            Debug.LogError("[CaptureSystemManager] Start Button が設定されていません！", this);
        }

        // トリガーゾーンのイベント設定
        SetupTriggerZones();

        // FrameCapturerの初期化
        InitializeFrameCapturer();

        // JointRecorderの初期化
        InitializeJointRecorder();

        // ステータス更新
        UpdateStatus("待機中");
    }

    /// <summary>
    /// トリガーゾーンのイベント設定
    /// </summary>
    private void SetupTriggerZones()
    {
        if (captureStartZone != null)
        {
            captureStartZone.onZoneEntered.AddListener(OnCaptureStartZoneEntered);
        }
        else
        {
            Debug.LogError("[CaptureSystemManager] Capture Start Zone が設定されていません！", this);
        }

        if (captureEndZone != null)
        {
            captureEndZone.onZoneEntered.AddListener(OnCaptureEndZoneEntered);
        }
        else
        {
            Debug.LogError("[CaptureSystemManager] Capture End Zone が設定されていません！", this);
        }

        if (stopZone != null)
        {
            stopZone.onZoneEntered.AddListener(OnStopZoneEntered);
        }
        else
        {
            Debug.LogError("[CaptureSystemManager] Stop Zone が設定されていません！", this);
        }
    }

    /// <summary>
    /// FrameCapturerの初期化
    /// </summary>
    private void InitializeFrameCapturer()
    {
        frameCapturer = gameObject.GetComponent<FrameCapturer>();
        if (frameCapturer == null)
        {
            frameCapturer = gameObject.AddComponent<FrameCapturer>();
        }

        // 設定を適用
        frameCapturer.targetCamera = captureCamera;
        frameCapturer.captureWidth = captureWidth;
        frameCapturer.captureHeight = captureHeight;
        frameCapturer.targetFrameRate = targetFrameRate;
        frameCapturer.useFixedFrameRate = useFixedFrameRate;
        frameCapturer.outputFolderName = outputFolderName;
    }

    /// <summary>
    /// JointRecorderの初期化
    /// </summary>
    private void InitializeJointRecorder()
    {
        jointRecorder = gameObject.GetComponent<JointRecorder>();
        if (jointRecorder == null)
        {
            jointRecorder = gameObject.AddComponent<JointRecorder>();
        }

        // 設定を適用
        jointRecorder.outputFolderName = outputFolderName;
        jointRecorder.recordFrameRate = targetFrameRate;
    }

    /// <summary>
    /// カメラ座標に基づいて出力フォルダ名を設定
    /// </summary>
    private void SetupOutputFolderWithCameraPosition()
    {
        if (captureCamera == null)
        {
            Debug.LogWarning("[CaptureSystemManager] Capture Camera が設定されていないため、デフォルトのフォルダ名を使用します。", this);
            return;
        }

        // カメラの座標を取得
        Vector3 cameraPos = captureCamera.transform.position;

        // フォルダ名を生成（小数点1桁まで）
        string folderName = $"CapturedFrames_{cameraPos.x:F1}_{cameraPos.y:F1}_{cameraPos.z:F1}";

        // FrameCapturer と JointRecorder に設定
        if (frameCapturer != null)
        {
            frameCapturer.outputFolderName = folderName;
        }

        if (jointRecorder != null)
        {
            jointRecorder.outputFolderName = folderName;
        }

        Debug.Log($"[CaptureSystemManager] 出力フォルダ名を設定しました: {folderName}", this);
    }

    /// <summary>
    /// スタートボタンが押されたときの処理
    /// </summary>
    public void OnStartButtonClicked()
    {
        if (isRunning)
        {
            Debug.LogWarning("[CaptureSystemManager] 既に実行中です。", this);
            return;
        }

        // 検証
        if (!ValidateSettings())
        {
            return;
        }

        // 実行開始
        StartCaptureSequence();
    }

    /// <summary>
    /// 設定の検証
    /// </summary>
    private bool ValidateSettings()
    {
        bool isValid = true;

        if (characterPrefab == null)
        {
            Debug.LogError("[CaptureSystemManager] Character Prefab が設定されていません！", this);
            isValid = false;
        }

        if (animationClip == null)
        {
            Debug.LogError("[CaptureSystemManager] Animation Clip が設定されていません！", this);
            isValid = false;
        }

        if (captureCamera == null)
        {
            Debug.LogError("[CaptureSystemManager] Capture Camera が設定されていません！", this);
            isValid = false;
        }

        if (captureStartZone == null || captureEndZone == null || stopZone == null)
        {
            Debug.LogError("[CaptureSystemManager] いずれかの Trigger Zone が設定されていません！", this);
            isValid = false;
        }

        return isValid;
    }

    /// <summary>
    /// 撮影シーケンスを開始
    /// </summary>
    private void StartCaptureSequence()
    {
        isRunning = true;
        UpdateStatus("キャラクター生成中...");

        // カメラ座標に基づいて出力フォルダ名を設定
        SetupOutputFolderWithCameraPosition();

        // キャラクターを生成
        SpawnCharacter();

        // アニメーションを開始（Root Motionで自動的に移動する）
        StartAnimation();

        UpdateStatus("移動中（撮影待機）");
    }

    /// <summary>
    /// キャラクターを生成
    /// </summary>
    private void SpawnCharacter()
    {
        currentCharacter = Instantiate(characterPrefab, spawnPosition, Quaternion.identity);
        currentCharacter.name = "Character_Instance";

        // Playerタグを設定（トリガー検知に必要）
        if (!currentCharacter.CompareTag("Player"))
        {
            currentCharacter.tag = "Player";
            Debug.Log("[CaptureSystemManager] キャラクターに Player タグを設定しました。", this);
        }

        // コンポーネントを取得または追加
        animationController = currentCharacter.GetComponent<CharacterAnimationController>();
        if (animationController == null)
        {
            animationController = currentCharacter.AddComponent<CharacterAnimationController>();
        }

        // 設定を適用
        animationController.animationClip = animationClip;
        animationController.animationSpeed = animationSpeed;

        // JointRecorder の対象を設定
        if (jointRecorder != null)
        {
            Animator animator = currentCharacter.GetComponent<Animator>();
            if (animator != null)
            {
                jointRecorder.targetAnimator = animator;
            }
        }

        Debug.Log($"[CaptureSystemManager] キャラクターを生成しました: {spawnPosition}", this);
    }

    /// <summary>
    /// アニメーションを開始
    /// </summary>
    private void StartAnimation()
    {
        if (animationController != null)
        {
            animationController.PlayAnimation();
        }
    }


    /// <summary>
    /// 撮影開始ゾーンに入った
    /// </summary>
    private void OnCaptureStartZoneEntered(GameObject character)
    {
        if (!isRunning) return;

        Debug.Log("[CaptureSystemManager] 撮影開始ゾーンに到達しました。", this);
        UpdateStatus("撮影中...");

        // 撮影開始
        if (frameCapturer != null)
        {
            frameCapturer.StartCapture();
        }

        // 関節記録開始
        if (jointRecorder != null)
        {
            jointRecorder.StartRecording();
        }

        // ステータス更新コルーチン開始
        StartCoroutine(UpdateCaptureStatus());
    }

    /// <summary>
    /// 撮影終了ゾーンに入った
    /// </summary>
    private void OnCaptureEndZoneEntered(GameObject character)
    {
        if (!isRunning) return;

        Debug.Log("[CaptureSystemManager] 撮影終了ゾーンに到達しました。", this);

        // 撮影停止
        if (frameCapturer != null)
        {
            frameCapturer.StopCapture();
        }

        // 関節記録停止
        if (jointRecorder != null)
        {
            jointRecorder.StopRecording();
        }

        // ステータス更新コルーチン停止
        StopCoroutine(UpdateCaptureStatus());

        UpdateStatus($"撮影完了 - {frameCapturer.GetCapturedFrameCount()} フレーム");
    }

    /// <summary>
    /// 停止ゾーンに入った
    /// </summary>
    private void OnStopZoneEntered(GameObject character)
    {
        if (!isRunning) return;

        Debug.Log("[CaptureSystemManager] 停止ゾーンに到達しました。", this);

        // すべて停止
        StopSequence();
    }

    /// <summary>
    /// シーケンス全体を停止
    /// </summary>
    private void StopSequence()
    {
        // アニメーション停止
        if (animationController != null)
        {
            animationController.StopAnimation();
        }

        // 念のため撮影も停止
        if (frameCapturer != null && frameCapturer.IsCapturing())
        {
            frameCapturer.StopCapture();
        }

        // 関節記録も停止
        if (jointRecorder != null && jointRecorder.IsRecording())
        {
            jointRecorder.StopRecording();
        }

        // キャラクター削除
        if (currentCharacter != null)
        {
            Destroy(currentCharacter);
            currentCharacter = null;
        }

        // トリガーゾーンをリセット（次回のために）
        ResetTriggerZones();

        isRunning = false;
        UpdateStatus("完了 - 待機中");

        Debug.Log("[CaptureSystemManager] シーケンスが完了しました。", this);
    }

    /// <summary>
    /// トリガーゾーンをリセット
    /// </summary>
    private void ResetTriggerZones()
    {
        if (captureStartZone != null)
        {
            captureStartZone.ResetTrigger();
        }

        if (captureEndZone != null)
        {
            captureEndZone.ResetTrigger();
        }

        if (stopZone != null)
        {
            stopZone.ResetTrigger();
        }

        Debug.Log("[CaptureSystemManager] トリガーゾーンをリセットしました。", this);
    }

    /// <summary>
    /// 撮影中のステータス更新コルーチン
    /// </summary>
    private System.Collections.IEnumerator UpdateCaptureStatus()
    {
        while (frameCapturer != null && frameCapturer.IsCapturing())
        {
            int frameCount = frameCapturer.GetCapturedFrameCount();
            UpdateStatus($"撮影中... {frameCount} フレーム");
            yield return new WaitForSeconds(0.1f);
        }
    }

    /// <summary>
    /// ステータステキストを更新
    /// </summary>
    private void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log($"[Status] {message}");
    }

    private void OnDestroy()
    {
        // クリーンアップ
        if (currentCharacter != null)
        {
            Destroy(currentCharacter);
        }

        if (startButton != null)
        {
            startButton.onClick.RemoveListener(OnStartButtonClicked);
        }
    }
}

