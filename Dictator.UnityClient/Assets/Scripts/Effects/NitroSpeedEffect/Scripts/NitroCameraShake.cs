using UnityEngine;

public class NitroCameraShake : MonoBehaviour
{
    [SerializeField] private Camera _camera;

    [Header("Shake")]
    [SerializeField] private float _maxTranslationShake = 0.05f;
    [SerializeField] private float _maxRotationShake = 0.3f;
    [SerializeField] private float _frequency = 20f;
    [SerializeField] private float _smoothTime = 0.05f;

    private float _currentIntensity;
    private Vector3 _currentTranslationOffset;
    private Vector3 _currentRotationOffset;
    private Vector3 _translationVelocity;
    private Vector3 _rotationVelocity;
    private float _seed;

    private bool _originSaved;
    private Vector3 _savedLocalPosition;
    private Quaternion _savedLocalRotation;

    private void Awake()
    {
        if (_camera == null)
            _camera = Camera.main;

        if (_camera == null)
        {
            Debug.LogError("NitroCameraShake: Camera не найдена.");
            return;
        }

        _seed = Random.Range(0f, 100f);
    }

    public void SetIntensity(float intensity)
    {
        _currentIntensity = Mathf.Clamp01(intensity);
    }


    private void Update()
    {
        if (_camera == null)
            return;

        if (!_originSaved)
        {
            _savedLocalPosition = _camera.transform.localPosition;
            _savedLocalRotation = _camera.transform.localRotation;
            _originSaved = true;
        }

        float time = Time.time * _frequency;

        Vector3 targetTranslation = Vector3.zero;
        Vector3 targetRotation = Vector3.zero;

        if (_currentIntensity > 0f)
        {
            targetTranslation = new Vector3(
                (Mathf.PerlinNoise(_seed + 0.0f, time) - 0.5f) * 2f,
                (Mathf.PerlinNoise(_seed + 1.0f, time) - 0.5f) * 2f,
                0f
            ) * _maxTranslationShake * _currentIntensity;

            targetRotation = new Vector3(
                (Mathf.PerlinNoise(_seed + 2.0f, time) - 0.5f) * 2f,
                (Mathf.PerlinNoise(_seed + 3.0f, time) - 0.5f) * 2f,
                (Mathf.PerlinNoise(_seed + 4.0f, time) - 0.5f) * 2f
            ) * _maxRotationShake * _currentIntensity;
        }

        _currentTranslationOffset = new Vector3(
            Mathf.SmoothDamp(_currentTranslationOffset.x, targetTranslation.x, ref _translationVelocity.x, _smoothTime),
            Mathf.SmoothDamp(_currentTranslationOffset.y, targetTranslation.y, ref _translationVelocity.y, _smoothTime),
            Mathf.SmoothDamp(_currentTranslationOffset.z, targetTranslation.z, ref _translationVelocity.z, _smoothTime)
        );

        _currentRotationOffset = new Vector3(
            Mathf.SmoothDamp(_currentRotationOffset.x, targetRotation.x, ref _rotationVelocity.x, _smoothTime),
            Mathf.SmoothDamp(_currentRotationOffset.y, targetRotation.y, ref _rotationVelocity.y, _smoothTime),
            Mathf.SmoothDamp(_currentRotationOffset.z, targetRotation.z, ref _rotationVelocity.z, _smoothTime)
        );

        _camera.transform.localPosition = _savedLocalPosition + _currentTranslationOffset;
        _camera.transform.localRotation = _savedLocalRotation * Quaternion.Euler(_currentRotationOffset);
    }


    public void ResetShake()
    {
        SetIntensity(0f);
    }

    private void OnDestroy()
    {
        if (_camera == null)
            return;

        _camera.transform.localPosition -= _currentTranslationOffset;
        _camera.transform.localRotation *= Quaternion.Inverse(Quaternion.Euler(_currentRotationOffset));
    }
}