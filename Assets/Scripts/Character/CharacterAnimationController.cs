using UnityEngine;

/// <summary>
/// キャラクターのアニメーション制御クラス
/// Animation コンポーネントを使用してシンプルに実装
/// </summary>
public class CharacterAnimationController : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("再生するアニメーションクリップ（Inspector で差し替え可能）")]
    public AnimationClip animationClip;

    [Tooltip("アニメーションをループ再生するか")]
    public bool loopAnimation = true;

    [Tooltip("アニメーションの再生速度（1.0 = 通常速度）")]
    [Range(0.1f, 3.0f)]
    public float animationSpeed = 1.0f;

    private Animation animComponent;
    private bool isPlaying = false;

    private void Awake()
    {
        // Animation コンポーネントを取得または追加
        animComponent = GetComponent<Animation>();
        if (animComponent == null)
        {
            animComponent = gameObject.AddComponent<Animation>();
            Debug.Log($"[CharacterAnimationController] {gameObject.name} に Animation コンポーネントを追加しました。", this);
        }
    }

    private void Start()
    {
        // 初期設定
        if (animComponent != null)
        {
            animComponent.playAutomatically = false;
        }
    }

    /// <summary>
    /// アニメーションを再生開始
    /// </summary>
    public void PlayAnimation()
    {
        if (animationClip == null)
        {
            Debug.LogError("[CharacterAnimationController] AnimationClip が設定されていません！Inspector で設定してください。", this);
            return;
        }

        if (animComponent == null)
        {
            Debug.LogError("[CharacterAnimationController] Animation コンポーネントが見つかりません！", this);
            return;
        }

        // アニメーションクリップを追加
        animComponent.AddClip(animationClip, animationClip.name);
        
        // ループ設定
        AnimationState state = animComponent[animationClip.name];
        if (state != null)
        {
            state.wrapMode = loopAnimation ? WrapMode.Loop : WrapMode.Once;
            state.speed = animationSpeed;
        }

        // 再生
        animComponent.clip = animationClip;
        animComponent.Play(animationClip.name);
        isPlaying = true;

        Debug.Log($"[CharacterAnimationController] アニメーション '{animationClip.name}' を再生開始しました。", this);
    }

    /// <summary>
    /// アニメーションを停止
    /// </summary>
    public void StopAnimation()
    {
        if (animComponent != null && isPlaying)
        {
            animComponent.Stop();
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
        
        if (animComponent != null && animationClip != null)
        {
            AnimationState state = animComponent[animationClip.name];
            if (state != null)
            {
                state.speed = animationSpeed;
            }
        }
    }

    /// <summary>
    /// 再生中かどうか
    /// </summary>
    public bool IsPlaying()
    {
        return isPlaying && animComponent != null && animComponent.isPlaying;
    }

    private void OnDestroy()
    {
        StopAnimation();
    }
}

