using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GravityController : MonoBehaviour
{
    [SerializeField] private float gravityMagnitude = 9.81f;
    [SerializeField] private KeyCode invertKey = KeyCode.G;
    [SerializeField] private float rotationSmoothness = 10f;

    // CinemachineBrainのWorld Up Overrideにセットした空のオブジェクト
    [SerializeField] private Transform cameraUpTarget;

    private Rigidbody rb;
    private bool isGravityInverted = false;

    // このプレイヤーにだけ適用する重力方向（通常は下向き -Y、反転で上向き +Y）
    private Vector3 GravityDir => new Vector3(0f, isGravityInverted ? 1f : -1f, 0f);

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // 標準重力(Physics.gravity)は使わず、自前でこのRigidbodyにだけ加える
        rb.useGravity = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(invertKey))
        {
            isGravityInverted = !isGravityInverted;
        }
    }

    void FixedUpdate()
    {
        // ★ Physics.gravity(グローバル設定) は書き換えず、このRigidbodyにだけ重力を加える。
        //   これにより他オブジェクト（敵・アイテム・ガラス破片など）は巻き込まれない。
        rb.AddForce(GravityDir * gravityMagnitude * rb.mass);

        // プレイヤーとカメラの基準を同時に回転させる
        RotateAll();
    }

    private void RotateAll()
    {
        // 1. 重力の逆向き（プレイヤーにとっての新しい上方向）
        Vector3 targetUp = -GravityDir;

        // ★ここが解決のキモ★
        // 「現在のプレイヤーの右向き（横軸）」を回転の固定軸として、今のUpから目標のUpへの回転を作ります。
        // これにより、旋回後であっても、初期状態の時とまったく同じ「横軸中心の綺麗な宙返り」になります。
        Quaternion fromTo = Quaternion.FromToRotation(transform.up, targetUp);

        // プレイヤーの目標回転
        Quaternion targetRotation = fromTo * transform.rotation;

        // プレイヤーを滑らかに回転（初期状態の時の気持ちいい挙動）
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothness * Time.fixedDeltaTime);

        // 2. カメラの基準も、プレイヤーと完全に同じ回転をさせて同期する
        if (cameraUpTarget != null)
        {
            cameraUpTarget.rotation = Quaternion.Slerp(cameraUpTarget.rotation, targetRotation, rotationSmoothness * Time.fixedDeltaTime);
        }
    }
}
