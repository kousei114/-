using UnityEngine;

/// <summary>
/// プレイヤーが触れた床を消すコンポーネントです。
/// このコンポーネントを付けたオブジェクトの Collider は Is Trigger を有効にしてください。
/// </summary>
[RequireComponent(typeof(Collider))]
public class BreakableFloor : MonoBehaviour
{
    [Header("破壊設定")]
    [SerializeField, Min(0f)] private float destroyDelay = 0f;
    [SerializeField] private GameObject destructionEffect;

    private bool isBroken;

    private void Reset()
    {
        // コンポーネントを追加した時点で、接触検出用の Collider にしておく。
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isBroken || !IsPlayer(other))
        {
            return;
        }

        Break();
    }

    private bool IsPlayer(Collider other)
    {
        return other.CompareTag("Player") || other.transform.root.CompareTag("Player");
    }

    private void Break()
    {
        isBroken = true;

        if (destructionEffect != null)
        {
            Instantiate(destructionEffect, transform.position, transform.rotation);
        }

        Destroy(gameObject, destroyDelay);
    }
}
