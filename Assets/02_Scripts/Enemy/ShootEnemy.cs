using UnityEngine;
using System;
using Cysharp.Threading.Tasks;

public class ShootEnemy : MonoBehaviour
{
    [SerializeField] private GameObject _taserEffect;

    [SerializeField] private float _shootDelay = 0.1f; // 총을 뻗는 애니메이션 타이밍 (켜지는 시간)
    [SerializeField] private float _effectDuration = 0.8f; // 이펙트가 터지고 유지되는 시간 (꺼지는 시간)

    private void Start()
    {
        if (_taserEffect != null)
        {
            _taserEffect.SetActive(false);
        }
    }
    public void Shoot()
    {
        // 인스펙터에 할당 안 했을 경우를 대비한 안전 장치
        if (_taserEffect == null)
        {
            Debug.LogError("ShootEnemy: 테이저 이펙트가 할당되지 않았습니다!");
            return;
        }

        ShootRoutine().Forget();
    }

    private async UniTaskVoid ShootRoutine()
    {
        // 오브젝트가 파괴되면 진행 중인 대기를 취소하기 위한 토큰
        var token = this.GetCancellationTokenOnDestroy();

        try
        {
            // 잠시 기다리기
            await UniTask.Delay(TimeSpan.FromSeconds(_shootDelay), cancellationToken: token);

            // 이펙트 켜기
            if (_taserEffect != null)
            {
                _taserEffect.SetActive(true);
                var ps = _taserEffect.GetComponent<ParticleSystem>();
                if (ps != null) ps.Play(true);
            }

            // 이펙트가 재생될 시간만큼 대기
            await UniTask.Delay(TimeSpan.FromSeconds(_effectDuration), cancellationToken: token);

            // 기다린 후 이펙트 끄기
            if (_taserEffect != null)
            {
                _taserEffect.SetActive(false);
            }
        }
        catch (OperationCanceledException)
        {
            if (_taserEffect != null)
            {
                _taserEffect.SetActive(false);
            }
        }
    }
}