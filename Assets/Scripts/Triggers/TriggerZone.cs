using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// トリガーゾーン（壁）の検知を行うクラス
/// キャラクターが通過したときにイベントを発火する
/// </summary>
public class TriggerZone : MonoBehaviour
{
    /// <summary>
    /// トリガーゾーンのタイプ
    /// </summary>
    public enum ZoneType
    {
        CaptureStart,  // 撮影開始
        CaptureEnd,    // 撮影終了
        Stop           // 全停止
    }

    [Header("Zone Settings")]
    [Tooltip("このトリガーゾーンの役割")]
    public ZoneType zoneType = ZoneType.CaptureStart;

    [Header("Debug Visualization")]
    [Tooltip("Scene Viewでギズモを表示するか")]
    public bool showGizmo = true;

    [Tooltip("ギズモの色")]
    public Color gizmoColor = Color.green;

    [Header("Events")]
    [Tooltip("トリガーに入ったときに発火するイベント")]
    public UnityEvent<GameObject> onZoneEntered = new UnityEvent<GameObject>();

    private BoxCollider triggerCollider;
    private bool hasTriggered = false;

    private void Awake()
    {
        // Box Colliderの検証
        triggerCollider = GetComponent<BoxCollider>();
        if (triggerCollider == null)
        {
            Debug.LogError($"[TriggerZone] {gameObject.name} に BoxCollider が見つかりません！", this);
            return;
        }

        if (!triggerCollider.isTrigger)
        {
            Debug.LogWarning($"[TriggerZone] {gameObject.name} の BoxCollider が Trigger になっていません。自動で設定します。", this);
            triggerCollider.isTrigger = true;
        }

        // タイプに応じてデフォルトの色を設定
        if (gizmoColor == Color.clear || gizmoColor == Color.white)
        {
            switch (zoneType)
            {
                case ZoneType.CaptureStart:
                    gizmoColor = Color.green;
                    break;
                case ZoneType.CaptureEnd:
                    gizmoColor = Color.yellow;
                    break;
                case ZoneType.Stop:
                    gizmoColor = Color.red;
                    break;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Playerタグのオブジェクトのみ反応
        if (other.CompareTag("Player"))
        {
            // 既にトリガー済みなら無視
            if (hasTriggered)
            {
                return;
            }

            hasTriggered = true;
            Debug.Log($"[TriggerZone] {zoneType} がトリガーされました: {other.gameObject.name}", this);
            onZoneEntered.Invoke(other.gameObject);
        }
    }

    /// <summary>
    /// トリガーをリセット（次のキャラクターのために）
    /// </summary>
    public void ResetTrigger()
    {
        hasTriggered = false;
    }

    // Scene Viewでギズモを描画（デバッグ用）
    private void OnDrawGizmos()
    {
        if (!showGizmo) return;

        // 色を設定（半透明）
        Color color = gizmoColor;
        color.a = 0.3f;
        Gizmos.color = color;

        // Box Colliderのサイズと位置を取得
        BoxCollider col = GetComponent<BoxCollider>();
        if (col != null)
        {
            // ワールド座標でボックスを描画
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(col.center, col.size);

            // 枠線を描画（見やすくするため）
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireCube(col.center, col.size);
            Gizmos.matrix = oldMatrix;
        }
        else
        {
            // Colliderがない場合は位置にマーカーを表示
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }

    // 選択時に常にギズモを表示
    private void OnDrawGizmosSelected()
    {
        if (!showGizmo) return;

        // 選択時は不透明で表示
        Gizmos.color = gizmoColor;

        BoxCollider col = GetComponent<BoxCollider>();
        if (col != null)
        {
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(col.center, col.size);
            Gizmos.matrix = oldMatrix;
        }
    }
}

