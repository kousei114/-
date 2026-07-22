using System.Collections;
using UnityEngine;

public class Freezable : MonoBehaviour
{
    [SerializeField] private MonoBehaviour[] behavioursToPause;
    [SerializeField] private bool isFrozen;

    private bool[] enabledStates;
    private Coroutine freezeCoroutine;

    public bool IsFrozen => isFrozen;

    public void Freeze(float duration)
    {
        if (!isFrozen)
        {
            PauseBehaviours();
            isFrozen = true;
        }

        if (freezeCoroutine != null)
        {
            StopCoroutine(freezeCoroutine);
        }

        freezeCoroutine = StartCoroutine(UnfreezeAfter(duration));
        Debug.Log($"Frozen: {name} ({duration:F1}s)");
    }

    private void PauseBehaviours()
    {
        enabledStates = new bool[behavioursToPause.Length];

        for (int i = 0; i < behavioursToPause.Length; i++)
        {
            MonoBehaviour behaviour = behavioursToPause[i];
            if (behaviour == null)
            {
                continue;
            }

            enabledStates[i] = behaviour.enabled;
            behaviour.enabled = false;
        }
    }

    private IEnumerator UnfreezeAfter(float duration)
    {
        yield return new WaitForSeconds(duration);

        for (int i = 0; i < behavioursToPause.Length; i++)
        {
            MonoBehaviour behaviour = behavioursToPause[i];
            if (behaviour != null)
            {
                behaviour.enabled = enabledStates[i];
            }
        }

        isFrozen = false;
        freezeCoroutine = null;
        Debug.Log($"Unfrozen: {name}");
    }

    private void OnDisable()
    {
        if (freezeCoroutine != null)
        {
            StopCoroutine(freezeCoroutine);
            freezeCoroutine = null;
        }

        if (isFrozen && enabledStates != null)
        {
            for (int i = 0; i < behavioursToPause.Length; i++)
            {
                MonoBehaviour behaviour = behavioursToPause[i];
                if (behaviour != null)
                {
                    behaviour.enabled = enabledStates[i];
                }
            }

            isFrozen = false;
        }
    }
}
