using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoopTimerUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI timerText;
    public Text legacyTimerText; // For legacy UI Text component
    public Image timerFillImage;
    public Slider timerSlider;
    
    [Header("Visual Settings")]
    public Color normalColor = Color.white;
    public Color warningColor = Color.yellow;
    public Color criticalColor = Color.red;
    public float warningThreshold = 60f; // seconds
    public float criticalThreshold = 30f; // seconds
    
    [Header("Animation")]
    public bool pulseOnCritical = true;
    public float pulseSpeed = 2f;
    public bool flashOnWarning = true;
    public float flashSpeed = 1f;
    
    [Header("Format Settings")]
    public bool showMilliseconds = false;
    public bool showHours = false;
    public string timeFormat = "mm:ss";
    
    private LoopManager loopManager;
    private float originalScale = 1f;
    private Color originalColor;
    private bool isFlashing = false;
    
    void Start()
    {
        loopManager = FindObjectOfType<LoopManager>();
        
        if (loopManager == null)
        {
            Debug.LogWarning("LoopTimerUI: No LoopManager found in scene!");
            return;
        }
        
        // Subscribe to timer updates
        loopManager.OnTimeUpdate.AddListener(UpdateTimer);
        
        // Store original values
        if (timerText != null)
        {
            originalScale = timerText.transform.localScale.x;
            originalColor = timerText.color;
        }
        else if (legacyTimerText != null)
        {
            originalScale = legacyTimerText.transform.localScale.x;
            originalColor = legacyTimerText.color;
        }
    }
    
    void UpdateTimer(float timeRemaining)
    {
        // Update text display
        UpdateTimerText(timeRemaining);
        
        // Update fill/slider
        UpdateTimerFill(timeRemaining);
        
        // Update visual effects
        UpdateVisualEffects(timeRemaining);
    }
    
    void UpdateTimerText(float timeRemaining)
    {
        string formattedTime = FormatTime(timeRemaining);
        
        if (timerText != null)
            timerText.text = formattedTime;
        else if (legacyTimerText != null)
            legacyTimerText.text = formattedTime;
    }
    
    void UpdateTimerFill(float timeRemaining)
    {
        if (loopManager == null) return;
        
        float fillAmount = timeRemaining / loopManager.loopDurationSeconds;
        
        if (timerFillImage != null)
            timerFillImage.fillAmount = fillAmount;
            
        if (timerSlider != null)
            timerSlider.value = fillAmount;
    }
    
    void UpdateVisualEffects(float timeRemaining)
    {
        Color targetColor = GetTimerColor(timeRemaining);
        
        // Update text color
        if (timerText != null)
            timerText.color = targetColor;
        else if (legacyTimerText != null)
            legacyTimerText.color = targetColor;
            
        // Update fill color
        if (timerFillImage != null)
            timerFillImage.color = targetColor;
        
        // Handle pulsing and flashing
        if (timeRemaining <= criticalThreshold && pulseOnCritical)
        {
            PulseTimer();
        }
        else if (timeRemaining <= warningThreshold && flashOnWarning)
        {
            FlashTimer();
        }
        else
        {
            ResetEffects();
        }
    }
    
    Color GetTimerColor(float timeRemaining)
    {
        if (timeRemaining <= criticalThreshold)
            return criticalColor;
        else if (timeRemaining <= warningThreshold)
            return warningColor;
        else
            return normalColor;
    }
    
    void PulseTimer()
    {
        float pulse = Mathf.Sin(Time.time * pulseSpeed) * 0.2f + 1f;
        Vector3 scale = Vector3.one * originalScale * pulse;
        
        if (timerText != null)
            timerText.transform.localScale = scale;
        else if (legacyTimerText != null)
            legacyTimerText.transform.localScale = scale;
    }
    
    void FlashTimer()
    {
        if (!isFlashing)
        {
            isFlashing = true;
            InvokeRepeating(nameof(ToggleFlash), 0f, 1f / flashSpeed);
        }
    }
    
    void ToggleFlash()
    {
        bool visible = Time.time % (2f / flashSpeed) < (1f / flashSpeed);
        
        if (timerText != null)
            timerText.enabled = visible;
        else if (legacyTimerText != null)
            legacyTimerText.enabled = visible;
    }
    
    void ResetEffects()
    {
        // Reset scale
        Vector3 normalScale = Vector3.one * originalScale;
        if (timerText != null)
            timerText.transform.localScale = normalScale;
        else if (legacyTimerText != null)
            legacyTimerText.transform.localScale = normalScale;
        
        // Reset flashing
        if (isFlashing)
        {
            isFlashing = false;
            CancelInvoke(nameof(ToggleFlash));
            
            if (timerText != null)
                timerText.enabled = true;
            else if (legacyTimerText != null)
                legacyTimerText.enabled = true;
        }
    }
    
    string FormatTime(float timeInSeconds)
    {
        if (timeInSeconds < 0) timeInSeconds = 0;
        
        int hours = Mathf.FloorToInt(timeInSeconds / 3600f);
        int minutes = Mathf.FloorToInt((timeInSeconds % 3600f) / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        int milliseconds = Mathf.FloorToInt((timeInSeconds * 1000f) % 1000f);
        
        if (showHours)
        {
            if (showMilliseconds)
                return $"{hours:00}:{minutes:00}:{seconds:00}.{milliseconds:000}";
            else
                return $"{hours:00}:{minutes:00}:{seconds:00}";
        }
        else
        {
            if (showMilliseconds)
                return $"{minutes:00}:{seconds:00}.{milliseconds:000}";
            else
                return $"{minutes:00}:{seconds:00}";
        }
    }
    
    public void SetTimerFormat(bool includeHours, bool includeMilliseconds)
    {
        showHours = includeHours;
        showMilliseconds = includeMilliseconds;
    }
    
    public void SetWarningThresholds(float warning, float critical)
    {
        warningThreshold = warning;
        criticalThreshold = critical;
    }
    
    public void SetColors(Color normal, Color warning, Color critical)
    {
        normalColor = normal;
        warningColor = warning;
        criticalColor = critical;
    }
    
    void OnDestroy()
    {
        if (loopManager != null)
            loopManager.OnTimeUpdate.RemoveListener(UpdateTimer);
    }
}

