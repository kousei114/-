using UnityEngine;

public class Wallpass : MonoBehaviour
{
    [Header("スキル設定")]
    [SerializeField] private float maxDashDistance = 4f;   // 最大ダッシュ（テレポート）距離
    [SerializeField] private float maxWallThickness = 1.2f; // 通り抜けられる「壁1枚」の最大厚み
    [SerializeField] private LayerMask wallLayer;          // 壁のレイヤー

    [SerializeField] private int maxUses = 3;
    [SerializeField] private int startingUses = 2;
    [SerializeField] private int usesRemaining;

    private CharacterController characterController;

    void Start()
    {
        // CharacterController（使っている場合）を取得
        characterController = GetComponent<CharacterController>();
        usesRemaining = Mathf.Clamp(startingUses, 0, maxUses);
    }

    void Update()
    {
        // Wキーが一回押された瞬間を検知
        if (Input.GetKeyDown(KeyCode.W))
        {
            TryPhaseDash();
        }
    }

    public void TryPhaseDash()
    {
        if (usesRemaining <= 0)
        {
            Debug.Log("Wall pass has no uses remaining.");
            return;
        }

        Vector3 origin = transform.position;
        Vector3 direction = transform.forward; // プレイヤーの正面方向

        // 1. 正面に壁があるかチェック（入口の検出）
        if (Physics.Raycast(origin, direction, out RaycastHit frontHit, maxDashDistance, wallLayer))
        {
            // 2. 最大距離の地点から、プレイヤーに向かって逆向きにRayを飛ばす（出口の検出）
            Vector3 reverseRayOrigin = origin + (direction * maxDashDistance);
            Vector3 reverseDirection = -direction;

            if (Physics.Raycast(reverseRayOrigin, reverseDirection, out RaycastHit backHit, maxDashDistance, wallLayer))
            {
                // 入口と出口の距離（＝壁の厚み）を計測
                float wallThickness = Vector3.Distance(frontHit.point, backHit.point);

                // 壁の厚みが許容範囲内（＝壁1枚）であればすり抜ける
                if (wallThickness <= maxWallThickness)
                {
                    // 壁の裏側から、少しだけプレイヤーのサイズ分（マージン）進んだ安全な位置を着地点にする
                    Vector3 destination = backHit.point + (direction * 0.5f);

                    // ※地面に埋まらないよう、高さをプレイヤーの現在地に合わせる
                    destination.y = transform.position.y;

                    PerformTeleport(destination);
                    ConsumeUse();
                    Debug.Log($"壁抜け成功！ 壁の厚み: {wallThickness:F2}m");
                    return;
                }
                else
                {
                    Debug.Log($"壁が厚すぎるため抜けられません（厚み: {wallThickness:F2}m）");
                }
            }
        }

        // 壁がない、または壁が分厚くて抜けられない場合は通常のショートダッシュにする
        Vector3 defaultDestination = origin + (direction * maxDashDistance);

        // 通常のダッシュ先が壁に遮られないかだけチェック
        if (Physics.Raycast(origin, direction, out RaycastHit simpleHit, maxDashDistance, wallLayer))
        {
            defaultDestination = simpleHit.point - (direction * 0.5f);
        }

        PerformTeleport(defaultDestination);
        ConsumeUse();
    }

    private void ConsumeUse()
    {
        usesRemaining--;
        Debug.Log($"Wall pass uses remaining: {usesRemaining}");
    }

    public void AddUses(int amount)
    {
        usesRemaining = Mathf.Clamp(usesRemaining + amount, 0, maxUses);
        Debug.Log($"Wall pass uses remaining: {usesRemaining}");
    }

    private void PerformTeleport(Vector3 targetPosition)
    {
        // 瞬間移動させる（CharacterControllerなどを使っている場合は一度無効化して位置更新するのが安全です）
        transform.position = targetPosition;
    }
}
