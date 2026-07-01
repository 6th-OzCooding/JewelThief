using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class Turret : BaseTrap
{
    [Header("References")]
    [SerializeField] private Transform _headTransform;
    [SerializeField] private Transform _fireTransform;

    [Header("Detection")]
    [SerializeField] private Vector3 _detectionPositionOffset = new Vector3(0f, -0.5f, 0f);
    [SerializeField] private float _rayDistance = 10f;
    [SerializeField] private LayerMask _playerLayerMask;

    [Header("Head Scan")]
    [SerializeField] private float _scanHalfAngle = 45f;
    [SerializeField] private float _scanSpeed = 60f;

    [Header("Fire")]
    [SerializeField] private float _warmUpTime = 2f;
    [SerializeField] private float _cooldownTime = 4f;
    [SerializeField] private int _bulletCount = 18;
    [SerializeField] private int _fireIntervalMs = 100;

    private Quaternion _baseHeadLocalRotation;
    private float _currentScanAngle;
    private int _scanDirection = 1;
    private bool _isFireRoutineRunning = false;

    protected override void Awake()
    {
        base.Awake();

        if (_headTransform != null)
            _baseHeadLocalRotation = _headTransform.localRotation;
    }

    private void Update()
    {
        if (GameManager.Instance.IsPaused)
            return;

        if (_isFireRoutineRunning)
            return;

        ScanHead();


        Debug.DrawRay(_fireTransform.position + _detectionPositionOffset, _fireTransform.forward * _rayDistance, Color.red);


        if (!Physics.Raycast(_fireTransform.position + _detectionPositionOffset, _fireTransform.forward, out RaycastHit hit,
                _rayDistance, _playerLayerMask, QueryTriggerInteraction.Ignore))
            return;

        _isFireRoutineRunning = true;


        Debug.Log("플레이어 감지");
        FireRoutineAsync(this.GetCancellationTokenOnDestroy()).Forget();
    }

    private void ScanHead()
    {
        if (_headTransform == null)
            return;

        _currentScanAngle += _scanDirection * _scanSpeed * Time.deltaTime;

        if (_currentScanAngle >= _scanHalfAngle)
        {
            _currentScanAngle = _scanHalfAngle;
            _scanDirection = -1;
        }
        else if (_currentScanAngle <= -_scanHalfAngle)
        {
            _currentScanAngle = -_scanHalfAngle;
            _scanDirection = 1;
        }

        _headTransform.localRotation =
            _baseHeadLocalRotation * Quaternion.AngleAxis(_currentScanAngle, Vector3.up);
    }


    private async UniTaskVoid FireRoutineAsync(CancellationToken token)
    {
        _isFireRoutineRunning = true;

        Debug.Log("예열");
        await PauseAwareDelay(_warmUpTime, token);

        for (int i = 0; i < _bulletCount; i++)
        {
            GameManager.Sound.PlaySFX(SoundId.SFX_GunTrap01);

            var bullet = GameManager.Pool.SpawnFromPool<Bullet>("Turret_Bullet", _fireTransform.position);
            bullet.Init(_fireTransform.forward);

            await PauseAwareDelay(_fireIntervalMs / 1000f, token);
        }

        Debug.Log("재장전");

        await PauseAwareDelay(_cooldownTime, token);

        _isFireRoutineRunning = false;
    }

    private async UniTask PauseAwareDelay(float seconds, CancellationToken token)
    {
        float elapsedTime = 0f;

        while (elapsedTime < seconds)
        {
            token.ThrowIfCancellationRequested();

            if (GameManager.Instance.IsPaused)
            {
                await UniTask.WaitUntil(() => !GameManager.Instance.IsPaused, cancellationToken: token);
                continue;
            }

            elapsedTime += Time.deltaTime;

            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
    }
}
