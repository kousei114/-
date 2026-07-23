using UnityEngine;

public class Player_Input : MonoBehaviour
{
    private float moveInput = 0f;
    private float rotateInput = 0f;

    [SerializeField] private bool isInputInverted = false;  // 操作反転デバフ中かどうかのフラグ

    // 同一オブジェクトにある移動スクリプトへの参照
    private Player_movement movement;

    void Start()
    {
        movement = GetComponent<Player_movement>();
    }

    void Update()
    {
        // 入力の状態を毎フレーム監視
        moveInput = 0f;
        rotateInput = 0f;

        if (Input.GetKey("up"))
        {
            moveInput = 1f;
        }
        if (Input.GetKey("down"))
        {
            moveInput = -1f;
        }
        if (Input.GetKey("right"))
        {
            rotateInput = 1f;
        }
        if (Input.GetKey("left"))
        {
            rotateInput = -1f;
        }

        // 操作反転デバフの適用
        if (isInputInverted)
        {
            moveInput *= -1f;
            rotateInput *= -1f; // 旋回も反転（右を押すと左に回る）
        }

        // 加工した入力値を移動スクリプトへ届ける
        if (movement != null)
        {
            movement.SetInputValues(moveInput, rotateInput);
        }
    }

    // ★ BuffManagerなど外部から操作反転を切り替える窓口
    public void SetInputInvert(bool invert)
    {
        isInputInverted = invert;
    }
}