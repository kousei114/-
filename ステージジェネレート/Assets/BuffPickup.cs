using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BuffPickup : MonoBehaviour
{
    [SerializeField] private BuffManager buffManager;
    [SerializeField] private bool destroyAfterPickup = true;

    private bool collected;

    private void OnTriggerEnter(Collider other)
    {
        if (collected || buffManager == null)
        {
            return;
        }

        if (other.GetComponentInParent<Player_movement>() == null)
        {
            return;
        }

        if (!buffManager.ApplyRandomBuff())
        {
            return;
        }

        collected = true;

        if (destroyAfterPickup)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
