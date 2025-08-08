using UnityEngine;
using System.Collections;

public class MemoryFragment : MonoBehaviour
{
    [Header("Fragment Settings")]
    public float timeBonus = 10f; // Changed to 10 seconds
    public bool oneTimeCollection = true;
    public float detectionRange = 2f;
    public int fragmentID = 0; // Unique ID for this fragment

    [Header("Shadow Mode Visibility")]
    public bool onlyVisibleInShadowMode = true;
    public float fadeSpeed = 5f;

    [Header("Visual Effects")]
    public ParticleSystem collectionEffect;
    public ParticleSystem ambientEffect; // NEW: Ambient particles that turn off in light mode
    public Light fragmentLight;
    public Renderer fragmentRenderer;
    public Color shadowModeColor = new Color(0.5f, 0f, 1f, 0.8f); // Purple/dark color
    public Color lightModeColor = new Color(1f, 1f, 1f, 0.1f); // Nearly invisible

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip collectionSound;

    [Header("Animation")]
    public bool rotateFragment = true;
    public float rotationSpeed = 50f;
    public bool floatFragment = true;
    public float floatHeight = 0.5f;
    public float floatSpeed = 2f;

    private bool hasBeenCollected = false;
    private Vector3 startPosition;
    private FormShift playerFormShift;
    private FragmentCounter fragmentCounter;
    private Material fragmentMaterial;
    private float targetAlpha = 1f;
    private float currentAlpha = 1f;

    void Start()
    {
        startPosition = transform.position;
        
        // Find player's FormShift component
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerFormShift = player.GetComponent<FormShift>();

        // Find fragment counter
        fragmentCounter = FindObjectOfType<FragmentCounter>();

        // Get or create material
        if (fragmentRenderer == null)
            fragmentRenderer = GetComponent<Renderer>();
            
        if (fragmentRenderer != null)
        {
            fragmentMaterial = fragmentRenderer.material;
        }

        // Auto-assign audio source
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        // Auto-find particle systems if not assigned
        if (ambientEffect == null)
        {
            ParticleSystem[] particles = GetComponentsInChildren<ParticleSystem>();
            if (particles.Length > 0)
                ambientEffect = particles[0];
        }

        // Set initial visibility
        UpdateVisibility();
    }

    void Update()
    {
        if (hasBeenCollected) return;

        // Update visibility based on player form
        UpdateVisibility();

        // Animate fragment
        AnimateFragment();

        // Check for collection
        CheckForCollection();
    }

    void UpdateVisibility()
    {
        if (!onlyVisibleInShadowMode || playerFormShift == null)
        {
            targetAlpha = 1f;
            SetParticleEffects(true);
            return;
        }

        bool inShadowMode = playerFormShift.IsShadow();

        // Set target alpha based on player form
        if (inShadowMode)
        {
            targetAlpha = shadowModeColor.a; // Visible in shadow mode
        }
        else
        {
            targetAlpha = lightModeColor.a; // Nearly invisible in light mode
        }

        // Smooth alpha transition
        currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, fadeSpeed * Time.deltaTime);

        // Apply to material
        if (fragmentMaterial != null)
        {
            Color currentColor = inShadowMode ? shadowModeColor : lightModeColor;
            currentColor.a = currentAlpha;
            fragmentMaterial.color = currentColor;
        }

        // Apply to light
        if (fragmentLight != null)
        {
            fragmentLight.intensity = currentAlpha * 2f;
            fragmentLight.color = inShadowMode ? shadowModeColor : lightModeColor;
        }

        // NEW: Control particle effects based on mode
        SetParticleEffects(inShadowMode);
    }

    void SetParticleEffects(bool enableEffects)
    {
        // Control ambient particle effect
        if (ambientEffect != null)
        {
            if (enableEffects && !ambientEffect.isPlaying)
            {
                ambientEffect.Play();
            }
            else if (!enableEffects && ambientEffect.isPlaying)
            {
                ambientEffect.Stop();
            }
        }

        // Update collection effect color if it exists
        if (collectionEffect != null && playerFormShift != null)
        {
            var main = collectionEffect.main;
            Color particleColor = playerFormShift.IsShadow() ? shadowModeColor : lightModeColor;
            particleColor.a = currentAlpha;
            main.startColor = particleColor;
        }
    }

    void AnimateFragment()
    {
        // Rotation
        if (rotateFragment)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }

        // Floating motion
        if (floatFragment)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

    void CheckForCollection()
    {
        if (playerFormShift == null) return;

        float distance = Vector3.Distance(transform.position, playerFormShift.transform.position);
        
        if (distance <= detectionRange)
        {
            // Only collect if visible enough (in shadow mode)
            if (!onlyVisibleInShadowMode || currentAlpha > 0.5f)
            {
                CollectFragment();
            }
        }
    }

    void CollectFragment()
    {
        if (hasBeenCollected) return;

        hasBeenCollected = true;

        // Add time bonus to loop manager using correct method name
        LoopManager loopManager = FindObjectOfType<LoopManager>();
        if (loopManager != null)
        {
            loopManager.ReportProgress(timeBonus); // 10 seconds bonus
            Debug.Log($"Memory fragment collected! Added {timeBonus} seconds.");
        }

        // Update fragment counter using correct method name
        if (fragmentCounter != null)
        {
            fragmentCounter.AddFragment(fragmentID, gameObject.name);
        }

        // Play collection effects
        PlayCollectionEffects();

        // Hide or destroy fragment
        if (oneTimeCollection)
        {
            StartCoroutine(DestroyAfterEffect());
        }
        else
        {
            // Reset for re-collection
            hasBeenCollected = false;
        }
    }

    void PlayCollectionEffects()
    {
        // Play particle effect
        if (collectionEffect != null)
        {
            collectionEffect.Play();
        }

        // Play sound
        if (audioSource != null && collectionSound != null)
        {
            audioSource.PlayOneShot(collectionSound);
        }

        // Flash effect
        if (fragmentLight != null)
        {
            StartCoroutine(FlashEffect());
        }
    }

    IEnumerator FlashEffect()
    {
        float originalIntensity = fragmentLight.intensity;
        
        for (int i = 0; i < 3; i++)
        {
            fragmentLight.intensity = originalIntensity * 3f;
            yield return new WaitForSeconds(0.1f);
            fragmentLight.intensity = originalIntensity;
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator DestroyAfterEffect()
    {
        // Wait for effects to finish
        yield return new WaitForSeconds(1f);
        
        // Fade out
        float fadeTime = 1f;
        float elapsed = 0f;
        
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(currentAlpha, 0f, elapsed / fadeTime);
            
            if (fragmentMaterial != null)
            {
                Color color = fragmentMaterial.color;
                color.a = alpha;
                fragmentMaterial.color = color;
            }
            
            if (fragmentLight != null)
            {
                fragmentLight.intensity = alpha * 2f;
            }
            
            yield return null;
        }
        
        gameObject.SetActive(false);
    }

    public void ResetFragment()
    {
        hasBeenCollected = false;
        gameObject.SetActive(true);
        transform.position = startPosition;
        
        if (fragmentMaterial != null)
        {
            Color color = shadowModeColor;
            color.a = 1f;
            fragmentMaterial.color = color;
        }

        // Reset particle effects
        if (ambientEffect != null && playerFormShift != null && playerFormShift.IsShadow())
        {
            ambientEffect.Play();
        }
    }

    // Getters
    public bool HasBeenCollected() => hasBeenCollected;
    public float GetTimeBonus() => timeBonus;
    public int GetFragmentID() => fragmentID;

    void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Draw visibility indicator
        if (Application.isPlaying)
        {
            Gizmos.color = currentAlpha > 0.5f ? Color.green : Color.red;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 2f, Vector3.one * 0.5f);
        }
    }
}

