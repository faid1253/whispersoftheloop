using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class LightPlayer : MonoBehaviour
{
    [Header("Light Beam Settings")]
    public float maxDistance = 50f;
    public int maxReflections = 5;
    public LayerMask reflectableLayers = 4096; // Layer 12 (Mirror)
    public LayerMask blockingLayers = 512; // Layer 9 (Ground)

    [Header("Form Requirements")]
    public bool requiresLightForm = true;
    public float activationRange = 5f;
    public bool oneTimeActivation = true;

    [Header("Player Mirror Settings")]
    public bool playerActsAsMirror = true;
    public LayerMask receiverLayer; // New: Assign the "GhostWalkable" layer here in the Inspector!
    public LayerMask playerLayer = 256; // Layer 8 (Player)
    public bool forceHorizontalReflection = true;
    public bool shadowModePassesThrough = true; // NEW: Shadow mode ignores light beams

    [Header("Visual Settings")]
    public Color beamColor = Color.white;
    public float beamWidth = 0.1f;
    public Material beamMaterial;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip activationSound;

    [Header("Debug")] // Added debug header for clarity
    public bool showDebugMessages = true; // Ensure this is checked in Inspector

    private LineRenderer lineRenderer;
    private FormShift playerFormShift;
    private bool isActive = false;
    private bool hasBeenActivated = false;
    private bool playerInRange = false;
    private List<Vector3> beamPoints = new List<Vector3>();
    private List<LightReceiver> hitReceivers = new List<LightReceiver>();

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        SetupLineRenderer();
        
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerFormShift = player.GetComponent<FormShift>();
        else
            Debug.LogWarning("LightSource: Player GameObject with 'Player' tag not found!");

        // Ensure the light source starts in a clean, unactivated state for testing
        ResetActivation(); 
        if (showDebugMessages) Debug.Log($"LightSource '{name}' initialized. hasBeenActivated: {hasBeenActivated}, isActive: {isActive}");
    }

    void SetupLineRenderer()
    {
        lineRenderer.material = beamMaterial;
        lineRenderer.startWidth = beamWidth;
        lineRenderer.endWidth = beamWidth;
        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = 0;
        
        if (beamMaterial != null)
        {
            beamMaterial.color = beamColor;
        }
    }

    void Update()
    {
        if (showDebugMessages) Debug.Log($"LightSource '{name}' Update: hasBeenActivated={hasBeenActivated}, oneTimeActivation={oneTimeActivation}, isActive={isActive}");

        // Only check proximity and update beam state if one-time activation is off,
        // or if it's on but hasn't been activated yet.
        if (!oneTimeActivation || !hasBeenActivated)
        {
            CheckPlayerProximity();
            UpdateBeamState();
        }
        else if (showDebugMessages)
        {
            Debug.Log($"LightSource '{name}': Skipping proximity and beam state update because oneTimeActivation is true and hasBeenActivated is true.");
        }
        
        // If the beam is active, calculate its path
        if (isActive)
        {
            CalculateBeamPath();
        }
        else
        {
            // If not active, clear the beam visuals and deactivate receivers
            ClearBeam();
        }
    }

    void CheckPlayerProximity()
    {
        if (playerFormShift == null) return;

        float distance = Vector3.Distance(transform.position, playerFormShift.transform.position);
        bool wasInRange = playerInRange;
        playerInRange = distance <= activationRange;

        if (showDebugMessages) Debug.Log($"LightSource '{name}': Player distance={distance}, activationRange={activationRange}, playerInRange={playerInRange}.");

        // Play sound only when entering range
        if (playerInRange && !wasInRange && audioSource != null && activationSound != null)
        {
            audioSource.PlayOneShot(activationSound);
            if (showDebugMessages) Debug.Log($"LightSource '{name}': Playing activation sound.");
        }
    }

    void UpdateBeamState()
    {
        bool shouldBeActive = ShouldBeActive();
        if (showDebugMessages) Debug.Log($"LightSource '{name}': UpdateBeamState. shouldBeActive={shouldBeActive}, current isActive={isActive}.");
        
        if (shouldBeActive != isActive)
        {
            isActive = shouldBeActive;
            
            // If the beam just became active and one-time activation is enabled, mark it as activated
            if (isActive && oneTimeActivation)
            {
                hasBeenActivated = true;
                if (showDebugMessages) Debug.Log($"LightSource '{name}': Beam activated for one-time. hasBeenActivated set to TRUE.");
            }
            
            // If the beam just became inactive, clear any lit receivers
            if (!isActive)
            {
                ClearReceivers();
                if (showDebugMessages) Debug.Log($"LightSource '{name}': Beam became inactive. Clearing receivers.");
            }
        }
    }

    bool ShouldBeActive()
    {
        bool result = false;
        if (requiresLightForm && playerFormShift != null)
        {
            result = playerInRange && playerFormShift.IsLight();
            if (showDebugMessages) Debug.Log($"LightSource '{name}': ShouldBeActive (requiresLightForm): playerInRange={playerInRange}, IsLight={playerFormShift.IsLight()}, Result={result}.");
        }
        else
        {
            result = playerInRange;
            if (showDebugMessages) Debug.Log($"LightSource '{name}': ShouldBeActive (no requiresLightForm): playerInRange={playerInRange}, Result={result}.");
        }
        return result;
    }

    void CalculateBeamPath()
    {
        beamPoints.Clear();
        ClearReceivers(); // Clear receivers from previous frame before re-calculating
        
        Vector3 currentPosition = transform.position;
        Vector3 currentDirection = transform.forward;
        
        beamPoints.Add(currentPosition);
        
        for (int i = 0; i < maxReflections; i++)
        {
            RaycastHit hit;
            
            // --- IMPORTANT: Include receiverLayer in allLayers ---
            LayerMask allLayers = reflectableLayers | blockingLayers | playerLayer | receiverLayer; 
            
            // Optional: for visual debugging in the Scene view
            Debug.DrawRay(currentPosition, currentDirection * maxDistance, Color.cyan); 

            if (Physics.Raycast(currentPosition, currentDirection, out hit, maxDistance, allLayers))
            {
                beamPoints.Add(hit.point);
                
                // Check if we hit the player (acts as mirror, only if not in shadow mode or passes through disabled)
                if (playerActsAsMirror && hit.collider.CompareTag("Player"))
                {
                    // Skip reflection if player is in shadow mode and passes through is enabled
                    if (shadowModePassesThrough && playerFormShift != null && playerFormShift.IsShadow())
                    {
                        // Continue beam through player
                        currentPosition = hit.point + currentDirection * 0.01f;
                        if (showDebugMessages) Debug.Log("LightSource: Player in shadow mode, beam passes through.");
                        continue; 
                    }
                    
                    Vector3 playerNormal;
                    
                    if (forceHorizontalReflection)
                    {
                        // Force horizontal reflection - ignore Y component
                        Vector3 horizontalDirection = new Vector3(currentDirection.x, 0, currentDirection.z).normalized;
                        // For a capsule, the normal might be tricky. A simple horizontal reflection might work better.
                        // We'll use the hit.normal but flatten it.
                        playerNormal = new Vector3(hit.normal.x, 0, hit.normal.z).normalized; 
                        currentDirection = Vector3.Reflect(horizontalDirection, playerNormal);
                        currentDirection.y = 0; // Keep it horizontal
                        currentDirection = currentDirection.normalized;
                    }
                    else
                    {
                        playerNormal = (hit.point - hit.collider.transform.position).normalized; // This is less accurate for a capsule
                        currentDirection = Vector3.Reflect(currentDirection, hit.normal); // Use actual hit normal for general reflection
                    }
                    
                    currentPosition = hit.point + currentDirection * 0.01f;
                    if (showDebugMessages) Debug.Log("LightSource: Light reflected off player horizontally!");
                    continue; 
                }
                
                // Check if we hit a receiver
                LightReceiver receiver = hit.collider.GetComponent<LightReceiver>();
                if (receiver != null)
                {
                    receiver.SetLit(true);
                    if (!hitReceivers.Contains(receiver))
                        hitReceivers.Add(receiver);
                    if (showDebugMessages) Debug.Log($"LightSource: Light hit receiver: {receiver.name}");
                }
                
                // Check if we hit a mirror
                Mirror mirror = hit.collider.GetComponent<Mirror>(); // Assuming 'Mirror' script exists
                if (mirror != null && ((1 << hit.collider.gameObject.layer) & reflectableLayers) != 0)
                {
                    currentDirection = Vector3.Reflect(currentDirection, hit.normal);
                    currentPosition = hit.point + currentDirection * 0.01f;
                    if (showDebugMessages) Debug.Log("LightSource: Light reflected off mirror!");
                    continue;
                }
                
                // If we hit a blocking surface, stop the beam
                if (((1 << hit.collider.gameObject.layer) & blockingLayers) != 0)
                {
                    if (showDebugMessages) Debug.Log("LightSource: Light blocked by surface.");
                    break;
                }
            }
            else
            {
                // If no hit, extend beam to max distance and stop
                beamPoints.Add(currentPosition + currentDirection * maxDistance);
                break;
            }
        }
        
        // Update line renderer visuals
        lineRenderer.positionCount = beamPoints.Count;
        for (int i = 0; i < beamPoints.Count; i++)
        {
            lineRenderer.SetPosition(i, beamPoints[i]);
        }
    }

    void ClearBeam()
    {
        lineRenderer.positionCount = 0;
        beamPoints.Clear();
        ClearReceivers();
    }

    void ClearReceivers()
    {
        foreach (LightReceiver receiver in hitReceivers)
        {
            if (receiver != null)
                receiver.SetLit(false); // Turn off receivers that were previously hit
        }
        hitReceivers.Clear();
    }

    // Public methods for external control/testing
    public void ResetActivation()
    {
        hasBeenActivated = false;
        isActive = false;
        ClearBeam(); // Ensure beam is cleared on reset
        if (showDebugMessages) Debug.Log($"LightSource '{name}': ResetActivation called. hasBeenActivated={hasBeenActivated}, isActive={isActive}.");
    }

    public bool IsActive() => isActive;
    public bool IsPlayerInRange() => playerInRange;
    public bool HasBeenActivated() => hasBeenActivated;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, activationRange);
        
        Gizmos.color = beamColor;
        Gizmos.DrawRay(transform.position, transform.forward * 3f);
        
        // Draw the beam path in Gizmos for debugging in editor
        if (Application.isPlaying && beamPoints.Count > 1)
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < beamPoints.Count - 1; i++)
            {
                Gizmos.DrawLine(beamPoints[i], beamPoints[i + 1]);
            }
        }
    }
}
