using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class LoopManager : MonoBehaviour
{
    [Header("Loop Settings")]
    public float loopDurationSeconds = 300f; // 5 minutes default
    public Transform loopSpawnPoint;
    public bool autoStartLoop = true;

    [Header("Progress Settings")]
    public float progressTimeBonus = 30f;
    public float checkpointTimeBonus = 10f;

    [Header("Events")]
    public UnityEvent OnLoopStart;
    public UnityEvent OnLoopReset;
    public UnityEvent OnProgress;
    public UnityEvent<float> OnTimeUpdate; // Passes remaining time

    [Header("Debug")]
    public bool showDebugInfo = true;

    private float timeRemaining;
    private bool loopActive = false;
    private bool isPaused = false;
    private GameObject player;
    private CharacterController playerController;

    void Start()
    {
        FindPlayer();
        if (autoStartLoop)
            StartLoop();
    }

    void Update()
    {
        if (!loopActive || isPaused) return;

        timeRemaining -= Time.deltaTime;
        OnTimeUpdate?.Invoke(timeRemaining);

        if (timeRemaining <= 0f)
        {
            ResetLoop();
        }

        if (showDebugInfo && Input.GetKeyDown(KeyCode.R))
        {
            ResetLoop(); // Manual reset for testing
        }
    }

    void FindPlayer()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerController = player.GetComponent<CharacterController>();
        }
    }

    public void StartLoop()
    {
        timeRemaining = loopDurationSeconds;
        loopActive = true;
        isPaused = false;
        OnLoopStart?.Invoke();
        
        if (showDebugInfo)
            Debug.Log($"Loop started - Duration: {loopDurationSeconds} seconds");
    }

    public void ResetLoop()
    {
        if (showDebugInfo)
            Debug.Log("Loop reset triggered");

        // Move player back to spawn point
        if (player != null && loopSpawnPoint != null)
        {
            if (playerController != null)
                playerController.enabled = false;

            player.transform.position = loopSpawnPoint.position;
            player.transform.rotation = loopSpawnPoint.rotation;

            if (playerController != null)
                playerController.enabled = true;
        }

        // Reset timer and trigger events
        timeRemaining = loopDurationSeconds;
        OnLoopReset?.Invoke();
        
        if (showDebugInfo)
            Debug.Log("Player moved to spawn point and world shuffled");
    }

    public void ReportProgress(float timeBonus = -1f)
    {
        if (timeBonus < 0f)
            timeBonus = progressTimeBonus;

        timeRemaining = Mathf.Min(loopDurationSeconds, timeRemaining + timeBonus);
        OnProgress?.Invoke();
        
        if (showDebugInfo)
            Debug.Log($"Progress reported - Time bonus: {timeBonus} seconds");
    }

    public void ReportCheckpoint()
    {
        ReportProgress(checkpointTimeBonus);
        
        if (showDebugInfo)
            Debug.Log("Checkpoint reached");
    }

    public void PauseLoop(bool pause)
    {
        isPaused = pause;
        
        if (showDebugInfo)
            Debug.Log($"Loop {(pause ? "paused" : "resumed")}");
    }

    public void PauseLoopTemporary(float pauseDuration)
    {
        StartCoroutine(TemporaryPause(pauseDuration));
    }

    IEnumerator TemporaryPause(float duration)
    {
        PauseLoop(true);
        yield return new WaitForSeconds(duration);
        PauseLoop(false);
    }

    public void StopLoop()
    {
        loopActive = false;
        isPaused = false;
        
        if (showDebugInfo)
            Debug.Log("Loop stopped");
    }

    // Getters
    public float GetTimeRemaining() => timeRemaining;
    public float GetTimeElapsed() => loopDurationSeconds - timeRemaining;
    public float GetProgressPercentage() => (loopDurationSeconds - timeRemaining) / loopDurationSeconds;
    public bool IsLoopActive() => loopActive;
    public bool IsLoopPaused() => isPaused;

    // Setters
    public void SetLoopDuration(float newDuration)
    {
        loopDurationSeconds = newDuration;
        if (loopActive)
            timeRemaining = Mathf.Min(timeRemaining, newDuration);
    }

    void OnDrawGizmosSelected()
    {
        if (loopSpawnPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(loopSpawnPoint.position, 1f);
            Gizmos.DrawLine(loopSpawnPoint.position, loopSpawnPoint.position + loopSpawnPoint.forward * 2f);
        }
    }
}

