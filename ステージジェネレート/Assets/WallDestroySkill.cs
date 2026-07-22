using UnityEngine;

public class WallDestroySkill : MonoBehaviour
{
    [SerializeField] private KeyCode activationKey = KeyCode.B;
    [SerializeField, Min(0f)] private float range = 5f;
    [SerializeField] private float rayOriginHeight = 0.5f;
    [SerializeField] private LayerMask targetLayers = ~0;

    private void Update()
    {
        if (Input.GetKeyDown(activationKey))
        {
            TryDestroyTarget();
        }
    }

    public void TryDestroyTarget()
    {
        Vector3 origin = transform.position + Vector3.up * rayOriginHeight;

        if (!Physics.Raycast(origin, transform.forward, out RaycastHit hit, range, targetLayers, QueryTriggerInteraction.Ignore))
        {
            Debug.Log("No wall to destroy in range.");
            return;
        }

        DestructibleWall wall = hit.collider.GetComponentInParent<DestructibleWall>();
        if (wall == null)
        {
            Debug.Log("The targeted wall is not destructible.");
            return;
        }

        wall.DestroyWall();
    }
}
