using UnityEngine;
using System.Collections.Generic;

public class Manager : MonoBehaviour
{
    [Header("Spawn Settings")]
    public Transform hubPlatform; // Main platform (stays fixed)
    public int numberOfLightSources = 2;
    public int numberOfLightReceivers = 3;
    public int numberOfActivationPlatforms = 2; // These are the ones that rise up
    public int numberOfMemoryFragments = 5;

    [Header("Prefabs")]
    public GameObject lightSourcePrefab;
    public GameObject lightReceiverPrefab;
    public GameObject activationPlatformPrefab; // The platforms that activate and rise
    public GameObject memoryFragmentPrefab;

    [Header("Spawn Area")]
    public Vector3 spawnAreaSize = new Vector3(20f, 0f, 20f);
    public float minDistanceBetweenObjects = 3f;
    public float heightOffset = 1f;

    [Header("Activation Platform Settings")]
    public Vector3 activationPlatformSize = new Vector3(2f, 0.5f, 2f);
    public Material activationPlatformMaterial;

    [Header("Memory Fragment Settings")]
    public float fragmentHeightRange = 2f; // How high fragments can spawn
    public float fragmentMinHeight = 0.5f;

    [Header("Randomization")]
    public bool randomizeOnStart = true;
    public bool randomizeOnLoopReset = true;
    public float randomSeed = 0f; // 0 = use random seed

    private List<GameObject> spawnedObjects = new List<GameObject>();
    private List<Vector3> usedPositions = new List<Vector3>();
    private LoopManager loopManager;

    void Start()
    {
        // Find loop manager for reset events
        loopManager = FindObjectOfType<LoopManager>();
        if (loopManager != null && randomizeOnLoopReset)
        {
            // Subscribe to loop reset event (you may need to add this to LoopManager)
            // loopManager.OnLoopReset += RandomizeLayout;
        }

        // Auto-find hub platform if not assigned
        if (hubPlatform == null)
        {
            GameObject hub = GameObject.Find("HubPlatform");
            if (hub != null)
                hubPlatform = hub.transform;
        }

        if (randomizeOnStart)
        {
            RandomizeLayout();
        }
    }

    public void RandomizeLayout()
    {
        // Set random seed if specified
        if (randomSeed != 0f)
        {
            Random.InitState((int)randomSeed);
        }

        // Clear previous spawns
        ClearSpawnedObjects();

        // Generate new layout
        SpawnLightSources();
        SpawnLightReceivers();
        SpawnActivationPlatforms(); // These are the puzzle reward platforms
        SpawnMemoryFragments(); // NEW: Randomize memory fragments too
        ConnectPuzzleElements();

        Debug.Log($"Randomized complete layout: {numberOfLightSources} sources, {numberOfLightReceivers} receivers, {numberOfActivationPlatforms} activation platforms, {numberOfMemoryFragments} fragments");
    }

    void ClearSpawnedObjects()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null)
                DestroyImmediate(obj);
        }
        spawnedObjects.Clear();
        usedPositions.Clear();
    }

    void SpawnLightSources()
    {
        for (int i = 0; i < numberOfLightSources; i++)
        {
            Vector3 spawnPos = GetRandomSpawnPosition();
            if (spawnPos != Vector3.zero)
            {
                GameObject lightSource = CreateLightSource(spawnPos, i);
                spawnedObjects.Add(lightSource);
                usedPositions.Add(spawnPos);
            }
        }
    }

    void SpawnLightReceivers()
    {
        for (int i = 0; i < numberOfLightReceivers; i++)
        {
            Vector3 spawnPos = GetRandomSpawnPosition();
            if (spawnPos != Vector3.zero)
            {
                GameObject lightReceiver = CreateLightReceiver(spawnPos, i);
                spawnedObjects.Add(lightReceiver);
                usedPositions.Add(spawnPos);
            }
        }
    }

    void SpawnActivationPlatforms()
    {
        for (int i = 0; i < numberOfActivationPlatforms; i++)
        {
            Vector3 spawnPos = GetRandomSpawnPosition();
            if (spawnPos != Vector3.zero)
            {
                GameObject platform = CreateActivationPlatform(spawnPos, i);
                spawnedObjects.Add(platform);
                usedPositions.Add(spawnPos);
            }
        }
    }

    void SpawnMemoryFragments()
    {
        for (int i = 0; i < numberOfMemoryFragments; i++)
        {
            Vector3 spawnPos = GetRandomFragmentPosition();
            if (spawnPos != Vector3.zero)
            {
                GameObject fragment = CreateMemoryFragment(spawnPos, i);
                spawnedObjects.Add(fragment);
                usedPositions.Add(spawnPos);
            }
        }
    }

    Vector3 GetRandomSpawnPosition()
    {
        Vector3 hubCenter = hubPlatform != null ? hubPlatform.position : transform.position;
        int maxAttempts = 50;
        
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector3 randomPos = new Vector3(
                Random.Range(-spawnAreaSize.x / 2f, spawnAreaSize.x / 2f),
                heightOffset,
                Random.Range(-spawnAreaSize.z / 2f, spawnAreaSize.z / 2f)
            ) + hubCenter;

            // Check if position is far enough from other objects
            bool validPosition = true;
            foreach (Vector3 usedPos in usedPositions)
            {
                if (Vector3.Distance(randomPos, usedPos) < minDistanceBetweenObjects)
                {
                    validPosition = false;
                    break;
                }
            }

            if (validPosition)
                return randomPos;
        }

        Debug.LogWarning("Could not find valid spawn position after maximum attempts");
        return Vector3.zero;
    }

    Vector3 GetRandomFragmentPosition()
    {
        Vector3 hubCenter = hubPlatform != null ? hubPlatform.position : transform.position;
        int maxAttempts = 50;
        
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector3 randomPos = new Vector3(
                Random.Range(-spawnAreaSize.x / 2f, spawnAreaSize.x / 2f),
                Random.Range(fragmentMinHeight, fragmentMinHeight + fragmentHeightRange),
                Random.Range(-spawnAreaSize.z / 2f, spawnAreaSize.z / 2f)
            ) + hubCenter;

            // Check if position is far enough from other objects
            bool validPosition = true;
            foreach (Vector3 usedPos in usedPositions)
            {
                if (Vector3.Distance(randomPos, usedPos) < minDistanceBetweenObjects * 0.5f) // Fragments can be closer
                {
                    validPosition = false;
                    break;
                }
            }

            if (validPosition)
                return randomPos;
        }

        Debug.LogWarning("Could not find valid fragment spawn position after maximum attempts");
        return Vector3.zero;
    }

    GameObject CreateLightSource(Vector3 position, int index)
    {
        GameObject lightSource;
        
        if (lightSourcePrefab != null)
        {
            lightSource = Instantiate(lightSourcePrefab, position, Quaternion.identity);
        }
        else
        {
            // Create basic light source
            lightSource = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            lightSource.transform.position = position;
            lightSource.transform.localScale = new Vector3(0.5f, 1f, 0.5f);
            
            // Add components - use string name to avoid type reference issues
            lightSource.AddComponent(System.Type.GetType("LightSource"));
            lightSource.AddComponent<LineRenderer>();
            
            // Set material
            Renderer renderer = lightSource.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = Color.yellow;
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", Color.yellow * 0.5f);
                renderer.material = mat;
            }
        }

        lightSource.name = $"LightSource_{index}";
        
        // Random rotation
        lightSource.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
        
        return lightSource;
    }

    GameObject CreateLightReceiver(Vector3 position, int index)
    {
        GameObject lightReceiver;
        
        if (lightReceiverPrefab != null)
        {
            lightReceiver = Instantiate(lightReceiverPrefab, position, Quaternion.identity);
        }
        else
        {
            // Create basic light receiver
            lightReceiver = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lightReceiver.transform.position = position;
            lightReceiver.transform.localScale = Vector3.one;
            
            // Add components
            lightReceiver.AddComponent<LightReceiver>();
            lightReceiver.AddComponent<BoxCollider>();
            
            // Set material
            Renderer renderer = lightReceiver.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = Color.red;
                renderer.material = mat;
            }
        }

        lightReceiver.name = $"LightReceiver_{index}";
        lightReceiver.tag = "LightReceiver";
        
        return lightReceiver;
    }

    GameObject CreateActivationPlatform(Vector3 position, int index)
    {
        GameObject platform;
        
        if (activationPlatformPrefab != null)
        {
            platform = Instantiate(activationPlatformPrefab, position, Quaternion.identity);
        }
        else
        {
            // Create basic activation platform
            platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
            platform.transform.position = position;
            platform.transform.localScale = activationPlatformSize;
            
            // Set material
            Renderer renderer = platform.GetComponent<Renderer>();
            if (renderer != null)
            {
                if (activationPlatformMaterial != null)
                {
                    renderer.material = activationPlatformMaterial;
                }
                else
                {
                    Material mat = new Material(Shader.Find("Standard"));
                    mat.color = Color.blue;
                    renderer.material = mat;
                }
            }

            // Add a simple rising script
            platform.AddComponent<PlatformRiser>();
        }

        platform.name = $"ActivationPlatform_{index}";
        platform.tag = "Platform";
        
        // Start inactive - will be activated by light receiver
        platform.SetActive(false);
        
        return platform;
    }

    GameObject CreateMemoryFragment(Vector3 position, int index)
    {
        GameObject fragment;
        
        if (memoryFragmentPrefab != null)
        {
            fragment = Instantiate(memoryFragmentPrefab, position, Quaternion.identity);
        }
        else
        {
            // Create basic memory fragment
            fragment = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            fragment.transform.position = position;
            fragment.transform.localScale = Vector3.one * 0.5f;
            
            // Add MemoryFragment component
            MemoryFragment memoryComponent = fragment.AddComponent<MemoryFragment>();
            memoryComponent.fragmentID = index;
            memoryComponent.timeBonus = 10f;
            
            // Set material
            Renderer renderer = fragment.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = new Color(0.5f, 0f, 1f, 0.8f); // Purple
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", Color.magenta * 0.3f);
                renderer.material = mat;
            }

            // Remove collider and add trigger
            Collider col = fragment.GetComponent<Collider>();
            if (col != null)
                DestroyImmediate(col);
            
            SphereCollider trigger = fragment.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 1f;
        }

        fragment.name = $"MemoryFragment_{index}";
        fragment.tag = "MemoryFragment";
        
        return fragment;
    }

    void ConnectPuzzleElements()
    {
        // Connect light receivers to activation platforms
        List<GameObject> receivers = new List<GameObject>();
        List<GameObject> platforms = new List<GameObject>();
        
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj.name.Contains("LightReceiver"))
                receivers.Add(obj);
            else if (obj.name.Contains("ActivationPlatform"))
                platforms.Add(obj);
        }

        // Randomly assign platforms to receivers
        for (int i = 0; i < receivers.Count && i < platforms.Count; i++)
        {
            LightReceiver receiver = receivers[i].GetComponent<LightReceiver>();
            if (receiver != null)
            {
                receiver.objectsToActivate = new GameObject[] { platforms[i] };
                Debug.Log($"Connected {receivers[i].name} to {platforms[i].name}");
            }
        }
    }

    // Public methods
    public void SetRandomSeed(float seed)
    {
        randomSeed = seed;
    }

    public void ForceRandomize()
    {
        RandomizeLayout();
    }

    void OnDrawGizmosSelected()
    {
        Vector3 center = hubPlatform != null ? hubPlatform.position : transform.position;
        
        // Draw spawn area
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(center + Vector3.up * heightOffset, spawnAreaSize);
        
        // Draw used positions
        Gizmos.color = Color.red;
        foreach (Vector3 pos in usedPositions)
        {
            Gizmos.DrawWireSphere(pos, minDistanceBetweenObjects / 2f);
        }
    }
}

// Simple script to make platforms rise when activated
[System.Serializable]
public class PlatformRiser : MonoBehaviour
{
    public float riseSpeed = 2f;
    public float maxHeight = 5f;
    
    private Vector3 startPosition;
    private bool isRising = false;
    
    void Start()
    {
        startPosition = transform.position;
    }
    
    void Update()
    {
        if (isRising && transform.position.y < startPosition.y + maxHeight)
        {
            transform.position += Vector3.up * riseSpeed * Time.deltaTime;
        }
    }
    
    void OnEnable()
    {
        isRising = true;
        if (startPosition == Vector3.zero)
            startPosition = transform.position;
    }
    
    void OnDisable()
    {
        isRising = false;
        if (startPosition != Vector3.zero)
            transform.position = startPosition;
    }
}

