using UnityEngine;
using System.Collections.Generic;

public class WorldShuffler : MonoBehaviour
{
    [System.Serializable]
    public class ShuffleVariant
    {
        [Header("Target Object")]
        public Transform target;
        public string variantName = "Unnamed Variant";
        
        [Header("Position Variants")]
        public Vector3[] localPositions;
        public bool useLocalPositions = true;
        
        [Header("Rotation Variants")]
        public Vector3[] localRotations;
        public bool useLocalRotations = false;
        
        [Header("Scale Variants")]
        public Vector3[] localScales;
        public bool useLocalScales = false;
        
        [Header("Activation Variants")]
        public bool[] activeStates;
        public bool useActiveStates = false;
        
        [Header("Material Variants")]
        public Material[] materials;
        public bool useMaterials = false;
        public Renderer targetRenderer;
        
        private int currentIndex = 0;
        
        public void Shuffle()
        {
            if (target == null) return;
            currentIndex = (currentIndex + 1) % GetMaxVariants();
            ApplyVariant(currentIndex);
        }
        
        public void SetVariant(int index)
        {
            if (target == null) return;
            currentIndex = Mathf.Clamp(index, 0, GetMaxVariants() - 1);
            ApplyVariant(currentIndex);
        }
        
        public void RandomizeVariant()
        {
            if (target == null) return;
            int maxVariants = GetMaxVariants();
            if (maxVariants <= 1) return;
            
            int newIndex;
            do
            {
                newIndex = Random.Range(0, maxVariants);
            } while (newIndex == currentIndex && maxVariants > 1);
            
            currentIndex = newIndex;
            ApplyVariant(currentIndex);
        }
        
        void ApplyVariant(int index)
        {
            if (useLocalPositions && localPositions != null && index < localPositions.Length)
                target.localPosition = localPositions[index];
                
            if (useLocalRotations && localRotations != null && index < localRotations.Length)
                target.localEulerAngles = localRotations[index];
                
            if (useLocalScales && localScales != null && index < localScales.Length)
                target.localScale = localScales[index];
                
            if (useActiveStates && activeStates != null && index < activeStates.Length)
                target.gameObject.SetActive(activeStates[index]);
                
            if (useMaterials && materials != null && index < materials.Length)
            {
                if (targetRenderer == null)
                    targetRenderer = target.GetComponent<Renderer>();
                if (targetRenderer != null)
                    targetRenderer.material = materials[index];
            }
        }
        
        int GetMaxVariants()
        {
            int max = 1;
            if (useLocalPositions && localPositions != null)
                max = Mathf.Max(max, localPositions.Length);
            if (useLocalRotations && localRotations != null)
                max = Mathf.Max(max, localRotations.Length);
            if (useLocalScales && localScales != null)
                max = Mathf.Max(max, localScales.Length);
            if (useActiveStates && activeStates != null)
                max = Mathf.Max(max, activeStates.Length);
            if (useMaterials && materials != null)
                max = Mathf.Max(max, materials.Length);
            return max;
        }
        
        public int GetCurrentIndex() => currentIndex;
        public int GetMaxVariantCount() => GetMaxVariants();
    }
    
    [Header("Shuffle Settings")]
    public ShuffleVariant[] variants;
    public bool randomizeOnShuffle = true;
    public bool shuffleOnStart = false;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip shuffleSound;
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    
    void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        if (shuffleOnStart)
            ShuffleAll();
    }
    
    public void ShuffleAll()
    {
        if (variants == null) return;
        
        foreach (var variant in variants)
        {
            if (randomizeOnShuffle)
                variant.RandomizeVariant();
            else
                variant.Shuffle();
        }
        
        PlayShuffleEffects();
        if (showDebugInfo)
            Debug.Log($"World shuffled - {variants.Length} variants updated");
    }
    
    // Shuffle variant by index - NO OVERLOADING
    public void ShuffleVariantByIndex(int variantIndex)
    {
        if (variants == null || variantIndex < 0 || variantIndex >= variants.Length)
            return;
            
        if (randomizeOnShuffle)
            variants[variantIndex].RandomizeVariant();
        else
            variants[variantIndex].Shuffle();
            
        PlayShuffleEffects();
        if (showDebugInfo)
            Debug.Log($"Variant '{variants[variantIndex].variantName}' shuffled");
    }
    
    // Shuffle variant by name - UNIQUE NAME
    public void ShuffleVariantByName(string variantName)
    {
        if (variants == null) return;
        
        for (int i = 0; i < variants.Length; i++)
        {
            if (variants[i].variantName == variantName)
            {
                ShuffleVariantByIndex(i);
                return;
            }
        }
        
        if (showDebugInfo)
            Debug.LogWarning($"Variant with name '{variantName}' not found");
    }
    
    public void SetVariantState(int variantIndex, int stateIndex)
    {
        if (variants == null || variantIndex < 0 || variantIndex >= variants.Length)
            return;
            
        variants[variantIndex].SetVariant(stateIndex);
        if (showDebugInfo)
            Debug.Log($"Variant '{variants[variantIndex].variantName}' set to state {stateIndex}");
    }
    
    public void ResetAllVariants()
    {
        if (variants == null) return;
        
        foreach (var variant in variants)
            variant.SetVariant(0);
            
        if (showDebugInfo)
            Debug.Log("All variants reset to initial state");
    }
    
    void PlayShuffleEffects()
    {
        if (audioSource != null && shuffleSound != null)
            audioSource.PlayOneShot(shuffleSound);
    }
    
    public void EnableRandomization() => randomizeOnShuffle = true;
    public void DisableRandomization() => randomizeOnShuffle = false;
    
    public int GetVariantCount() => variants?.Length ?? 0;
    
    // Get variant by index - UNIQUE NAME
    public ShuffleVariant GetVariantByIndex(int index)
    {
        if (variants == null || index < 0 || index >= variants.Length)
            return null;
        return variants[index];
    }
    
    // Get variant by name - UNIQUE NAME
    public ShuffleVariant GetVariantByName(string name)
    {
        if (variants == null) return null;
        
        foreach (var variant in variants)
        {
            if (variant.variantName == name)
                return variant;
        }
        return null;
    }
    
    // Additional helper methods with unique names
    public bool HasVariantWithName(string name)
    {
        return GetVariantByName(name) != null;
    }
    
    public int FindVariantIndexByName(string name)
    {
        if (variants == null) return -1;
        
        for (int i = 0; i < variants.Length; i++)
        {
            if (variants[i].variantName == name)
                return i;
        }
        return -1;
    }
    
    void OnDrawGizmosSelected()
    {
        if (variants == null) return;
        
        foreach (var variant in variants)
        {
            if (variant.target == null) continue;
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(variant.target.position, 0.5f);
            
            if (variant.useLocalPositions && variant.localPositions != null)
            {
                Gizmos.color = Color.yellow;
                foreach (var pos in variant.localPositions)
                {
                    Vector3 worldPos = variant.target.parent != null ? 
                        variant.target.parent.TransformPoint(pos) : pos;
                    Gizmos.DrawWireCube(worldPos, Vector3.one * 0.3f);
                }
            }
        }
    }
}

