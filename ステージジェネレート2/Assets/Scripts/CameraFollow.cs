using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField]
    private Transform target;

    private Vector3 offset;

    void Start()
    {
        offset = transform.position - target.position;
    }

    void LateUpdate()
    {
        if (target == null) return;

        transform.position = target.position + offset;

        // LookAtÇÕèëÇ©Ç»Ç¢ÅI
    }
}