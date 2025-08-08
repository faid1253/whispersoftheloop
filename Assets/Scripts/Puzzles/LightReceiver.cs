using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class LightReceiver : MonoBehaviour
{
    [Header("Receiver Settings")]
    public float activationDelay = 0.1f; // Keeping for future use, but bypassed for direct activation
    public float deactivationDelay = 0.1f; // Keeping for future use, but bypassed for direct deactivation
    public bool requiresContinuousLight = true; // Keeping for future use, but bypassed for direct deactivation
    public bool oneTimeActivation = false; // Keeping for future use, but bypassed for direct activation

    [Header("Visual Feedback")]
    public Renderer receiverRenderer;
    public Material litMaterial;
    public Material unlitMaterial;
    public Light receiverLight;
    public ParticleSystem activationEffect;
    public Color litColor = Color.green;
    public Color unlitColor = Color.red;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip activationSound;
    public AudioClip deactivationSound;

    [Header("Connected Objects")]
    public GameObject[] objectsToActivate;
    public GameObject[] objectsToDeactivate;
    public LightReceiver[] chainedReceivers;

    [Header("Events")]
    public UnityEvent OnLit;
    public UnityEvent OnUnlit;
    public UnityEvent OnActivated;
    public UnityEvent OnDeactivated;

    [Header("Debug")] // Re-added for clarity and control
    public bool showDebugMessages = true; // Ensure this is checked in Inspector

    private bool isLit = false;
    private bool isActivated = false;
    private bool hasBeenActivated = false;
    private Coroutine activationCoroutine;
    private Coroutine deactivationCoroutine;

    void Start()
    {
        // Auto-assign renderer if not set
        if (receiverRenderer == null)
            receiverRenderer = GetComponent<Renderer>();

        // Create materials if not assigned (important for visual feedback)
        if (litMaterial == null)
        {
            litMaterial = new Material(Shader.Find("Standard"));
            litMaterial.color = litColor;
            if (showDebugMessages) Debug.Log($"LightReceiver '{name}': Created default litMaterial.");
        }
        
        if (unlitMaterial == null)
        {
            unlitMaterial = new Material(Shader.Find("Standard"));
            unlitMaterial.color = unlitColor;
            if (showDebugMessages) Debug.Log($"LightReceiver '{name}': Created default unlitMaterial.");
        }

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        UpdateVisuals();
        if (showDebugMessages)
            Debug.Log($"LightReceiver '{name}' initialized. Current lit state: {isLit}");
    }

    public void SetLit(bool lit)
    {
        if (showDebugMessages) Debug.Log($"LightReceiver '{name}': SetLit called with 'lit' = {lit}. Current isLit: {isLit}, isActivated: {isActivated}, hasBeenActivated: {hasBeenActivated}, oneTimeActivation: {oneTimeActivation}");

        // If the lit state hasn't changed, or it's one-time activated and trying to unlit, return.
        // This 'oneTimeActivation' check is now the only one at the top.
        if (isLit == lit) 
        {
            if (showDebugMessages) Debug.Log($"LightReceiver '{name}': Lit state already {lit}. Returning.");
            return;
        }
        
        // This is the oneTimeActivation check that prevents unlit after first activation
        if (oneTimeActivation && hasBeenActivated && !lit)
        {
            if (showDebugMessages) Debug.Log($"LightReceiver '{name}': One-time activated, preventing unlit. Returning.");
            return;
        }

        isLit = lit; // Update the internal lit state

        // Cancel any running coroutines to prevent conflicting activations/deactivations
        if (activationCoroutine != null)
        {
            StopCoroutine(activationCoroutine);
            activationCoroutine = null;
            if (showDebugMessages) Debug.Log($"LightReceiver '{name}': Cancelled activation coroutine.");
        }
        if (deactivationCoroutine != null)
        {
            StopCoroutine(deactivationCoroutine);
            deactivationCoroutine = null;
            if (showDebugMessages) Debug.Log($"LightReceiver '{name}': Cancelled deactivation coroutine.");
        }

        if (lit) // If becoming lit
        {
            OnLit?.Invoke();
            if (showDebugMessages) Debug.Log($"LightReceiver '{name}': OnLit event invoked. Calling Activate() directly.");
            Activate(); // Directly call Activate() - bypassing delays for immediate activation
        }
        else // If becoming unlit
        {
            OnUnlit?.Invoke();
            if (showDebugMessages) Debug.Log($"LightReceiver '{name}': OnUnlit event invoked. Calling Deactivate() directly.");
            Deactivate(); // Directly call Deactivate() - bypassing delays for immediate deactivation
        }

        UpdateVisuals(); // Always update visuals after state change
    }

    // DelayedActivation and DelayedDeactivation coroutines are now unused due to direct calls in SetLit,
    // but kept for future re-implementation if delays are needed again.
    IEnumerator DelayedActivation()
    {
        if (showDebugMessages) Debug.Log($"LightReceiver '{name}': DelayedActivation coroutine running. Waiting {activationDelay}s...");
        yield return new WaitForSeconds(activationDelay);
        if (isLit) Activate();
        else if (showDebugMessages) Debug.Log($"LightReceiver '{name}': DelayedActivation finished, but isLit is now false. Not activating.");
    }

    IEnumerator DelayedDeactivation()
    {
        if (showDebugMessages) Debug.Log($"LightReceiver '{name}': DelayedDeactivation coroutine running. Waiting {deactivationDelay}s...");
        yield return new WaitForSeconds(deactivationDelay);
        if (!isLit) Deactivate();
        else if (showDebugMessages) Debug.Log($"LightReceiver '{name}': DelayedDeactivation finished, but isLit is now true. Not deactivating.");
    }

    void Activate()
    {
        if (isActivated) // Already active, prevent re-activation
        {
            if (showDebugMessages) Debug.Log($"LightReceiver '{name}': Already activated. Returning from Activate().");
            return;
        }

        isActivated = true;
        hasBeenActivated = true; // Mark as activated for one-time use

        if (showDebugMessages)
            Debug.Log($"LightReceiver '{name}' ACTIVATING {objectsToActivate.Length} objects. isActivated: {isActivated}, hasBeenActivated: {hasBeenActivated}");

        // Activate connected objects
        for (int i = 0; i < objectsToActivate.Length; i++)
        {
            if (objectsToActivate[i] != null)
            {
                objectsToActivate[i].SetActive(true);
                if (showDebugMessages)
                    Debug.Log($"Activated object: {objectsToActivate[i].name}");
            }
            else
            {
                if (showDebugMessages)
                    Debug.LogWarning($"Object at index {i} in objectsToActivate is null in LightReceiver '{name}'!");
            }
        }

        // Deactivate specified objects
        for (int i = 0; i < objectsToDeactivate.Length; i++)
        {
            if (objectsToDeactivate[i] != null)
            {
                objectsToDeactivate[i].SetActive(false);
                if (showDebugMessages)
                    Debug.Log($"Deactivated object: {objectsToDeactivate[i].name}");
            }
            else
            {
                if (showDebugMessages)
                    Debug.LogWarning($"Object at index {i} in objectsToDeactivate is null in LightReceiver '{name}'!");
            }
        }

        // Chain to other receivers
        foreach (LightReceiver receiver in chainedReceivers)
        {
            if (receiver != null)
            {
                receiver.SetLit(true);
                if (showDebugMessages)
                    Debug.Log($"Chaining to receiver: {receiver.name}");
            }
        }

        // Play activation effects
        PlayActivationEffects();

        OnActivated?.Invoke();
        if (showDebugMessages) Debug.Log($"Light receiver '{gameObject.name}' OnActivated event invoked.");
    }

    void Deactivate()
    {
        // If one-time activation is true and it has been activated, prevent deactivation
        if (oneTimeActivation && hasBeenActivated)
        {
            if (showDebugMessages) Debug.Log($"LightReceiver '{name}': One-time activated, preventing deactivation. Returning from Deactivate().");
            return;
        }

        if (!isActivated) // Already inactive, prevent re-deactivation
        {
            if (showDebugMessages) Debug.Log($"LightReceiver '{name}': Already deactivated. Returning from Deactivate().");
            return;
        }

        isActivated = false;

        if (showDebugMessages)
            Debug.Log($"LightReceiver '{name}' DEACTIVATING objects. isActivated: {isActivated}");

        // Deactivate connected objects
        for (int i = 0; i < objectsToActivate.Length; i++)
        {
            if (objectsToActivate[i] != null)
            {
                objectsToActivate[i].SetActive(false);
                if (showDebugMessages)
                    Debug.Log($"Deactivated object: {objectsToActivate[i].name}");
            }
            else
            {
                if (showDebugMessages)
                    Debug.LogWarning($"Object at index {i} in objectsToActivate is null during Deactivate in LightReceiver '{name}'!");
            }
        }

        // Reactivate specified objects
        for (int i = 0; i < objectsToDeactivate.Length; i++)
        {
            if (objectsToDeactivate[i] != null)
            {
                objectsToDeactivate[i].SetActive(true);
                if (showDebugMessages)
                    Debug.Log($"Reactivated object: {objectsToDeactivate[i].name}");
            }
            else
            {
                if (showDebugMessages)
                    Debug.LogWarning($"Object at index {i} in objectsToDeactivate is null during Deactivate in LightReceiver '{name}'!");
            }
        }

        // Unchain from other receivers
        foreach (LightReceiver receiver in chainedReceivers)
        {
            if (receiver != null)
            {
                receiver.SetLit(false);
                if (showDebugMessages)
                    Debug.Log($"Unchaining from receiver: {receiver.name}");
            }
        }

        // Play deactivation effects
        PlayDeactivationEffects();

        OnDeactivated?.Invoke();
        if (showDebugMessages) Debug.Log($"Light receiver '{gameObject.name}' OnDeactivated event invoked.");
    }

    void PlayActivationEffects()
    {
        // Play particle effect
        if (activationEffect != null)
            activationEffect.Play();

        // Play sound
        if (audioSource != null && activationSound != null)
            audioSource.PlayOneShot(activationSound);
    }

    void PlayDeactivationEffects()
    {
        // Stop particle effect
        if (activationEffect != null)
            activationEffect.Stop();

        // Play sound
        if (audioSource != null && deactivationSound != null)
            audioSource.PlayOneShot(deactivationSound);
    }

    void UpdateVisuals()
    {
        // Update material
        if (receiverRenderer != null)
        {
            if (isLit && litMaterial != null)
            {
                receiverRenderer.material = litMaterial;
                if (showDebugMessages) Debug.Log($"LightReceiver '{name}': Visual set to LIT material.");
            }
            else if (!isLit && unlitMaterial != null)
            {
                receiverRenderer.material = unlitMaterial;
                if (showDebugMessages) Debug.Log($"LightReceiver '{name}': Visual set to UNLIT material.");
            }
            else
            {
                if (showDebugMessages) Debug.LogWarning($"LightReceiver '{name}': Renderer or materials not assigned for visual update.");
            }
        }
        else
        {
            if (showDebugMessages) Debug.LogWarning($"LightReceiver '{name}': No receiverRenderer assigned for visual update!");
        }

        // Update light
        if (receiverLight != null)
        {
            receiverLight.enabled = isLit;
            receiverLight.color = isLit ? litColor : unlitColor;
            if (showDebugMessages) Debug.Log($"LightReceiver '{name}': ReceiverLight enabled: {isLit}, color: {(isLit ? litColor : unlitColor)}.");
        }
        else
        {
            if (showDebugMessages) Debug.LogWarning($"LightReceiver '{name}': No receiverLight assigned!");
        }
    }

    public void ForceActivate()
    {
        if (showDebugMessages) Debug.Log($"LightReceiver '{name}': ForceActivate called.");
        SetLit(true);
    }

    public void ForceDeactivate()
    {
        if (showDebugMessages) Debug.Log($"LightReceiver '{name}': ForceDeactivate called.");
        SetLit(false);
    }

    public void Reset()
    {
        if (showDebugMessages) Debug.Log($"LightReceiver '{name}': Resetting state.");
        isLit = false;
        isActivated = false;
        hasBeenActivated = false;
        
        if (activationCoroutine != null)
        {
            StopCoroutine(activationCoroutine);
            activationCoroutine = null;
        }
        if (deactivationCoroutine != null)
        {
            StopCoroutine(deactivationCoroutine);
            deactivationCoroutine = null;
        }

        // Ensure connected objects are in their default (deactivated) state on reset
        foreach (GameObject obj in objectsToActivate)
        {
            if (obj != null) obj.SetActive(false);
        }
        foreach (GameObject obj in objectsToDeactivate)
        {
            if (obj != null) obj.SetActive(true); // Assuming objectsToDeactivate are active by default
        }
        foreach (LightReceiver receiver in chainedReceivers)
        {
            if (receiver != null) receiver.SetLit(false);
        }

        UpdateVisuals();
    }

    // Getters
    public bool IsLit() => isLit;
    public bool IsActivated() => isActivated;
    public bool HasBeenActivated() => hasBeenActivated;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = isLit ? Color.green : Color.red;
        Gizmos.DrawWireCube(transform.position, Vector3.one);
        
        // Draw connections to activated objects
        Gizmos.color = Color.blue;
        foreach (GameObject obj in objectsToActivate)
        {
            if (obj != null)
                Gizmos.DrawLine(transform.position, obj.transform.position);
        }
        
        // Draw connections to chained receivers
        Gizmos.color = Color.yellow;
        foreach (LightReceiver receiver in chainedReceivers)
        {
            if (receiver != null)
                Gizmos.DrawLine(transform.position, receiver.transform.position);
        }
    }
}
