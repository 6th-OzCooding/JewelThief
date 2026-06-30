using UnityEngine;

public class UISpinner : MonoBehaviour
{
    [SerializeField] private RectTransform _target;
    [SerializeField] private float _rotateSpeed = 180f;

    private void Awake()
    {
        if (_target == null)
            _target = transform as RectTransform;
    }

    private void Update()
    {
        if (_target == null)
            return;

        _target.Rotate(0f, 0f, -_rotateSpeed * Time.unscaledDeltaTime);
    }
}