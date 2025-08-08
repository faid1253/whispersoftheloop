using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Collections;

public class Portal : MonoBehaviour
{
    [Header("Portal Settings")]
    public string targetSceneName;
    public Transform targetPosition; // For same-scene teleportation
    public bool useSceneTransition = true;
    public float activationDelay = 1f;

    [Header("Activation Requirements")]
    public bool requiresAllFragments = false;
    public int requiredFragmentCount = 0;
    public bool requiresSpecificFragments = false;
    public int[] requiredFragmentIDs;

    [Header("Visual Effects")]
    public ParticleSystem portalEffect;
    public Light portalLight;
    public Renderer portalRenderer;
    public Material activeMaterial;
    public Material inactiveMaterial;
    public float pulseSpeed = 2f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip activationSound;
    public AudioClip teleportSound;
    public AudioClip lockedSound;

    [Header("Events")]
    public UnityEvent OnPortalActivated;
    public UnityEvent OnPortalUsed;
    public UnityEvent OnPortalLocked;

    private bool isActive = false;
    private bool isActivating = false;
    private FragmentCounter fragmentCounter;
    private Collider portalCollider;
    private float originalLightIntensity;

    void Start()
    {
        fragmentCounter = FindObjectOfType<FragmentCounter>();
        portalCollider = GetComponent<Collider>();
        
        if (portalCollider != null)
            portalCollider.isTrigger = true;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (portalLight != null)
            originalLightIntensity = portalLight.intensity;

        UpdatePortalState();
    }

    void Update()
    {
        UpdatePortalState();
        AnimatePortal();
    }

    void UpdatePortalState()
    {
        bool shouldBeActive = CheckActivationRequirements();
        
        if (shouldBeActive && !isActive)
        {
            ActivatePortal();
        }
        else if (!shouldBeActive && isActive)
        {
            DeactivatePortal();
        }
    }

    bool CheckActivationRequirements()
    {
        if (fragmentCounter == null)
            return !requiresAllFragments && requiredFragmentCount == 0;

        if (requiresAllFragments)
        {
            return fragmentCounter.HasAllFragments();
        }

        if (requiredFragmentCount > 0)
        {
            return fragmentCounter.GetFragmentCount() >= requiredFragmentCount;
        }

        if (requiresSpecificFragments && requiredFragmentIDs != null)
        {
            foreach (int id in requiredFragmentIDs)
            {
                if (!fragmentCounter.HasFragment(id))
                    return false;
            }
            return true;
        }

        return true; // No requirements
    }

    void ActivatePortal()
    {
        isActive = true;
        
        // Update visuals
        if (portalRenderer != null && activeMaterial != null)
            portalRenderer.material = activeMaterial;
            
        if (portalEffect != null && !portalEffect.isPlaying)
            portalEffect.Play();

        // Play activation sound
        if (audioSource != null && activationSound != null)
            audioSource.PlayOneShot(activationSound);

        OnPortalActivated?.Invoke();
        
        Debug.Log($"Portal activated - Target: {(useSceneTransition ? targetSceneName : "Local Position")}");
    }

    void DeactivatePortal()
    {
        isActive = false;
        
        // Update visuals
        if (portalRenderer != null && inactiveMaterial != null)
            portalRenderer.material = inactiveMaterial;
            
        if (portalEffect != null && portalEffect.isPlaying)
            portalEffect.Stop();
    }

    void AnimatePortal()
    {
        if (!isActive) return;

        // Pulse the light
        if (portalLight != null)
        {
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f;
            portalLight.intensity = originalLightIntensity * (0.5f + pulse * 0.5f);
        }

        // Rotate the portal
        if (portalRenderer != null)
        {
            transform.Rotate(Vector3.up, 30f * Time.deltaTime, Space.Self);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isActivating)
        {
            if (isActive)
            {
                UsePortal();
            }
            else
            {
                PlayLockedFeedback();
            }
        }
    }

    public void UsePortal()
    {
        if (!isActive || isActivating) return;

        StartCoroutine(PortalSequence());
    }

    IEnumerator PortalSequence()
    {
        isActivating = true;

        // Play teleport sound
        if (audioSource != null && teleportSound != null)
            audioSource.PlayOneShot(teleportSound);

        // Visual feedback
        if (portalEffect != null)
        {
            var emission = portalEffect.emission;
            emission.rateOverTime = emission.rateOverTime.constant * 3f;
        }

        // Wait for activation delay
        yield return new WaitForSeconds(activationDelay);

        // Perform teleportation
        if (useSceneTransition && !string.IsNullOrEmpty(targetSceneName))
        {
            SceneManager.LoadScene(targetSceneName);
        }
        else if (targetPosition != null)
        {
            TeleportPlayer();
        }

        OnPortalUsed?.Invoke();
        isActivating = false;
    }

    void TeleportPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null)
                cc.enabled = false;

            player.transform.position = targetPosition.position;
            player.transform.rotation = targetPosition.rotation;

            if (cc != null)
                cc.enabled = true;

            Debug.Log("Player teleported to target position");
        }
    }

    void PlayLockedFeedback()
    {
        if (audioSource != null && lockedSound != null)
            audioSource.PlayOneShot(lockedSound);

        OnPortalLocked?.Invoke();

        // Show requirement message
        string message = GetRequirementMessage();
        Debug.Log($"Portal locked: {message}");
    }

    string GetRequirementMessage()
    {
        if (requiresAllFragments)
            return "Collect all memory fragments to activate this portal";
        
        if (requiredFragmentCount > 0)
        {
            int current = fragmentCounter?.GetFragmentCount() ?? 0;
            return $"Collect {requiredFragmentCount - current} more fragments to activate this portal";
        }

        if (requiresSpecificFragments)
            return "Collect the required memory fragments to activate this portal";

        return "Portal requirements not met";
    }

    public void ForceActivate()
    {
        isActive = true;
        ActivatePortal();
    }

    public void ForceDeactivate()
    {
        isActive = false;
        DeactivatePortal();
    }

    // Getters
    public bool IsActive() => isActive;
    public bool IsActivating() => isActivating;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = isActive ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, 2f);
        
        if (targetPosition != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(targetPosition.position, 1f);
            Gizmos.DrawLine(transform.position, targetPosition.position);
        }
    }
}

