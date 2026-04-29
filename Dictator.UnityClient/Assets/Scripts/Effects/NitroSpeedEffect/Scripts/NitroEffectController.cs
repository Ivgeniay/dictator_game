using UnityEngine;

public class NitroEffectController : MonoBehaviour
{
    [SerializeField] private NitroPostProcessEffect _postProcessEffect;
    [SerializeField] private NitroFOVEffect _fovEffect;
    [SerializeField] private NitroCameraShake _cameraShake;

    [Header("Activation")]
    [SerializeField] private float _fadeInDuration = 0.2f;
    [SerializeField] private float _fadeOutDuration = 0.5f;
    [SerializeField] private AnimationCurve _fadeInCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private AnimationCurve _fadeOutCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    private float _currentIntensity;
    private float _targetIntensity;
    private bool _isActive;

    private void Awake()
    {
        ValidateComponents();
        ResetEffects();
    }

    private void ValidateComponents()
    {
        if (_postProcessEffect == null)
            Debug.LogWarning("NitroEffectController: NitroPostProcessEffect не назначен.");

        if (_fovEffect == null)
            Debug.LogWarning("NitroEffectController: NitroFOVEffect не назначен.");

        if (_cameraShake == null)
            Debug.LogWarning("NitroEffectController: NitroCameraShake не назначен.");
    }

    public void Activate()
    {
        if (_isActive)
            return;

        _isActive = true;
        _targetIntensity = 1f;

        StopAllCoroutines();
        StartCoroutine(FadeRoutine(_currentIntensity, 1f, _fadeInDuration, _fadeInCurve));
    }

    public void Deactivate()
    {
        if (!_isActive)
            return;

        _isActive = false;
        _targetIntensity = 0f;

        StopAllCoroutines();
        StartCoroutine(FadeRoutine(_currentIntensity, 0f, _fadeOutDuration, _fadeOutCurve));
    }

    public void SetIntensityDirect(float intensity)
    {
        StopAllCoroutines();
        _currentIntensity = Mathf.Clamp01(intensity);
        _isActive = _currentIntensity > 0f;
        ApplyIntensity(_currentIntensity);
    }

    private System.Collections.IEnumerator FadeRoutine(
        float from,
        float to,
        float duration,
        AnimationCurve curve)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float curvedT = curve.Evaluate(t);
            _currentIntensity = Mathf.Lerp(from, to, curvedT);
            ApplyIntensity(_currentIntensity);
            yield return null;
        }

        _currentIntensity = to;
        ApplyIntensity(_currentIntensity);
    }

    private void ApplyIntensity(float intensity)
    {
        _postProcessEffect?.SetIntensity(intensity);
        _fovEffect?.SetIntensity(intensity);
        _cameraShake?.SetIntensity(intensity);
    }

    private void ResetEffects()
    {
        _currentIntensity = 0f;
        ApplyIntensity(0f);
    }

    private void OnDestroy()
    {
        ResetEffects();
    }
}