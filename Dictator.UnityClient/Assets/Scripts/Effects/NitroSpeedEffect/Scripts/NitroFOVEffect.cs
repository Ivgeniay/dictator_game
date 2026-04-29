using UnityEngine;

public class NitroFOVEffect : MonoBehaviour
{
    [SerializeField] private Camera _camera;

    [Header("FOV")]
    [SerializeField] private float _baseFOV = 60f;
    [SerializeField] private float _targetFOV = 80f;
    [SerializeField] private AnimationCurve _fovCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private float _currentIntensity;
    private float _velocityFOV;

    [Header("Smoothing")]
    [SerializeField] private float _smoothTimeIn = 0.15f;
    [SerializeField] private float _smoothTimeOut = 0.3f;

    private void Awake()
    {
        if (_camera == null)
            _camera = Camera.main;

        if (_camera == null)
        {
            Debug.LogError("NitroFOVEffect: Camera не найдена.");
            return;
        }

        _baseFOV = _camera.fieldOfView;
    }

    public void SetIntensity(float intensity)
    {
        _currentIntensity = Mathf.Clamp01(intensity);
    }

    private void Update()
    {
        if (_camera == null)
            return;

        float curvedIntensity = _fovCurve.Evaluate(_currentIntensity);
        float targetFOV = Mathf.Lerp(_baseFOV, _targetFOV, curvedIntensity);
        float smoothTime = _currentIntensity > 0f ? _smoothTimeIn : _smoothTimeOut;

        _camera.fieldOfView = Mathf.SmoothDamp(
            _camera.fieldOfView,
            targetFOV,
            ref _velocityFOV,
            smoothTime
        );
    }

    public void ResetFOV()
    {
        SetIntensity(0f);
    }

    private void OnDestroy()
    {
        if (_camera != null)
            _camera.fieldOfView = _baseFOV;
    }
}