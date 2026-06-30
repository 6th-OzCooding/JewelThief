using UnityEngine;

public class MushroomTrap : BaseTrap
{
    [SerializeField] private ParticleSystem _smokeParticle;

    private bool _isActivated = false;

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
            }
            else
            {
                Debug.LogError("MushroomTrap: _smokeParticle이 연결되지 않았습니다.");
            }

            GameManager.Sound.PlaySFX(SoundId.SFX_Explosion03);
        }
    }
}