using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlatformElevator : MonoBehaviour
{
    [Header("Elevation Settings")]
    public float elevationHeight = 5f; // How high the platform should elevate
    public float elevationSpeed = 2f;  // How fast it moves
    public bool startsElevated = false; // If true, it starts at elevationHeight

    [Header("Player Detection")]
    public string playerTag = "Player"; // Ensure your player GameObject has this tag

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private bool isElevating = false;
    private bool isLowering = false;
    private int playersOnPlatform = 0; // Counter for multiple players/colliders

    // NEW: Store previous position to calculate delta movement
    private Vector3 previousPosition; 
    public Vector3 DeltaPosition { get; private set; } // NEW: Public property for player to access

    void Start()
    {
        // Store the initial position of the platform
        startPosition = transform.position;

        // Calculate the target elevated position
        targetPosition = startPosition + Vector3.up * elevationHeight;

        // Set initial state based on 'startsElevated'
        if (startsElevated)
        {
            transform.position = targetPosition;
        }
        else
        {
            transform.position = startPosition;
        }

        // Initialize previousPosition
        previousPosition = transform.position;

        // Ensure the platform has a collider for detection
        Collider platformCollider = GetComponent<Collider>();
        if (platformCollider == null)
        {
            Debug.LogWarning($"PlatformElevator on {gameObject.name} requires a Collider component to detect player contact!");
        }
        // IMPORTANT: For OnTriggerEnter/Exit, make sure 'Is Trigger' is CHECKED on the platform's collider.
    }

    void Update()
    {
        // Calculate delta position before moving
        DeltaPosition = transform.position - previousPosition;
        previousPosition = transform.position;

        if (isElevating)
        {
            // Move platform upwards
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, elevationSpeed * Time.deltaTime);
            if (transform.position == targetPosition)
            {
                isElevating = false; // Stop elevating when target reached
                Debug.Log($"{gameObject.name} finished elevating.");
            }
        }
        else if (isLowering)
        {
            // Move platform downwards
            transform.position = Vector3.MoveTowards(transform.position, startPosition, elevationSpeed * Time.deltaTime);
            if (transform.position == startPosition)
            {
                isLowering = false; // Stop lowering when start position reached
                Debug.Log($"{gameObject.name} finished lowering.");
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Debug statement to check for any trigger entry
        Debug.Log($"Platform {gameObject.name} trigger entered by: {other.gameObject.name}");

        if (other.CompareTag(playerTag))
        {
            playersOnPlatform++;
            if (playersOnPlatform == 1) // Only elevate on the first player contact
            {
                isElevating = true;
                isLowering = false;
                Debug.Log($"{gameObject.name} received Player contact (OnTriggerEnter). Starting elevation.");
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Debug statement to check for any trigger exit
        Debug.Log($"Platform {gameObject.name} trigger exited by: {other.gameObject.name}");

        if (other.CompareTag(playerTag))
        {
            playersOnPlatform--;
            if (playersOnPlatform <= 0) // Only lower when no players are left
            {
                playersOnPlatform = 0; // Ensure it doesn't go negative
                isLowering = true;
                isElevating = false;
                Debug.Log($"{gameObject.name} Player exited (OnTriggerExit). Starting lowering.");
            }
        }
    }
}
