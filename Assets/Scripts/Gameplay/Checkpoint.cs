using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class Checkpoint : MonoBehaviour
{
    [Header("Checkpoint Settings")]
    public float pauseDuration = 3f;
    public float timeBonus = 10f;
    public bool oneTimeUse = false;
    public bool resetOnLoopReset = true;

    [Header("Visual Feedback")]
    public GameObject activeVisual;
    public GameObject usedVisual;
    public ParticleSystem activationEffect;
    public Light checkpointLight;
    public Color activeColor = Color.green;
    public Color usedColor = Color.gray;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip activationSound;

    [Header("Events")]
    public UnityEvent OnCheckpointActivated;
    public UnityEvent OnCheckpointUsed;

    private LoopManager loopManager;
    private bool hasBeenUsed = false;
    private bool isActivating = false;
    private Collider checkpointCollider;

    void Start()
    {
        loopManager = FindObjectOfType<LoopManager>();
        checkpointCollider = GetComponent<Collider>();
        
        if (checkpointCollider != null)
            checkpointCollider.isTrigger = true;

        // Auto-setup audio source if not assigned
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        // Subscribe to loop reset events
        if (loopManager != null && resetOnLoopReset)
        {
            loopManager.OnLoopReset.AddListener(ResetCheckpoint);
        }

        UpdateVisuals();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasBeenUsed && !isActivating)
        {
            ActivateCheckpoint();
        }
    }

    public void ActivateCheckpoint()
    {
        if (hasBeenUsed && oneTimeUse) return;
        if (isActivating) return;

        StartCoroutine(CheckpointSequence());
    }

    IEnumerator CheckpointSequence()
    {
        isActivating = true;

        // Play activation effects
        PlayActivationEffects();

        // Pause the loop timer
        if (loopManager != null)
        {
            loopManager.PauseLoop(true);
        }

        // Wait for pause duration
        yield return new WaitForSeconds(pauseDuration);

        // Resume the loop timer and give time bonus
        if (loopManager != null)
        {
            loopManager.PauseLoop(false);
            loopManager.ReportProgress(timeBonus);
        }

        // Mark as used if one-time use
        if (oneTimeUse)
        {
            hasBeenUsed = true;
            UpdateVisuals();
        }

        // Invoke events
        OnCheckpointActivated?.Invoke();
        if (oneTimeUse)
            OnCheckpointUsed?.Invoke();

        isActivating = false;

        Debug.Log($"Checkpoint activated! Paused for {pauseDuration} seconds, granted {timeBonus} seconds bonus");
    }

    void PlayActivationEffects()
    {
        // Play particle effect
        if (activationEffect != null)
        {
            activationEffect.Play();
        }

        // Play sound effect
        if (audioSource != null && activationSound != null)
        {
            audioSource.PlayOneShot(activationSound);
        }

        // Flash the light
        if (checkpointLight != null)
        {
            StartCoroutine(FlashLight());
        }
    }

    IEnumerator FlashLight()
    {
        if (checkpointLight == null) yield break;

        float originalIntensity = checkpointLight.intensity;
        Color originalColor = checkpointLight.color;

        // Flash brighter
        checkpointLight.intensity = originalIntensity * 2f;
        checkpointLight.color = Color.white;

        yield return new WaitForSeconds(0.2f);

        // Return to normal
        checkpointLight.intensity = originalIntensity;
        checkpointLight.color = originalColor;
    }

    void UpdateVisuals()
    {
        bool isActive = !hasBeenUsed || !oneTimeUse;

        // Update visual objects
        if (activeVisual != null)
            activeVisual.SetActive(isActive);
            
        if (usedVisual != null)
            usedVisual.SetActive(!isActive);

        // Update light color
        if (checkpointLight != null)
        {
            checkpointLight.color = isActive ? activeColor : usedColor;
            checkpointLight.intensity = isActive ? 1f : 0.3f;
        }

        // Update collider
        if (checkpointCollider != null && oneTimeUse)
        {
            checkpointCollider.enabled = isActive;
        }
    }

    public void ResetCheckpoint()
    {
        hasBeenUsed = false;
        isActivating = false;
        UpdateVisuals();
        
        Debug.Log("Checkpoint reset");
    }

    public void ForceActivate()
    {
        if (!isActivating)
            ActivateCheckpoint();
    }

    // Getters
    public bool IsUsed() => hasBeenUsed;
    public bool IsActivating() => isActivating;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = hasBeenUsed ? Color.red : Color.green;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 2f);
        
        // Draw activation range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 1.5f);
    }
}

