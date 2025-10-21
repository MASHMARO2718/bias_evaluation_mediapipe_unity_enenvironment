using UnityEngine;

/// <summary>
/// キャラクターのアニメーション制御クラス
/// Animator コンポーネントを使用してシンプルに実装
/// </summary>
public class CharacterAnimationController : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("再生するアニメーションクリップ（Inspector で差し替え可能）")]
    public AnimationClip animationClip;

    [Tooltip("アニメーション再生速度（1.0 = 通常速度）")]
    [Range(0.1f, 3.0f)]
    public float animationSpeed = 1.0f;

    private Animator animator;
    private bool isPlaying = false;

    private void Awake()
    {
        // Animator コンポーネントを取得
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("[CharacterAnimationController] Animator コンポーネントが見つかりません！Prefabに追加してください。", this);
        }
    }

    /// <summary>
    /// アニメーションを再生開始
    /// </summary>
    public void PlayAnimation()
    {
        if (animationClip == null)
        {
            Debug.LogError("[CharacterAnimationController] AnimationClip が設定されていません！", this);
            return;
        }

        if (animator == null)
        {
            Debug.LogError("[CharacterAnimationController] Animator コンポーネントが見つかりません！", this);
            return;
        }

        // Root Motionを有効化
        animator.applyRootMotion = true;
        
        // 速度設定
        animator.speed = animationSpeed;

        // アニメーションを再生（AnimatorのデフォルトステートがWalkingになっている想定）
        animator.enabled = true;
        isPlaying = true;

        Debug.Log($"[CharacterAnimationController] アニメーション再生開始。Apply Root Motion: {animator.applyRootMotion}", this);
    }

    /// <summary>
    /// アニメーションを停止
    /// </summary>
    public void StopAnimation()
    {
        if (animator != null && isPlaying)
        {
            animator.enabled = false;
            isPlaying = false;
            Debug.Log("[CharacterAnimationController] アニメーションを停止しました。", this);
        }
    }

    /// <summary>
    /// アニメーション速度を変更
    /// </summary>
    public void SetAnimationSpeed(float speed)
    {
        animationSpeed = Mathf.Clamp(speed, 0.1f, 3.0f);
        
        if (animator != null)
        {
            animator.speed = animationSpeed;
        }
    }

    /// <summary>
    /// 再生中かどうか
    /// </summary>
    public bool IsPlaying()
    {
        return isPlaying && animator != null && animator.enabled;
    }

    private void OnDestroy()
    {
        StopAnimation();
    }
}
