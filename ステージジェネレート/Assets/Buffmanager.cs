using System.Collections;
using UnityEngine;

public class BuffManager : MonoBehaviour
{
    public enum BuffType
    {
        SpeedUp,
        WallpassUse,
        InvertControls,
        DriftDebuff,
        RunawayDebuff
    }

    [Header("References")]
    [SerializeField] private Player_movement playerMovement;
    [SerializeField] private Wallpass wallpass;

    [Header("Selection Weights")]
    [SerializeField, Min(0)] private int speedUpWeight = 40;
    [SerializeField, Min(0)] private int wallpassUseWeight = 25;
    [SerializeField, Min(0)] private int invertControlsWeight = 15;
    [SerializeField, Min(0)] private int driftDebuffWeight = 12;
    [SerializeField, Min(0)] private int runawayDebuffWeight = 8;

    [Header("Effect Values")]
    [SerializeField] private float speedUpMultiplier = 1.5f;
    [SerializeField] private float driftMultiplier = 2f;
    [SerializeField] private float runawaySpeedMultiplier = 8f;

    [Header("Temporary Effect Durations")]
    [SerializeField, Min(0f)] private float speedUpDuration = 10f;
    [SerializeField, Min(0f)] private float invertControlsDuration = 5f;
    [SerializeField, Min(0f)] private float driftDebuffDuration = 5f;
    [SerializeField, Min(0f)] private float runawayDebuffDuration = 3f;

    [Header("Runtime State")]
    [SerializeField] private string activeTemporaryEffect = "None";

    private Coroutine activeTemporaryEffectCoroutine;
    private int temporaryEffectVersion;

    private void Awake()
    {
        if (playerMovement != null && wallpass == null)
        {
            wallpass = playerMovement.GetComponent<Wallpass>();
        }
    }

    public void PlayGambling()
    {
        ApplyRandomBuff();
    }

    public bool ApplyRandomBuff()
    {
        int totalWeight = speedUpWeight + wallpassUseWeight + invertControlsWeight
            + driftDebuffWeight + runawayDebuffWeight;

        if (totalWeight <= 0)
        {
            Debug.LogWarning("Buff selection weights must total at least 1.");
            return false;
        }

        BuffType selectedBuff = SelectWeightedBuff(Random.Range(0, totalWeight));
        return ApplyBuff(selectedBuff);
    }

    private BuffType SelectWeightedBuff(int roll)
    {
        if (roll < speedUpWeight) return BuffType.SpeedUp;
        roll -= speedUpWeight;

        if (roll < wallpassUseWeight) return BuffType.WallpassUse;
        roll -= wallpassUseWeight;

        if (roll < invertControlsWeight) return BuffType.InvertControls;
        roll -= invertControlsWeight;

        if (roll < driftDebuffWeight) return BuffType.DriftDebuff;
        return BuffType.RunawayDebuff;
    }

    private bool ApplyBuff(BuffType buff)
    {
        if (buff == BuffType.WallpassUse)
        {
            if (wallpass == null)
            {
                Debug.LogWarning("Wallpass reference is missing.");
                return false;
            }

            wallpass.AddUses(1);
            Debug.Log("Applied buff: WallpassUse");
            return true;
        }

        if (playerMovement == null)
        {
            Debug.LogWarning("Player Movement reference is missing.");
            return false;
        }

        StartTemporaryEffect(buff, GetDuration(buff));
        return true;
    }

    private void StartTemporaryEffect(BuffType buff, float duration)
    {
        temporaryEffectVersion++;

        if (activeTemporaryEffectCoroutine != null)
        {
            StopCoroutine(activeTemporaryEffectCoroutine);
        }

        ResetTemporaryEffects();
        ApplyTemporaryEffect(buff);
        activeTemporaryEffect = buff.ToString();
        activeTemporaryEffectCoroutine = StartCoroutine(ClearTemporaryEffectAfterDuration(temporaryEffectVersion, duration));

        Debug.Log($"Applied temporary buff: {buff} ({duration:F1}s)");
    }

    private void ApplyTemporaryEffect(BuffType buff)
    {
        Player_Input input = playerMovement.GetComponent<Player_Input>();

        switch (buff)
        {
            case BuffType.SpeedUp:
                playerMovement.SetSpeedMultiplier(speedUpMultiplier);
                break;

            case BuffType.InvertControls:
                if (input != null)
                {
                    input.SetInputInvert(true);
                }
                break;

            case BuffType.DriftDebuff:
                playerMovement.SetDriftMode(true);
                playerMovement.SetSpeedMultiplier(driftMultiplier);
                break;

            case BuffType.RunawayDebuff:
                playerMovement.SetRunawaySpeedMultiplier(runawaySpeedMultiplier);
                playerMovement.SetRunawayMode(true);
                break;
        }
    }

    private IEnumerator ClearTemporaryEffectAfterDuration(int version, float duration)
    {
        yield return new WaitForSeconds(duration);

        if (version != temporaryEffectVersion)
        {
            yield break;
        }

        ResetTemporaryEffects();
        activeTemporaryEffect = "None";
        activeTemporaryEffectCoroutine = null;
        Debug.Log("Temporary buff expired.");
    }

    private float GetDuration(BuffType buff)
    {
        switch (buff)
        {
            case BuffType.SpeedUp:
                return speedUpDuration;
            case BuffType.InvertControls:
                return invertControlsDuration;
            case BuffType.DriftDebuff:
                return driftDebuffDuration;
            case BuffType.RunawayDebuff:
                return runawayDebuffDuration;
            default:
                return 0f;
        }
    }

    private void ResetTemporaryEffects()
    {
        if (playerMovement == null)
        {
            return;
        }

        playerMovement.SetSpeedMultiplier(1f);
        playerMovement.SetDriftMode(false);
        playerMovement.SetRunawayMode(false);

        Player_Input input = playerMovement.GetComponent<Player_Input>();
        if (input != null)
        {
            input.SetInputInvert(false);
        }
    }
}
