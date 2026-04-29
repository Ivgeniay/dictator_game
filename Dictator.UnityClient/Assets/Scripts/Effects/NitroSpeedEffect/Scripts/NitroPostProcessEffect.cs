using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class NitroPostProcessEffect : MonoBehaviour
{
    [SerializeField] private Volume _globalVolume;
    [SerializeField] private Material _speedLinesMaterial;

    [Header("Speed Lines")]
    [SerializeField] private float _speedLinesIntensity = 0.7f;

    [Header("Vignette")]
    [SerializeField] private float _vignetteIntensity = 0.5f;
    [SerializeField] private float _vignetteSmoothness = 0.4f;

    [Header("Chromatic Aberration")]
    [SerializeField] private float _chromaticAberrationIntensity = 0.4f;

    [Header("Motion Blur")]
    [SerializeField] private float _motionBlurIntensity = 0.25f;
    [SerializeField] private MotionBlurQuality _motionBlurQuality = MotionBlurQuality.Low;

    private Vignette _vignette;
    private ChromaticAberration _chromaticAberration;
    private MotionBlur _motionBlur;

    private float _currentIntensity;

    private static readonly int ShaderIntensityId = Shader.PropertyToID("_Intensity");

    private void Awake()
    {
        if (_globalVolume == null)
        {
            Debug.LogError("NitroPostProcessEffect: Global Volume не назначен.");
            return;
        }

        _globalVolume.profile.TryGet(out _vignette);
        _globalVolume.profile.TryGet(out _chromaticAberration);
        _globalVolume.profile.TryGet(out _motionBlur);

        if (_motionBlur != null)
            _motionBlur.quality.value = _motionBlurQuality;

        ResetEffects();
    }

    public void SetIntensity(float intensity)
    {
        _currentIntensity = Mathf.Clamp01(intensity);

        ApplySpeedLines(_currentIntensity);
        ApplyVignette(_currentIntensity);
        ApplyChromaticAberration(_currentIntensity);
        ApplyMotionBlur(_currentIntensity);
    }

    private void ApplySpeedLines(float intensity)
    {
        if (_speedLinesMaterial == null)
            return;

        _speedLinesMaterial.SetFloat(ShaderIntensityId, intensity * _speedLinesIntensity);
    }

    private void ApplyVignette(float intensity)
    {
        if (_vignette == null)
            return;

        _vignette.active = intensity > 0f;
        _vignette.intensity.value = intensity * _vignetteIntensity;
        _vignette.smoothness.value = intensity * _vignetteSmoothness;
    }

    private void ApplyChromaticAberration(float intensity)
    {
        if (_chromaticAberration == null)
            return;

        _chromaticAberration.active = intensity > 0f;
        _chromaticAberration.intensity.value = intensity * _chromaticAberrationIntensity;
    }

    private void ApplyMotionBlur(float intensity)
    {
        if (_motionBlur == null)
            return;

        _motionBlur.active = intensity > 0f;
        _motionBlur.intensity.value = intensity * _motionBlurIntensity;
    }

    public void ResetEffects()
    {
        SetIntensity(0f);
    }

    private void OnDestroy()
    {
        ResetEffects();
    }
}