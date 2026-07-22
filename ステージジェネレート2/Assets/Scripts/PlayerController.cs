using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("スタート地点")]
    [SerializeField] private Transform startPoint;

    [Header("テスト地点")]
    [SerializeField] private Transform testA;
    [SerializeField] private Transform testB;
    [SerializeField] private Transform testC;
    [SerializeField] private Transform testD;
    [SerializeField] private Transform testE;
    [SerializeField] private Transform testF;
    [SerializeField] private Transform testG;
    [SerializeField] private Transform testH;

    [Header("移動速度")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("回転速度")]
    [SerializeField] private float turnSpeed = 180f;

    private Rigidbody rb;

    private float moveInput;
    private float turnInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = true;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void Start()
    {
        if (startPoint != null)
        {
            Warp(startPoint);
        }
    }

    private void Update()
    {
        // 入力取得
        moveInput = Input.GetAxisRaw("Vertical");
        turnInput = Input.GetAxisRaw("Horizontal");

        // テストワープ
        if (Input.GetKeyDown(KeyCode.Alpha1) && testA != null) Warp(testA);
        if (Input.GetKeyDown(KeyCode.Alpha2) && testB != null) Warp(testB);
        if (Input.GetKeyDown(KeyCode.Alpha3) && testC != null) Warp(testC);
        if (Input.GetKeyDown(KeyCode.Alpha4) && testD != null) Warp(testD);
        if (Input.GetKeyDown(KeyCode.Alpha5) && testE != null) Warp(testE);
        if (Input.GetKeyDown(KeyCode.Alpha6) && testF != null) Warp(testF);
        if (Input.GetKeyDown(KeyCode.Alpha7) && testG != null) Warp(testG);
        if (Input.GetKeyDown(KeyCode.Alpha8) && testH != null) Warp(testH);
    }

    private void FixedUpdate()
    {
        // 回転
        Quaternion rotation =
            Quaternion.Euler(
                0,
                rb.rotation.eulerAngles.y + turnInput * turnSpeed * Time.fixedDeltaTime,
                0);

        rb.MoveRotation(rotation);

        // 前後移動
        Vector3 movement =
            transform.forward *
            moveInput *
            moveSpeed *
            Time.fixedDeltaTime;

        rb.MovePosition(rb.position + movement);
    }

    private void Warp(Transform point)
    {
        rb.position = point.position;
        rb.rotation = point.rotation;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}