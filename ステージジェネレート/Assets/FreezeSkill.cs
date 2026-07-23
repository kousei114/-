using UnityEngine;

public class FreezeSkill : MonoBehaviour
{
    [SerializeField] private KeyCode activationKey = KeyCode.F;
    [SerializeField, Min(0f)] private float range = 5f;
    [SerializeField, Min(0f)] private float freezeDuration = 3f;
    [SerializeField] private float rayOriginHeight = 0.5f;
    [SerializeField] private LayerMask targetLayers = ~0;

    private void Update()
    {
        if (Input.GetKeyDown(activationKey))
        {
            TryFreezeTarget();
        }
    }

    public void TryFreezeTarget()
    {
        Vector3 origin = transform.position + Vector3.up * rayOriginHeight;

        if (!Physics.Raycast(origin, transform.forward, out RaycastHit hit, range, targetLayers, QueryTriggerInteraction.Ignore))
        {
            Debug.Log("No freeze target in range.");
            return;
        }

        Freezable target = hit.collider.GetComponentInParent<Freezable>();
        if (target == null)
        {
            Debug.Log("The targeted object cannot be frozen.");
            return;
        }

        target.Freeze(freezeDuration);
    }
}
