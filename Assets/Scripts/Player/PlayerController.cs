using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float gravity = -20f;
    public float jumpHeight = 1.6f;
    
    [Header("Camera Settings")]
    public Transform cameraPivot; // Assign your CameraPivot GameObject here
    public float lookSensitivity = 1f;
    public float verticalLookLimit = 85f; // Prevents camera from flipping over
    
    private CharacterController cc;
    private Vector3 vel; // Current velocity, primarily for gravity/jump
    private float cameraRotationX = 0f; // Stores the current vertical camera rotation

    // Variable to store the PlatformElevator we are currently on
    private PlatformElevator currentPlatform; 

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        
        // Lock the cursor to the center of the screen
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleCameraRotation();

        // Calculate player input movement
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 playerInputMove = Vector3.zero;
        if (new Vector3(h, 0f, v).magnitude >= 0.1f)
        {
            Vector3 dir = new Vector3(h, 0f, v).normalized;
            playerInputMove = transform.forward * v + transform.right * h;
            playerInputMove = playerInputMove.normalized * moveSpeed * Time.deltaTime;
        }

        // Apply gravity
        // Only apply gravity if not grounded, or if vel.y is already positive (jumping)
        if (!cc.isGrounded || vel.y > 0) 
        {
            vel.y += gravity * Time.deltaTime;
        }
        else if (cc.isGrounded && vel.y < 0) // Reset vertical velocity if grounded and falling
        {
            vel.y = -2f; 
            Debug.Log("PlayerController: Resetting vel.y to -2f (grounded).");
        }

        // Handle Jumping
        if (Input.GetButtonDown("Jump") && cc.isGrounded)
        {
            vel.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            Debug.Log($"PlayerController: Jumping! New vel.y: {vel.y}");
            // Clear currentPlatform on jump to ensure it's not trying to apply delta while airborne
            currentPlatform = null; 
            Debug.Log("PlayerController: Jumped, clearing currentPlatform reference.");
        }

        // Combine all movements into a single vector
        Vector3 combinedMove = playerInputMove + (vel * Time.deltaTime);

        // Apply platform movement if on a moving platform
        if (currentPlatform != null) 
        {
            Debug.Log($"PlayerController: Checking platform movement. currentPlatform: {currentPlatform.name}, IsGrounded: {cc.isGrounded}, PlatformDelta: {currentPlatform.DeltaPosition}"); 

            // If player is no longer grounded on this platform, clear the reference
            // This is a more robust check than just !cc.isGrounded
            if (!cc.isGrounded && !currentPlatform.GetComponent<Collider>().bounds.Intersects(cc.bounds))
            {
                Debug.Log($"PlayerController: Player NOT grounded and not intersecting {currentPlatform.name}. Clearing currentPlatform reference.");
                currentPlatform = null;
            }
            else // If still on platform (or just landed), add its delta to combined movement
            {
                combinedMove += currentPlatform.DeltaPosition;
                Debug.Log($"PlayerController: Added platform delta {currentPlatform.DeltaPosition} to combinedMove. New combinedMove: {combinedMove}");
            }
        }

        // Execute the single CharacterController.Move call
        cc.Move(combinedMove);
        // Debug.Log($"PlayerController: Final cc.Move applied. Player pos: {transform.position}. IsGrounded: {cc.isGrounded}");
    }

    void HandleCameraRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

        cameraRotationX -= mouseY; 
        cameraRotationX = Mathf.Clamp(cameraRotationX, -verticalLookLimit, verticalLookLimit);
        
        if (cameraPivot != null)
        {
            cameraPivot.localRotation = Quaternion.Euler(cameraRotationX, 0f, 0f);
        }
        else
        {
            Debug.LogWarning("Camera Pivot not assigned in PlayerController!");
        }

        transform.Rotate(Vector3.up * mouseX);
    }
    
    // HandleMovement, HandleJumping, ApplyGravity are now integrated into Update()
    // and no longer call cc.Move() directly.

    // OnControllerColliderHit is called when the CharacterController hits another collider
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Debug.Log($"PlayerController: OnControllerColliderHit with {hit.gameObject.name}. Hit Normal: {hit.normal}. IsGrounded (from CC): {cc.isGrounded}");

        PlatformElevator platform = hit.collider.GetComponent<PlatformElevator>();
        if (platform != null)
        {
            // Set currentPlatform if hitting the top of the platform AND the CC considers itself grounded.
            // hit.normal.y > 0.7f is a good threshold for detecting a hit from above.
            if (hit.normal.y > 0.7f && cc.isGrounded) 
            {
                currentPlatform = platform;
                Debug.Log($"Player is on platform: {platform.name}. Setting currentPlatform based on top hit and grounded state.");
            }
            else 
            {
                 Debug.Log($"Player hit {platform.name} but not from top OR not grounded. Not setting currentPlatform. Hit Normal.y: {hit.normal.y}, IsGrounded (from CC): {cc.isGrounded}");
            }
        }
        // No explicit clearing of currentPlatform here. It's handled in Update if !cc.isGrounded.
    }

    public bool IsGrounded()
    {
        return cc.isGrounded;
    }

    public Vector3 GetVelocity()
    {
        return cc.velocity;
    }
}
