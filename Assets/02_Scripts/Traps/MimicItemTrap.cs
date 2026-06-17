using UnityEngine;

public class MimicItemTrap : MonoBehaviour, IInteractable, IDisarmable
{
    [Header("트랩 데이터")]
    [SerializeField] private int _trapId = 40000001;

    [Header("기본 설정")]
    [SerializeField] private string _interactPrompt = "보석 획득";
    [SerializeField] private float _damage = 1f;
    [SerializeField] private float _soundRadius = 1f;
    [SerializeField] private float _timeReductionAmount = 1f;

    private bool _isDisarmed = false;
    public bool CanInteract => !_isDisarmed;   // 도구로 무력화됐는지 여부 확인

    public string InteractPrompt => _interactPrompt;

    public void TryInteract(IInteractor interactor)
    {
        if (!CanInteract) return;

        Debug.Log($"[함정 발동] ID: {_trapId} - 함정 발동!");

        // interactor.Transform.GetComponent<PlayerController>()?.TakeDamage(_damage);

        TriggerNoise();   // 소음 발생
        Destroy(gameObject, 0.2f);   // 파괴
    }

    public void Disarm()
    {
        if (_isDisarmed) return;

        _isDisarmed = true;
        Debug.Log($"[함정 무력화 상태] ID: {_trapId} - 도구로 해제되었습니다.");

        Destroy(gameObject);
    }

    private void TriggerNoise()
    {
        Collider[] caughtEnemies = Physics.OverlapSphere(transform.position, _soundRadius);
        foreach (Collider col in caughtEnemies)
        {
            //Enemy enemy = col.GetComponent<Enemy>();

            //if (enemy != null)
            {
                //enemy.HearNoise(transform.position);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _soundRadius);
    }
}
