using Cysharp.Threading.Tasks;
using UnityEngine;

public class MushroomTrap : BaseDisarmableObejct
{
    [SerializeField] private ParticleSystem _smokeParticle;

    private bool _isActivated = false;

    protected override void LoadData(string id) { }

    protected override void OnDisarm()
    {
        base.OnDisarm();
        _isDisarmed = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isActivated || _isDisarmed) return;

        if (other.CompareTag("Player"))
        {
            _isActivated = true;
            Debug.Log("버섯 함정 발동 - 연막 분출");

            if (_smokeParticle != null)
            {
                _smokeParticle.Play();
                // DespawnAfterParticleAsync().Forget();
            }
            else
            {
                Debug.LogError("MushroomTrap: _smokeParticle이 연결되지 않았습니다.");
            }

            GameManager.Sound.PlaySFX(SoundId.SFX_Explosion03);
        }
    }

    // TODO (김경훈 - 26.06.30) - Trap쪽이 풀로 관리되는 경우 주석해제
    //private async UniTaskVoid DespawnAfterParticleAsync()
    //{
    //    float waitSeconds = _smokeParticle.main.duration + _smokeParticle.main.startLifetime.constantMax;

    //    await UniTask.Delay(System.TimeSpan.FromSeconds(waitSeconds),
    //        cancellationToken: this.GetCancellationTokenOnDestroy());

    //    GameManager.Pool.DespawnToPool(this.gameObject);
    //}
}