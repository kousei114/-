using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Player_movement : MonoBehaviour
{
    private Rigidbody rb;
    private Collider playerCollider; // コライダーの参照

    private float moveInput = 0f;
    private float rotateInput = 0f;

    private float currentMoveValue = 0f;
    private float currentRotateValue = 0f;

    [SerializeField] private float speed = 6f;
    [SerializeField] private float rotateSpeed = 180f;

    [SerializeField] private float speedMultiplier = 1f;
    [SerializeField] private float runawaySpeedMultiplier = 8f;
    [SerializeField] private float activeSpeedMultiplier = 1f;
    [SerializeField] private float collisionSkinWidth = 0.02f;

    [Header("通常時の慣性")]
    [SerializeField] private float normalAcceleration = 100f;
    [SerializeField] private float normalDeceleration = 100f;

    [Header("ドリフト時の慣性")]
    [SerializeField] private float driftAcceleration = 1.5f;
    [SerializeField] private float driftDeceleration = 0.4f;

    [Header("デバフ状態フラグ")]
    [SerializeField] private bool isDrifting = false;
    [SerializeField] private bool isRunaway = false; // 制御不能の暴走デバフ

    [Header("物理素材（ツルツルにする用）")]
    [SerializeField] private PhysicMaterial slipperyMaterial; // 作成したツルツル素材をセット
    private PhysicMaterial originalMaterial; // 元の物理素材を保存しておく用

    [Header("アニメーション（StarterAssets ヒト型モデル用）")]
    [Tooltip("未設定なら子オブジェクトの Animator を自動取得します")]
    [SerializeField] private Animator animator;
    [Tooltip("アニメ速度の滑らかさ。大きいほど素早く追従")]
    [SerializeField] private float animationSpeedChangeRate = 10f;

    private bool hasAnimator;
    private float animationBlend; // Animator に渡す平滑化した速度
    private int animIDSpeed;
    private int animIDMotionSpeed;

    void Start()
    {
        Application.targetFrameRate = 60;
        rb = GetComponent<Rigidbody>();
        playerCollider = GetComponent<Collider>(); // コライダーを取得

        if (playerCollider != null)
        {
            originalMaterial = playerCollider.sharedMaterial; // 元の素材をキープ
        }

        // Animator の取得（インスペクタ未設定なら自身→子から探す）
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
        hasAnimator = animator != null;

        // Animator パラメータのハッシュ化（StarterAssets の命名に一致）
        animIDSpeed = Animator.StringToHash("Speed");
        animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }

    // Player_Input から入力値を受け取るための関数（自動前進を削除し、純粋に渡された値を使用）
    public void SetInputValues(float move, float rotate)
    {
        moveInput = move;
        rotateInput = rotate;
    }

    void FixedUpdate()
    {
        // 1. 状態に応じた加減速スピードの決定（暴走時もドリフト同様に滑る挙動にします）
        // 暴走は速度のみを上げるデバフなので、ドリフト用の慣性は使わない。
        bool useDriftPhysics = isDrifting && !isRunaway;
        bool driftPhysicsEnabled = isDrifting && !isRunaway;
        float activeAccel = driftPhysicsEnabled ? driftAcceleration : normalAcceleration;
        float activeDecel = driftPhysicsEnabled ? driftDeceleration : normalDeceleration;

        // 2. 加減速の計算
        float moveStep = (moveInput != 0f) ? activeAccel : activeDecel;
        currentMoveValue = Mathf.MoveTowards(currentMoveValue, moveInput, moveStep * Time.fixedDeltaTime);

        float rotateStep = (rotateInput != 0f) ? activeAccel : activeDecel;
        currentRotateValue = Mathf.MoveTowards(currentRotateValue, rotateInput, rotateStep * Time.fixedDeltaTime);

        // 3. 回転の処理（0でないとき、ではなく微小な値より大きい時で判定）
        // ★ transform.Rotate（物理無視の直接回転）だとRigidbodyの物理状態とズレて
        //    旋回時に床/壁へめり込むため、物理経由の rb.MoveRotation に変更。
        if (Mathf.Abs(currentRotateValue) > 0.01f)
        {
            float yRotation = currentRotateValue * rotateSpeed * Time.fixedDeltaTime;
            Quaternion deltaRotation = Quaternion.Euler(0f, yRotation, 0f);
            rb.MoveRotation(rb.rotation * deltaRotation);
        }

        // 4. 移動の処理（★ここを修正）
        // 慣性の値が完全に0になるのを防ぐため、微小な閾値で判定し、入力があるか、
        // もしくは慣性で動いている間（currentMoveValueが0でない時）に確実に速度変更が乗るようにします
        activeSpeedMultiplier = isRunaway ? runawaySpeedMultiplier : speedMultiplier;

        // アニメーションに渡す「実際の移動速さ(m/s)」。前後どちらでも歩行/走行を再生するため絶対値を使う。
        float planarSpeed = 0f;

        if (Mathf.Abs(currentMoveValue) > 0.01f)
        {
            float currentSpeed = speed * activeSpeedMultiplier;
            planarSpeed = Mathf.Abs(currentMoveValue) * currentSpeed;

            Vector3 moveDelta = transform.forward * currentMoveValue * currentSpeed * Time.fixedDeltaTime;

            // 壁に当たったら止まるのではなく、壁に沿って滑らせる（collide & slide）
            Vector3 slidDelta = CollideAndSlide(moveDelta);

            rb.MovePosition(rb.position + slidDelta);
        }
        else
        {
            // 動いていない時は値をきっちり0にしておく
            currentMoveValue = 0f;
        }

        // 5. Animator の更新（Speed ブレンドツリーで Idle/Walk/Run を駆動）
        UpdateAnimator(planarSpeed);
    }

    // 壁に沿って滑る移動量を計算する（collide & slide）。
    // 進行方向をスイープし、壁に当たったら残りの移動を壁面に投影して滑らせる。
    // これにより斜めに壁へ当たっても止まらず、壁沿いに進める。
    private Vector3 CollideAndSlide(Vector3 delta)
    {
        Vector3 totalMotion = Vector3.zero;
        Vector3 remaining = delta;
        const int maxIterations = 3; // 角での引っかかり対策に数回まで滑りを繰り返す

        for (int i = 0; i < maxIterations; i++)
        {
            float distance = remaining.magnitude;
            if (distance < 0.0001f) break;

            Vector3 direction = remaining / distance;

            if (rb.SweepTest(direction, out RaycastHit hit, distance, QueryTriggerInteraction.Ignore))
            {
                // 壁の手前（skin幅ぶん余裕を残す）まで進む
                float allowed = Mathf.Max(0f, hit.distance - collisionSkinWidth);
                totalMotion += direction * allowed;

                // 進めなかった残りを、壁面に沿う成分だけに投影して滑らせる
                Vector3 leftover = remaining - direction * allowed;
                remaining = Vector3.ProjectOnPlane(leftover, hit.normal);
            }
            else
            {
                // 何にも当たらなければ残りをそのまま進む
                totalMotion += remaining;
                break;
            }
        }

        return totalMotion;
    }

    // 平滑化した速度を Animator に渡す。移動基盤(Rigidbody)には一切影響しない。
    private void UpdateAnimator(float planarSpeed)
    {
        if (!hasAnimator) return;

        // 目標速度へ滑らかに補間（急な立ち上がり/停止のガクつきを防ぐ）
        animationBlend = Mathf.Lerp(animationBlend, planarSpeed, Time.fixedDeltaTime * animationSpeedChangeRate);
        if (animationBlend < 0.01f) animationBlend = 0f;

        animator.SetFloat(animIDSpeed, animationBlend);
        // 入力中はアニメ再生速度を等倍に、停止中は0に寄せる
        animator.SetFloat(animIDMotionSpeed, planarSpeed > 0.01f ? 1f : 0f);
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
    }

    public void SetRunawaySpeedMultiplier(float multiplier)
    {
        runawaySpeedMultiplier = multiplier;
    }

    public void SetDriftMode(bool drift)
    {
        isDrifting = drift;
    }

    // 外部から暴走モード（制御不能スピード）を切り替える関数
    public void SetRunawayMode(bool runaway)
    {
        isRunaway = runaway;

        if (playerCollider == null) return;

        if (isRunaway)
        {
            isDrifting = false;
            // コライダーを摩擦ゼロのツルツル素材に変更（壁に当たっても滑る！）
            // 暴走中も物理マテリアルは変更せず、ドリフト状態だけを解除する。
            isDrifting = false;
        }
        else
        {
            // 元の摩擦に戻す
            playerCollider.sharedMaterial = originalMaterial;
        }

        Debug.Log($"暴走モード: {isRunaway}");
    }

    // --- StarterAssets アニメーションイベントの受け皿 ---
    // Walk_N / Run_N のアニメが OnFootstep を、着地アニメが OnLand を呼ぶ。
    // 受け取る相手がいないと「has no receiver」警告が出るため、ここで受ける。
    // 足音SEを鳴らしたくなったら、この中に AudioSource.PlayClipAtPoint などを追加する。
    private void OnFootstep(AnimationEvent animationEvent)
    {
        // TODO: 足音SEを鳴らす場合はここに実装
    }

    private void OnLand(AnimationEvent animationEvent)
    {
        // TODO: 着地SEを鳴らす場合はここに実装
    }
}
