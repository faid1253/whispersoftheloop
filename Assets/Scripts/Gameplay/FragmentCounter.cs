using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class FragmentCounter : MonoBehaviour
{
    [Header("Fragment Tracking")]
    public int totalFragmentsInLevel = 5;
    public bool saveProgress = true;
    public string saveKey = "FragmentProgress";
    
    [Header("Events")]
    public UnityEvent<int> OnFragmentCountChanged;
    public UnityEvent<int, string> OnFragmentCollected; // ID, Name
    public UnityEvent OnAllFragmentsCollected;
    public UnityEvent OnProgressSaved;
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    
    private HashSet<int> collectedFragments = new HashSet<int>();
    private Dictionary<int, string> fragmentNames = new Dictionary<int, string>();
    
    void Start()
    {
        if (saveProgress)
            LoadProgress();
    }
    
    public void AddFragment(int fragmentID, string fragmentName = "")
    {
        if (collectedFragments.Contains(fragmentID))
        {
            if (showDebugInfo)
                Debug.LogWarning($"Fragment {fragmentID} already collected!");
            return;
        }
        
        collectedFragments.Add(fragmentID);
        
        if (!string.IsNullOrEmpty(fragmentName))
            fragmentNames[fragmentID] = fragmentName;
        
        // Invoke events
        OnFragmentCountChanged?.Invoke(collectedFragments.Count);
        OnFragmentCollected?.Invoke(fragmentID, fragmentName);
        
        if (showDebugInfo)
            Debug.Log($"Fragment collected: {fragmentID} ({fragmentName}) - Total: {collectedFragments.Count}/{totalFragmentsInLevel}");
        
        // Check if all fragments collected
        if (collectedFragments.Count >= totalFragmentsInLevel)
        {
            OnAllFragmentsCollected?.Invoke();
            
            if (showDebugInfo)
                Debug.Log("All fragments collected!");
        }
        
        if (saveProgress)
            SaveProgress();
    }
    
    public void RemoveFragment(int fragmentID)
    {
        if (collectedFragments.Remove(fragmentID))
        {
            fragmentNames.Remove(fragmentID);
            OnFragmentCountChanged?.Invoke(collectedFragments.Count);
            
            if (showDebugInfo)
                Debug.Log($"Fragment removed: {fragmentID} - Total: {collectedFragments.Count}/{totalFragmentsInLevel}");
            
            if (saveProgress)
                SaveProgress();
        }
    }
    
    public void ResetProgress()
    {
        collectedFragments.Clear();
        fragmentNames.Clear();
        OnFragmentCountChanged?.Invoke(0);
        
        if (saveProgress)
            SaveProgress();
        
        if (showDebugInfo)
            Debug.Log("Fragment progress reset");
    }
    
    void SaveProgress()
    {
        if (!saveProgress) return;
        
        // Convert HashSet to array for serialization
        int[] fragmentArray = new int[collectedFragments.Count];
        collectedFragments.CopyTo(fragmentArray);
        
        // Create save data
        FragmentSaveData saveData = new FragmentSaveData
        {
            collectedFragmentIDs = fragmentArray,
            totalFragments = totalFragmentsInLevel
        };
        
        // Save to PlayerPrefs as JSON
        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString(saveKey, json);
        PlayerPrefs.Save();
        
        OnProgressSaved?.Invoke();
        
        if (showDebugInfo)
            Debug.Log($"Fragment progress saved: {collectedFragments.Count} fragments");
    }
    
    void LoadProgress()
    {
        if (!saveProgress) return;
        
        if (PlayerPrefs.HasKey(saveKey))
        {
            string json = PlayerPrefs.GetString(saveKey);
            
            try
            {
                FragmentSaveData saveData = JsonUtility.FromJson<FragmentSaveData>(json);
                
                collectedFragments.Clear();
                foreach (int id in saveData.collectedFragmentIDs)
                {
                    collectedFragments.Add(id);
                }
                
                // Update total if saved data has different count
                if (saveData.totalFragments > 0)
                    totalFragmentsInLevel = saveData.totalFragments;
                
                OnFragmentCountChanged?.Invoke(collectedFragments.Count);
                
                if (showDebugInfo)
                    Debug.Log($"Fragment progress loaded: {collectedFragments.Count} fragments");
            }
            catch (System.Exception e)
            {
                if (showDebugInfo)
                    Debug.LogError($"Failed to load fragment progress: {e.Message}");
                
                ResetProgress();
            }
        }
    }
    
    // Getters
    public int GetFragmentCount() => collectedFragments.Count;
    public int GetTotalFragments() => totalFragmentsInLevel;
    public float GetProgressPercentage() => (float)collectedFragments.Count / totalFragmentsInLevel;
    public bool HasFragment(int fragmentID) => collectedFragments.Contains(fragmentID);
    public bool HasAllFragments() => collectedFragments.Count >= totalFragmentsInLevel;
    public string GetFragmentName(int fragmentID) => fragmentNames.ContainsKey(fragmentID) ? fragmentNames[fragmentID] : "";
    
    public List<int> GetCollectedFragmentIDs()
    {
        return new List<int>(collectedFragments);
    }
    
    public Dictionary<int, string> GetCollectedFragments()
    {
        Dictionary<int, string> result = new Dictionary<int, string>();
        foreach (int id in collectedFragments)
        {
            result[id] = GetFragmentName(id);
        }
        return result;
    }
    
    // Setters
    public void SetTotalFragments(int total)
    {
        totalFragmentsInLevel = total;
        OnFragmentCountChanged?.Invoke(collectedFragments.Count);
    }
    
    public void SetSaveKey(string key)
    {
        saveKey = key;
    }
    
    // Manual control methods
    public void ForceAddFragment(int fragmentID, string name = "")
    {
        AddFragment(fragmentID, name);
    }
    
    public void ForceRemoveFragment(int fragmentID)
    {
        RemoveFragment(fragmentID);
    }
    
    public void ForceResetProgress()
    {
        ResetProgress();
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && saveProgress)
            SaveProgress();
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && saveProgress)
            SaveProgress();
    }
}

[System.Serializable]
public class FragmentSaveData
{
    public int[] collectedFragmentIDs;
    public int totalFragments;
}

