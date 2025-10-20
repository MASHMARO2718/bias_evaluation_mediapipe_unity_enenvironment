using UnityEngine;

/// <summary>
/// キャラクターを指定方向に移動させるシンプルなコンポーネント
/// アニメーションが「その場歩き」の場合に、実際の移動を担当する
/// </summary>
public class CharacterMover : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("移動速度（m/s）")]
    public float moveSpeed = 2.0f;

    [Tooltip("移動方向（通常はVector3.forward）")]
    public Vector3 moveDirection = Vector3.forward;

    [Tooltip("移動を有効にするか")]
    public bool isMoving = false;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (!isMoving) return;

        if (rb != null)
        {
            // Rigidbodyがある場合は物理演算を使用
            Vector3 velocity = transform.TransformDirection(moveDirection.normalized) * moveSpeed;
            rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);
        }
        else
        {
            // Rigidbodyがない場合はTransformを直接移動
            transform.position += transform.TransformDirection(moveDirection.normalized) * moveSpeed * Time.fixedDeltaTime;
        }
    }

    /// <summary>
    /// 移動を開始
    /// </summary>
    public void StartMoving()
    {
        isMoving = true;
        Debug.Log($"[CharacterMover] 移動開始: 速度={moveSpeed} m/s, 方向={moveDirection}", this);
    }

    /// <summary>
    /// 移動を停止
    /// </summary>
    public void StopMoving()
    {
        isMoving = false;
        
        // Rigidbodyの速度をリセット
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }
        
        Debug.Log("[CharacterMover] 移動停止", this);
    }

    /// <summary>
    /// 移動速度を設定
    /// </summary>
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }

    /// <summary>
    /// 移動方向を設定
    /// </summary>
    public void SetMoveDirection(Vector3 direction)
    {
        moveDirection = direction;
    }
}

