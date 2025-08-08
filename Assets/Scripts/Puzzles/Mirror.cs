using UnityEngine;
using UnityEngine.Events;

public class Mirror : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 60f;
    public bool allowXRotation = true;
    public bool allowYRotation = true;
    public bool allowZRotation = false;
    
    [Header("Rotation Limits")]
    public bool useRotationLimits = false;
    public Vector3 minRotation = new Vector3(-45f, -45f, 0f);
    public Vector3 maxRotation = new Vector3(45f, 45f, 0f);

    [Header("Interaction")]
    public float interactionRange = 3f;
    public KeyCode rotateUpKey = KeyCode.UpArrow;
    public KeyCode rotateDownKey = KeyCode.DownArrow;
    public KeyCode rotateLeftKey = KeyCode.LeftArrow;
    public KeyCode rotateRightKey = KeyCode.RightArrow;
    public KeyCode resetKey = KeyCode.R;

    [Header("Visual Feedback")]
    public GameObject interactionPrompt;
    public Renderer mirrorRenderer;
    public Material normalMaterial;
    public Material highlightMaterial;
    public ParticleSystem reflectionEffect;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip rotationSound;
    public AudioClip resetSound;

    [Header("Events")]
    public UnityEvent OnPlayerEnterRange;
    public UnityEvent OnPlayerExitRange;
    public UnityEvent OnMirrorRotated;
    public UnityEvent OnMirrorReset;

    private bool playerInRange = false;
    private GameObject player;
    private Vector3 initialRotation;
    private Vector3 currentEulerAngles;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        initialRotation = transform.eulerAngles;
        currentEulerAngles = initialRotation;
        
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        UpdateMaterial(false);
    }

    void Update()
    {
        CheckPlayerProximity();
        
        if (playerInRange)
        {
            HandleInput();
        }
    }

    void CheckPlayerProximity()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.transform.position);
        bool wasInRange = playerInRange;
        playerInRange = distance <= interactionRange;

        if (playerInRange != wasInRange)
        {
            if (playerInRange)
            {
                OnPlayerEnterRange?.Invoke();
                ShowInteractionPrompt(true);
                UpdateMaterial(true);
            }
            else
            {
                OnPlayerExitRange?.Invoke();
                ShowInteractionPrompt(false);
                UpdateMaterial(false);
            }
        }
    }

    void HandleInput()
    {
        Vector3 rotationInput = Vector3.zero;
        bool hasInput = false;

        // Vertical rotation (X-axis)
        if (allowXRotation)
        {
            if (Input.GetKey(rotateUpKey))
            {
                rotationInput.x = -1f;
                hasInput = true;
            }
            else if (Input.GetKey(rotateDownKey))
            {
                rotationInput.x = 1f;
                hasInput = true;
            }
        }

        // Horizontal rotation (Y-axis)
        if (allowYRotation)
        {
            if (Input.GetKey(rotateLeftKey))
            {
                rotationInput.y = -1f;
                hasInput = true;
            }
            else if (Input.GetKey(rotateRightKey))
            {
                rotationInput.y = 1f;
                hasInput = true;
            }
        }

        // Apply rotation
        if (hasInput)
        {
            RotateMirror(rotationInput);
        }

        // Reset rotation
        if (Input.GetKeyDown(resetKey))
        {
            ResetRotation();
        }
    }

    void RotateMirror(Vector3 rotationInput)
    {
        Vector3 deltaRotation = rotationInput * rotationSpeed * Time.deltaTime;
        currentEulerAngles += deltaRotation;

        // Apply rotation limits
        if (useRotationLimits)
        {
            currentEulerAngles.x = Mathf.Clamp(currentEulerAngles.x, minRotation.x, maxRotation.x);
            currentEulerAngles.y = Mathf.Clamp(currentEulerAngles.y, minRotation.y, maxRotation.y);
            currentEulerAngles.z = Mathf.Clamp(currentEulerAngles.z, minRotation.z, maxRotation.z);
        }

        transform.eulerAngles = currentEulerAngles;

        // Play rotation sound
        if (audioSource != null && rotationSound != null && !audioSource.isPlaying)
        {
            audioSource.PlayOneShot(rotationSound);
        }

        // Play reflection effect
        if (reflectionEffect != null && !reflectionEffect.isPlaying)
        {
            reflectionEffect.Play();
        }

        OnMirrorRotated?.Invoke();
    }

    public void ResetRotation()
    {
        currentEulerAngles = initialRotation;
        transform.eulerAngles = initialRotation;

        // Play reset sound
        if (audioSource != null && resetSound != null)
        {
            audioSource.PlayOneShot(resetSound);
        }

        OnMirrorReset?.Invoke();
        
        Debug.Log("Mirror rotation reset");
    }

    void ShowInteractionPrompt(bool show)
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(show);
    }

    void UpdateMaterial(bool highlighted)
    {
        if (mirrorRenderer == null) return;

        if (highlighted && highlightMaterial != null)
            mirrorRenderer.material = highlightMaterial;
        else if (normalMaterial != null)
            mirrorRenderer.material = normalMaterial;
    }

    public void SetRotation(Vector3 eulerAngles)
    {
        currentEulerAngles = eulerAngles;
        transform.eulerAngles = eulerAngles;
    }

    public void SetRotationSpeed(float speed)
    {
        rotationSpeed = speed;
    }

    public void SetInteractionRange(float range)
    {
        interactionRange = range;
    }

    // Manual rotation methods for external control
    public void RotateUp() => RotateMirror(Vector3.left);
    public void RotateDown() => RotateMirror(Vector3.right);
    public void RotateLeft() => RotateMirror(Vector3.down);
    public void RotateRight() => RotateMirror(Vector3.up);

    // Getters
    public bool IsPlayerInRange() => playerInRange;
    public Vector3 GetCurrentRotation() => currentEulerAngles;
    public Vector3 GetInitialRotation() => initialRotation;

    void OnDrawGizmosSelected()
    {
        // Draw interaction range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
        
        // Draw mirror normal
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * 2f);
        
        // Draw rotation limits if enabled
        if (useRotationLimits)
        {
            Gizmos.color = Color.red;
            // This is a simplified visualization - in practice you'd want more complex gizmo drawing
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
        }
    }
}

