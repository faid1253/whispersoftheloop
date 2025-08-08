using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class InfiniteGameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public float loopDuration = 60f; // 1 minute timer
    public float platformResetHeight = 1.5f; // Auto-reset when platform reaches this height (lowered for quicker reset)
    public KeyCode leaveKey = KeyCode.L; // L key to leave/quit

    [Header("Reset Components")]
    public Manager puzzleSpawner;
    public LoopManager loopManager;
    public Transform[] memoryFragments; // Assign all memory fragments
    public Transform[] platforms; // Assign all platforms to monitor

    [Header("Player")]
    public Transform player;
    public Transform spawnPoint;

    [Header("UI")]
    public bool showInstructions = true;
    public float instructionDisplayTime = 5f;

    [Header("Debug")]
    public bool showDebugInfo = true;
    public bool showPlatformHeights = false; // NEW: Show platform heights in console

    private bool gameActive = true;
    private float instructionTimer = 2f;
    private int currentRound = 1;
    private List<MemoryFragment> allFragments = new List<MemoryFragment>();
    private float lastHeightCheck = 0f;
    private float heightCheckInterval = 0.1f; // Check every 0.1 seconds for more responsiveness

    void Start()
    {
        InitializeGame();
        
        if (showInstructions)
        {
            instructionTimer = instructionDisplayTime;
        }
    }

    void Update()
    {
        if (!gameActive) return;

        // Check for leave key
        if (Input.GetKeyDown(leaveKey))
        {
            LeaveGame();
            return;
        }

        // Update instruction timer
        if (instructionTimer > 0f)
        {
            instructionTimer -= Time.deltaTime;
        }

        // Check platform heights more frequently for quicker response
        if (Time.time - lastHeightCheck >= heightCheckInterval)
        {
            CheckPlatformHeights();
            lastHeightCheck = Time.time;
        }

        // Debug info
        if (showDebugInfo && Input.GetKeyDown(KeyCode.R))
        {
            ForceReset();
        }
    }

    void InitializeGame()
    {
        // Auto-find components if not assigned
        if (puzzleSpawner == null)
            puzzleSpawner = FindObjectOfType<Manager>();
            
        if (loopManager == null)
            loopManager = FindObjectOfType<LoopManager>();
            
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        // Find all memory fragments
        FindAllMemoryFragments();

        // Set loop duration to 1 minute
        if (loopManager != null)
        {
            loopManager.SetLoopDuration(loopDuration);
        }

        // Start first round
        StartNewRound();
        
        if (showDebugInfo)
            Debug.Log($"Infinite Game Manager initialized - Round {currentRound} - Reset Height: {platformResetHeight}");
    }

    void FindAllMemoryFragments()
    {
        allFragments.Clear();
        MemoryFragment[] fragments = FindObjectsOfType<MemoryFragment>();
        
        foreach (MemoryFragment fragment in fragments)
        {
            allFragments.Add(fragment);
        }
        
        if (showDebugInfo)
            Debug.Log($"Found {allFragments.Count} memory fragments");
    }

    void CheckPlatformHeights()
    {
        if (platforms == null || platforms.Length == 0)
        {
            // Auto-find platforms if not assigned
            FindActivationPlatforms();
        }

        // Check each platform height
        foreach (Transform platform in platforms)
        {
            if (platform != null && platform.gameObject.activeInHierarchy)
            {
                float currentHeight = platform.position.y;
                
                if (showPlatformHeights && showDebugInfo)
                {
                    Debug.Log($"Platform {platform.name} height: {currentHeight:F2}");
                }
                
                if (currentHeight >= platformResetHeight)
                {
                    if (showDebugInfo)
                        Debug.Log($"Platform {platform.name} reached height {currentHeight:F2} (trigger: {platformResetHeight}), triggering reset");
                    
                    TriggerReset();
                    return; // Only reset once per frame
                }
            }
        }
    }

    void FindActivationPlatforms()
    {
        // Look for activation platforms specifically
        List<Transform> foundPlatforms = new List<Transform>();
        
        // First try to find by tag
        GameObject[] platformObjects = GameObject.FindGameObjectsWithTag("Platform");
        if (platformObjects.Length > 0)
        {
            foreach (GameObject obj in platformObjects)
            {
                // Only include platforms that can rise (have PlatformRiser component or are activation platforms)
                if (obj.name.ToLower().Contains("activation") || obj.GetComponent<PlatformRiser>() != null)
                {
                    foundPlatforms.Add(obj.transform);
                }
            }
        }
        
        // If no tagged platforms found, look for objects with "ActivationPlatform" in name
        if (foundPlatforms.Count == 0)
        {
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.ToLower().Contains("activationplatform"))
                {
                    foundPlatforms.Add(obj.transform);
                }
            }
        }
        
        platforms = foundPlatforms.ToArray();
        
        if (showDebugInfo)
            Debug.Log($"Found {platforms.Length} activation platforms to monitor");
    }

    void TriggerReset()
    {
        currentRound++;
        StartNewRound();
        
        if (showDebugInfo)
            Debug.Log($"Auto-reset triggered at height {platformResetHeight} - Starting Round {currentRound}");
    }

    void StartNewRound()
    {
        // Reset player position
        if (player != null && spawnPoint != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null)
                cc.enabled = false;
                
            player.position = spawnPoint.position;
            player.rotation = spawnPoint.rotation;
            
            if (cc != null)
                cc.enabled = true;
        }

        // Reset loop manager
        if (loopManager != null)
        {
            loopManager.SetLoopDuration(loopDuration); // 1 minute
            loopManager.StartLoop();
        }

        // Randomize puzzle layout (this will create new platforms at ground level)
        if (puzzleSpawner != null)
        {
            puzzleSpawner.ForceRandomize();
        }

        // Reset all memory fragments
        ResetMemoryFragments();

        // Reset light sources
        ResetLightSources();

        // Re-find platforms after spawner creates new ones
        platforms = null; // Force re-finding of platforms
        
        if (showDebugInfo)
            Debug.Log($"Round {currentRound} started - New layout generated - Reset height: {platformResetHeight}");
    }

    void ResetMemoryFragments()
    {
        // Reset existing fragments
        foreach (MemoryFragment fragment in allFragments)
        {
            if (fragment != null)
            {
                fragment.ResetFragment();
            }
        }

        // Find any new fragments that might have been created
        FindAllMemoryFragments();
    }

    void ResetLightSources()
    {
        // Find and reset all light sources using component search instead of type reference
        MonoBehaviour[] allComponents = FindObjectsOfType<MonoBehaviour>();
        
        foreach (MonoBehaviour component in allComponents)
        {
            if (component.GetType().Name == "LightSource")
            {
                // Use reflection to call ResetActivation method
                var resetMethod = component.GetType().GetMethod("ResetActivation");
                if (resetMethod != null)
                {
                    resetMethod.Invoke(component, null);
                }
            }
        }
        
        if (showDebugInfo)
            Debug.Log("Light sources reset");
    }

    void ForceReset()
    {
        TriggerReset();
    }

    void LeaveGame()
    {
        gameActive = false;
        
        if (showDebugInfo)
            Debug.Log("Player pressed L - Leaving game");

        // You can customize what happens when leaving:
        // Option 1: Quit application
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif

        // Option 2: Load main menu scene
        // SceneManager.LoadScene("MainMenu");

        // Option 3: Restart current scene
        // SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Public methods for external control
    public void SetLoopDuration(float duration)
    {
        loopDuration = duration;
        if (loopManager != null)
            loopManager.SetLoopDuration(duration);
    }

    public void SetPlatformResetHeight(float height)
    {
        platformResetHeight = height;
        if (showDebugInfo)
            Debug.Log($"Platform reset height changed to: {height}");
    }

    public int GetCurrentRound()
    {
        return currentRound;
    }

    public bool IsGameActive()
    {
        return gameActive;
    }

    // GUI for instructions and debug info
    void OnGUI()
    {
        if (!gameActive) return;

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 16;
        style.normal.textColor = Color.black;

        // Show instructions
        if (instructionTimer > 2f)
        {
            GUI.Label(new Rect(10, 10, 400, 120), 
                "INFINITE PUZZLE MODE\n" +
                "• Collect fragments in Shadow mode (Q)\n" +
                "• Solve light puzzles in Light mode (Q)\n" +
                $"• Game resets when platform reaches Y={platformResetHeight}\n" +
                "• Press L to LEAVE", style);
        }

        // Show current round and timer
        GUI.Label(new Rect(10, Screen.height - 80, 200, 60), 
            $"Round: {currentRound}\n" +
            $"Time: {(loopManager != null ? loopManager.GetTimeRemaining().ToString("F1") : "0.0")}s", style);

        // Show debug info
        if (showDebugInfo)
        {
            string platformInfo = "";
            if (platforms != null)
            {
                foreach (Transform platform in platforms)
                {
                    if (platform != null && platform.gameObject.activeInHierarchy)
                    {
                        platformInfo += $"{platform.name}: Y={platform.position.y:F1}\n";
                    }
                }
            }
            
            GUI.Label(new Rect(Screen.width - 250, 10, 240, 150), 
                $"Fragments: {allFragments.Count}\n" +
                $"Reset Height: {platformResetHeight}\n" +
                $"Press R to force reset\n" +
                $"Platform Heights:\n{platformInfo}", style);
        }
    }
}

