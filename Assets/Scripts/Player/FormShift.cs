using UnityEngine;
using UnityEngine.Events;

public class FormShift : MonoBehaviour
{
    public enum Form { Light, Shadow }
    
    [Header("Form Settings")]
    public Form currentForm = Form.Light;
    public KeyCode shiftKey = KeyCode.Q;

    [Header("Visuals")]
    public Renderer[] formRenderers;
    public Color lightColor = new Color(1f, 1f, 1f, 0.6f);
    public Color shadowColor = new Color(0.2f, 0.7f, 1f, 0.6f);
    public ParticleSystem lightFormParticles;
    public ParticleSystem shadowFormParticles;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip shiftToLightSound;
    public AudioClip shiftToShadowSound;

    [Header("Events")]
    public UnityEvent OnShiftToLight;
    public UnityEvent OnShiftToShadow;

    private int playerLayer;
    private int ghostWalkableLayer;

    void Start()
    {
        playerLayer = LayerMask.NameToLayer("Player");
        ghostWalkableLayer = LayerMask.NameToLayer("GhostWalkable");
        
        ApplyVisuals();
        UpdatePhysics();
    }

    void Update()
    {
        if (Input.GetKeyDown(shiftKey))
        {
            ToggleForm();
        }
    }

    public void ToggleForm()
    {
        currentForm = currentForm == Form.Light ? Form.Shadow : Form.Light;
        ApplyVisuals();
        UpdatePhysics();
        PlayShiftEffects();
        InvokeEvents();
    }

    void ApplyVisuals()
    {
        Color targetColor = currentForm == Form.Light ? lightColor : shadowColor;
        
        foreach (var renderer in formRenderers)
        {
            if (renderer != null)
            {
                var material = renderer.material;
                material.SetColor("_BaseColor", targetColor);
                material.SetColor("_Color", targetColor); // For legacy materials
            }
        }

        // Handle particle effects
        if (lightFormParticles != null)
        {
            if (currentForm == Form.Light && !lightFormParticles.isPlaying)
                lightFormParticles.Play();
            else if (currentForm == Form.Shadow && lightFormParticles.isPlaying)
                lightFormParticles.Stop();
        }

        if (shadowFormParticles != null)
        {
            if (currentForm == Form.Shadow && !shadowFormParticles.isPlaying)
                shadowFormParticles.Play();
            else if (currentForm == Form.Light && shadowFormParticles.isPlaying)
                shadowFormParticles.Stop();
        }
    }

    void UpdatePhysics()
    {
        // In Shadow form, player can walk on GhostWalkable surfaces
        // In Light form, player cannot interact with GhostWalkable surfaces
        bool ignoreGhostWalkable = currentForm != Form.Shadow;
        Physics.IgnoreLayerCollision(playerLayer, ghostWalkableLayer, ignoreGhostWalkable);
    }

    void PlayShiftEffects()
    {
        if (audioSource != null)
        {
            AudioClip clipToPlay = currentForm == Form.Light ? shiftToLightSound : shiftToShadowSound;
            if (clipToPlay != null)
                audioSource.PlayOneShot(clipToPlay);
        }
    }

    void InvokeEvents()
    {
        if (currentForm == Form.Light)
            OnShiftToLight?.Invoke();
        else
            OnShiftToShadow?.Invoke();
    }

    public bool IsLight() => currentForm == Form.Light;
    public bool IsShadow() => currentForm == Form.Shadow;

    public void SetForm(Form newForm)
    {
        if (currentForm != newForm)
        {
            currentForm = newForm;
            ApplyVisuals();
            UpdatePhysics();
            InvokeEvents();
        }
    }
}

