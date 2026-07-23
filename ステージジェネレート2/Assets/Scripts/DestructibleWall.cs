using UnityEngine;

public class DestructibleWall : MonoBehaviour
{
    [SerializeField] private GameObject destructionEffect;
    [SerializeField, Min(0f)] private float destroyDelay;

    private bool isDestroyed;

    public void DestroyWall()
    {
        if (isDestroyed)
        {
            return;
        }

        isDestroyed = true;

        if (destructionEffect != null)
        {
            Instantiate(destructionEffect, transform.position, transform.rotation);
        }

        Destroy(gameObject, destroyDelay);
    }
}
