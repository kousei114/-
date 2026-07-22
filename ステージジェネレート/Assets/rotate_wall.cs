using UnityEngine;

public class rotate_wall : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private float rotateSpeed = 30f; // 1秒間の回転角度（インスペクターから調整可能）

    void Start()
    {
        // 自身の Rigidbody を取得
        rb = GetComponent<Rigidbody>();
    }

    // 物理演算を伴う回転は FixedUpdate で行う
    void FixedUpdate()
    {
        // 画像の軸の向きに合わせて、オブジェクトの「ローカルのX軸（またはY軸）」をベースに回転を作ります
        // もしこれでまだ見た目が変わらない場合は、Vector3.right を Vector3.up に変えてみてください
        Vector3 rotationAxis = transform.InverseTransformDirection(Vector3.up);

        Quaternion deltaRotation = Quaternion.AngleAxis(rotateSpeed * Time.fixedDeltaTime, rotationAxis);
        rb.MoveRotation(rb.rotation * deltaRotation);
    }
}